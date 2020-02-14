dos2unix configure-environment.sh
source ./configure-environment.sh

dockerStatus=$(docker ps 2>&1)
if [[ $dockerStatus = Cannot* ]]
then
	shopt -s expand_aliases
	alias docker='docker -H tcp://0.0.0.0:2375'
fi

function create_queue() {
	queueName=$1
	if [ "$AwsQueueIsFifo" = "true" ]
	then
		queueName=$queueName".fifo"
	fi
	queueUrl=$(aws sqs list-queues | jq -r ".QueueUrls[] | select(endswith(\"$1\"))")
	if [ "$queueUrl" = "" ]
	then
		queueUrl=$(aws sqs create-queue --queue-name $1 | jq -r ".QueueUrl")
	fi
	echo "$queueUrl"
}

queueUrl=$(create_queue $AwsQueueName)
deadLetterQueueUrl=$(create_queue $AwsQueueName"-exceptions")
deadLetterQueueArn=$(aws sqs get-queue-attributes --queue-url $deadLetterQueueUrl --attribute-names QueueArn | jq -r ".Attributes.QueueArn")
aws sqs set-queue-attributes --queue-url $queueUrl --attributes "{\"RedrivePolicy\":\"{\\\"maxReceiveCount\\\":\\\"3\\\",\\\"deadLetterTargetArn\\\":\\\"$deadLetterQueueArn\\\"}\",\"ReceiveMessageWaitTimeSeconds\":\"$AwsQueueLongPollTimeSeconds\"}"
echo "RedrivePolicy set to queue $queueUrl"

roleArn=$(aws iam list-roles | jq -r ".Roles[] | select(.RoleName==\"$roleName\") | .Arn")
if [ "$roleArn" = "" ]
then
	echo "Role $roleName does not exist, creating role..."
	roleArn=$(aws iam create-role \
		--role-name $roleName \
		--assume-role-policy-document file://assume-role-policy-document.json | jq -r ".Role.Arn")
	policyArn=$(aws iam list-policies | jq -r ".Policies[] | select(.PolicyName==\"$policyName\") | .Arn")
	aws iam attach-role-policy \
		--role-name $roleName \
		--policy-arn $policyArn
else
	echo "Role $roleName exists"
fi

eval $(aws ecr get-login --no-include-email)

clusterArn=$(aws ecs list-clusters | jq -r ".clusterArns[] | select(. | contains(\"$clusterName\"))")
if [ "$clusterArn" = "" ]
then
	echo "Cluster $clusterName does not exists, creating cluster..."
	result=$(aws ecs create-cluster --cluster-name $clusterName)
else
	echo "Cluster $clusterName exists"
fi

function ip_permission() {
	echo "{\"IpProtocol\": \"tcp\", \"FromPort\": $1, \"ToPort\": $1, \"IpRanges\": [{\"CidrIp\": \"0.0.0.0/0\", \"Description\": \"Port $1\"}]}"
}
securityGroupId=$(aws ec2 describe-security-groups | jq -r ".SecurityGroups[] | select(.GroupName==\"$securityGroup\") | .GroupId")
if [ "$securityGroupId" = "" ]
then
	securityGroupId=$(aws ec2 create-security-group --description $securityGroup --group-name $securityGroup | jq -r ".GroupId")
	aws ec2 authorize-security-group-ingress \
		--group-id $securityGroupId \
		--ip-permissions "[$(ip_permission $sqsWriterPort),$(ip_permission $sqsReaderPort)]"
else
	echo "Security group $securityGroup exists"
fi

function push_image_and_create_task_definition(){
	cd $1/src/$1
	echo "Building docker images..."
	result=$(docker build -t $2 .)
	echo "Docker build is ready"

	registryId=$(aws ecr describe-repositories | jq -r ".repositories[] | select(.repositoryName==\"$2\") | .registryId")
	if [ "$registryId" = "" ]
	then
		registryId=$(aws ecr create-repository --repository-name $2 | jq -r ".repository.registryId")
	fi

	imageId=$(docker images --filter "reference=$2:latest" -q)
	imageTag=$registryId.dkr.ecr.$AwsRegion.amazonaws.com/$2
	docker tag $imageId $imageTag
	docker push $imageTag

	taskRevision=$(aws ecs describe-task-definition --task-definition $1 | jq -r ".taskDefinition.revision")
	if [ "$taskRevision" = "" ]
	then
		echo "Task definition $1 does not exits, creating task definition..."
		containerDefinition="name=$1,\
		image=$imageTag,\
		environment=[\
			{name=AwsQueueIsFifo,value=$AwsQueueIsFifo},\
			{name=AwsRegion,value=$AwsRegion},\
			{name=AwsQueueName,value=$AwsQueueName},\
			{name=AwsAccessKey,value=$AwsAccessKey},\
			{name=AwsSecretKey,value=$AwsSecretKey},\
			{name=AwsQueueAutomaticallyCreate,value=$AwsQueueAutomaticallyCreate},\
			{name=AwsQueueLongPollTimeSeconds,value=$AwsQueueLongPollTimeSeconds}\
		],\
		logConfiguration={\
			logDriver=awslogs,\
			options={\
				awslogs-group=ecs/$1,\
				awslogs-region=$AwsRegion,\
				awslogs-stream-prefix=ecs\
			}\
		}"
		# Replace tabs with nothing
		containerDefinition=${containerDefinition//	/}

		taskRevision=$(aws ecs register-task-definition \
			--family $1 \
			--execution-role-arn $roleArn \
			--network-mode awsvpc \
			--container-definitions $containerDefinition \
			--requires-compatibilities "FARGATE" \
			--cpu "256" \
			--memory "512" | jq -r ".taskDefinition.revision")
	else
		echo "Task definition $1 exists"
	fi

	serviceStatus=$(aws ecs describe-services --cluster $clusterName --services $1 | jq -r ".services[] | select(.serviceName==\"$1\") | .status")
	availabilityZone=$AwsRegion"a"
	subnetId=$(aws ec2 describe-subnets | jq -r ".Subnets[] | select(.AvailabilityZone==\"$availabilityZone\") | .SubnetId")
	if [ "$serviceStatus" != "ACTIVE" ]
	then
		echo "Service $1 does not exist, creating service..."
		result=$(aws logs create-log-group --log-group-name ecs/$1)

		result=$(aws ecs create-service --cluster $clusterName \
			--service-name $1 \
			--task-definition "$1:$taskRevision" \
			--desired-count 1 \
			--launch-type "FARGATE" \
			--network-configuration "awsvpcConfiguration={subnets=[$subnetId],securityGroups=[$securityGroupId],assignPublicIp=ENABLED}")
	else
		echo "Service $1 exists, updating service..."
		result=$(aws ecs update-service --cluster $clusterName \
			--service $1 \
			--task-definition "$1:$taskRevision" \
			--desired-count 1 \
			--network-configuration "awsvpcConfiguration={subnets=[$subnetId],securityGroups=[$securityGroupId],assignPublicIp=ENABLED}")
	fi

	taskStatus=NONE
	while [ "$taskStatus" != "RUNNING" ]
	do
		sleep 5
		taskArn=$(aws ecs list-tasks --cluster $clusterName --family $1 | jq -r ".taskArns[0]")
		taskStatus=$(aws ecs describe-tasks --cluster $clusterName --tasks $taskArn | jq -r ".tasks[0].lastStatus")
	done

	networkInterfaceId=$(aws ecs describe-tasks --cluster $clusterName --tasks $taskArn | jq -r ".tasks[0].attachments[0].details[] | select(.name==\"networkInterfaceId\") | .value")
	ipAddress=$(aws ec2 describe-network-interfaces --filters "Name=network-interface-id,Values=$networkInterfaceId" | jq -r ".NetworkInterfaces[0].Association.PublicIp")
	if [ "$1" = "$sqsWriterProjectName" ]
	then
		echo "$1 endpoint: http://$ipAddress:$sqsWriterPort"
	else
		echo "$1 endpoint: http://$ipAddress:$sqsReaderPort"
	fi

	cd .. && cd .. && cd ..
}

push_image_and_create_task_definition $sqsReaderProjectName $sqsReaderRepository
push_image_and_create_task_definition $sqsWriterProjectName $sqsWriterRepository
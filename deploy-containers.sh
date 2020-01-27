export AWS_ACCESS_KEY_ID=$AwsAccessKey
export AWS_SECRET_ACCESS_KEY=$AwsSecretKey
export AWS_DEFAULT_REGION=$AwsRegion

clusterName=aws-examples-csharp
securityGroup=$clusterName
sqsReaderProjectName=SqsReader
sqsReaderRepository=aws.examples.csharp.sqs.reader
sqsWriterProjectName=SqsWriter
sqsReaderRepository=aws.examples.csharp.sqs.writer
taskRole=ecsTaskExecutionRole
roleArn=$(aws iam list-roles | jq -r ".Roles[] | select(.RoleName==\"$taskRole\") | .Arn")

eval $(aws ecr get-login --no-include-email)

clusterArn=$(aws ecs list-clusters | jq -r ".clusterArns[] | select(. | contains(\"$clusterName\"))")
if [ "$clusterArn" = "" ]
then
	aws ecs create-cluster --cluster-name $clusterName
fi

securityGroupId=$(aws ec2 describe-security-groups | jq -r ".SecurityGroups[] | select(.GroupName==\"$securityGroup\") | .GroupId")
if [ "$securityGroupId" = "" ]
then
	securityGroupId=$(aws ec2 create-security-group --description $securityGroup --group-name $securityGroup | jq -r ".GroupId")
	aws ec2 authorize-security-group-ingress \
		--group-id $securityGroupId \
		--ip-permissions '[{"IpProtocol": "tcp", "FromPort": 5100, "ToPort": 5100, "IpRanges": [{"CidrIp": "0.0.0.0/0", "Description": "Port 5100"}]},{"IpProtocol": "tcp", "FromPort": 5200, "ToPort": 5200, "IpRanges": [{"CidrIp": "0.0.0.0/0", "Description": "Port 5200"}]}]'
fi

function push_image_and_create_task_definition(){
	cd $1/src/$1
	docker build -t $2 .

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
	fi

	existingService=$(aws ecs describe-services --cluster $clusterName --services $1 | jq -r ".services[] | select(.AvailabilityZone==\"$availabilityZone\") | .SubnetId")
	if [ "$existingService" = "" ]
	then
		aws logs create-log-group --log-group-name ecs/$1

		availabilityZone=$AwsRegion"a"
		subnetId=$(aws ec2 describe-subnets | jq -r ".Subnets[] | select(.AvailabilityZone==\"$availabilityZone\") | .SubnetId")
		aws ecs create-service --cluster $clusterName \
			--service-name $1 \
			--task-definition "$1:$taskRevision" \
			--desired-count 1 \
			--launch-type "FARGATE" \
			--network-configuration "awsvpcConfiguration={subnets=[$subnetId],securityGroups=[$securityGroupId],assignPublicIp=ENABLED}"

		taskArn=$(aws ecs list-tasks --cluster $clusterName --family $1 | jq -r ".taskArns[0]")
		networkInterfaceId=$(aws ecs describe-tasks --cluster $clusterName --tasks $taskArn | jq -r ".tasks[0].attachments[0].details[] | select(.name==\"networkInterfaceId\") | .value")
		ipAddress=$(aws ec2 describe-network-interfaces --filters "Name=network-interface-id,Values=$networkInterfaceId" | jq -r ".NetworkInterfaces[0].Association.PublicIp")
		if [ "$1" = "$sqsWriterProjectName" ]
		then
			echo "Endpoint: http://$ipAddress:5100"
		fi
	fi

	cd .. && cd .. && cd ..
}

push_image_and_create_task_definition $sqsReaderProjectName $sqsReaderRepository
push_image_and_create_task_definition $sqsWriterProjectName $sqsReaderRepository
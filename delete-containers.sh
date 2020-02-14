dos2unix configure-environment.sh
source ./configure-environment.sh

function delete_services() {
	for taskDefinition in $(aws ecs list-task-definitions | jq -r ".taskDefinitionArns[] | select(. | contains(\"$1\"))")
	do
		IFS='/' read -r -a definitionVersion <<< $taskDefinition
		definitionVersion=${definitionVersion[${#definitionVersion[@]}-1]}
		echo "Deleting task definition $taskDefinition"
		eval $(echo "aws ecs deregister-task-definition --task-definition "$definitionVersion"") > /dev/null
		echo "Task definition $taskDefinition deleted"
	done

	aws logs delete-log-group --log-group-name ecs/$1

	echo "Deleting service $1..."
	result=$(aws ecs delete-service --cluster $clusterName --service $1 --force)
	echo "Service $1 deleted"

	taskArn=$(aws ecs list-tasks --cluster $clusterName --family $1 | jq -r ".taskArns[0]")
	taskStatus=RUNNING
	while [ "$taskStatus" = "RUNNING" ]
	do
		sleep 5
		taskStatus=$(aws ecs describe-tasks --cluster $clusterName --tasks $taskArn | jq -r ".tasks[0].lastStatus")
	done

	echo "Deleting repository $2..."
	result=$(aws ecr delete-repository --repository-name $2 --force)
	echo "Repository $2 deleted"
}

function delete_queue() {
	queueName=$1
	if [ "$AwsQueueIsFifo" = "true" ]
	then
		queueName=$queueName".fifo"
	fi
	queueUrl=$(aws sqs get-queue-url --queue-name $queueName | jq -r ".QueueUrl")
	aws sqs delete-queue --queue-url $queueUrl
}

delete_services $sqsReaderProjectName $sqsReaderRepository
delete_services $sqsWriterProjectName $sqsWriterRepository

echo "Deleting cluster $clusterName..."
result=$(aws ecs delete-cluster --cluster $clusterName)
echo "Cluster $clusterName deleted"

aws ec2 delete-security-group --group-name $securityGroup

policyArn=$(aws iam list-policies | jq -r ".Policies[] | select(.PolicyName==\"$policyName\") | .Arn")
aws iam detach-role-policy --role-name $roleName --policy-arn $policyArn
aws iam delete-role --role-name $roleName

echo "Deleting $actorsTable table..."
result=$(aws dynamodb delete-table --table-name $actorsTable)
echo "Table $actorsTable deleted"
echo "Deleting $moviesTable table..."
result=$(aws dynamodb delete-table --table-name $moviesTable)
echo "Table $moviesTable deleted"

delete_queue $AwsQueueName
delete_queue $AwsQueueName"-exceptions"
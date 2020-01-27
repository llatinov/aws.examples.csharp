export AWS_ACCESS_KEY_ID=$AwsAccessKey
export AWS_SECRET_ACCESS_KEY=$AwsSecretKey
export AWS_DEFAULT_REGION=$AwsRegion

clusterName=aws-examples-csharp
securityGroup=$clusterName
sqsReaderProjectName=SqsReader
sqsReaderRepository=aws.examples.csharp.sqs.reader
sqsWriterProjectName=SqsWriter
sqsReaderRepository=aws.examples.csharp.sqs.writer

function delete_services() {
	for taskDefinition in $(aws ecs list-task-definitions | jq -r ".taskDefinitionArns[] | select(. | contains(\"$1\"))")
	do
		IFS='/' read -r -a definitionVersion <<< $taskDefinition
		definitionVersion=${definitionVersion[${#definitionVersion[@]}-1]}
		eval $(echo "aws ecs deregister-task-definition --task-definition "$definitionVersion"")
	done

	aws logs delete-log-group --log-group-name ecs/$1

	aws ecs delete-service --cluster $clusterName --service $1 --force

	taskArn=$(aws ecs list-tasks --cluster $clusterName --family $1 | jq -r ".taskArns[0]")
	taskStatus=RUNNING
	while [ "$taskStatus" = "RUNNING" ]
	do
		sleep 5
		taskStatus=$(aws ecs describe-tasks --cluster $clusterName --tasks $taskArn | jq -r ".tasks[0].lastStatus")
	done

	aws ecr delete-repository --repository-name $2 --force
}

delete_services $sqsReaderProjectName $sqsReaderRepository
delete_services $sqsWriterProjectName $sqsWriterRepository

aws ecs delete-cluster --cluster $clusterName

aws ec2 delete-security-group --group-name $securityGroup
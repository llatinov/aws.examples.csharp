dos2unix local-configure-environment.sh
source ./local-configure-environment.sh

function cleanup {
	docker-compose -f local-docker-compose-containers.yml down
}

trap cleanup EXIT

function create_queue() {
	queueName=$1
	if [ "$AwsQueueIsFifo" = "true" ]
	then
		queueName=$queueName".fifo"
	fi
	queueUrl=$(aws sqs list-queues --endpoint-url=http://localhost:4576 | jq -r ".QueueUrls[] | select(endswith(\"$1\"))")
	if [ "$queueUrl" = "" ]
	then
		queueUrl=$(aws sqs create-queue --queue-name $1 --endpoint-url=http://localhost:4576 | jq -r ".QueueUrl")
	fi
	echo "$queueUrl"
}

queueUrl=$(create_queue $AwsQueueName)
deadLetterQueueUrl=$(create_queue $AwsQueueName"-exceptions")
deadLetterQueueArn=$(aws sqs get-queue-attributes --queue-url $deadLetterQueueUrl --attribute-names QueueArn --endpoint-url=http://localhost:4576 | jq -r ".Attributes.QueueArn")
aws sqs set-queue-attributes --queue-url $queueUrl --attributes "{\"RedrivePolicy\":\"{\\\"maxReceiveCount\\\":\\\"3\\\",\\\"deadLetterTargetArn\\\":\\\"$deadLetterQueueArn\\\"}\",\"ReceiveMessageWaitTimeSeconds\":\"$AwsQueueLongPollTimeSeconds\"}" --endpoint-url=http://localhost:4576
echo "RedrivePolicy set to queue $queueUrl"

docker-compose -f local-docker-compose-containers.yml build
docker-compose -f local-docker-compose-containers.yml up
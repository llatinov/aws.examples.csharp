if [ -z "$AwsAccessKey" ]
then
  echo "Please provide AwsAccessKey"
  exit 1
fi

if [ -z "$AwsSecretKey" ]
then
  echo "Please provide AwsSecretKey"
  exit 1
fi

if [ -z "$AwsRegion" ]
then
  echo "Please provide AwsRegion"
  exit 1
fi

export AWS_ACCESS_KEY_ID=$AwsAccessKey
export AWS_SECRET_ACCESS_KEY=$AwsSecretKey
export AWS_DEFAULT_REGION=$AwsRegion

export AwsQueueAutomaticallyCreate=${AwsQueueAutomaticallyCreate:-true}
export AwsQueueIsFifo=${AwsQueueIsFifo:-false}
export AwsQueueLongPollTimeSeconds=${AwsQueueLongPollTimeSeconds:-20}
export AwsQueueName=${AwsQueueName:-aws-examples-sqs-queue}

export roleName=aws-examples-admin-role
export policyName=AdministratorAccess
export actorsLambda=ActorsLambdaFunction
export moviesLambda=MoviesLambdaFunction
export clusterName=aws-examples-csharp
export securityGroup=$clusterName
export sqsReaderProjectName=SqsReader
export sqsReaderRepository=aws.examples.csharp.sqs.reader
export sqsWriterProjectName=SqsWriter
export sqsWriterRepository=aws.examples.csharp.sqs.writer
export actorsTable=Actors
export moviesTable=Movies
export entriesTable=LogEntries
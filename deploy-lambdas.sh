export AWS_ACCESS_KEY_ID=$AwsAccessKey
export AWS_SECRET_ACCESS_KEY=$AwsSecretKey
export AWS_DEFAULT_REGION=$AwsRegion

roleName=lambda-admin-role
policyName=AdministratorAccess
timeout=30
lambdasZipFile=DynamoDbLambdas/src/DynamoDbLambdas/bin/Release/netcoreapp2.1/DynamoDbLambdas.zip
actorsLambda=ActorsLambdaFunction
actorsTable=Actors
actorsHandler=DynamoDbLambdas::DynamoDbLambdas.ActorsFunction::FunctionHandler
moviesLambda=MoviesLambdaFunction
moviesTable=Movies
moviesHandler=DynamoDbLambdas::DynamoDbLambdas.MoviesFunction::FunctionHandler
entriesTable=LogEntries

actorsStreamArn=$(aws dynamodb describe-table --table-name $actorsTable | jq -r ".Table | select(.TableName==\"$actorsTable\") | .LatestStreamArn")
if [ "$actorsStreamArn" = "" ]
then
	actorsStreamArn=$(aws dynamodb create-table \
		--table-name $actorsTable \
		--attribute-definitions 'AttributeName=Id,AttributeType=S' \
		--key-schema 'AttributeName=Id,KeyType=HASH' \
		--provisioned-throughput 'ReadCapacityUnits=5,WriteCapacityUnits=5' \
		--stream-specification 'StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES' | jq -r ".TableDescription.LatestStreamArn")
fi

moviesStreamArn=$(aws dynamodb describe-table --table-name $moviesTable | jq -r ".Table | select(.TableName==\"$moviesTable\") | .LatestStreamArn")
if [ "$moviesStreamArn" = "" ]
then
	moviesStreamArn=$(aws dynamodb create-table \
		--table-name $moviesTable \
		--attribute-definitions AttributeName=Title,AttributeType=S \
		--key-schema AttributeName=Title,KeyType=HASH \
		--provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
		--stream-specification StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES | jq -r ".TableDescription.LatestStreamArn")
fi

entriesArn=$(aws dynamodb describe-table --table-name $entriesTable | jq -r ".Table | select(.TableName==\"$entriesTable\") | .Arn")
if [ "$entriesArn" = "" ]
then
	aws dynamodb create-table \
		--table-name $entriesTable \
		--attribute-definitions AttributeName=Message,AttributeType=S AttributeName=DateTime,AttributeType=S \
		--key-schema AttributeName=Message,KeyType=HASH AttributeName=DateTime,KeyType=RANGE \
		--provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5
fi

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

function create_or_update_lambda() {
	functionArn=$(aws lambda list-functions | jq -r ".Functions[] | select(.FunctionName==\"$1\") | .FunctionArn")
	if [ "$functionArn" = "" ]
	then
		echo "Function $1 does not exist, creating function..."
		aws lambda create-function \
			--function-name $1 \
			--runtime dotnetcore2.1 \
			--role $roleArn \
			--handler $2 \
			--environment "Variables={AWS_SQS_QUEUE_NAME=$AwsQueueName, AWS_SQS_IS_FIFO=$AwsQueueIsFifo}" \
			--timeout $timeout \
			--zip-file fileb://$lambdasZipFile
		aws lambda create-event-source-mapping \
			--function-name $1 \
			--event-source-arn $3 \
			--starting-position LATEST
	else
		echo "Function $1 exists, updating function..."
		aws lambda update-function-code \
			--function-name $1 \
			--zip-file fileb://$lambdasZipFile
		aws lambda update-function-configuration \
			--function-name $1 \
			--role $roleArn \
			--handler $2 \
			--environment "Variables={AWS_SQS_QUEUE_NAME=$AwsQueueName, AWS_SQS_IS_FIFO=$AwsQueueIsFifo}" \
			--timeout $timeout
	fi
}

dotnet lambda package --project-location DynamoDbLambdas/src/DynamoDbLambdas

create_or_update_lambda $actorsLambda $actorsHandler $actorsStreamArn
create_or_update_lambda $moviesLambda $moviesHandler $moviesStreamArn
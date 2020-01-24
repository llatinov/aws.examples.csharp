roleName=lambda-admin-role
policyName=AdministratorAccess
lambdasZipFile=DynamoDbLambdas/src/DynamoDbLambdas/bin/Release/netcoreapp2.1/DynamoDbLambdas.zip
actorsLambda=ActorsLambdaFunction
actorsTable=Actors
actorsHandler=DynamoDbLambdas::DynamoDbLambdas.ActorFunction::FunctionHandler

actorsArn=$(aws dynamodb describe-table --table-name $actorsTable --endpoint-url=http://localhost:4569 | jq -r ".Table | select(.TableName==\"$actorsTable\") | .LatestStreamArn")
if [ "$actorsArn" = "" ]
then
	echo "Table $actorsTable does not exist"
	exit 1
fi

roleArn=$(aws iam list-roles --endpoint-url=http://localhost:4593 | jq -r ".Roles[] | select(.RoleName==\"$roleName\") | .Arn")
if [ "$roleArn" = "" ]
then
	echo "Role $roleName does not exist, creating role..."
	roleArn=$(aws iam create-role \
		--role-name $roleName \
		--assume-role-policy-document file://assume-role-policy-document.json \
		--endpoint-url=http://localhost:4593 | jq -r ".Role.Arn")
	policyArn=$(aws iam list-policies --endpoint-url=http://localhost:4593 | jq -r ".Policies[] | select(.PolicyName==\"$policyName\") | .Arn")
	aws iam attach-role-policy \
		--role-name $roleName \
		--policy-arn $policyArn \
		--endpoint-url=http://localhost:4593
else
	echo "Role $roleName exists"
fi

function create_or_update_lambda() {
	functionArn=$(aws lambda list-functions --endpoint-url=http://localhost:4574 | jq -r ".Functions[] | select(.FunctionName==\"$1\") | .FunctionArn")
	if [ "$functionArn" = "" ]
	then
		echo "Function $1 does not exist, creating function..."
		aws lambda create-function \
			--function-name $1 \
			--runtime dotnetcore2.1 \
			--role $roleArn \
			--handler $2 \
			--zip-file fileb://$lambdasZipFile \
			--endpoint-url=http://localhost:4574
		aws lambda create-event-source-mapping \
			--function-name $1 \
			--event-source-arn $3 \
			--starting-position LATEST \
			--endpoint-url=http://localhost:4574
	else
		echo "Function $1 exists, updating function..."
		aws lambda update-function-code \
			--function-name $1 \
			--zip-file fileb://$lambdasZipFile \
			--endpoint-url=http://localhost:4574
		aws lambda update-function-configuration \
			--function-name $1 \
			--role $roleArn \
			--handler $2 \
			--endpoint-url=http://localhost:4574
	fi
}

dotnet lambda package --project-location DynamoDbLambdas/src/DynamoDbLambdas

create_or_update_lambda $actorsLambda $actorsHandler $actorsArn
export AWS_ACCESS_KEY_ID=$AwsAccessKey
export AWS_SECRET_ACCESS_KEY=$AwsSecretKey
export AWS_DEFAULT_REGION=$AwsRegion

roleName=lambda-admin-role
policyName=AdministratorAccess
actorsLambda=ActorsLambdaFunction
moviesLambda=MoviesLambdaFunction
actorsTable=Actors
moviesTable=Movies
entriesTable=LogEntries

function delete_lambda() {
	for mapping in $(aws lambda list-event-source-mappings --function-name $1 | jq --compact-output ".EventSourceMappings[] | {UUID: .UUID}")
	do
		uuid=$(jq -r ".UUID" <<< $mapping)
		aws lambda delete-event-source-mapping --uuid $uuid
	done

	aws lambda delete-function --function-name $1

	MSYS_NO_PATHCONV=1 aws logs delete-log-group --log-group-name /aws/lambda/$1
}

delete_lambda $actorsLambda

delete_lambda $moviesLambda

aws dynamodb delete-table --table-name $actorsTable

aws dynamodb delete-table --table-name $moviesTable

aws dynamodb delete-table --table-name $entriesTable
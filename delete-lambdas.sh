dos2unix configure-environment.sh
source ./configure-environment.sh

function delete_lambda() {
	for mapping in $(aws lambda list-event-source-mappings --function-name $1 | jq --compact-output ".EventSourceMappings[] | {UUID: .UUID}")
	do
		uuid=$(jq -r ".UUID" <<< $mapping)
		echo "Deleting event source mapping $uuid..."
		result=$(aws lambda delete-event-source-mapping --uuid $uuid)
		echo "Event source mapping $uuid deleted"
	done

	echo "Deleting $1 lambda..."
	result=$(aws lambda delete-function --function-name $1)
	echo "Lambda $1 deleted"

	MSYS_NO_PATHCONV=1 aws logs delete-log-group --log-group-name /aws/lambda/$1
}

delete_lambda $actorsLambda
delete_lambda $moviesLambda

echo "Deleting $actorsTable table..."
result=$(aws dynamodb delete-table --table-name $actorsTable)
echo "Table $actorsTable deleted"
echo "Deleting $moviesTable table..."
result=$(aws dynamodb delete-table --table-name $moviesTable)
echo "Table $moviesTable deleted"
echo "Deleting $entriesTable table..."
result=$(aws dynamodb delete-table --table-name $entriesTable)
echo "Table $entriesTable deleted"

policyArn=$(aws iam list-policies | jq -r ".Policies[] | select(.PolicyName==\"$policyName\") | .Arn")
aws iam detach-role-policy --role-name $roleName --policy-arn $policyArn
aws iam delete-role --role-name $roleName
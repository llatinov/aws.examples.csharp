dos2unix configure-environment.sh
source ./configure-environment.sh

cd DynamoDbServerless
export actorsTableArn=$(aws dynamodb describe-table --table-name $actorsTable | jq -r ".Table | select(.TableName==\"$actorsTable\") | .TableArn")
export moviesTableArn=$(aws dynamodb describe-table --table-name $moviesTable | jq -r ".Table | select(.TableName==\"$moviesTable\") | .TableArn")
sls remove --region $AwsRegion
cd ..
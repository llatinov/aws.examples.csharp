dos2unix configure-environment.sh
source ./configure-environment.sh

cd DynamoDbServerless
dos2unix build.sh
echo "Building and packaging lambda..."
result=$(./build.sh)
echo "Lambda packaged"
export actorsTableArn=$(aws dynamodb describe-table --table-name $actorsTable | jq -r ".Table | select(.TableName==\"$actorsTable\") | .TableArn")
export moviesTableArn=$(aws dynamodb describe-table --table-name $moviesTable | jq -r ".Table | select(.TableName==\"$moviesTable\") | .TableArn")
sls deploy --region $AwsRegion
cd ..
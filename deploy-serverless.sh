dos2unix configure-environment.sh
source ./configure-environment.sh

cd DynamoDbServerless
dos2unix build.sh
echo "Building and packaging lambda..."
result=$(./build.sh)
echo "Lambda packaged"
export actorsTableArn=$(aws dynamodb describe-table --table-name $actorsTable | jq -r ".Table | select(.TableName==\"$actorsTable\") | .TableArn")
sls deploy --region $AwsRegion
cd ..
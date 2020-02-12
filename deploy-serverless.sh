dos2unix configure-environment.sh
source ./configure-environment.sh

cd DynamoDbServerless
dos2unix build.sh
echo "Building and packaging lambda..."
result=$(./build.sh)
echo "Lambda packaged"

export actorsTableArn=$(aws dynamodb describe-table --table-name $actorsTable | jq -r ".Table | select(.TableName==\"$actorsTable\") | .TableArn")
if [ "$actorsTableArn" = "" ]
then
	echo "Table $actorsTable does not exits, creating table..."
	export actorsTableArn=$(aws dynamodb create-table \
		--table-name $actorsTable \
		--attribute-definitions 'AttributeName=FirstName,AttributeType=S' 'AttributeName=LastName,AttributeType=S' \
		--key-schema 'AttributeName=FirstName,KeyType=HASH' 'AttributeName=LastName,KeyType=RANGE' \
		--provisioned-throughput 'ReadCapacityUnits=5,WriteCapacityUnits=5' \
		--stream-specification 'StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES' | jq -r ".TableDescription.TableArn")
else
	echo "Table $actorsTable exists"
fi

export moviesTableArn=$(aws dynamodb describe-table --table-name $moviesTable | jq -r ".Table | select(.TableName==\"$moviesTable\") | .TableArn")
if [ "$moviesTableArn" = "" ]
then
	echo "Table $moviesTable does not exits, creating table..."
	export moviesTableArn=$(aws dynamodb create-table \
		--table-name $moviesTable \
		--attribute-definitions AttributeName=Title,AttributeType=S \
		--key-schema AttributeName=Title,KeyType=HASH \
		--provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
		--stream-specification StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES | jq -r ".TableDescription.TableArn")
else
	echo "Table $moviesTable exists"
fi

sls deploy --region $AwsRegion
cd ..
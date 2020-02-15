dos2unix local-configure-environment.sh
source ./local-configure-environment.sh

function cleanup {
	mv serverless.yml serverless-localstack.yml
	mv serverless-original.yml serverless.yml
	cd ..
}

trap cleanup EXIT

export actorsTableArn=$(aws dynamodb describe-table --table-name $actorsTable --endpoint-url=http://localhost:4569 | jq -r ".Table | select(.TableName==\"$actorsTable\") | .TableArn")
if [ "$actorsTableArn" = "" ]
then
	echo "Table $actorsTable does not exits, creating table..."
	export actorsTableArn=$(aws dynamodb create-table \
		--table-name $actorsTable \
		--attribute-definitions 'AttributeName=FirstName,AttributeType=S' 'AttributeName=LastName,AttributeType=S' \
		--key-schema 'AttributeName=FirstName,KeyType=HASH' 'AttributeName=LastName,KeyType=RANGE' \
		--provisioned-throughput 'ReadCapacityUnits=5,WriteCapacityUnits=5' \
		--stream-specification 'StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES' \
		--endpoint-url=http://localhost:4569 | jq -r ".TableDescription.TableArn")
else
	echo "Table $actorsTable exists"
fi

export moviesTableArn=$(aws dynamodb describe-table --table-name $moviesTable --endpoint-url=http://localhost:4569 | jq -r ".Table | select(.TableName==\"$moviesTable\") | .TableArn")
if [ "$moviesTableArn" = "" ]
then
	echo "Table $moviesTable does not exits, creating table..."
	export moviesTableArn=$(aws dynamodb create-table \
		--table-name $moviesTable \
		--attribute-definitions AttributeName=Title,AttributeType=S \
		--key-schema AttributeName=Title,KeyType=HASH \
		--provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
		--stream-specification StreamEnabled=true,StreamViewType=NEW_AND_OLD_IMAGES \
		--endpoint-url=http://localhost:4569 | jq -r ".TableDescription.TableArn")
else
	echo "Table $moviesTable exists"
fi


cd DynamoDbServerless
npm install serverless-localstack
aws s3api create-bucket --bucket $localstackBucket --endpoint-url=http://localhost:4572
mv serverless.yml serverless-original.yml
mv serverless-localstack.yml serverless.yml

dos2unix build.sh
echo "Building and packaging lambda..."
result=$(./build.sh)
if [[ $result == *"error"* ]]; then
	echo "ERROR during lambda build"
	echo $result
else
	echo "Lambda packaged"
fi

sls deploy --stage local

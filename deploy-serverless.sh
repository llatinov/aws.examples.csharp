export AWS_ACCESS_KEY_ID=$AwsAccessKey
export AWS_SECRET_ACCESS_KEY=$AwsSecretKey

cd DynamoDbServerless
./build.sh
sls deploy --region $AwsRegion
cd ..
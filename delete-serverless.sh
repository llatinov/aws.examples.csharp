export AWS_ACCESS_KEY_ID=$AwsAccessKey
export AWS_SECRET_ACCESS_KEY=$AwsSecretKey

cd DynamoDbServerless
sls remove --region $AwsRegion
cd ..
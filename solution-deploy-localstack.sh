# In case of Windows
if [[ "$OSTYPE" == "cygwin" ]] || [[ "$OSTYPE" == "msys" ]]
then
	export TMP_FOLDER="//c/tmp/localstack"
else
	export TMP_FOLDER="/tmp/localstack"
fi

docker-compose -f local-docker-compose-localstack.yml up -d

until [ $(docker-compose -f local-docker-compose-localstack.yml logs | grep 'Ready.' | wc -w) -gt 0 ]
do
	echo 'Waiting for Localstack';
	sleep 5;
done

dos2unix local-deploy-lambdas.sh
./local-deploy-lambdas.sh

dos2unix local-deploy-serverless.sh
./local-deploy-serverless.sh

dos2unix local-deploy-containers.sh
./local-deploy-containers.sh
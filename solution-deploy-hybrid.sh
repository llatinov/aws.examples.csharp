dos2unix configure-environment.sh
source ./configure-environment.sh

dos2unix deploy-lambdas.sh
./deploy-lambdas.sh

dos2unix deploy-serverless.sh
./deploy-serverless.sh

export AwsQueueAutomaticallyCreate=true
result=$(docker-compose build)
docker-compose up
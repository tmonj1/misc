#
# prerequisites
#
#   export AWS_REGION=ap-northeast-1
#   export AWS_ACCOUNT_ID=&lt;aws account id>
#   export AWS_ECR_URL=${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com

AWS_ECR_URL=${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com

# build the image and add a tag to it
APP_NAME=demo1
APP_VERSION=0.1.1
APP_TAG=${APP_NAME}:${APP_VERSION}

docker build -t ${APP_TAG} -t ${AWS_ECR_URL}/${APP_TAG} .

# login to Amazon ECR
aws ecr get-login-password --region ${AWS_REGION} | docker login --username AWS --password-stdin ${AWS_ECR_URL}

# create a new repo if not exist
aws ecr describe-repositories --repository-names ${APP_NAME} > /dev/null || {
    aws ecr create-repository --repository-name ${APP_NAME}
}

# push the image to Amazon ECR
docker push ${AWS_ECR_URL}/${APP_TAG}

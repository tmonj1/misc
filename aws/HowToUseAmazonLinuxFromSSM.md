# How to use Amazon Linux 2 from AWS SSM Session Manager

## 1. Prerequisites

* Your Amazon Linux 2 instance must be accessible from SSM

## 2. Operation example

## (1) Become "ec2-user"

```bash
#you are ssm-user when starting SSM session
$ whoami
ssm-user
#become "ec2-user"
$ sudo su --login ec2-user
#now you are ec2-user
$ whoami
ec2-user
```

* You can become `root`, too.

## (2) Use AWS CLI

```bash
#copy a file on S3 to local directory (need AmazonS3ReadOnlyAccess)
$ aws s3 cp s3://<S3 bucket name>/path/to/file file
#list ec2 instances (require AmazonEC2FullAccess)
$ aws ec2 describe-instances --region ap-northeast-1
```
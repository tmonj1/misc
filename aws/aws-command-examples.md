# AWS CLI command examples

## list all instances, output InstanceId only.
aws ec2 describe-instances --query 'Reservations[].Instances[].[InstanceId]' --output text

## list instances with SEP tag equal to 14.2
aws ec2 describe-instances --query 'Reservations[].Instances[].[InstanceId]' --filter "Name=tag:SEP,Values=14.2" --output text

* --filter is case sensitive

## list instances with their state being "running"
aws ec2 describe-instances --filter "Name=instance-state-code,Values=16" --query "Reservations[].Instances[].[InstanceId]"

## start instances

https://dev.classmethod.jp/cloud/aws/awscli-tips-ec2-start-stop/


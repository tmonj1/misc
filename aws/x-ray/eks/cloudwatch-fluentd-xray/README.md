- [前提](#前提)
- [0. Docker イメージ作成](#0-docker-イメージ作成)
- [1. Docker イメージの ECR への登録](#1-docker-イメージの-ecr-への登録)
- [2. AWSのベースリソースの構築](#2-awsのベースリソースの構築)
- [3. EKS クラスターの構築](#3-eks-クラスターの構築)
- [4. EKS クラスターにアプリケーションをデプロイ](#4-eks-クラスターにアプリケーションをデプロイ)
  - [5. サービスアカウントの作成](#5-サービスアカウントの作成)
  - [6. CloudWatch, FluentD, X-Rayのデーモンを実行](#6-cloudwatch-fluentd-x-rayのデーモンを実行)
  - [7. サービスのデプロイ](#7-サービスのデプロイ)
- [8. 後始末](#8-後始末)

---

## 前提

以下がインストールされていること

* Git bash (Windowsのみ)
* aws cli
* eksctl
* (optional) jq
* (optional) envsubst

## 0. Docker イメージ作成

```bash
# App1 のイメージの作成 (ビルドもこの中で実行される)
$ cd app1
$ docker build -t app1:0.2 .
# App2 も同様に実行
$ cd app2
$ docker build -t app2:0.2 .
```

## 1. Docker イメージの ECR への登録

ECR レジストリに App1 と App2 のイメージを登録。

```bash
# 準備
$ export AWS_REGION=ap-northeast-1
$ export AWS_ACCOUNT_ID=&lt;aws account id>
$ export AWS_ECR_URL=${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com
# ECRリポジトリの作成
$ aws cloudformation create-stack --stack-name x-ray-demo-ecr-repos --template-body file://x-ray-demo-ecr-cfn.yaml
{
    "StackId": "arn:aws:cloudformation:ap-northeast-1:<aws_account_id>:stack/x-ray-demo-ecr-repos/8d65aa10-eb07-11ea-a24d-06d571b61710"
}
# ECR レジストリに docker ログイン
$ aws ecr get-login-password --region ap-northeast-1 | docker login --username AWS --password-stdin ${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com
Login Succeeded
# App1 イメージにタグを打つ
$ docker tag app1:0.2 ${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com/tmj/x-ray-demo-app1:0.2
# App1 イメージを ECR に push
$ docker push ${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com/tmj/x-ray-demo-app1:0.2
The push refers to repository [<aws_account_id>.dkr.ecr.ap-northeast-1.amazonaws.com/tmj/x-ray-demo-app1]
66db7cdbecc2: Pushed 
d605ac5fa9d8: Pushed 
2e849d361dc1: Pushed 
c370033eb984: Pushed 
2135da4d457a: Pushed 
b24b2d7a1887: Pushed 
d0f104dc0a1f: Pushed 
0.1: digest: sha256:bd0fbf507c256a5576b048bfb38ea9fb235267b9e66ff2c19543e84bad73c1b9 size: 1792
# 同様に App2 も push
$ (省略)
```

## 2. AWSのベースリソースの構築

```bash
#AWS ベースリソースの構築
$ aws cloudformation create-stack --stack-name x-ray-demo-eks-base --template-body file://base-resource-cfn.yaml
#確認
$ aws cloudformation list-stacks --stack-status-filter CREATE_COMPLETE
(出力結果省略) 
#出力の取得
$ aws cloudformation describe-stacks --stack-name x-ray-demo-eks-base --query Stacks[].Outputs[]
[
    {
        "OutputKey": "RouteTable", 
        "OutputValue": "rtb-030d34cf833f06fba"
    }, 
    {
        "OutputKey": "Subnets", 
        "OutputValue": "subnet-06932a8cbf762650f,subnet-09c7da4014e70ab0a"
    }, 
    {
        "OutputKey": "VPC", 
        "OutputValue": "vpc-01b4145786124c58c"
    }
]
```

## 3. EKS クラスターの構築

```bash
#EKSクラスターの構築
#クラスターを新規作成
$ eksctl create cluster -f x-ray-cluster-eksctl.yaml
:
: (snip)
:
[✔]  EKS cluster "x-ray-demo-cluster" in "ap-northeast-1" region is ready
#カレントコンテキストに設定されたことを確認
$ kubectl config current-context
xxx@x-ray-demo-cluster.ap-northeast-1.eksctl.io
```

## 4. EKS クラスターにアプリケーションをデプロイ

```bash
#Namespaceの作成
$ kubectl apply -f x-ray-demo-ns.yaml
#作成したNamespaceにコンテキストを設定
#kubectl config set-context x-ray-demo --cluster <CLUSTER> --user <AUTHINFO> --namespace x-ray-demo-ns
$ kubectl config set-context x-ray-demo --cluster x-ray-demo-cluster.ap-northeast-1.eksctl.io \
 --user monji@x-ray-demo-cluster.ap-northeast-1.eksctl.io --namespace x-ray-demo-ns
#カレントコンテキストを変更
$ kubectl config use-context x-ray-demo
```

### 5. サービスアカウントの作成

```bash
#OIDCプロバイダーを作成
$ eksctl utils associate-iam-oidc-provider --cluster x-ray-demo-cluster --approve
#サービスアカウントの作成
$ kubectl apply -f x-ray-base.yaml
#確認
$ aws eks describe-cluster --name x-ray-demo-cluster --region ${AWS_REGION} --query "cluster.identity.oidc.issuer" --output text
https://oidc.eks.ap-northeast-1.amazonaws.com/id/B85F6C1F624724CDB78146BD04D8B039
#サービスアカウントとIAMロールの紐付け
$ eksctl create iamserviceaccount --name fluentd \
  --namespace amazon-cloudwatch \
  --cluster x-ray-demo-cluster \
  --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
  --approve \
  --override-existing-serviceaccounts

$ eksctl create iamserviceaccount --name cloudwatch-agent \
  --namespace amazon-cloudwatch \
  --cluster x-ray-demo-cluster \
  --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
  --approve \
  --override-existing-serviceaccounts

$ eksctl create iamserviceaccount --name xrayd \
  --namespace amazon-cloudwatch \
  --cluster x-ray-demo-cluster \
  --attach-policy-arn arn:aws:iam::aws:policy/AWSXRayDaemonWriteAccess \
  --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
  --approve \
  --override-existing-serviceaccounts
#確認(3つそれぞれにIAMロールのarnが設定されている)
$ kubectl -n amazon-cloudwatch get sa -o yaml |grep arn
eks.amazonaws.com/role-arn: arn:aws:iam::<aws_account_id>:role/eksctl-x-ray-demo-cluster-addon-iamserviceac-Role1-1DPB5C4W53UGR
eks.amazonaws.com/role-arn: arn:aws:iam::<aws_account_id>:role/eksctl-x-ray-demo-cluster-addon-iamserviceac-Role1-4W8YSL3IH1ZU
eks.amazonaws.com/role-arn: arn:aws:iam::<aws_account_id>:role/eksctl-x-ray-demo-cluster-addon-iamserviceac-Role1-8ZGBUT4FG4F9
```

### 6. CloudWatch, FluentD, X-Rayのデーモンを実行

```bash
#Daemonsetとして実行
$ kubectl apply -f cloudwatch-fluentd-xray.yaml
```

### 7. サービスのデプロイ

```bash
#App1とApp2のデプロイ
$ envsubst < x-ray-demo-apps-deploy.template.yaml | kubectl apply -f -
#Serviceの公開 (ELB -> App2 -> App1)
$ kubectl apply -f x-ray-demo-apps-svc.yaml
#サービスのURLを取得
$ kubectl get svc
NAME       TYPE           CLUSTER-IP       EXTERNAL-IP                                                                    PORT(S)        AGE
app1-svc   ClusterIP      10.100.4.31      <none>                                                                         2080/TCP       10m
app2-svc   LoadBalancer   10.100.167.232   a26d871d0f6524f58bbbca197b539f18-1736671221.ap-northeast-1.elb.amazonaws.com   80:31485/TCP   10m
#サービスの実行確認 (ヘルスチェックURLで確認)
$ curl a26d871d0f6524f58bbbca197b539f18-1736671221.ap-northeast-1.elb.amazonaws.com/health
ok
```

## 8. 後始末

```bash
#ECRリポジトリの削除
$ aws cloudformation delete-stack --stack-name x-ray-demo-ecr-repos
#サービスの削除
$ kubectl delete -f x-ray-demo-apps-svc.yaml
#デプロイの削除
$ envsubst < x-ray-demo-apps-deploy.template.yaml | kubectl delete -f -
#デーモンセットの削除
$ kubectl delete -f cloudwatch-fluentd-xray.yaml
#IAMサービスアカウントの削除
$ kubectl apply -f x-ray-base.yaml
#$ eksctl delete iamserviceaccount --cluster x-ray-demo-cluster cloudwatch-agent
#$ eksctl delete iamserviceaccount --cluster x-ray-demo-cluster fluentd
#$ eksctl delete iamserviceaccount --cluster x-ray-demo-cluster xrayd
#OIDCプロバイダーとIAMロールの削除
$ OIDCURL=$(aws eks describe-cluster --name x-ray-demo-cluster --output json | jq -r .cluster.identity.oidc.issuer | sed -e "s*https://**")
$ aws iam delete-open-id-connect-provider --open-id-connect-provider-arn arn:aws:iam::${AWS_ACCOUNT_ID}:oidc-provider/${OIDCURL}
#EKSクラスターの削除
$ eksctl delete cluster -f x-ray-cluster-eksctl.yaml
#AWSベースリソースの削除
$ aws cloudformation delete-stack --stack-name x-ray-demo-eks-base
```

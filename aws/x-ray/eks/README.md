## 1. Docker イメージの ECR への登録

ECR レジストリに App1 と App2 のイメージを登録。

```bash
# 準備
$ export AWS_ACCOUNT_ID=372853230800
# ECR レジストリの作成
$ aws cloudformation create-stack --stack-name x-ray-demo-ecr-repos --template-body file://x-ray-demo-ecr-cfn.yaml
{
    "StackId": "arn:aws:cloudformation:ap-northeast-1:<aws_account_id>:stack/x-ray-demo-ecr-repos/8d65aa10-eb07-11ea-a24d-06d571b61710"
}
# ECR レジストリに docker ログイン
$ aws ecr get-login-password --region ap-northeast-1 | docker login --username AWS --password-stdin ${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com
Login Succeeded
# App1 イメージにタグを打つ
$ docker tag app1:0.1 ${AWS_ACCOUNT_ID}.dkr.ecr.us-east-1.amazonaws.com/tmj/x-ray-demo-app1:0.1
# App1 イメージを ECR に push
$ docker push ${AWS_ACCOUNT_ID}.dkr.ecr.us-east-1.amazonaws.com/tmj/x-ray-demo-app1:0.1
# 同様に App2 も push
$ (省略)
```

詳しくは[AWS CLI を使用した Amazon ECR の開始方法](https://docs.aws.amazon.com/ja_jp/AmazonECR/latest/userguide/getting-started-cli.html)を参照。

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

## 3.1 EKS クラスターの新規作成

```bash
#EKSクラスターの構築
#クラスターを新規作成
$ eksctl create cluster -f x-ray-cluster-eksctl.yaml
[ℹ]  eksctl version 0.24.0
[ℹ]  using region ap-northeast-1
[✔]  using existing VPC (vpc-01b4145786124c58c) and subnets (private:[] public:[subnet-06932a8cbf762650f subnet-09c7da4014e70ab0a])
[!]  custom VPC/subnets will be used; if resulting cluster doesnt function as expected, make sure to review the configuration of VPC/subnets
[ℹ]  nodegroup "x-ray-demo-nodegroup" will use "ami-048669b0687eb3ad4" [AmazonLinux2/1.17]
[ℹ]  using Kubernetes version 1.17
[ℹ]  creating EKS cluster "x-ray-demo-cluster" in "ap-northeast-1" region with un-managed nodes
[ℹ]  1 nodegroup (x-ray-demo-nodegroup) was included (based on the include/exclude rules)
[ℹ]  will create a CloudFormation stack for cluster itself and 1 nodegroup stack(s)
[ℹ]  will create a CloudFormation stack for cluster itself and 0 managed nodegroup stack(s)
[ℹ]  if you encounter any issues, check CloudFormation console or try 'eksctl utils describe-stacks --region=ap-northeast-1 --cluster=x-ray-demo-cluster'
[ℹ]  CloudWatch logging will not be enabled for cluster "x-ray-demo-cluster" in "ap-northeast-1"
[ℹ]  you can enable it with 'eksctl utils update-cluster-logging --region=ap-northeast-1 --cluster=x-ray-demo-cluster'
[ℹ]  Kubernetes API endpoint access will use default of {publicAccess=true, privateAccess=false} for cluster "x-ray-demo-cluster" in "ap-northeast-1"
[ℹ]  2 sequential tasks: { create cluster control plane "x-ray-demo-cluster", 2 sequential sub-tasks: { no tasks, create nodegroup "x-ray-demo-nodegroup" } }
[ℹ]  building cluster stack "eksctl-x-ray-demo-cluster-cluster"
[ℹ]  deploying stack "eksctl-x-ray-demo-cluster-cluster"
[ℹ]  building nodegroup stack "eksctl-x-ray-demo-cluster-nodegroup-x-ray-demo-nodegroup"
[ℹ]  deploying stack "eksctl-x-ray-demo-cluster-nodegroup-x-ray-demo-nodegroup"
[ℹ]  waiting for the control plane availability...
[✔]  saved kubeconfig as "/Users/taromonji/.kube/config"
[ℹ]  no tasks
[✔]  all EKS cluster resources for "x-ray-demo-cluster" have been created
[ℹ]  adding identity "arn:aws:iam::<aws_account_id>:role/eksctl-x-ray-demo-cluster-nodegro-NodeInstanceRole-29MLLSEH9364" to auth ConfigMap
[ℹ]  nodegroup "x-ray-demo-nodegroup" has 0 node(s)
[ℹ]  waiting for at least 1 node(s) to become ready in "x-ray-demo-nodegroup"
[ℹ]  nodegroup "x-ray-demo-nodegroup" has 1 node(s)
[ℹ]  node "ip-192-168-0-180.ap-northeast-1.compute.internal" is ready
[ℹ]  kubectl command should work with "/Users/taromonji/.kube/config", try 'kubectl get nodes'
[✔]  EKS cluster "x-ray-demo-cluster" in "ap-northeast-1" region is ready
#カレントコンテキストに設定されたことを確認
$ kubectl config current-context
xxx@x-ray-demo-cluster.ap-northeast-1.eksctl.io
#ノードの確認
$ kubectl get nodes
NAME                                               STATUS   ROLES    AGE     VERSION
ip-192-168-0-180.ap-northeast-1.compute.internal   Ready    <none>   9m43s   v1.17.9-eks-4c6976
```

* eksctl は内部的には CloudFormation を使ってクラスターを構築している。このため、CloudFormation のコンソール画面から構築状況が確認できる。
* eksctl でクラスターを作ると、作成したクラスターがカレントコンテキストになるようにkubeconfigも更新される。

## 3.2 作成したクラスターの確認

確認のため、Nginx 1個だけを含むテスト Pod をデプロイしてみる。

```bash
#テスト Pod のデプロイ
$ kubectl apply -f test-pod.yaml
pod/nginx-pod created
#Pod ができていることを確認
$ kubectl get pods
NAME        READY   STATUS    RESTARTS   AGE
nginx-pod   1/1     Running   0          16s
#ローカルPCの8080番ポートを Nginx Pod の80番ポートにフォワーディング
$ kubectl port-forward nginx-pod 8080:80
# ここでブラウザから"http://localhost:8080"にアクセスし、"Welcome to Nginx!"画面が出ればOK
# 確認できたらテスト Pod は不要なので削除
$ kubectl delete pod nginx-pod
pod "nginx-pod" deleted
```

## 4. EKS クラスターにアプリケーションをデプロイ

### 4.1 Namespace の作成

```bash
#Namespaceの作成
$ kubectl apply -f x-ray-demo-ns.yaml
#作成したNamespaceにコンテキストを設定
$ kubectl config set-context x-ray-demo-ns --cluster <CLUSTER>
```

### 4.2 アプリケーションのデプロイ

(TBD)

```bash
```

## 5. 後始末

```bash
#ECRリポジトリの削除
$ aws cloudformation delete-stack --stack-name x-ray-demo-ecr-repos
#EKSクラスターの削除
$ eksctl delete cluster -f x-ray-cluster-eksctl.yaml
#AWSベースリソースの削除
$ aws cloudformation delete-stack --stack-name x-ray-demo-eks-base
```
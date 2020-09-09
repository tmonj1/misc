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
$ export AWS_ACCOUNT_ID=372853230800
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

## 3.2 作成したクラスターの構成確認

### (1) クラスターの情報

```bash
#クラスターのマスターサービスとクラスターサービス (CoreDNS) のアドレスを取得
$ kubectl cluster-info
Kubernetes master is running at https://46FBBF6C50D0CE7FD0AEEFA34A3EA24D.gr7.ap-northeast-1.eks.amazonaws.com
CoreDNS is running at https://46FBBF6C50D0CE7FD0AEEFA34A3EA24D.gr7.ap-northeast-1.eks.amazonaws.com/api/v1/namespaces/kube-system/services/kube-dns:dns/proxy

#クラスター名を取得
$ eksctl get cluster
NAME                    REGION
x-ray-demo-cluster      ap-northeast-1

#クラスターのノードグループの情報を取得
$ eksctl get nodegroups --cluster x-ray-demo-cluster -o yaml
- Cluster: x-ray-demo-cluster
  CreationTime: "2020-08-31T10:20:37.253Z"
  DesiredCapacity: 1
  ImageID: ami-048669b0687eb3ad4
  InstanceType: t2.small
  MaxSize: 2
  MinSize: 1
  Name: x-ray-demo-nodegroup
  NodeInstanceRoleARN: arn:aws:iam::<aws_account_id>:role/eksctl-x-ray-demo-cluster-nodegro-NodeInstanceRole-1UG2OG9O7B4RS
  StackName: eksctl-x-ray-demo-cluster-nodegroup-x-ray-demo-nodegroup

#ノードの情報を取得
$ kubectl get node (-o yaml)
NAME                                              STATUS   ROLES    AGE    VERSION
ip-192-168-1-36.ap-northeast-1.compute.internal   Ready    <none>   158m   v1.17.9-eks-4c6976

#ノードがどのEC2インスタンスで動作しているか (see https://github.com/weaveworks/eksctl/issues/509)
$ kubectl get nodes ip-192-168-1-36.ap-northeast-1.compute.internal -o jsonpath="{.spec.providerID}"
aws:///ap-northeast-1c/i-04a8c1e1ac2bdb122
```
(TBD)
* security group は勝手に作られるらしい？大丈夫なの？ -> 指定可能
* インスタンスプロファイルも勝手に作られるらしい？指定できないの？ -> 指定可能
* custom security hardened AMIは使えない？ -> 使える
see [EKS入門者向けに「今こそ振り返るEKSの基礎」というタイトルで登壇しました](https://dev.classmethod.jp/articles/eks_basic/)

## 3.3 Podのデプロイ確認

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
#kubectl config set-context x-ray-demo --cluster <CLUSTER> --user <AUTHINFO> --namespace x-ray-demo-ns
$ kubectl config set-context x-ray-demo --cluster x-ray-demo-cluster.ap-northeast-1.eksctl.io \
 --user monji@x-ray-demo-cluster.ap-northeast-1.eksctl.io --namespace x-ray-demo-ns
#カレントコンテキストを変更
$ kubectl config use-context x-ray-demo
```

### 4.2 サービスアカウントの作成

```bash
#K8sのサービスアカウントとOIDCプロバイダーを関連付ける
$ eksctl utils associate-iam-oidc-provider --cluster x-ray-demo-cluster --approve
#サービスアカウントの作成
$ kubectl apply -f x-ray-base.yaml
#確認
$ kubectl -n amazon-cloudwatch get sa 
NAME               SECRETS   AGE
cloudwatch-agent   1         68s
default            1         68s
fluentd            1         68s
xrayd              1         68s
```

### 4.3 サービスアカウントとIAMとの紐付け

```bash
#クラスターのOIDCプロバイダー機能を有効化
$ eksctl utils associate-iam-oidc-provider --name x-ray-demo-cluster --approve
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
  --approve \
  --override-existing-serviceaccounts
```

### 4.4 CloudWatch, FluentD, X-Rayのデーモンを実行

```bash
#Daemonsetとして実行
$ kubectl apply -f cloudwatch-fluentd-xray.yaml
```

### 4.5 サービスのデプロイ

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

## 5. 後始末

```bash
#ECRリポジトリの削除
$ aws cloudformation delete-stack --stack-name x-ray-demo-ecr-repos
#デプロイの削除
$ kubectl delete deploy app1
$ kubectl delete deploy app2
#デーモンセットの削除
$ kubectl delete -f cloudwatch-fluentd-xray.yaml
#IAMサービスアカウントの削除
$ eksctl delete iamserviceaccount --cluster x-ray-demo-cluster cloudwatch-agent
$ eksctl delete iamserviceaccount --cluster x-ray-demo-cluster fluentd
$ eksctl delete iamserviceaccount --cluster x-ray-demo-cluster xrayd
#EKSクラスターの削除
$ eksctl delete cluster -f x-ray-cluster-eksctl.yaml
#AWSベースリソースの削除
$ aws cloudformation delete-stack --stack-name x-ray-demo-eks-base
```

---

## A1. VPCの要件

### (1) クラスターVPCの要件 (Required)

  * サブネット
    * サブネット2つ以上、内1つはpublic (推奨: private と public それぞれ1個以上)
  * ネットワーク
    * 外部用ALB用にpublicサブネットが必要
    * デフォルトでは、クラスターイントロスペクションと起動時のノード登録のため、ノードにもoutboundのインターネットアクセスが必要
    * Dockerイメージ取得のため、ノードに outbound のインターネットアクセスとS3アクセスが必要

source:
  Cluster VPC consideration

---
* `appmesh.k8s.aws/sidecarInjectorWebhook: enabled` を namespace の `metadata.labels` に設定すると、この ns 下で作成された Pod には自動的に Envoy proxy が注入される。
  * 出典: [Using sidecar injection on Amazon EKS with AWS App Mesh](https://aws.amazon.com/jp/blogs/containers/using-sidecar-injection-on-amazon-eks-with-aws-app-mesh/)
* kubernetes で LoadBalancer タイプで Service を作成すると、NLB が直接 Pod (実際にはDeployment) に接続するのではなく、間に ClusterIP のサービス(?) が追加されているようだ。たとえば、ポート80:5000でNLBを作ると、80:32644 のように30,000番台のポート番号の何者かに転送する形で作られる（このポート番号は勝手に振られる）。その後、NLBとPodの間に追加されたClusterIPサービスにより32644から5000に再度マッピングされるようだ。
* [Configure App Mesh integration with Kubernetes](https://docs.aws.amazon.com/eks/latest/userguide/mesh-k8s-integration.html#configure-app-mesh) で X-Ray 出力できるか試してみる
  * service-a は app1-svcとapp2-svc に置換
  * helm upgrade のところで `--set tracing.enable=true` と `--set tracing.provider=x-ray` を追加
    * 出典: [eks-charts の GitHub のREADME.md](https://github.com/aws/eks-charts/tree/master/stable/appmesh-controller)
  ---
  #下記はうまく行かなかった (`helm upgrade` で `--set tracing.provider=x-ray` を追加)
  helm upgrade -i appmesh-controller eks/appmesh-controller     --namespace appmesh-system     --set region=$AWS_REGION     --set serviceAccount.create=false     --set serviceAccount.name=appmesh-controller --set tracing.enabled=true --set tracing.provider=x-ray
  #下記もダメ
  helm upgrade -i appmesh-inject eks/appmesh-inject \
--namespace appmesh-system \
--set mesh.name=my-mesh \
--set mesh.create=false \
--set tracing.enabled=true \
--set tracing.provider=x-ray
  ---
  eksctl create iamserviceaccount \
    --cluster $CLUSTER_NAME \
    --namespace x-ray-demo-ns \
    --name my-service-a \
    --attach-policy-arn  arn:aws:iam::372853230800:policy/x-ray-demo-policy \
    --override-existing-serviceaccounts \
    --approve

    
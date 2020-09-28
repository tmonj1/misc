
# App Mesh環境構築手順

#### 0. 前提

* ECRにイメージ (app1とapp2) がすでに登録されていること。

(参考) app1イメージのECRへの登録

```bash
#app2のタグを打つ
$ docker tag app1:0.2 ${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com/myapp-app1:0.2
#app2をECRにpush
```

#### 1. 環境変数の設定

```bash
# export AWS_PROFILE=default
# export AWS_DEFAULT_REGION=ap-northeast-1
export AWS_REGION=ap-northeast-1
export AWS_ACCOUNT_ID=<AWS_ACCOUNT_ID>
export AWS_ECR_URL=$AWS_ACCOUNT_ID.dkr.ecr.ap-northeast-1.amazonaws.com
export AWS_CLUSTER_NAME=app
```

#### 2. マニフェストテンプレートの編集

`generate_files.sh`を開き、ファイル先頭付近の`rlist`を編集して必要な置換文字列をすべて設定する。その他、アプリケーションの追加など必要があればマニフェストファイルテンプレート(`.template.yaml`)と関連する設定ファイルを編集する。

```bash
$ vi generate_files.sh
#
# repalcement list
#
rlist=('{{CLUSTER_NAME}} => app')                     #クラスタ名
rlist+=('{{APP_NAMESPACE}} => app-ns')                #アプリケーションの名前空間
rlist+=('{{MESH_NAME}} => my-mesh')                   #メッシュ名
rlist+=('{{SUBNET_1a}} => subnet-058ecd35d42d6516a')  #サブネット1aのID
rlist+=('{{SUBNET_1c}} => subnet-068a2568991910099')  #サブネット1cのID
```

#### 3. マニフェストファイルの生成

テンプレートファイル (.template.yamlなど) からマニフェストファイルを生成する。

```bash
#appフォルダに生成後のファイルが作成される
$ ./generate_files.sh -e -o app *.template.*
#appフォルダにtemplate以外のファイルをコピー
$ find . -type f -maxdepth 1|grep -v template |xargs -IX cp X app
```

#### 4. クラスタ構築

```bash
#appフォルダに移動
$ cd app
#クラスター作成
$ eksctl create cluster -f cluster.eksctl.yaml
:
: (snip)
:
[✔]  EKS cluster "app" in "ap-northeast-1" region is ready
```

#### 5. AWS App Mesh Controller For K8s の導入 

[Tutorial: Configure App Mesh integration with Kubernetes](https://docs.aws.amazon.com/app-mesh/latest/userguide/mesh-k8s-integration.html)の手順に従ってMeshコントローラをクラスターにデプロイする。今回の例ではECSとFargateは使わないため、この2つの手順はスキップしてよい。

```bash
#eks-chartsをhelmリポジトリに追加
$ helm repo add eks https://aws.github.io/eks-charts

#App Mesh Controller For K8s の CRD をインストール
$ kubectl apply -k "https://github.com/aws/eks-charts/stable/appmesh-controller/crds?ref=master"
customresourcedefinition.apiextensions.k8s.io/gatewayroutes.appmesh.k8s.aws created
customresourcedefinition.apiextensions.k8s.io/meshes.appmesh.k8s.aws created
customresourcedefinition.apiextensions.k8s.io/virtualgateways.appmesh.k8s.aws created
customresourcedefinition.apiextensions.k8s.io/virtualnodes.appmesh.k8s.aws created
customresourcedefinition.apiextensions.k8s.io/virtualrouters.appmesh.k8s.aws created
customresourcedefinition.apiextensions.k8s.io/virtualservices.appmesh.k8s.aws created

#appmesh-system名前空間を作成
$ kubectl create ns appmesh-system
namespace/appmesh-system created

#OIDCプロバイダを作成
eksctl utils associate-iam-oidc-provider \
    --region=$AWS_REGION \
    --cluster $AWS_CLUSTER_NAME \
    --approve
[ℹ]  eksctl version 0.27.0
[ℹ]  using region ap-northeast-1
[✔]  created IAM Open ID Connect provider for cluster "app" in "ap-northeast-1"

#AWSAppMeshFullAccessとAWSCloudMapFullAccess権限を持つサービスアカウントを作成
eksctl create iamserviceaccount \
    --cluster $AWS_CLUSTER_NAME \
    --namespace appmesh-system \
    --name appmesh-controller \
    --attach-policy-arn arn:aws:iam::aws:policy/AWSCloudMapFullAccess \
    --attach-policy-arn arn:aws:iam::aws:policy/AWSAppMeshFullAccess \
    --override-existing-serviceaccounts \
    --approve
:
: (snip)
:
[ℹ]  created serviceaccount "appmesh-system/appmesh-controller"

#Meshコントローラをデプロイ
$ helm upgrade -i appmesh-controller eks/appmesh-controller \
    --namespace appmesh-system \
    --set region=$AWS_REGION \
    --set serviceAccount.create=false \
    --set serviceAccount.name=appmesh-controller \
    --set tracing.enabled=true \
    --set tracing.provider=x-ray
:
: (snip)
:
AWS App Mesh controller installed!
```

#### 6. コンテキストの設定

```bash
#app namespace作成
$ kubectl apply -f app-ns.yaml
namespace/app-ns created

#コンテキストの確認
CURRENT  NAME                            CLUSTER                       AUTHINFO                        NAMESPACE
         docker-desktop                  docker-desktop                docker-desktop                       
         docker-for-desktop              docker-desktop                docker-desktop                       
*        x@app.ap-northeast-1.eksctl.io  app.ap-northeast-1.eksctl.io  x@app.ap-northeast-1.eksctl.io 

#コンテキスト設定
$ kubectl config set-context app --cluster app.ap-northeast-1.eksctl.io --user <yourname>@app.ap-northeast-1.eksctl.io --namespace app-ns

#カレントコンテキストを設定
$ kubectl config use-context app
Switched to context "app".
```

#### 6. サービスアカウントの作成

```bash
#OIDCプロバイダ作成 (2.で実行済みのときはスキップしてよい)
$ eksctl utils associate-iam-oidc-provider --cluster app --approve
[ℹ]  eksctl version 0.27.0
[ℹ]  using region ap-northeast-1
[ℹ]  will create IAM Open ID Connect provider for cluster "app" in "ap-northeast-1"
[✔]  created IAM Open ID Connect provider for cluster "app" in "ap-northeast-1"

#確認 (下記コマンドに対してURLが出力されればOK)
$ aws eks describe-cluster --name ${AWS_CLUSTER_NAME} --region ${AWS_REGION} --query "cluster.identity.oidc.issuer" --output text
https://oidc.eks.ap-northeast-1.amazonaws.com/id/36457240B5F2BF047C14C6C1DE23E13F

#fluentdのサービスアカウント作成
https://oidc.eks.ap-northeast-1.amazonaws.com/id/6AAE875F95B98C0AC6AE60F36B9E6494
eksctl create iamserviceaccount --name fluentd \
  --namespace amazon-cloudwatch \
  --cluster ${AWS_CLUSTER_NAME} \
  --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
  --approve \
  --override-existing-serviceaccounts
:
: (snip)
:
[ℹ]  created serviceaccount "amazon-cloudwatch/fluentd"

#cloudwatch-agentのサービスアカウント作成
$ eksctl create iamserviceaccount --name cloudwatch-agent \
  --namespace amazon-cloudwatch \
  --cluster ${AWS_CLUSTER_NAME} \
  --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
  --approve \
  --override-existing-serviceaccounts
:
: (snip)
:
[ℹ]  created serviceaccount "amazon-cloudwatch/cloudwatch-agent"

#xraydのサービスアカウント作成
$ eksctl create iamserviceaccount --name xrayd \
  --namespace amazon-cloudwatch \
  --cluster ${AWS_CLUSTER_NAME} \
  --attach-policy-arn arn:aws:iam::aws:policy/AWSXRayDaemonWriteAccess \
  --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
  --approve \
  --override-existing-serviceaccounts
:
: (snip)
:
[ℹ]  created serviceaccount "amazon-cloudwatch/xrayd"

#確認(3つそれぞれにIAMロールのarnが設定されている)
$ kubectl -n amazon-cloudwatch get sa -o yaml |grep arn
eks.amazonaws.com/role-arn: arn:aws:iam::<AWS_ACCOUNT_ID>:role/eksctl-app-addon-iamserviceaccount-amazon-cl-Role1-39BJT2WOQ0KM
eks.amazonaws.com/role-arn: arn:aws:iam::<AWS_ACCOUNT_ID>:role/eksctl-app-addon-iamserviceaccount-amazon-cl-Role1-FOWVSLB1CLO0
eks.amazonaws.com/role-arn: arn:aws:iam::<AWS_ACCOUNT_ID>:role/eksctl-app-addon-iamserviceaccount-amazon-cl-Role1-1N6IWYE9O02O1
```

#### 7. アプリケーションのデプロイ

```bash
#Proxy認可の有効化 (https://docs.aws.amazon.com/app-mesh/latest/userguide/proxy-authorization.html)
$ aws iam create-policy --policy-name app-policy --policy-document file://app-proxy-auth.json

#app1のサービスアカウント生成
$ eksctl create iamserviceaccount \
    --cluster $AWS_CLUSTER_NAME \
    --namespace app-ns \
    --name app1-svc \
    --attach-policy-arn arn:aws:iam::aws:policy/AWSXRayDaemonWriteAccess \
    --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
    --attach-policy-arn  arn:aws:iam::${AWS_ACCOUNT_ID}:policy/app-policy \
    --override-existing-serviceaccounts \
    --approve

#app2のサービスアカウント生成 (X-RayとCWのポリシーはApp Meshでは不要なはずだが試してないのでこのままにしておく)
$ eksctl create iamserviceaccount \
    --cluster $AWS_CLUSTER_NAME \
    --namespace app-ns \
    --name app2-svc \
    --attach-policy-arn arn:aws:iam::aws:policy/AWSXRayDaemonWriteAccess \
    --attach-policy-arn arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy \
    --attach-policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess \
    --attach-policy-arn arn:aws:iam::${AWS_ACCOUNT_ID}:policy/app-policy \
    --override-existing-serviceaccounts \
    --approve

#Deploymentのデプロイ
$ kubectl apply -f app-deploy.yaml 
deployment.apps/app1 created
deployment.apps/app2 created

#Serviceのデプロイ
$ kubectl apply -f app-svc.yaml
service/app1-svc created
service/app2-svc created
```

#### 8. Meshリソースのデプロイ

```bash
#Meshの構築
$ kubectl apply -f app-mesh.yaml
mesh.appmesh.k8s.aws/my-mesh created

#Meshリソースのデプロイ (Virtual Node)
$ kubectl apply -f app-virtual-node.yaml
virtualnode.appmesh.k8s.aws/app1-svc created
virtualnode.appmesh.k8s.aws/app2-svc created

#Meshリソースのデプロイ (Virtual Router)
$ kubectl apply -f app-virtual-router.yaml
virtualrouter.appmesh.k8s.aws/app1-svc-virtual-router created
virtualrouter.appmesh.k8s.aws/app2-svc-virtual-router created

#Meshリソースのデプロイ (Virtual Service)
$ kubectl apply -f app-virtual-service.yaml
virtualservice.appmesh.k8s.aws/app1-svc created
virtualservice.appmesh.k8s.aws/app2-svc created
```

#### 9. 動作確認

```bash
#ServiceのURLを確認
$ kubectl get svc
NAME       TYPE           CLUSTER-IP      EXTERNAL-IP                                                                   PORT(S)        AGE
app1-svc   ClusterIP      10.100.21.87    <none>                                                                        2080/TCP       150m
app2-svc   LoadBalancer   10.100.11.134   a2bfc809c21814c7caa03c2b57bf59a0-929161559.ap-northeast-1.elb.amazonaws.com   80:30372/TCP   150m

#Serviceにヘルスチェックのリクエストを発行
$ curl a2bfc809c21814c7caa03c2b57bf59a0-929161559.ap-northeast-1.elb.amazonaws.com/health
ok

#ServiceにAPI コールリクエストを発行
$ curl a2bfc809c21814c7caa03c2b57bf59a0-929161559.ap-northeast-1.elb.amazonaws.com/call1
```

#### 10. 後始末

(1) Meshリソースの削除

```bash
#Meshリソースの削除 (Virtual Service)
$ kubectl delete -f app-virtual-service.yaml 

#Meshリソースの削除 (Virtual Router)
$ kubectl delete -f app-virtual-router.yaml 
virtualrouter.appmesh.k8s.aws "app1-svc-virtual-router" deleted
virtualrouter.appmesh.k8s.aws "app2-svc-virtual-router" deleted

#Meshリソースの削除 (Virtual Node)
$ kubectl delete -f app-virtual-node.yaml 
virtualnode.appmesh.k8s.aws "app1-svc" deleted
virtualnode.appmesh.k8s.aws "app2-svc" deleted

#Meshの削除
$ kubectl delete -f app-mesh.yaml
```

(2) アプリケーションリソースの削除

```bash
#Serviceの削除
$ kubectl delete -f app-svc.yaml 
service "app1-svc" deleted
service "app2-svc" deleted

#Deploymentの削除
$ kubectl delete -f app-deploy.yaml 
deployment.apps "app1" deleted
deployment.apps "app2" deleted

#アプリケーションのサービスアカウントの削除
kubectl delete sa app1-svc
kubectl delete sa app2-svc

#IAMロールの削除
* app1-svcなどのサービスアカウントに紐付いたロールを削除する
* AWS CLIでも削除できるが、ロールからすべてのポリシーを一つずつdetachした後、ロールそのものを削除する手順になる
* CLIだとかなり複雑になるため、AWSコンソールから削除したほうが早い

#カスタムポリシーの削除
* カスタムポリシー`app-policy`をAWSコンソールから削除

#Daemonのサービスアカウントの削除
kubectl delete sa -n amazon-cloudwatch xrayd 
kubectl delete sa -n amaon-coudwatch cloudwatch-agent
kubectl delete sa -n amaon-coudwatch fluentd

#サービスアカウントに対応するIAMロールの削除 (AWSコンソールから手作業で削除。CLIからも削除できるが削除対象のIAMロールのIDを調べるのが大変なので。)

#OIDCプロバイダーの削除
$ OIDCURL=$(aws eks describe-cluster --name app --output json | jq -r .cluster.identity.oidc.issuer | sed -e "s*https://**")
$ aws iam delete-open-id-connect-provider --open-id-connect-provider-arn arn:aws:iam::${AWS_ACCOUNT_ID}:oidc-provider/${OIDCURL}

#名前空間の削除
$ kubectl delete -f app-ns.yaml
```

(3) Meshコントローラの削除

```bash
#appmesh-systemサービスアカウントを削除
eksctl delete iamserviceaccount --cluster $AWS_CLUSTER_NAME \
    --namespace appmesh-system \
    --name appmesh-controller

#Meshコントローラを削除
$ helm delete appmesh-controller eks/appmesh-controller 

#appmesh-system名前空間の削除
$ kubectl delete ns appmesh-system
```

(4) MeshのCRDの削除

```bash
#App Mesh Controller For K8s の CRD を削除
$ kubectl ??? -k "https://github.com/aws/eks-charts/stable/appmesh-controller/crds?ref=master"
```

(5) クラスターの削除

```bash
#EKSクラスターの削除
$ eksctl delete cluster -f cluster.eksctl.yaml
```

#### 参考文献

1. [Tutorial: Configure App Mesh integration with Kubernetes](https://docs.aws.amazon.com/app-mesh/latest/userguide/mesh-k8s-integration.html)
1. [[AWS Black Belt Online Seminar] AWS App Mesh](https://d1.awsstatic.com/webinars/jp/pdf/services/20200721_BlackBelt_AWS_App_Mesh.pdf)
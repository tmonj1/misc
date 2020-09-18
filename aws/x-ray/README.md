# EKSでログ、メトリックス、分散トレーシング環境を構築する方法

ASP.NET Core のサンプルプログラム (app1とapp2) を Amazon EKS クラスターにデプロイし、CloudWatch と X-Ray を使ってログ、メトリックス、分散トレーシングの情報が取れるようにする。

本リポジトリの構成は以下のとおり。

```
.
├ app1 (app1のプロジェクトフォルダ)
├ app2 (app2のプロジェクトフォルダ)
├ xrayd (オンプレミス用のX-Rayイメージ作成ファイル)
└ eks
   ├ cloudwatch-fuentd-xray (EKSマニフェストファイル)
   │   └ README.md(./eks/cloudwatch-fluentd-xray/README.md)
   └ appmesh (EKSマニフェストファイル(App Mesh使用時))
   │   └ README.md(./eks/appmesh/README.md)
```

* X-RayについてはオンプレまたはEC2インスタンス上で利用する手順についても説明している。詳細はxraydフォルダの[README.md](./xrayd/README.md)を参照

**サンプルアプリケーションの構成**

* マイクロサービス2個 (app1とapp2)、AWSサービス1個 (S3) からなる
* app2からapp1とS3を呼び出す
* ログをFluentD、メトリクスをCloudWatch、分散トレーシングをX-Rayで出力

```
Client ──HTTP(80)──> ELB ──> Pod(App1) ──> Pod(App2)
                                    └────> S3
```

**EKSクラスターの構成**

* Region: ap-northeast-1
* VPC: 1個
* AZ: 1aと1cを使用 (必要ならyamlを編集し、別のAZにする)
* Subnet: public * 2個
* Node: 1個 (Max 2個) (Amazon Linux 2 EKS最適化インスタンス)

**Podの構成**

X-Ray daemonは各Podに展開する。CloudWatchとFluentDはDaemonSetとして展開する(App Meshの場合、X-Ray daemonはinjectの設定をすれば自動で展開される)。

```
┏━━━━━━┓┏━━━━━━┓┏━━━━━━┓┏━━━━━━┓┏━━━━━━┓
┃ App1 ┃┃ App2 ┃┃ XRay ┃┃  CW  ┃┃ FluD ┃
┗━━━━━━┛┗━━━━━━┛┗━━━━━━┛┗━━━━━━┛┗━━━━━━┛
┏━━━━━━━━━━━━━━━━━━━━━━┓┏━━━━━━┓┏━━━━━━┓
┃         Pod          ┃┃  DS  ┃┃  DS  ┃
┗━━━━━━━━━━━━━━━━━━━━━━┛┗━━━━━━┛┗━━━━━━┛
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
┃                 Node                 ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
```

**使用する主なAWSサービス**

* Amazon Elastic Container Registry (ECR)
* Amazon Elastic Kubernetes Service (EKS)
* Amazon CloudWatch
* Amazon CloudWatch Logs
* Amazon CloudWatch Container Insights
* Amazon X-Ray
* Amazon App Mesh
* AWS STS

※ Amazon ECS と Fargate は使用しない

**ツール類のバージョン**

* kubectl v1.17.9 (server/clientどちらも同じ)
* eksctl 0.27.0
* helm chart v3.3.1
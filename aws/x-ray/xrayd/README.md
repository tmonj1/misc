---
marp: true
---

<!--
headingDivider: 3
theme: theme
paginate: true
header: ""
footer: ""
-->

# X-Ray デーモンを Docker コンテナとして実行する方法

---

目次
- [1. X-Ray デーモンコンテナの作成](#1-x-ray-デーモンコンテナの作成)
- [2. オンプレミス実行](#2-オンプレミス実行)
  - [2.1 X-Ray デーモン単独で Docker コンテナとして実行](#21-x-ray-デーモン単独で-docker-コンテナとして実行)
  - [2.2 Docker-Compose でアプリケーションと X-Ray デーモンを実行](#22-docker-compose-でアプリケーションと-x-ray-デーモンを実行)
- [3. AWS上での実行](#3-aws上での実行)
  - [3.1 EC2インスタンス上でアプリケーションと X-Ray デーモンを実行](#31-ec2インスタンス上でアプリケーションと-x-ray-デーモンを実行)
  - [3.2 EC2インスタンス上でDocker-Composeで実行](#32-ec2インスタンス上でdocker-composeで実行)

## 1. X-Ray デーモンコンテナの作成

```Dockerfile
#Dockerfile
FROM amazonlinux
RUN yum install -y unzip
RUN curl -o daemon.zip https://s3.us-east-2.amazonaws.com/aws-xray-assets.us-east-2/xray-daemon/aws-xray-daemon-linux-3.x.zip
RUN unzip daemon.zip && cp xray /usr/bin/xray
ENTRYPOINT ["/usr/bin/xray", "-t", "0.0.0.0:2000", "-b", "0.0.0.0:2000"]
EXPOSE 2000/udp
EXPOSE 2000/tcp
```

```bash
#X-Rayデーモンのイメージのビルド
$ docker build -t xray-daemon .
```

出典: [ローカルで X-Ray デーモンを実行する](https://docs.aws.amazon.com/ja_jp/xray/latest/devguide/xray-daemon-local.html)

## 2. オンプレミス実行

### 2.1 X-Ray デーモン単独で Docker コンテナとして実行

```bash
#実行
$ docker run \
      --attach STDOUT \
      -v ~/.aws/:/root/.aws/:ro \
      --net=host \
      -e AWS_REGION=ap-northeast-1 \
      --name xray-daemon \
      -p 2000:2000/udp \
      xray-daemon -o
```

出典: [ローカルで X-Ray デーモンを実行する](https://docs.aws.amazon.com/ja_jp/xray/latest/devguide/xray-daemon-local.html)

* 引数の意味については上記の出典を参照
* 末尾の `-o` は x-ray の起動オプションに追加される。EC2 インスタンス以外で実行するときはこの指定が必要 (指定しないと起動時にインスタンスのメタデータを取得するため `169.254.169.254` にアクセスしに行き、タイムアウトするまで戻って来なくなるため起動に時間がかかる)


### 2.2 Docker-Compose でアプリケーションと X-Ray デーモンを実行

構成:
```
┏━━━━━━┓┏━━━━━━┓┏━━━━━━┓
┃ App1 ┃┃ App2 ┃┃ XRay ┃
┗━━━━━━┛┗━━━━━━┛┗━━━━━━┛
┏━━━━━━━━━━━━━━━━━━━━━━┓
┃         Docker       ┃
┗━━━━━━━━━━━━━━━━━━━━━━┛
```

* 詳細は `docker-compose.yml` を参照。

## 3. AWS上での実行

### 3.1 EC2インスタンス上でアプリケーションと X-Ray デーモンを実行

* X-Ray デーモンを EC2インスタンス上で構成し、実行（やり方は[Amazon EC2 で X-Ray デーモンを実行する](https://docs.aws.amazon.com/ja_jp/xray/latest/devguide/xray-daemon-ec2.html)を参照)
* `appSettings.json` に `"AWSXRayPlugins": "EC2Plugin"` を追加し、インスタンスのメタデータを取得するように設定を修正

### 3.2 EC2インスタンス上でDocker-Composeで実行

* 構成としては**2.2 Docker-Compose でアプリケーションと X-Ray デーモンを実行** と同じ
* EC2の場合、インスタンスのメタ情報が取得できるようになるため、app2のappSettings.jsonに以下の設定を追加

```json
"XRay": {
  "DisableXRayTracing": "false",
  "UseRuntimeErrors": "true",
  "AWSXRayPlugins": "EC2Plugin",  // <- この行を追加 
  "CollectSqlQueries": "false"
}
```

# Jenkins Master/Agent 構成

JenkinsのマスターとエージェントからなるDocker Composeファイル。

## 1. 前提

* `SSH Agent Plugin`がインストールされていること

## 2. 起動手順 (初回)

初回起動時のみ、SSHキーの設定とAWSの接続設定をおこなう。

### (1) SSHキーの設定

* `docker-compose up -d jenkins_master` を実行し、Jenkinsマスターを起動
* `docker exec -it jenkins_master bash`でマスターにログイン
* `whoami`で`jenkins`ユーザーであることを確認
* `cd ~/.ssh && ssh-keygen -t rsa -f "dockercli_agent" -P "" -C ""`でSSHキーを生成
* `dockercli_agent.pub`の中身を覚えておく
* `docker-compose.yml"の`ENKINS_SLAVE_SSH_PUBKEY`環境変数に公開鍵の値を指定
* `docker-compose up -d`を実行し、Jenkinsエージェントも起動
* `docker-compose ps`でマスターとエージェントの両方が起動したことを確認

### (2) AWSの設定

* `docker exec -it --user jenkins jenkins_node_dockercli`でエージェントにログイン
* `whoami`を実行し、`jenkins`ユーザーであることを確認
* `aws configure`を実行し、アクセスキーID、シークレットキー、デフォルトリージョン、デフォルト出力フォーマットを対話的に入力

### (3) サーバ証明書の設定

Jenkinsエージェントの`/home/jenkins/.aspnet/https`フォルダにあらかじめサーバ証明書 (ビルド対象のASP.NET 5 Web Appのサーバ証明書) をコピーしておく。

```bash
# Jenkinsエージェントにjenkinsユーザーでログインし、"~/.aspnet/https"フォルダを作成
$ docker exec -it --user jenkins jenkins_node_dockercli bash
[jenkins]$ mkdir -p ~/.aspnet/https
[jenkins]$ exit

# "~/.aspnet/https"フォルダに証明書をコピー
$ docker cp ~/.aspnet/https/aspnetapp.pfx jenkins_node_dockercli:/home/jenkins/.aspnet/https/aspnetapp.pfx

# "~/.aspnet"以下のファイルの所有者をrootからjenkinsに変更
$ docker exec -it jenkins_node_dockercli bash
[root]$ chown -R jenkins:jenkins /home/jenkins/.aspnet
[root]$ exit
```

## 3. ノードの作成

* Jenkinsマスター画面の`プラグインの管理`画面で`新規ノード作成`を選択
* ノード名を指定し、"Permanent Agent" をチェックして`OK`ボタンをクリック

## 4. バックアップ

```bash
$ cd jenkins_backup
$ ./jenkins_backup.sh master
$ ./jenkins_backup.sh agent
```

* リストアはスクリプトがないので自力で`tar xvf`で戻す。

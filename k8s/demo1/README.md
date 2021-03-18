# サンプルプログラム: demo1

## 1. イメージのビルドと実行

### (1) Dockerを使う場合 (httpのみ)

```bash
#ビルド
$ docker build -t demo:0.1 .

#実行
$ docker run -p 5000:80 --name demo1 -d --rm demo1:0.1

#停止
$ docker stop demo1
```

### (2) Docker Composeを使う場合 (http/https両方に対応)

```bash
#ビルドと実行
$ docker-compose up -d --build

#停止
$ docker-comose down
```

### (3) 実行確認

ブラウザで以下のURLのいずれかを実行し、正しく画面が表示されることを確認する。Docker Composeを使った場合、`https://localhost:5001`も使用可能。

|URL|表示内容|
|:--|:--|
|http://localhost:5000| "hello, world"|
|http://localhost:5000/envs| 環境変数一覧|
|http://localhost:5000/headers | リクエストヘッダ一覧|

## 1. イメージのDockerレジストリへの登録

### (1) Docker Hubへの登録

* 事前に Docker Hub にユーザ登録しておくこと
* Docker Hub にブラウザからログインし、"{ユーザアカウント名}/demo1"という名前のリポジトリを作成しておくこと (CLIからは作成できない)

```bash
#Docker Hubへのログイン
$ docker login -u {アカウント名} -p {パスワード}

#イメージのタグ付
$ docker tag demo1:0.1 {アカウント名}/demo1:0.1

#イメージのpush
$ docker push {アカウント名}/demo1:0.1
```

### (2) Amazon ECR への登録

* [./push-to-ecr.sh](./push-to-ecr.sh)を参照。
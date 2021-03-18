# サンプルプログラム: demo1

## 1. Dockerを使う場合

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

## 2. Kubernetesを使う場合

### (1) クラスタの展開

* Docker for Mac がインストール済みで Kubernetes が有効化されていること

```bash
#クラスターの展開
$ k apply -f demo1-cluster.yml

#クラスターの削除
$ k delete -f demo1-cluster.yml
```

### (2) 実行確認

```bash
#クラスターホストのIPアドレスを確認 (kubernetes.docker.internal)
$ k cluster-info
Kubernetes master is running at https://kubernetes.docker.internal:6443
KubeDNS is running at https://kubernetes.docker.internal:6443/api/v1/namespaces/kube-system/services/kube-dns:dns/proxy

To further debug and diagnose cluster problems, use 'kubectl cluster-info dump'.

#NodePortとClusterIPが開放しているポート番号を確認 (32072と5000)
k get svc |grep demo1
NAME                 TYPE           CLUSTER-IP      EXTERNAL-IP      PORT(S)        AGE
demo1-headless       ClusterIP      None            <none>           5002/TCP       2m20s
demo1-nodeport       NodePort       10.99.187.50    <none>           80:32072/TCP   2m20s
demo1-clusterip      ClusterIP      10.97.151.147   <none>           5000/TCP       2m20s
demo1-externalname   ExternalName   <none>          www.google.com   <none>         16s
demo1-lb             LoadBalancer   10.101.126.253  localhost        5030:31844/TCP 4s

* demo1-headlessにもポート番号が設定されているが、これは無視される

#NodePortの実行確認
$ curl http://kubernetes.docker.internal:32072
Hello World!

#ClusterIPの実行確認
$ kbb -i -- wget -O- http://demo1-clusterip:5000/
Connecting to demo1-clusterip:5000 (10.97.151.147:5000)
writing to stdout
-                    100% |********************************|    12  0:00:00 ETA
written to stdout
Hello World!
pod "busybox" deleted

#Headlessの実行確認
$ kbb -i -- wget -O- http://demo1-headless/     
Connecting to demo1-headless (10.1.0.27:80)
writing to stdout
-                    100% |********************************|    12  0:00:00 ETA
written to stdout
Hello World!
pod "busybox" deleted

* 上の`10.1.0.27`はPodのIPアドレス (`k get pod -o wide`で確認できる)。

#ExternalNameの実行確認 (調査用コンテナからnslookupでCNAMEとしてwww.google.comが設定されていることを確認)
#(MacOSではnslookupは何回か実行しないと成功しない)
$ kbb -it -- sh
/ nslookup demo1-externalname
Server:         10.96.0.10
Address:        10.96.0.10:53

demo1-externalname.default.svc.cluster.local    canonical name = www.google.com
Name:   www.google.com
Address: 172.217.26.36

*** Cant find demo1-externalname.svc.cluster.local: No answer
*** Cant find demo1-externalname.cluster.local: No answer
*** Cant find demo1-externalname.default.svc.cluster.local: No answer
*** Cant find demo1-externalname.svc.cluster.local: No answer
*** Cant find demo1-externalname.cluster.local: No answer

#LoadBalancerの実行確認
$ curl http://localhost:5030
Hello World! 
#じつはLoadBalancerとともに作成されるNodePortにもアクセス可能
$ curl http://kubernetes.docker.internal:5030 
Hello World! 
```

### (3) クラスター構成

## 3. イメージのDockerレジストリへの登録

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


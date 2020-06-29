# How to use ecsctl

## 1. Command Reference for Quick Start

### (1) eksctl.io

`eksctl` is a cli tool created by Weaveworks, **NOT** by AWS. [eksctl.io](https://eksctl.io) is the site you can get most detailed information on this tool.

### (2) type "--help"

You can see the full command line options for each cli command by appending `--help` for the cli command, as shown below.

```bash
#show full command line options for the "create cluster" command
$ eksctl create cluser --help
```

## 2. Basics

### (1) Recommended Reads

* [Introduction@eksctl.io](https://eksctl.io/introduction/)
* [「eksctl」コマンドを使ったAmazon EKS構築入門](https://dev.classmethod.jp/articles/getting-started-amazon-eks-with-eksctl/)

### (2) kubeconfig

[kubectlの接続設定ファイル（kubeconfig）の概要](https://qiita.com/shoichiimamura/items/91208a9b30e701d1e7f2)

```bash
#Show kubeconfig
$ kubectl view config
apiVersion: v1
clusters:
- cluster:
    certificate-authority-data: DATA+OMITTED
    server: https://kubernetes.docker.internal:6443
  name: docker-desktop
contexts:
- context:
    cluster: docker-desktop
    user: docker-desktop
  name: docker-desktop
current-context: docker-desktop
kind: Config
preferences: {}
users:
- name: docker-desktop
  user:
    client-certificate-data: REDACTED
    client-key-data: REDACTED
```

* kubectl config get-contexts                # list contexts
* kubectl config use-context [context-name]  # set the current context
* kubectl config set-context [context-name]  # add a context

## 3. Example

### 3.1 Create/Delete a Cluster

```bash
#Create a minimal cluster
$ eksctl create cluster \
--name prod \
--version 1.16 \
--region ap-northeast-1 \
--nodegroup-name standard-workers \
--node-type t2.micro \
--node-volume-size 30 \
--nodes 1 \
--nodes-min 1 \
--nodes-max 2 \
--ssh-access \
--ssh-public-key ~/.ssh/key2_pub.pem \
--managed
#Delete a cluster
$ eksctl delete cluster --name prod --wait
```

---
* How to use an external storage
* How to use a custom image
* Cluster credential?
* Create cluster example using a config file
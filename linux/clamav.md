Amazon Linux 2 に ClamAV をセットアップする。

注意: [こちら](https://qiita.com/aosho235/items/e70ccf3b7464369bebab)の記事によると `t2-micro` では `clamd` が実行できないかもしれないとのこと。

## 1. 全体像

大きく分けてシグニチャ (パターンファイル) データベースとウィルススキャナの２つで構成されている。
ただし、通常は `freshclam` でデータベースを構築し、clamdデーモンを実行するだけでよい。データベースは `cron` で毎日最新状態に更新される (特に設定しなくても ClamAV インストール時に設定される)。

```
ClamAV
　├ シグニチャデータベース (CVD)
　│　├ 初期構築用ツール (freshclam)
　│　├ CVDの操作ツール (sigtool, clambc)
　│　└ 設定ファイル用ツール (clamconf, clamav-config)
　└ ウィルススキャナー
　　　├ コマンドラインツール (clamscan)
　　　└ デーモン (clamd)
　　　　├ スキャン実行ツール (clamdscan)
　　　　└ モニタツール (clamdtop)
```

## 2. インストール手順

### (1) シグニチャデータベースのセットアップ (freshclam)

```bash
#Amazon Linux の場合は amazon-linux-etras を使って epel をインストール
$ sudo amazon-linux-extras install -y epel
#epelからClamAVをインストール
$ sudo yum install -y clamav clamav-update clamd
#freshclam.confを編集 (編集しなくても動くがログなど若干設定しておいたほうがよい)
$ sudo vi /etc/freshclam.conf
--
#NotifyClad /path/to/clamd.conf  → 
NotifyClamd /etc/clamd.d/scan.conf  # CVD更新後、clamdをリロードするように設定
--
#freshclam実行
$ sudo freshclam
```
### (2) Clamdデーモンの実行

設定ファイルを編集後、`systemctl` でデーモンを実行。
```bash
#設定ファイルを編集
$ sudo vi /etc/freshclam.conf
--
#LocalSocket /var/run/clamd.scan/clamd.sock →
LocalSocket /var/run/clamd.scan/clamd.sock

#LocalSocketMode 660 →
LocalSocketMode 660

#ExcludePath ^/proc/ →
ExcludePath ^/proc/
#ExcludePath ^/sys/ →
ExcludePath ^/sys/
--
```

ログファイルを準備しておかないと `systemctl start` を実行したときにエラーになる。

```bash
# 空のログファイルを作成し書き込み可能にしておく
$ sudo touch /var/log/clamd.scan
$ sudo chmod 666 /var/log/clamd.scan
```

デーモンを実行。

```bash
#デーモンを実行
$ sudo systemctl start clamd@scan
#状態を確認
$ systemctl status clamd@scan.service
● clamd@scan.service - clamd scanner (scan) daemon
   Loaded: loaded (/usr/lib/systemd/system/clamd@.service; disabled; vendor preset: disabled)
   Active: active (running) since Fri 2020-10-23 12:54:35 UTC; 14s ago
     Docs: man:clamd(8)
           man:clamd.conf(5)
           https://www.clamav.net/documents/
  Process: 12874 ExecStart=/usr/sbin/clamd -c /etc/clamd.d/%i.conf (code=exited, status=0/SUCCESS)
 Main PID: 12875 (clamd)
   CGroup: /system.slice/system-clamd.slice/clamd@scan.service
           └─12875 /usr/sbin/clamd -c /etc/clamd.d/scan.conf
#自動起動の設定
$ sudo systemctl enable clamd@scan
```

## 3. トラブルシューティング

### (1) デーモン実行でエラーになったとき

`journalctl` でログを見る。

```bash
# ログを確認 (以下の例では /var/log/clamd.scan にログをappendできないと言っている)
$ journalctl -u clamd@scan
-- Logs begin at Fri 2020-10-23 12:27:15 UTC, end at Fri 2020-10-23 12:52:36 UTC. --
Oct 23 12:46:20 ip-172-31-10-92.ap-northeast-1.compute.internal systemd[1]: Starting clamd scanner (scan) daemon...
Oct 23 12:46:20 ip-172-31-10-92.ap-northeast-1.compute.internal clamd[12727]: ERROR: Cant initialize the internal logger
Oct 23 12:46:20 ip-172-31-10-92.ap-northeast-1.compute.internal clamd[12727]: ERROR: Cant open /var/log/clamd.scan in append mode (check permissions!).
Oct 23 12:46:20 ip-172-31-10-92.ap-northeast-1.compute.internal clamd[12727]: ERROR: Cant open /var/log/clamd.scan in append mode (check permissions!).
Oct 23 12:46:20 ip-172-31-10-92.ap-northeast-1.compute.internal systemd[1]: clamd@scan.service: control process exited, code=exited status=1
Oct 23 12:46:20 ip-172-31-10-92.ap-northeast-1.compute.internal systemd[1]: Failed to start clamd scanner (scan) daemon.
```

## 4. リソース

1. [Amazon Linux 2にClamAVをインストールする](https://qiita.com/aosho235/items/e70ccf3b7464369bebab)
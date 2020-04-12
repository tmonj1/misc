# How to generate private root CA and server certificates

Creating a self-signed server certificate is easy. But creating a private CA certificate and then generating
server certificates from the CA is a little complicated. This document describes how to do it.

## Prerequisites

* OpenSSL 1.0.2 series or LibreSSL 2.6.5 is required.
  * frapsoft/openssl docker image (OpenSSL 1.0.2j)
  * macOS mojave (LibreSSL 2.6.5)
  * run "openssl version -a" to confirm OpenSSL version.

## Head first certificate generation

1. if root-ca and server directory exist, run "rm -rf root-ca server".
1. run "./certgen.sh"

You will have root.pem, root.der and root.pfx in root-ca directory, and server_crt in server directory.

## Step-by-step procedure of certificate generation

### 1. Generate a private key

There are three elements you should pay attention to when generating a private key:

* Key algorithms  
  RSA or ECDSA (do not use DSA because its effective key size is limited to 1,024)
* Key size  
  2048 is OK enough for RSA, secp256rl or something for ECDSA.
* Passphrase (optional)
  A passphrase is a common key used for encryping a private key. Although providing a passphrase is strongly recommended, it is incovenient and actually not being used in many cases, so you usually can omit this.

Key generation command examples are as below. A key is saved in PEM format.

```bash:
#
# generate a private key (RSA, 2048bit)
#

# (No encryption) RSA whose key size is 2048, no passphrase.
$ openssl genrsa -out fd.key 2048

# (With encryption) RSA, 2048, passphrase "april"
$ openssl genrsa -aes128 -passout 'pass:april' -out fd.key 2048

# you can confirm the output just by using cat command.
# If encrypted, the first two lines are Proc-Type and DEK-Info. If not, the output does not have these fields.
$ cat fd.key
-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: AES-128-CBC,D213862EE4709BFCAD3AC6E796EE7887

38rAV44klc7mGIntoGTYvkVxG5UjLsG2vg7GQlQlVAOruyB7FBJORuTIqE5y803Q
dAt9krKagxSDBF8WYcizSE0io8vTCspU08CKyE341Ro355b5kXphYPz/g7IlBvYO
:
: (the rest is omitted)
```

For more detail about genrsa command, see [genrsa of OpenSSL reference](https://www.openssl.org/docs/man1.1.1/man1/genrsa.html).

### 2. Creating a root CA

Only four settings are important:

```
[ v3_ca ]
subjectKeyIdentifier=hash  # always use "hash"
authorityKeyIdentifier=keyid:always,issuer # always use this setting
basicConstraints = critical, CA:true # set "CA:true" for CA. 
keyUsage = cRLSign, keyCertSign # always use this setting for CA.
```

#### (1) Create a private key

Create a private key in the way mentioned above and save it as "root_key.pem".

#### (2) Create a CSR (Certificate Signing Request)

Use "openssl req -new" command to create a CSR, with -key option to specify the key file, "-subj" to specify subject of your organization, and "-batch" to disable interactive operation.

```bash:
# Validity period is 365 days
$ (No Encryption)openssl req -new -batch -key fd.key -days 365 -subj '/C=JP/ST=Tokyo/O=My Org/CN=My Org Private CA/emailAddress=foo@bar.com' -out fd.csr

# Validity period is 365 days, supply the passphrase to key 
$ (With Encryption)openssl req -new -batch -key fd.key -days 365 -subj '/C=JP/ST=Tokyo/O=My Org/CN=My Org Private CA/emailAddress=foo@bar.com' -passin 'pass:april' -out fd.csr

# Comfirm it. add "-passout 'pass:april' for encrypted key.
$ openssl -text in fd.csr
Certificate Request:
    Data:
        Version: 0 (0x0)
        Subject: C=JP, ST=Tokyo, O=My Org, CN=My Org Private CA/emailAddress=foo@bar.com
:
: (the rest is omitted)
```

* When creating a CSR interactively (i.e. without "-batch" option), you are asked to input "challenge password".
 This is an optional field, and it is not used in reality, so you can leave it alone just by hitting ENTER.

#### (3) Generate a certificate

Use "openssl ca" command to generate a certificate. Before running the command, you have to prepare configuration/control files as bellow:

```
(.)
 ├ newcerts/    (an empty directory)
 ├ index.txt    (an empty file)
 ├ serial       ("00")
 └ crlnumber    (an empty file)

Once setting up these files, then you can run "openssl" command as bellow:

```bash:
# create a certificate
$openssl ca -in root.csr -selfsign -batch -keyfile root_key.pem -notext -config ./openssl.cnf -days 3650 -extfile v3_ca.txt -out root_crt.pem
```

#### (4) Confirm the generated certificate

Run "openssl x509 -text -purpose" command to confirm validity, signature algorithm, key usages, and extended key usages.

```
# comfirm the certificate
$ openssl x509 -text -purpose -noout -in root_crt.pem
```

### 3. Creating a server certificate

You can use settings bellow both for server and client certificates
because this includes all the settings for both uses.

```
[ v3_server ]
# alway set CA:FALSE for server/client certificates
basicConstraints=CA:FALSE
keyUsage = digitalSignature, keyEncipherment, nonRepudiation, keyAgreement, dataEncipherment
extendedKeyUsage = serverAuth, clientAuth
subjectKeyIdentifier=hash
authorityKeyIdentifier=keyid,issuer
```

#### (1) Create a private key

Create a private key in the way mentioned above and save it as "server_key.pem".

#### (2) Create a CSR (Certificate Signing Request)

Use "openssl req" command just like when creating a CSR for a private CA.

* Important notes
  * Always generate CSRs for **multidomain** certificates (Google chrome requires it).

```bash:
$ openssl req -new -key server_key.pem -subj "/C=JP/ST=Tokyo/O=MyOrg/OU=dev/CN=My Servers/" -config ./openssl.cnf -out server.csr
```

#### (3) Generate a certificate

When creating a server certificate, you should specify all the possible DNS names and IP addresses in [alt_names] part in the v3_servers.txt file as bellow:

```text:v3_servers.txt
[SAN]
basicConstraints = CA:false
keyUsage = critical, digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth
authorityKeyIdentifier=keyid,issuer

subjectAltName=@alt_names
basicConstraints=CA:FALSE
[alt_names]
DNS.1=localhost
IP.1=127.0.0.1
IP.2=192.168.11.5
```

Then run "openssl ca" command:

```bash:
$ openssl ca -batch -in server.csr -config ./openssl.cnf -cert ${ROOTDIR}/root_crt.pem -keyfile ${ROOTDIR}/root_key.pem -extfile v3_servers.txt -extensions SAN -out server_crt.pem -days 365
```

#### (4) Confirm the generated certificate

Run "openssl x509 -text -purpose" command to confirm validity, signature algorithm, key usages, and extended key usages.

```
# comfirm the certificate
$ openssl x509 -text -purpose -noout -in server_crt.pem
```

### 3. Testing the server certificate

Just run "node test-cert.js" and access "https://localhost:8443" with your favorite web browser.

### Resources

1. [OpenSSL cookbook](https://www.feistyduck.com/library/openssl-cookbook/online/index.html)  
  A must read for everyone to know SSL/TLS, server certificates and OpenSSL.
1. [OpenSSL](https://www.openssl.org)  
  The official OpenSSL homepage.
1. [OpenSSL commands](https://www.openssl.org/docs/man1.1.1/man1/)  
  The official OpenSSL command reference.
1. [CA certificates extracted from Mozilla](https://curl.haxx.se/docs/caextract.html)  
  Trustable Root CA certificates maintained by Mozilla and converted to the PEM format by Curl project.
1. [Transport Layer Security](https://en.wikipedia.org/wiki/Transport_Layer_Security)  
  Detailed description on TLS, including feature support tables of well-known browsers for SSL/TLS protocols and various cipher algorithms.
1. [今度こそopensslコマンドを理解して使いたい (1) ルートCAをスクリプトで作成する](https://qiita.com/3244/items/780469306a3c3051c9fe)  
  very clear and easy-to-read articles on Qiita.
1. [ET::ERR_CERT_REVOKED in Chrome/Chromium, introduced with MacOS Catalina](https://superuser.com/questions/1492207/neterr-cert-revoked-in-chrome-chromium-introduced-with-macos-catalina)  
  Google Chrome on iOS/macOS requires certificates whose validity period <= 825 days.
1. [Google Chrome で自組織のCAで署名したSSL証明書のサイトにアクセスすると NET::ERR_CERT_COMMON_NAME_INVALID エラーメッセージが表示される](https://www.ipentec.com/document/windows-chrime-error-net-err-cert-common-name-invalid-using-ssl-certificate-signed-with-local-ca)  
  Google Chrome ignores CN and use SAN (Subject Alternative Names) to check the site's validity.
1. [opensslでサーバ証明書とルート証明書を作成するスクリプト](https://qiita.com/masahiro-aoike/items/965bd827dc13894f6664)
1. [Self-signed certificates in iOS apps](https://medium.com/collaborne-engineering/self-signed-certificates-in-ios-apps-ff489bf8b96e)  
  Describe how to install a self-signed server certificate on iOS.
1. [How do you get Chrome to accept a self-signed certificate?](https://www.pico.net/kb/how-do-you-get-chrome-to-accept-a-self-signed-certificate)  
  Describe how to install a self-signed server certificate on Goole Chrome.
　
### Resources (info about Google Chrome SSL/TLS support)

|version|content|source|
|:--|:--|:--|
|56|remove SHA-1-signed certificate support| [Google Removing SHA-1 Support in Chrome 56](https://threatpost.com/google-removing-sha-1-support-in-chrome-56/122041/) |
|58|begin to check SAN and ignore CN| [Browser Watch: SSL/Security Changes in Chrome 58](https://www.thesslstore.com/blog/security-changes-in-chrome-58/) |
|65?|begin to check if validity is less equal 825 days|[SSL証明書の有効期限は2年に短縮される（2018年3月1日施行）](https://rms.ne.jp/digital-certificate-news/ballot-193)|
|75?|begin to check key usage field|[GOOGLE CHROME 75.X, A SELF-SIGNED CERTIFICATE AND ERR_SSL_KEY_USAGE_INCOMPATIBLE)(https://hexeract.wordpress.com/2019/06/13/google-chrome-75-x-a-self-signed-certificate-and-err_ssl_key_usage_incompatible/)|

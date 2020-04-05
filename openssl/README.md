# How to generate private root CA and server certificates

Creating a self-signed server certificate is easy. But creating a private CA certificate and then generating
server certificates from the CA is a little complicated. This document describes how to do it.

## Prerequisites

* OpenSSL 1.0.2 series or LibreSSL 2.6.5 is required.
  * frapsoft/openssl docker image (OpenSSL 1.0.2j)
  * macOS mojave (LibreSSL 2.6.5)
  * run "openssl version -a" to see OpenSSL version.

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

####  Create a CSR (Certificate Sining Request)

Use "openssl req -new" command to create a CSR, with -key option to specify the key file, "-subj" to specify subject of your organization, and "-batch" to disable interactive operation.

```bash:
#
# create a CSR for a private CA
#

* Important notes
  * Always generate CSRs for **multidomain** certificates (Google chrome requires it)[^6]

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
 This is an optional field, and it is not used in reality, so you can leave it alone just by entering ENTER.

### generate a certificate

Use "openssl  -new" command to create a CSR, with -key option to specify the key pair file, "-subj" to specify subject of your organization, and "-batch" to disable interactive operation.



### Resources

[^1]: [OpenSSL ccokbook](https://www.feistyduck.com/library/openssl-cookbook/online/index.html)  
  A must read for everyone to know SSL/TLS, server certificates and OpenSSL.
[^2]: [OpenSSL](https://www.openssl.org)  
  The official OpenSSL homepage.
  * [OpenSSL commands](https://www.openssl.org/docs/man1.1.1/man1/)  
    OpenSSL command reference.
[^3]: [CA certificates extracted from Mozilla](https://curl.haxx.se/docs/caextract.html)  
  Trustable Root CA certificates maintained by Mozilla and converted to the PEM format by Curl project.
[^4]: [Transport Layer Security](https://en.wikipedia.org/wiki/Transport_Layer_Security)  
  Detailed description on TLS, including feature support tables of well-known browsers for SSL/TLS protocols and various cipher algorithms.
[^5]: [ET::ERR_CERT_REVOKED in Chrome/Chromium, introduced with MacOS Catalina](https://superuser.com/questions/1492207/neterr-cert-revoked-in-chrome-chromium-introduced-with-macos-catalina)  
  Google Chrome on iOS/macOS requires certificates whose validity period <= 825 days.
[^6]: [Google Chrome で自組織のCAで署名したSSL証明書のサイトにアクセスすると NET::ERR_CERT_COMMON_NAME_INVALID エラーメッセージが表示される](https://www.ipentec.com/document/windows-chrime-error-net-err-cert-common-name-invalid-using-ssl-certificate-signed-with-local-ca)  
  Google Chrome ignores CN and use SAN (Subject Alternative Names) to check the site's validity.

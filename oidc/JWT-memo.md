## サマリ

この文章では、OIDCのIDトークンとしてのJWTについて説明します。

## JWTの構造

ヘッダ、ペイロード、署名の3つからなる。それぞれをBase64urlでエンコードし、"."で連結したものがJWT(署名なしのときはsignatureは空)。

```
header
.
payload
.
signature
```

ヘッダとペイロードはもともとJSON形式。なのでBase64urlをデコードすると、以下のようになっている（JSONの属性は単なる一例）。

```
{
    "typ": "JWT",
    "alg": "none"  // 署名なし
}
.
{
  "iss": "https://...",
  "sub": "user id",
  "aud": "resource server",
  "exp": 1000000,
  "jti": "randome string"
}
.
```

## JWS (JSON Web Signatures)

JWTに署名する場合、ヘッダのalg属性に署名アルゴリズムを指定する (署名しないときは"none"を指定)。指定可能なアルゴリズムは下記の4*3の組み合わせの計12種類 ("none"も入れると13種類)。

```
HMAC       SHA-256
RSA (2種) * SHA-384
ECDSA       SHA-512
```

* HMACとSHA-256を使った組み合わせはHS256、RSAとSHA-256を使った組み合わせはRS256と呼ばれる(ヘッダの"alg"にこのように指定する)。

HMACは共有鍵を使う。鍵の配布は別の管理プロセスでリソースサーバと認可サーバの間で共有しておく（鍵の配布はOAuth 2.0の仕様の範囲外）。

HMAC以外は非対称鍵。認可サーバが秘密鍵と公開鍵を保持し、JWK Set Documentに公開鍵を格納し、JWK Set Document エンドポイントで公開する。クライアントはこのエンドポイントから公開鍵を取得し、署名の検証をおこなう。

## JWK と JWK Set Document

JWKは公開鍵をJSON形式で格納するための仕様。JWK Set DocumentはJWKを配列形式で複数格納したJSONドキュメント。認可サーバは公開鍵をJWK Set Document形式でJWK Set Documentエンドポイントから公開する。

```JSON
"keys": [
  {
    "kty": "RSA",
    "n": "0vx7a...",
    "e": "AQAB",
    "alg": "RSA256",
    "kid": "1"
  },
  {
    "kty": "EC",
    :
    :
  }
]
```

## リソース

1. [OAuth & OpenID Connect 関連仕様まとめ](https://qiita.com/TakahikoKawasaki/items/185d34814eb9f7ac7ef3)
1. [JSON Web Key (JWK)(RFC7517)](https://tools.ietf.org/html/rfc7517)

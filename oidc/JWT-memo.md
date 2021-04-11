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

ヘッダとペイロードはもともとJSON形式。なのでBase64urlをデコードすると、以下のようになっている（下記は署名なしの例)。

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

署名ありのときは、alg (algorithm) 属性に署名アルゴリズム、kid属性にキーIDが設定される。

```
{
    "typ": "JWT",
    "alg":"RS256"  // see https://tools.ietf.org/html/rfc7518#section-3.1
    "kid":"default",
}
```

キーIDはどの公開鍵でデコードすれば良いのかを示している。公開鍵はIdPのDiscoveryエンドポイントから公開されているIdP/OAuth設定情報のjwks_uri属性で示されているURLから取得できる。Googleの場合、設定情報は以下のとおり。

Google Discovery Endpoint: https://accounts.google.com/.well-known/openid-configuration

```Json
ve
Copy
Pretty Print
{
 "issuer": "https://accounts.google.com",
 "authorization_endpoint": "https://accounts.google.com/o/oauth2/v2/auth",
 "device_authorization_endpoint": "https://oauth2.googleapis.com/device/code",
 "token_endpoint": "https://oauth2.googleapis.com/token",
 "userinfo_endpoint": "https://openidconnect.googleapis.com/v1/userinfo",
 "revocation_endpoint": "https://oauth2.googleapis.com/revoke",
 "jwks_uri": "https://www.googleapis.com/oauth2/v3/certs",
 "response_types_supported": [
  "code",
  "token",
  "id_token",
  "code token",
  "code id_token",
  "token id_token",
  "code token id_token",
  "none"
 ],
 "subject_types_supported": [
  "public"
 ],
 "id_token_signing_alg_values_supported": [
  "RS256"
 ],
 "scopes_supported": [
  "openid",
  "email",
  "profile"
 ],
 "token_endpoint_auth_methods_supported": [
  "client_secret_post",
  "client_secret_basic"
 ],
 "claims_supported": [
  "aud",
  "email",
  "email_verified",
  "exp",
  "family_name",
  "given_name",
  "iat",
  "iss",
  "locale",
  "name",
  "picture",
  "sub"
 ],
 "code_challenge_methods_supported": [
  "plain",
  "S256"
 ],
 "grant_types_supported": [
  "authorization_code",
  "refresh_token",
  "urn:ietf:params:oauth:grant-type:device_code",
  "urn:ietf:params:oauth:grant-type:jwt-bearer"
 ]
}
```

`jwks_uri`で指定されているURL(https://www.googleapis.com/oauth2/v3/certs)からJWKの情報を取得した結果は以下のとおり(値は若干変更している)。

```Json
{
  "keys": [
    {
      "alg": "RS256",
      "kty": "RSA",
      "kid": "d05ef20c1",
      "n": "r1Y_vV...",
      "e": "AQAB",
      "use": "sig"
    },
    {
      "alg": "RS256",
      "kty": "RSA",
      "kid": "f092b612",
      "e": "AQAB",
      "n": "4_...",
      "use": "sig"
    }
  ]
}
```

見てわかるとおり、JWKが2つ公開されており、それぞれ`kid`属性を持っているため、取得したJWSの`kid`を参照して適切な公開鍵を使って復号する。公開鍵は`e`と`n`で構成されているので、これを使えば復号できる。復号方法については下記のサイトに手順が詳しく解説されている。

* [OpenID Connect の JWT の署名を自力で検証してみると見えてきた公開鍵暗号の実装の話](https://qiita.com/bobunderson/items/d48f89e2b3e6ad9f9c4c)

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
1. [OpenID Connect の JWT の署名を自力で検証してみると見えてきた公開鍵暗号の実装の話](https://qiita.com/bobunderson/items/d48f89e2b3e6ad9f9c4c)
1. [JWT（JSON Web Token）の紹介](https://meetup-jp.toast.com/3511)

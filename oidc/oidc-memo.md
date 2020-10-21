## 1. 基本概念

### 1.1 OIDCとは

OAuth 2.0 の載る形で認証プロトコルとして定義されている。OAuth自体の変更はなく、拡張のみされている。

従来型SSOやSAMLとの違いは、WebブラウザとWebサイトの間の認証だけでなく、APIサーバとネイティブクライアントにも対応できるように設計されている。このため、APIサーバあるいはネイティブクライアントをサポートする場合にはOIDCを選択すべきである。

### 1.3 登場人物

OAuth 2.0と登場人物の名前が若干変わっている。リソースサーバは登場しない。

|OAuth 2.0|OIDC|
|:--|:--|
|リソース所有者|ユーザー|
|クライアント|Relying Party (RP)|
|認可サーバ|Identity Provider (IdP)|

* IdPはOP (OpenID Provider) と呼ばれることもある


## 2 SAMLとの比較

* RPはSAMLではSP (Service Provider) と呼ばれる
* IdPはSAMLでもIdP

|項目|SAML|OIDC|
|:--|:--|:--|
|認証情報|Assertion|IDトークン|
|形式|XML|JSON (JWT)|
|通信方式|SOAP|REST|
|対応する用途|Web site|Web site, API server, native app|
|IdPとSP/RPとの関係|事前に静的に構築|実行時に動的に構築|

SAMLはネイティブクライアントでは使えない。

* IdPとSP/RPとの間に**事前に**信頼関係を結んでおく必要がある
* 

SAMLはクライアントがSPにアクセスしに行ったときに、IdPにリダイレクトするように構成し、IdPでの認証が終わると再度SPに戻される。SPに戻る方式にはリダイレクトとPOSTメソッドの2通りがある。これをバインディングと言う。

* HTTPリダイレクトバインディング
  * クエリパラメータにアサーションを設定してリダイレクト
  * クエリパラメータなのでセキュリティ的には弱い（なのでできるだけ後述するPOSTバインディングを使う）
  * クエリパラメータなので長さに制限がある（SAMLのアサーションはXMLなのでJSONより長くなる）
  * ネイティブアプリの場合、カスタムURLでアプリに制御を戻すことになるが、そこから

## 3. OIDCフロー

### 3.1 認可リクエスト

|属性|必須|説明|
|:--|:--|:--|
|response_type|yes|"code token_id" を指定|
|scope|no|"openid"を指定|
|client_id|yes|クライアントID|
|redirect_uri|no|認可サーバから戻ってきたときに使用するURL。事前に認可サーバに登録していたときは指定しなくてもよい|

### 3.x UserInfo

クライアントが受け取ったIDトークンには、`sub`属性にユーザを識別するIDが設定されているが、これは通常は低レベルなIDであり、表示に適した文字列（実際の姓名など）ではない。ユーザに関するこうした追加の情報を取得できるようにするため、IdPはUserInfoエンドポイントを備えている。

クライアントは`Authorization`ヘッダにBearerトークンとしてアクセストークンを指定し、UserInfoエンドポイントにリクエストを送信することで、ユーザ情報を取得できる。

```HTTP
GET /userinfo HTTP/1.1
Host: server.example.com
Authorization: Bearer SlAV32hkKG
```

---

認証方法

* アプリがユーザIDとパスワードを永久保持
  * 何も悪いことがないように思えるが・・・漏洩はしないはず
  * クリデンシャルをユーザ本人が記憶しているのではなく、「保管」していること自体が問題なのだろう
    * OAuth的にはクリデンシャルを預けるのはよくない。ただモバイルアプリまでそれが適用されるのか・・・
* アプリが初回起動時にユーザIDとパスワードの入力を要求し、それをIDトークンに変えて永久保持
  * IDトークンは偽造しやすそうなので、こっちのほうがむしろ問題のように思える
    * サーバが発行したトークンの有効性を確認でき、トークンの発行が事実上不可能であれば安全そうだ -----> 要調査
  * クリデンシャルは保存していないという点ではパスワード保存方式よりは良いらしい

  ## 理解を確認するための質問

  * 結局アクセストークンと同じ発行ロジックなら、IDトークンなんて使わずアクセストークンにユーザの情報を追加すれば済む話では？
    * アクセストークンは認可情報でクライアントから中身は見えない、一方IDトークンはクライアントが中身を見れる
    * 認可と認証は役割が異なり、両方の情報が入ってほしくないケースがある（認可の情報だけ必要なのに不要な認証情報までがリソースサーバに渡っていくのがまずいケースがある）。
  * SSO?

---
oauth.ioにtwitterをIdPとしてサインインした場合

(1) twitterアイコンをクリック

```HTTP
// リクエスト (GET)
https://oauth.io/auth/twitter?k=Q6LOFHhoDefuPOBYz8TcrFEHq8Q&d=https%3A%2F%2Foauth.io%2F&opts=%7B%22cache%22%3Atrue%2C%22state%22%3A%22ycqqIDEIbx0D008xP_PIdmiBOHU%22%2C%22state_type%22%3A%22client%22%7D

// レスポンス (302)
Location: https://api.twitter.com/oauth/authenticate?oauth_token=3q0FPwAAAAAA5hZAAAABdR8VlKA
```

上記リダイレクトのレスポンスで下記のページが返される。

![twitter authentication](./assets/twitter-authn.png)

・・・とここでやっとわかったが、TwitterはOIDCではない。調べてみると、OAuth 1.0aということがわかった。正確には、ツイートなどユーザまで識別する必要があるときはOAuth 1.0a、パブリックなツイートの検索などユーザまで識別しなくてよいときはOAuth 2.0を使う。

Twitter Authentication Overview
https://developer.twitter.com/en/docs/authentication/overview

---
oauth.ioにgoogleをIdPとしてサインインした場合

(1) googleアイコンをクリック

```HTTP
//
// リクエスト (GET)
//
https://oauth.io/auth/google?k=Q6LOFHhoDefuPOBYz8TcrFEHq8Q&d=https%3A%2F%2Foauth.io%2F&opts=%7B%22cache%22%3Atrue%2C%22state%22%3A%22lWZZePlGJZyM0dzNnn6rtjC0Hd8%22%2C%22state_type%22%3A%22client%22%7D

// リクエストのクエリパラメータのデコード結果
k: Q6LOFHhoDefuPOBYz8TcrFEHq8Q
d: https://oauth.io/
opts: {"cache":true,"state":"lWZZePlGJZyM0dzNnn6rtjC0Hd8","state_type":"client"}

//
// レスポンス (302)
//
https://accounts.google.com/o/oauth2/auth?client_id=428291194808-0o4ccpsje6uti8f9tej6iu4rmlfgs0vo.apps.googleusercontent.com&response_type=code&redirect_uri=https%3A%2F%2Foauth.io%2Fauth&state=Q3C06BWbXQcVe-OZqPYF0Ss0Onk&scope=email%20profile&access_type=online

// レスポンスのLocationヘッダ
https://accounts.google.com/o/oauth2/auth?client_id=428291194808-0o4ccpsje6uti8f9tej6iu4rmlfgs0vo.apps.googleusercontent.com&response_type=code&redirect_uri=https%3A%2F%2Foauth.io%2Fauth&state=Q3C06BWbXQcVe-OZqPYF0Ss0Onk&scope=email%20profile&access_type=online

// レスポンスのLocationヘッダ (デコード)
client_id=428291194808-0o4ccpsje6uti8f9tej6iu4rmlfgs0vo.apps.googleusercontent.com
response_type=code
redirect_uri=https://oauth.io/auth
state=Q3C06BWbXQcVe-OZqPYF0Ss0Onk
scope=email profile
access_type=online
```

上記リダイレクトのレスポンスで下記のページが返される。

![google authentication](./assets/google-authn.png)

ここでしばらくトラフィックを調べていたところ、その先に進めなくなった。oauth.ioのサーバ側で500エラーが出るようになったので中止。しかし、なぜscopeにemailとprofileしか指定してないんだろう（少なくともopenidは必要なはず）。

→ サインインになっていた。まだ未登録なのでサインアップじゃないと当然エラーになる。

---
LinkedInにgoogleをIdPとしてサインインした場合

(1) トップページを表示

トップページを表示した時点でページ左上にGoogleログインが表示されている。これは[One Tap](https://developers.google.com/identity/one-tap/web/guides/load-one-tap-client-library)というライブラリで実現されている。

![LinkedIn Top page](./assets/linkedin-top.png)

以下のscriptタグをページに追加するとiframeでGoogleログインページが表示されるという仕組みらしい。

```HTML
<script src="https://accounts.google.com/gsi/client" async defer></script>
```

トラフィックを見ると、実際には以下のページがリクエストされている。

```
https://accounts.google.com/gsi/iframe/select?client_id=990339570472-k6nqn1tpmitg8pui82bfaun3jrpmiuhs.apps.googleusercontent.com&ux_mode=popup&ui_mode=card&context=signin&channel_id=54ede5d9c1fe435b5fc962e9f18e62f9a1525213062d436dcb8a3bdaff01b176&origin=https%3A%2F%2Fwww.linkedin.com&as=47LPva2Oz1j6h%2Ffrm5OeuQ
```

(2) "Taroとして続行"を実行

```
POST https://accounts.google.com/gsi/issue?user_id=113301110237667535367&client_id=990339570472-k6nqn1tpmitg8pui82bfaun3jrpmiuhs.apps.googleusercontent.com&origin=https%3A%2F%2Fwww.linkedin.com&select_by=user&consent_acquired=false&token=AJt9Q0ZGOMtbQfflBMMGfcyGM8cY%3A1602764232109&as=dT%2BwgusMD9wNbO8sGA8eUQ

user_id: 113301110237667535367
client_id: 990339570472-k6nqn1tpmitg8pui82bfaun3jrpmiuhs.apps.googleusercontent.com
origin: https://www.linkedin.com
select_by: user
consent_acquired: false
token: AJt9Q0ZGOMtbQfflBMMGfcyGM8cY:1602764232109
as: dT+wgusMD9wNbO8sGA8eUQ

---

oauth.ioにGitHubをIdPとしてサインアップした場合

(1) githubアイコンをクリック

```HTTP

// リクエスト (GET)
GET https://oauth.io/auth/github?k=Q6LOFHhoDefuPOBYz8TcrFEHq8Q&d=https%3A%2F%2Foauth.io%2F&opts=%7B%22cache%22%3Atrue%2C%22state%22%3A%22V5tUnjAcBF1yVVodYqJXGRHsGE4%22%2C%22state_type%22%3A%22client%22%7D

// レスポンスのLocationヘッダ
https://github.com/login/oauth/authorize?client_id=6726de0ae02af612d8c8&response_type=code&redirect_uri=https%3A%2F%2Foauth.io%2Fauth&scope=user&state=Ok6M7U0_GtlwhLTWuLLslZqg71E
```

リダイレクト先のGitHubで表示される画面 (同時にoauth.ioにuserスコープへの権限を許可したという旨のメール通知も来る)
![oauth.io github signup](./assets/oauthio-github-authn.png)

(2) Authorize oauthioボタンをクリック

```HTTP
POST https://github.com/login/oauth/authorize

authorize: 1
authenticity_token: QRP00VgPImttohF+DOXxs35iSlE/IFPQaTUy3V3aNLgK2HDzjgKAhTnfsGBWJ/5qyGUgb3kFYHATeFRjW2w2DQ==
client_id: 6726de0ae02af612d8c8
redirect_uri: https://oauth.io/auth
state: Ok6M7U0_GtlwhLTWuLLslZqg71E
scope: user
authorize: 1

// Response
(HTML)
```

![oauth.io github signup](./assets/oauthio-github-authn-pw.png)




---
どうやらソーシャルサインアップ（？）はOIDCではなくOAuth 2.0を使ってIdPのユーザ情報へのアクセス権だけを取得し、そこからユーザ情報を得てアカウントを新規作成する手順が一般的なようだ。


# (memo) ASP.NET Core 3 の認証と認可

ASP.NET Core 3 の認証・認可の設計と実装について調べたことの覚え書き。

## 1. 手始めに読むべきリソース

手っ取り早く全体像を知るには、以下のページがわかりやすい。

* [ASP.NET Core 2.0 Authentication (Qiita)](https://qiita.com/masakura/items/85c59e60cac7f0638c1b)

上記のページでデフォルトスキームという言葉が出てくるが、それについては以下のページの説明がわかりやすい。

* [What is the point of configuring DefaultScheme and DefaultChallengeScheme on ASP.NET Core?](https://stackoverflow.com/questions/52492666/what-is-the-point-of-configuring-defaultscheme-and-defaultchallengescheme-on-asp)

また、より包括的で詳細な説明としては、"ASP.NET Core in Action, 2nd ed."の "Chapter 14 Authentication: adding users to your application with Identity" を読むとよい。有償のオンライン版もある。

* [Chapter 14 Authentication: adding users to your application with Identity](https://livebook.manning.com/book/asp-net-core-in-action-second-edition/chapter-14/v-5/)


以降の記述は、これらの資料とソースコードを参照し、その内容をあとからいつでも全体像を思い出せるように要点だけまとめたものである。

## 2. コードの流れ

### (1) 認証サービスの登録

`services.AddAuthentication`メソッドで登録。このメソッドは`AuthenticationBuilder`を返すので、続けて認証ハンドラを登録する。

認証ハンドラとは、`CookieAuthenticationHandler`や`OpenIdConnectHandler`など`IAuthenticationHandler`を実装したクラスのこと。

```Csharp:Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // 認証サービスの登録
    services.AddAuthentication()
        .AddCookie("Cookies")
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = "https://idp.company.com";
            options.RequireHttpsMetadata = false;
            options.ClientId = "clientid";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.SaveTokens = true;
        });;
}
```

上記はOIDC認証をする場合のコード例だが、認証は一般的にプロトコルで定義された部分だけでは完結しない。OIDC認証でも認証状態の維持やSSOのために、クッキーに認証状態を保持したり、ログインページを作って接続したりするなど、追加の実装が必要になる。

### (2) 認証ミドルウェアの登録

```Csharp:Startup.cs
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // 静的ファイルは認証対象外
    app.UseStaticFiles();
    app.UseRouting();

    // ここで認証（と認可）を実行
    app.UseAuthentication();
    app.UseAuthorization();

    // MVCのルーティング
    app.UseEndpoints(endpoints =>
    {
        // RequireAuthorizationでApp全体に渡って匿名アクセスを禁止
        endpoints.MapDefaultControllerRoute()
            .RequireAuthorization();
    });
}
```

`app.UseAuthentication`で認証ミドルウェアをリクエストパイプラインに登録する。認証ミドルウェアは`AuthenticationMiddleware`という名前のクラスのオブジェクトであり、リクエストがここに到達すると`Invoke`メソッドが呼ばれる。

* AuthenticationMiddlewareのソースコードは[こちら](https://bit.ly/2TpqPzK)

```Csharp:AuthenticationMiddleware.cs
public async Task Invoke(HttpContext context)
{
    // (1) HandleRequestAsyncメソッドの呼び出し
    //   * OIDC認証のような外部認証ハンドラは本メソッドを実装している(Cookie認証ハンドラ等は実装していない)
    //   * 外部から認証コードを取得するのに使われる
    //   * 取得できたら後続の処理は実行せずリターン
    
    // コードは面倒なので省略

    // (2) AuthenticateAsyncメソッドの呼び出し
    //   * リクエストで送られてきた認証情報を検証し、ClaimsPrincipalを作成してcontext.Userに設定する
    var result = await context.AuthenticateAsync(defaultAuthenticate.Name);
    if (result?.Principal != null)
    {
        context.User = result.Principal;
    }

    await _next(context);
}
```

AuthenticationMiddlewareから下流のコードは以下:

* [CookieAuthenticationHandler](https://bit.ly/34phPkz)
* [OpenIdConnectHandler](https://bit.ly/3mmaloq)

## 2. クラス構成

### (1) PrincipleとIdentity

ASP.NET Coreでは、`HttpContext.User`がカレントユーザを表す。未認証の状態では、`User`はgenericなユーザでClaimが一つもない。

![image](https://user-images.githubusercontent.com/459311/97109498-e42ff680-1716-11eb-9daf-66b8f2800097.png)

* アナロジーとしては、ClaimsPrincipalが人、ClaimsIdentityはパスポートや免許証などを表す。認証の世界でIdentityが複数になるのはMFAなどの場合。
* ClaimsIdentity.AuthenticationTypeには"Bearer"、"Cookie"などの値が入る。
* ClaimsIdentity.IsAuthenticatedがfalseになるのはゲストユーザの場合。
* Roleも互換性のために残されているが使うべきではない

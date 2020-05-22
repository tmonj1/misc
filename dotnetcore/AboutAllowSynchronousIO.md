# About AllowSynchronousIO

In ASP.NET Core 3.0, Synchronous IO operations shown below are disallowed by default:

* `HttpRequest.Body.Read`
* `HttpResponse.Body.Write`
* `Body.Flush`
* Any methods dependent on above methods

In other words, you can use methods other than the above. The purpose of this change is
to promote the use of `PipeReader`/`PipeWriter` for optimized input/output of Request/Response,
instead of using `Stream` object which has long been a source of thread starvation.
It does NOT mean that you cannot use ALL synchronized I/O methods.

It is possible to disable this feature:

```C#
// allow sync IO globally
services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
```

or, this one:

```C#
// only allow sync IO on a per request basis
var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
if (syncIOFeature != null)
{
    syncIOFeature.AllowSynchronousIO = true;
}
```

* [Breaking changes for migration from Version 2.2 to 3.0](https://docs.microsoft.com/ja-jp/dotnet/core/compatibility/2.2-3.0#http-synchronous-io-disabled-in-all-servers)
* [[Announcement] AllowSynchronousIO disabled in all servers #7644](https://github.com/dotnet/aspnetcore/issues/7644)

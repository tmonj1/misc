# How to use Serilog in ASP.NET Core 3

- [- 5. References](#ulli5-referencesliul)
- [1. Why Serilog?](#1-why-serilog)
- [2. Setup](#2-setup)
  - [2.1 Create a new ASP.NET Core project](#21-create-a-new-aspnet-core-project)
  - [2.2 Install Serilog packages](#22-install-serilog-packages)
  - [2.3 Setup Serilog](#23-setup-serilog)
  - [2.4 Remove remnant code](#24-remove-remnant-code)
- [3. Configuration](#3-configuration)
  - [3.1 Use appSettings.json](#31-use-appsettingsjson)
  - [3.2 Enable request logging](#32-enable-request-logging)
  - [3.3 Trim out infrastructure events](#33-trim-out-infrastructure-events)
  - [3.4 Use JSON formatter to enhance log output](#34-use-json-formatter-to-enhance-log-output)
  - [3.5 (Optional) Use Seq](#35-optional-use-seq)
  - [3.6 Use Enricher to get contextual properties](#36-use-enricher-to-get-contextual-properties)
  - [3.7 Install and use additional Enrichers to augment log output](#37-install-and-use-additional-enrichers-to-augment-log-output)
  - [3.8 Add static properties](#38-add-static-properties)
  - [3.9 Override appSettings.json with environment variables](#39-override-appsettingsjson-with-environment-variables)
- [4. Advanced Topics](#4-advanced-topics)
  - [4.1 Logging on bootstrap](#41-logging-on-bootstrap)
  - [4.2 Logger injection](#42-logger-injection)
  - [4.3 More properties (user agent, client ip, and so on)](#43-more-properties-user-agent-client-ip-and-so-on)
  - [4.4 OpenTelemetry](#44-opentelemetry)
  - [4.5 Use with EFK stack on docker](#45-use-with-efk-stack-on-docker)
  - [4.6 Use Elastic Common Schema (ECS)](#46-use-elastic-common-schema-ecs)
  - [4.7 Elastic APM Serilog Enricher](#47-elastic-apm-serilog-enricher)
- [5. References](#5-references)
---
 
## 1. Why Serilog?

Although ASP.NET Core has built-in logging system which supports structured logging, **Serilog** is the logging framework of choice because of its benefits as shown below:

* Supports structured logging
* Supports almost all kinds of sinks (console, file, fluentd, and so on)
* Has a vast ecosystem and community support
* More fine-tuned for optimal logging

ASP.NET Core is designed from the beginning to support third party logging frameworks and enable to switch one framework to  another, so do not hesitate to use third party library.<sup>[1](#1)</sup> 

## 2. Setup

### 2.1 Create a new ASP.NET Core project

Create a new ASP.NET Core project with ``dotnet new web`` command, which generates a template code. Then you can run it and see what logs are output by default:

```bash
$ dotnet new web
$ dotnet run
info: Microsoft.Hosting.Lifetime[0]              # <— Log category
      Now listening on: https://localhost:5001   # <- Log message
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /Users/guest/Documents/loggingtest1
info: Microsoft.Hosting.Lifetime[0]
```

As you can see, all the output is at ``info`` level. Each log message consists of two lines, the first line indicates ``Log category`` (e.g. Microsoft.Hosting.Lifetime) and the second shows its log message.

### 2.2 Install Serilog packages

Install Serilog packages using ``dotnet add package commands`` or ``NuGet``.

```bash
$ dotnet add package Serilog
$ dotnet add package Serilog.AspNetCore
$ dotnet add package Serilog.Sinks.Console
$ dotnet add package Serilog.Sinks.File
$ dotnet add package Serilog.Sinks.Fluentd  # if needed
```

### 2.3 Setup Serilog

Put boilerplate code into your **Program.cs**. The code is explained and excerpt from [here](https://github.com/serilog/serilog-aspnetcore).<sup>[2](#2)</sup>

```CSharp
using Serilog;

public class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting web host");
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();  # Important. This replaces default log providers with Serilog providers.
}
```

The last line (.UseSerilog()) is important. This replaces default log providers (such as microsoft’s default Console provider) with Serilog providers, so that all log events go through the Serilog logging pipeline.

Let’s check if everything is OK by running the program at this point. Here is an example of logging output:

```
[10:08:05 DBG] Hosting starting
[10:08:05 DBG] Failed to locate the development https certificate at 'null'.
[10:08:05 DBG] Using development certificate: CN=localhost (Thumbprint: 323B6CB70076502438644551E877EAF2CC5D3B61)
[10:08:05 INF] Now listening on: https://localhost:5101
[10:08:05 INF] Now listening on: http://localhost:5100
[10:08:05 DBG] Loaded hosting startup assembly loggingtest1
[10:08:05 INF] Application started. Press Ctrl+C to shut down.
[10:08:05 INF] Hosting environment: Development
[10:08:05 INF] Content root path: /Users/guest/Documents/dev/loggingtest1
[10:08:05 DBG] Hosting started
```

Note that the output format has changed. Timestamp and event level properties come in, and log category has gone away (This format comes from Serilog’s default output template). Both this and previous output examples are in ``plain text`` format. You can choose another format. Here is an example for using compact JSON format in console output:

```CSharp
using Serilog.Formatting.Compact;

:
Log.Logger = new LoggerConfiguration()
      .WriteTo.Console(new CompactJsonFormatter()) // specifying compact JSON format
      .CreateLogger();
```

The output is like this:

```
{"@t":"2020-05-10T03:15:53.1136070Z","@m":"Hosting starting","@i":"32b26330","@l":"Debug","EventId":{"Id":1},"SourceContext":"Microsoft.Extensions.Hosting.Internal.Host"}
{"@t":"2020-05-10T03:15:53.2194480Z","@m":"Failed to locate the development https certificate at 'null'.","@i":"902e163b","@l":"Debug","certificatePath":null,"EventId":{"Id":2,"Name":"FailedToLocateDevelopmentCertificateFile"},"SourceContext":"Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer"}
:
:
```

The above output is in ``compact`` JSON format, so readability is a bit low. If you use ``Serilog.Formatting.Json.JsonFormatter`` instead, you will get more readable JSON output.<sup>[3](#3)</sup>

### 2.4 Remove remnant code

So far, so good. But there are some residual code in appSettings.json,  it would be better to remove them. 

Remove the **Logging** section in appSettings.json entirely. You can remove it because the section is for .NET Core built-in logging, which is already useless.

before:
```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

after:
```JSON
{
  "AllowedHosts”: “*”
}
```

## 3. Configuration

### 3.1 Use appSettings.json

Setup is done. The next step is configuration. Log providers and their settings are hard coded in the Program.cs so far. This is inconvenient for real world scenarios,  and it should be configured via appSettings.json. So, modify code a little to make the program use appSettings.json.

Here is the modified version. Note that logger creation code which used to be just below the entry point of the main function is gone. Instead of that, the logger is created via configurations (see (2)). There is one more logger in (1), which is only created to report errors at very early stage of bootstrapping.

```CSharp
public static int Main(string[] args)
{
  try
  {
    CreateHostBuilder(args).Build().Run();
    return 0;
  }
  catch (Exception ex)
  {
    // (1) Create a console logger to report fatal error on startup.
    if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
    {
      Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger();
    }
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
  }
  finally
  {
    Log.CloseAndFlush();
  }
}

public static IHostBuilder CreateHostBuilder(string[] args) =>
  Host.CreateDefaultBuilder(args)
      .ConfigureWebHostDefaults(webBuilder =>
      {
        webBuilder.UseStartup<Startup>();
      })
      // (2) Create loggers by reading configurations
      .UseSerilog((hostingContext, LoggerConfiguration) =>
      {
        LoggerConfiguration
          .ReadFrom.Configuration(hostingContext.Configuration);
      });
}
```

You need to add serilog settings in appSettings.json. This is very simple one (just add a Console provider):

```JSON
"Serilog": {
    "Using": [
      "Serilog.Sinks.Console"
    ],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console”
      }
    ]
  }
```

Run the program and see the log output. It should be like this (I called a Web API once, so API request log is shown, too):

```
[22:08:54 INF] Now listening on: https://localhost:5101
[22:08:54 INF] Now listening on: http://localhost:5100
[22:08:54 INF] Application started. Press Ctrl+C to shut down.
[22:08:54 INF] Hosting environment: Development
[22:08:54 INF] Content root path: /Users/guest/Documents/dev/loggingtest1
[22:09:22 INF] Request starting HTTP/1.1 GET https://localhost:5101/  
[22:09:23 INF] Executing endpoint '/ HTTP: GET'
[22:09:23 INF] Executed endpoint '/ HTTP: GET'
[22:09:23 INF] Request finished in 63.9093ms 200 
```

Because “MinimumLevel” (minimum output level) is set to “Information”, the DBG output is gone.

### 3.2 Enable request logging

Add ``app.UseSerilogRequestLogging();`` in Configure method in Startup.cs to augment HTTP request logging:

```CSharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
  if (env.IsDevelopment())
  {
    app.UseDeveloperExceptionPage();
  }

  // add this line to enhance request logging.
  // In general, put it before UseRouting(), after UseStaticFiles().
  app.UseSerilogRequestLogging();
            
  // :
  // :
}
```

The result is like this:

```
[22:13:22 INF] Now listening on: https://localhost:5101
[22:13:22 INF] Now listening on: http://localhost:5100
[22:13:22 INF] Application started. Press Ctrl+C to shut down.
[22:13:22 INF] Hosting environment: Development
[22:13:22 INF] Content root path: /Users/guest/Documents/dev/loggingtest1
[22:13:25 INF] Request starting HTTP/1.1 GET https://localhost:5101/  
[22:13:25 INF] Executing endpoint '/ HTTP: GET'
[22:13:25 INF] Executed endpoint '/ HTTP: GET'
[22:13:25 INF] HTTP GET / responded 200 in 43.0949 ms   # This line is added by adding UseSerilogRequestLogging()
[22:13:25 INF] Request finished in 58.7403ms 200 
```

### 3.3 Trim out infrastructure events

Now that it becomes possible to configure logger via appSettings.json, let’s begin to fine-tune configuration settings. First, there are too many infrastructure events (which is emitted by ASP.NET Core event handlers) output, so limit them only to warning or higher levels (but still set the minimum level of application events to Information). To do so, edit appSettings.json as below:

```JSON
 "Serilog": {
    "Using": [ "Serilog.Exceptions", "Serilog", "Serilog.Sinks.Console", "Serilog.Sinks.Seq" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "System": "Warning",
        "Microsoft" : "Warning"
      }
    }
```

The output is like this:

```
[15:15:18 INF] HTTP GET / responded 200 in 54.3920 ms
```

### 3.4 Use JSON formatter to enhance log output

All the infrastructure event output are gone. But this time the output log is too simple and almost useless to diagnose. The default format of console log only output log messages, so let’s change appSettings.json so that all event properties are output to console in JSON format: 

```JSON
"Serilog": {
    "Using": [
      "Serilog.Sinks.Console"
    ],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ]
  }
```

The output is like this:

```
{"RequestMethod":"GET","RequestPath":"/","StatusCode":200,"Elapsed":56.256475,"SourceContext":"Serilog.AspNetCore.RequestLoggingMiddleware"},"Renderings":{"Elapsed":[{"Format":"0.0000","Rendering":"56.2565"}]}}
```

### 3.5 (Optional) Use Seq

Seq is a log server which is easy-to-use and very suitable for Serilog. Installing Seq is very simple if you use docker:
$ docker run --rm -it -e ACCEPT_EULA=Y -p 5341:80 datalust/seq

In order to send log output to Seq, add Seq entry to WriteTo section in appSettings.json:

```JSON
“Serilog”: {
  "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "restrictedToMinimumLevel": "Verbose"
        }
      }
    ]
```

Then, run the application and open `http://localhost:5341` using your web browser if you are in a non-proxy network. Seq seems not to support proxy environment, so if your network uses a proxy, you cannot use Seq (at least as of this writing).

### 3.6 Use Enricher to get contextual properties

The content of log output is now much better than before, but it still lacks some important pieces of information such as “RequestId” etc., which are actually recorded internally by Serilog. These additional properties come from ambient execution context and can be output by enabling logging context as below:

```JSON
{
  "Serilog": {
    "Enrich": [
      "FromLogContext"
    ]
  }
}
```

The result is like this:

```
{"RequestMethod":"GET","RequestPath":"/","StatusCode":200,"Elapsed":39.965268,"SourceContext":"Serilog.AspNetCore.RequestLoggingMiddleware","RequestId":"0HLVPIM799T5B:00000001","SpanId":"|67e27681-4a794311adda6fa7.","TraceId":"67e27681-4a794311adda6fa7","ParentId":"","ConnectionId":"0HLVPIM799T5B"},"Renderings":{"Elapsed":[{"Format":"0.0000","Rendering":"39.9653"}]}}
```

Here is an explanation of important properties:

| Property      | Description                                                                                                     |
| :------------ | :-------------------------------------------------------------------------------------------------------------- |
| SourceContext | class name of the instance which emits the log entry. In other words, the class T of ILogger<T> in source code. |
| ConnectionId  | Unique connection Identifier set automatically by Kestrel.                                                      |
| RequestId     | Unique Request Identifier set automatically by Kestrel in {ConnectionId}:{SerialNumber} format.                 |
| TraceId       | TraceId is used for identifying unique transaction across multiple components in an microservices application.  |
| ParentId      | ParentId is the caller's id in the trace context.                                                               |
| SpanId        | SpanId is the identifier of a span (an entity executing an api) in the trace context.                           |

### 3.7 Install and use additional Enrichers to augment log output

Our log output is now very useful, but still has a room to improve. If we have machine name,  process id, and thread id in our log, it would be very useful in a microservices application environment. To do that, install these packages:

```bash
$ dotnet add package Serilog.Enrichers.Environment
$ dotnet add package Serilog.Enrichers.Process
$ dotnet add package Serilog.Enrichers.Thread
```

Then, add a few properties to appSettings.json:

```JSON
 "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithProcessId",
      "WithThreadId"
    ]
```

The output is like this:

```
{"Timestamp":"2020-05-16T22:16:18.0108970+09:00","Level":"Information","MessageTemplate":"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms","Properties":{"RequestMethod":"GET","RequestPath":"/","StatusCode":200,"Elapsed":40.431947,"SourceContext":"Serilog.AspNetCore.RequestLoggingMiddleware","RequestId":"0HLVPJD8N8PG5:00000001","SpanId":"|188e382d-45458c410db2f2ed.","TraceId":"188e382d-45458c410db2f2ed","ParentId":"","ConnectionId":"0HLVPJD8N8PG5","MachineName":"TAROnoMacBook-Air","ProcessId":97374,"ThreadId":4},"Renderings":{"Elapsed":[{"Format":"0.0000","Rendering":"40.4319"}]}}
```

### 3.8 Add static properties

If you want to add static properties (like fixed string), you can do it by adding custom properties in `properties` array in appSettings.json. This add application name as a custom property:

```JSON
{
  "Serilog": {
    "Properties": {
      "Application": "demo1"
    }
  }
}
```

The output is like this:

```
{"Timestamp":"2020-05-16T22:16:18.0108970+09:00","Level":"Information","MessageTemplate":"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms","Properties":{"RequestMethod":"GET","RequestPath":"/","StatusCode":200,"Elapsed":40.431947,"SourceContext":"Serilog.AspNetCore.RequestLoggingMiddleware","RequestId":"0HLVPJD8N8PG5:00000001","SpanId":"|188e382d-45458c410db2f2ed.","TraceId":"188e382d-45458c410db2f2ed","ParentId":"","ConnectionId":"0HLVPJD8N8PG5","MachineName":"TAROnoMacBook-Air","ProcessId":97374,"ThreadId":4,"Application":"demo1"},"Renderings":{"Elapsed":[{"Format":"0.0000","Rendering":"40.4319"}]}}
```

### 3.9 Override appSettings.json with environment variables

It is possible to override settings in appSettings.json with environment varialbles. For example, `Application` property in `Properties` can be overridden like this:

```bash
$ export Serilog__Properties__application=“demo2”  # <- override appSettings.json
```

Note that, `__` is used to separate hierarchy. In Windows, `:` is used instead.

## 4. Advanced Topics

### 4.1 Logging on bootstrap

In the above example code, there is no way of getting errors at the very early stage of bootstrap because it takes some time to read appSettings.json to configure our logger. This problem is discussed [here](https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/).

* After writing the above statements, I rewrite [HowToUseSerilogDemo](https://github.com/tmonj1/misc/tree/master/dotnetcore/HowToUseSerilogDemo) to be able to catch error on bootstrap. One of the side effects of this modification is not to be able to use appsettings.json to configure Serilog. It is still possible to allow some runtime configuration using environment variables or something, but you have to write many lines of custom code for that purpose. In reality, what you can do at most is to pick up some important settings which are expected to be changed according to the runtime environment and make them configurable.

### 4.2 Logger injection

You can use DI to inject Serilog logger to your class. For example, if you have LogTest class with constructor taking a parameter of type ILogger&lt;LogTest> and let the framework to instantiate LogTest, then you will get a Serilog logger via constructor:

```CSharp
  public class LogTest : ILogTest
  {
    private readonly ILogger<LogTest> _logger;
    public LogTest(ILogger<LogTest> logger)
    {
      // get a Serilog logger implementing ILogger interface.
      _logger = logger;
    }

    public void Log()
    {
      _logger.LogInformation("this is a LogTest message.");
    }
  }
```

### 4.3 More properties (user agent, client ip, and so on)

In the above example, there are not user agent properties nor client IP address. You can add these properties using Serilog.Sinks.ClientInfo or write your own code for that purpose.<sup>[4](#4)</sup>

### 4.4 OpenTelemetry

OpenTelemetry is a future standard for telemetry API and library. ASP.NET Core and other logging tools such as Prometheus will support OpenTelemetry in the near future. <sup>[5](#5)</sup>

### 4.5 Use with EFK stack on docker

When deploying your app on docker and sending logs to EFK (Elasticsearch, Fluentd, and Kibana) stack, just use Console logger with ElasticsearchJsonFormatter. Fluentd log driver on docker collects the console output and sends it to Elasticsearch.

### 4.6 Use Elastic Common Schema (ECS)

Elastic Common Schema is possibly a new de facto standard for logging community. <sup>[6](#6)</sup> It is possible to use this format by using `Elastic.CommonSchema.Serilog` package.<sup>[7](#7)</sup>

You can use ECS formatter like this:

```CSharp
var logger = new LoggerConfiguration()
    .WriteTo.Console(new EcsTextFormatter())
    .CreateLogger();
```

Or, You can specify it in appsettings.json:

```JSON
"WriteTo": [
  {
    "Name": "Console",
    "Args": {
      "formatter": "Elastic.CommonSchema.Serilog.EcsTextFormatter,Elastic.CommonSchema.Serilog"
    }
  }
]
```

### 4.7 Elastic APM Serilog Enricher

(TBD)

https://github.com/elastic/ecs-dotnet/tree/master/src/Elastic.Apm.SerilogEnricher

## 5. References

1. <a name="f1">[Logging with .NET Core](https://docs.microsoft.com/en-US/archive/msdn-magazine/2016/april/essential-net-logging-with-net-core)</a>
2. <a name="2">[Setting up Serilog in ASP.NET Core 3](https://nblumhardt.com/2019/10/serilog-in-aspnetcore-3/)</a>
3. <a name="3">[README.md serilog-formatting-compact @GitHub](https://github.com/serilog/serilog-formatting-compact)</a>
4. <a name="4">[Serilog Logging Best Practices](https://benfoster.io/blog/serilog-logging-best-practices)</a>
5. <a name="5">[Improvements in .NET Core 3.0 for troubleshooting and monitoring distributed apps](https://devblogs.microsoft.com/aspnet/improvements-in-net-core-3-0-for-troubleshooting-and-monitoring-distributed-apps/)</a>
6. <a name="6">[Introducing the Elastic Common Schema](https://www.elastic.co/blog/introducing-the-elastic-common-schema)</a>
7. <a name="7">(elastic/ecs-dotnet@GitHub)[https://github.com/elastic/ecs-dotnet)</a>
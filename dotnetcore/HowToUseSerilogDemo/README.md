# How to use Serilog in ASP.NET Core 3

This is an example code explaining how to use Serilog in ASP.NET Core 3.
There are two ways of running the example.

## 1. `dotnet run` the example program, and run SEQ on docker

* warning: This scenario does not work in a proxy environment probably because SEQ does not support proxy.
In that case, try #2 scenario (using docker).

```bash
# run Seq first.
$ docker run --rm -it -e ACCEPT_EULA=Y -p 5341:80 datalust/seq
# set ASPNETCORE_ENVIRONMENT to "Production" (Development is used for docker configuration)
$ export ASPNETCORE_ENVIRONMENT=Production
# then, run the demo program
$ dotnet run
```

After the program runs successfuly, open your browser and go to "http://localhost:5100" (or https://localhost:5101), and then go to "http://localhsot:5341" for SEQ.

## 2. Run both the program and SEQ on docker

```bash
# docker build and run
$ docker-compose up -d
```

After the program runs successfuly, open your browser and go to "http://localhost:5100" (or https://localhost:5101), and then go to "http://localhsot:5341" for SEQ.

## 3. Using fluentd

### 3.1 Setup and configuration

```bash
$ dotnet add serilog.formatting.elasticsearch
```

Writing logs to Elasticsearch with Fluentd using Serilog in ASP.NET Core
https://andrewlock.net/writing-logs-to-elasticsearch-with-fluentd-using-serilog-in-asp-net-core/

## 4. Supported Environment variables

| Name                    | Default       | Available Values                                                           |
| :---------------------- | :------------ | :------------------------------------------------------------------------- |
| LOG_MIN_LEVEL           | Information   | Verbose, Debug, Information, Warning, Error, Fatal                         |
| LOG_MIN_LEVEL_MICROSOFT | Warning       | Verbose, Debug, Information, Warning, Error, Fatal                         |
| LOG_MIN_LEVEL_SYSTEM    | Warning       | Verbose, Debug, Information, Warning, Error, Fatal                         |
| LOG_FORMATTER           | JsonFormatter | Elastic.CommonSchema.Serilog.EcsTextFormatter,Elastic.CommonSchema.Serilog |
| LOG_SEQ_URL             | ""            | http://localhost:5341                                                      |
| LOG_SEQ_APIKEY          | ""            | published API key                                                          |

## 5. Custom Serilog Configurator

```CSharp
// setup logger
var versionInfo = Assembly.GetEntryAssembly().GetApplicationVersionInfo();
Log.Logger = SerilogConfiguration.ConfigureDefault(loggerConfig => loggerConfig
    .Enrich.WithProperty("AppName", versionInfo.Name)
    .Enrich.WithProperty("Semver", versionInfo.SemanticVersion)
    .WriteTo.SeqWithUrl(Environment.GetEnvironmentVariable("LOG_SEQ_URL"))
).CreateLogger()
```
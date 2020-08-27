# About Telemetry

- [What is telemetry](#what-is-telemetry)
- [Observability](#observability)
- [Telemetry in a microservice application](#telemetry-in-a-microservice-application)
  - [Goal](#goal)
  - [Constituents of telemetry](#constituents-of-telemetry)
  - [Tools for telemetry](#tools-for-telemetry)
  - [Logs](#logs)
    - [(1) Use Logstash format](#1-use-logstash-format)
    - [(2) Transmit logs from containers](#2-transmit-logs-from-containers)
    - [(3) Store logs in elasticsearch](#3-store-logs-in-elasticsearch)
    - [(4) View and analyze logs using Kibana](#4-view-and-analyze-logs-using-kibana)
  - [Metrics](#metrics)
    - [(1) Overview](#1-overview)
    - [(2) Collect metrics](#2-collect-metrics)
  - [Distributed Tracing](#distributed-tracing)
- [Resources](#resources)
- [Footnotes](#footnotes)

## What is telemetry

`Telemetry` is the collection of measurements or other data at remote or inaccessible points
and their automatic transmission to receiving equipment for monitoring (source: Wikipedia).<sup>[1](#fn1)</sup>

`テレメトリ`とは、遠隔地や容易に近づけない場所の測定データやその他のデータの集まりのことであり、
また監視のためにそうしたデータを受信機器に自動で送信することである。

Important points:
* collection of measurements or other data
* automatic transmission (actively transmit, NOT passively collected)

## Observability

In control theory, `observability` is a measure of how well internal states of a system
can be inferred from knowledge of its external outputs (source: Wikipedia).<sup>[2](#fn2)</sup>

制御理論において、`可観測性`とはシステムの外部出力結果からシステムの内部状態がどれだけよく推測できるかを
表す尺度である。

## Telemetry in a microservice application

### Goal

`Make every component in a microservice app observable via telemetry.`

* Every component in a microservice application should be `observable`.
* `Observability` is achieved by enabling `telemetry` in each component.

### Constituents of telemetry

In a distributed system like microservices, telemetry usually consists of the following:

1. logs
2. metrics
3. distributed tracing
4. health check

### Tools for telemetry

| type of data        | tools                      | AWS  services |
| :------------------ | :------------------------- | :------------ |
| logs                | EFK                        | CloudWatch    |
| metrics             | StatsD/Prometheus/Graphana | CloudWatch    |
| distributed tracing | Jaeger                     | X-ray         |

* Service mesh tools such as Istio are also available.
* SaaS such as Datadog are also available. 

### Logs

#### (1) Use Logstash format

Just as The twelve factor app says, each component in a microservice app should emit
its log to console without buffering.<sup>[3](#fn3)</sup> The output should be in the
`Logstash` format because Docker container supports fluentd logging driver,
which writes logs to fluentd in this format (and Kubernetes also actively supports
fluentd).

**sidenote:** Elastic Common Schema seems to be an alternative to logstash format.

#### (2) Transmit logs from containers

Each component emit its logs to console, which then is transmitted via logging
driver (fluentd driver) to fluentd server.

Middleware containers such as database containers also emits logs. When it comes to
SQL Server container, it spits error logs to console<sup>[4](#fn4)</sup>, so it is
OK to route them to fluentd through fluentd logging driver. Other logs are stored
in the database itself, but they need not to be collect to the central log server
because they are used via the dedicated tools such as SQL Server Management Studio
(SSMS).

 * It is perfectly OK to transmit these logs to the central log server if you
 want. This is an example like that:
 [SQLServer+Logstash+Elasticsearch+kibana 5.6.3](https://sqlserversuki.com/monitoring/sqlserverlogstashelasticsearchkibana-5-6-3/)

#### (3) Store logs in elasticsearch

When using EFK stack for logging, logs are stored, and then analyzed, in Elasticsearch.

* When you develop your app using ASP.NET Core, it might be useful, especially in 
development environment, to use SEQ as the log store and viewer. You can use SEQ
and EFK at the same time (by transmitting logs to both console and SEQ).

#### (4) View and analyze logs using Kibana

You can view and analyze logs using Kibana.

### Metrics

#### (1) Overview

Metrics are collected as follows:

```
| service | --> | StatsD | --> | StatsD Exporter | --> | Prometheus | --> | Graphana |
```

1. each service, middleware, or node pushes metrics their own metrics to StatsD server, a metrics collector.
2. StatsD Exporter, a metrics format converter from statsd format to prometheus format, fetches metrics from StatsD server
3. Prometheus, a metrics aggregator, fetches metrics from StatsD Exporter
4. Graphana, a metrics viewer and analyzer, fetches metrics from Prometheus

In some cases, the route depicted above can be short cut; for example RabbitMQ can directly sends its metrics to Prometheus through a metrics emitting plugin.

#### (2) Collect metrics

Metrics data are collected from Each service, middleware, and node through some instrumentation. When it comes to services you created, you have to insert instrumental code into your service, then emit the resulting data to StatsD server.

In case of middleware and nodes, there might be dedicated plugins or something to collect metrics, so you can use them.

| middleware    | plugin | notes |
| :------------ | :----- | :---- |
| RabbitMQ      |        |       |
| SQL Server    |        |       |
| Oracle        |        |       |
| Elasticsearch |        |       |
| Redis         |        |       |
| k8s node      |        |       |

(TBD)


### Distributed Tracing

(TBD)

## Resources

* [What is Telemetry? The Guide to Application Monitoring](https://www.sumologic.jp/insight/what-is-telemetry/)

---
## Footnotes

<a name="fn1">1</a>: [Telemetry @Wikipedia](https://en.wikipedia.org/wiki/Telemetry)  
<a name="fn2">2</a>: [Observability @Wikipedia](https://en.wikipedia.org/wiki/Observability)  
<a name="fn3">3</a>: [The twelve factor app](https://12factor.net/ja/)  
<a name="fn4">4</a>: [Quickstart: Run SQL Server container images with Docker](https://docs.microsoft.com/ja-jp/sql/linux/quickstart-install-connect-docker?view=sql-server-ver15&pivots=cs1-bash)  

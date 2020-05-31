# Microservice Chassis

## 1. What is chassis

A `chassis` provides common functionalities which every component consisting of a service
should be equipped with, such as logging, metrics, authentication handling and so on.
It also defines a common rule set which all services should abide by, in order to enable
developers in different teams to create their new microservice easier.

### 1.1 Role of chassis

A chassis provides functionalities which are common to all (or at least multiple) services, no matter what language/framework is used.

* emit logs to the central log server
* export metrics for metrics collector 
* provide additional info for distributed tracing
* fetch configurations (via files and environment variables)
* provide a minimum set of "standard" libraries (for logging, data access, ORM and so on)
* health checks
* (service registry and discovery?)
* (security?)
* (support circuit breaking)
* (graceful termination on SIGTERM)

A chassis also provide app specific functionalities such as:

* handle session
* handle authentication data handed via http header
* access account store

### 1.2 Constituents of a chassis

A chassis consists of the following:

* a custom base image (additional setup based on an official OS/middleware image)
* a base source code
* custom packages (Nuget for .NET Core, NPM for Node.js)
* manual procedure to create the initial development environment and code base:

### 1.3 How to use it

Procedures to get a chassis when starting development of a new service:

  * get the base code from the repo of codebase.
  * install all the dependencies (via public and private registries)
  * follow the manual procedure for initial setup
    * modify some configurations in config files
    * add some code
  * test run it and confirm to check if the setup is completed successfully.

Procedures to run your newly created service on docker:

  * build your service
  * follow the procedure for executing it on the custom docker image

### 1.4 Supported programming languages (and framework)

* C# (ASP.NET Core)
* JavaScript (Node.js)
* (Python)

### 1.5 Supported runtime environments

* Docker (Docker Compose)
* Kubernetes
* AWS (ECS/EKS)
* (Azure)

## 2. Common design principle

### 2.1 Configurable via conf filles and environment variables

All settings needed to change in dev and/or production environments should be configurable
in the order of following priorities:

1. command parameters
2. environment variables
3. configuration files (individual service specific)
4. configuration files (machine wide)
5. program default (hard coded)

* Do not provide program default if it is do harm more than good.
* Use environment variables when possible.
* Use JSON for configuration file when possible.

## 3. C# (ASP.NET Core)

### 3.1 Summary

* [x] Base image: official microsoft image
  * use `sdk` image for dev, `runtime` for staging and production<sup>[1](#fn1)</sup>
  * use Alpine based images.
* [x] Web server: Kestrel
* [ ] Logger: Serilog + fluentd sink (+ SEQ optionally in dev)
  * [x] code base
  * [ ] startup logging
* RDB: Dapper / ADO.NET
* MQ: Rabbit MQ wrapper
  * use RabbitMQ.Client 6.0 package (compatible with .NET Core 3.1)
* authentication: bearer token (JWT / Minimum token)
* Unit Testing: xUnit + Fluent Assertions

## 4 Node.js

%%% (TBD)

## 5 Python

%%% (TBD)

## 6 Independent middleware services

%%% (TBD)

### 6.1 RDB
### 6.2 NoSQL
### 6.3 MQ
### 6.4 Distributed cache
### 6.4 Logging

* EFK stack (Elasticsearch + Fluentd + Kibana)
  * [x] Fluentd setup for ASP.NET Core (using Serilog)
  * [ ] Fluentd setup for Node.js
  * [ ] ES index structure
  * [ ] How to use Kibana

### 6.5 Metrics
### 6.6 Distributed tracing

* StatsD + Prometheus + Graphana

## 7. Cross-cutting concerns

1. app url  
  Each service needs to know the app's (NOT the service's) url when generating a link to an endpoint in its service.
  The service cannot know the app url by itself, it is needed to supply the url via configuration file or another means.

## 8. Resources

* Microservices in Action 

## 9. Footnotes

1. <a name="aaa1">[Official .NET Docker images](https://docs.microsoft.com/ja-jp/dotnet/architecture/microservices/net-core-net-framework-containers/official-net-docker-images)</a>
# How to use Redis in .NET Core

## Client library

There are many client libraries for Redis in .NET Core, and seemingly there are a few seemingly Microsoft **officail** libraries,
but the library to use is `StackExchange.Redis`.

```bash:
$ dotnet add package StackExchange.Redis
```

For details of `StackExchange.Redis`, please see (StackExchange.Redis)[https://stackexchange.github.io/StackExchange.Redis/).

* [notes] This is a Q and A for Redis libraries. [Differences between Microsoft.Extensions.Cashing.Redis and Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache](https://stackoverflow.com/questions/59847571/differences-between-microsoft-extensions-cashing-redis-and-microsoft-extensions)
* [notes2] [Using Redis with .Net C#@RedisLab](https://docs.redislabs.com/latest/rs/references/client_references/client_csharp/)

### ???

* Master/Replica configuration
* 
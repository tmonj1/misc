# How to use Redis

There are a log of feature for Redis, but this document only shows you how to use Redis as a distributed cache server.

The latest version of Redis is `6.0.6` for Redis itself and `5.0.6` for AWS Elasticache respectively as of time writing this. You can confirm them by yourself
on [redis/redis@Github](https://github.com/redis/redis) and [AWS page here](https://docs.aws.amazon.com/ja_jp/AmazonElastiCache/latest/red-ug/supported-engine-versions.html).

## 1. Server setup

### 1.1 Quick setup

You can start using redis using the [redis official docker image](https://hub.docker.com/_/redis)
just by typing a one liner as show below for development purpose:

```bash
#Use redis using the redis official docker image
$ docker run --name redis -d redis
#Login the redis containe12ddr
$ docker exec -it redis /bin/bash
#Check Redis server version
root@194aadb55f23:/data# redis-server --version
Redis server v=6.0.5 sha=00000000:0 malloc=jemalloc-5.1.0 bits=64 build=db63ea56716d515f
#Check Redis CLI version
root@194aadb55f23:/data# redis-cli --version
redis-cli 6.0.5
```

* [sidenote] Redis use 6379 port by default.

### 1.2 Configuration

#### (1) redis.conf

Redis can run without the configuration file, but if you needs some configurations,
you can add your own configuration file to `/usr/local/etc/redis/redis.conf` by
using a docker volume or creating your own docker image (see [here](https://hub.docker.com/_/redis)).

* For more information about `redis.conf`, see [here](https://redis.io/topics/config).
* A sample configuration file is [here](https://raw.githubusercontent.com/redis/redis/6.0/redis.conf)
* [【入門】Redis](https://qiita.com/wind-up-bird/items/f2d41d08e86789322c71) is also a good article on Redis configuration.

#### (2) CONFIG commands

You can change cofiguration settings at runtime without restarting Redis using [CONFIG SET](https://redis.io/commands/config-set) command. The list of configurable items can be obtained just by typing `CONFIG GET *`.

#### (3) logs

Redis does not emit log messages by default, but you can configure redis to do so.
For setting the log file path and the log output level, see [here](https://qiita.com/suin/items/3b513fffc300452b4e9e).

## 2. Redis basics - How to use Redis from redis cli

### 2.1 using Redis CLI

```bash
#Start Redis CLI
root@194aadb55f23:/data# redis-cli
127.0.0.1:6379>
#Exit CLI
127.0.0.1:6379> exit
```

* [sidenote] Use `redis-cli --raw` to avoid garbled text of multibyte characters.

Redis command reference is [here](https://redis.io/commands/set).

### 2.2 String

#### (1) Simple GET, SET and DEL

* Use `set <key> <value>` for adding a key-value pair.
* Use the same above command for updating value.
* Use `get <key>` for getting value from key.
 
```bash
#Add a key-value pair
127.0.0.1:6379> set k1 v1
OK
#Get value from key
127.0.0.1:6379> get k1
"v1"
#Update value
127.0.0.1:6379> set k1 v1-updated
OK
#Get updated value from key
127.0.0.1:6379> get k1
"v1-updated"
#Delete key
127.0.0.1:6379> del k1
(integer) 1
127.0.0.1:6379> get k1
(nil)
```

#### (2) Options for SET command

| Option          | Desc                            |
| :-------------- | :------------------------------ |
| EX seconds      | Set expire time in seconds      |
| PX milliseconds | Set expire time in milliseconds |
| NX              | Add only                        |
| XX              | Update only                     |
| KEEPTTL         | Retain the TTL                  |

```bash
#Use XX option if you want to update values for existing keys
127.0.0.1:6379> set k1 v1-updated2 XX
OK
127.0.0.1:6379> get k1
"v1-updated2"
#If the key doesn't exist for XX option, the command fails.
127.0.0.1:6379> set k3 v3 XX
(nil)
127.0.0.1:6379> get k3
(nil)
```

#### (3) KEYS and EXISTS for checking if keys exist

```bash
#List all keys
127.0.0.1:6379> keys *
1) "k2"
2) "k1"
3) "k3"
#Check if the key exists
127.0.0.1:6379> exists k1
(integer) 1  # 0 for non-existence
#Delete specified key(s)
127.0.0.1:6379> del k1 k2
(integer) 2
127.0.0.1:6379> keys *
1) "k3"
#Delete all keys
127.0.0.1:6379> flushall
OK
127.0.0.1:6379> keys *
(empty array)
```

#### (4) Expiration (TTL)

* Use SET command's `EX seconds` or `PX milliseconds` options for setting TTL.
* Use SET command's `KEEPTTL` option or `PERSIST` command for clearing TTL (= making TTL infinity).
* Use `EXPIRE seconds` or `PEXPIRE milliseconds` command to update TTL
(`EXPIREAT timestamp-in-seconds`/`PEXPIREAT timestamp-in-milliseconds` command is available, too).
* You can use SET command's `EX seconds` or `PX milliseconds` options to update TTL (the key-value is recreated in this case).
* USE `TTL` command to get the current TTL for a key.

```bash
#Add a key-value which expires in 10 seconds.
127.0.0.1:6379> set k1 v1 ex 10
OK
#Can get value within 10 sec from key creation.
127.0.0.1:6379> get k1
"v1"
#Get the current TTL
127.0.0.1:6379> ttl k1
(integer) 5  #remained TTL is 5 sec.
#Reset TTL to 10 sec again
127.0.0.1:6379> expire 10
(integer) 1
#Clear TTL
127.0.0.1:6379> persist k1
(integer) 1
127.0.0.1:6379> ttl k1
(integer) -1
#Reset TTL again
127.0.0.1:6379> expire 5
(integer) 1
#Cannot get value after 5 sec (key has been deleted automatically).
127.0.0.1:6379> get k1
(nil)
```

#### (5) Other operations

%%% mget/mset
%%% strlen, append
%%% INCR

### 2.3 Hash, Set, Ordered Set, and List

%%%

## 3. Client library (.NET)

There are many client libraries for Redis in .NET Core, and seemingly there are a few seemingly Microsoft **officail** libraries,
but the library to use is `StackExchange.Redis`.

```bash:
$ dotnet add package StackExchange.Redis
```

For details of `StackExchange.Redis`, please see (StackExchange.Redis)[https://stackexchange.github.io/StackExchange.Redis/).

* [notes] This is a Q and A for Redis libraries. [Differences between Microsoft.Extensions.Cashing.Redis and Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache](https://stackoverflow.com/questions/59847571/differences-between-microsoft-extensions-cashing-redis-and-microsoft-extensions)
* [notes2] [Using Redis with .Net C#@RedisLab](https://docs.redislabs.com/latest/rs/references/client_references/client_csharp/)

## 4. Advanced Topics

### 4.1 How to use Redis as LRU cache

Summary:

1. Set `maxmemory` to an appropriate value
2. Set `maxmemory-policy` to `volitile-lru`
3. Add a lot of data to Redis

When `used_memory' exceeds `maxmemory`, then least recenly used data is discarded from memory
to make space for new keys.

* see [Redis を LRU キャッシュとして使う](https://redis-documentasion-japanese.readthedocs.io/ja/latest/topics/lru-cache.html)

#### (1) Set maxmemory

```bash
#First, check the current used memory
127.0.0.1:6379> info memory
#Memory
used_memory:865208
used_memory_human:844.93K   #844.93kb used
:
: (snip)
:
#Set maxmemory to the value slightly larger than used_memory
127.0.0.1:6379> config set maxmemory 870K
OK
127.0.0.1:6379> config get maxmemory
maxmemory
870000 #maxmemory_human:849.61K (this can be obtained by INFO memory command)
```

#### (2) Set maxmemory-policy

```bash
127.0.0.1:6379> config set maxmemory-policy volatile-lru
OK
127.0.0.1:6379> config get maxmemory-policy
maxmemory-policy
volatile-lru
```

#### (3) Add keys and observe the behavior

```bash
#Add some keys, then see the memory
127.0.0.1:6379> info memory
used_memory:869968
used_memory_human:849.58K
#Check the number of keys
127.0.0.1:6379> info keyspace
db0:keys=29,expires=24,avg_ttl=4736149 #29 keys in total, 24 keys with expire
#Add one more key to make memory overflow
127.0.0.1:6379> set key1 "a large key value" ex 5000
OK
#Check memory usage and the number of keys
127.0.0.1:6379> info memory
used_memory:869960
used_memory_human:849.57K  #memory usage decreased
127.0.0.1:6379> info keyspace
db0:keys=27,expires=22,avg_ttl=4743197  #number of keys descreased, too
#Check the number of evicted keys
127.0.0.1:6379> info stats
evicted_keys:2
```

* [sidenote] Only keys with expire can be discarded when `maxmemory-policy` is set to `volatile-lru`. If you want all keys to be the target of collection, use `allkeys-lru` instead.

#### (4) Tuning

* You can set `maxmemory-samples` to a larger value (default is 5) to fine-tune LRU algorithm.

#### (5) Persist the settings to redis.conf

You can save your maxmemory related settings to redis.confi using `CONFIG REWRITE` command.

```bash
127.0.0.1:6379> config rewrite
```

(6) Using AWS Elasticache

Elasticache has a few points different from original Redis.

* Cannot use `maxmemory` (which is fixed by AWS), use AWS original `reserved-memory` (or `reserved-memory-percent`) instead.
* When `used_memory` > `maxmemory` - `reserved-memory`, eviction occurs.
* The default value of `reserved-memory` is 0 (not set), `reserved-memory-percent` is 25%. This is a recommended values by AWS.

* see [ElastiCache for Redis 運用小話 〜メドレー・TechLunch〜](https://developer.medley.jp/entry/2018/05/25/120000) for more detail.
* see [予約メモリの管理@AWS](https://docs.aws.amazon.com/ja_jp/AmazonElastiCache/latest/red-ug/redis-memory-management.html) for more detail about memory.

### 4.2 How to use Redis as session store

%%%

### 4.3 Operations

#### (0) Operations Basics

* [【入門】Redis](https://qiita.com/wind-up-bird/items/f2d41d08e86789322c71)
* [ElastiCache for Redis 運用小話 〜メドレー・TechLunch〜](https://developer.medley.jp/entry/2018/05/25/120000)
* [Redisの監視 ~ mackerel-plugin-redisを読み解く](https://soudai.hatenablog.com/entry/mackerel-plugin-redis)
* [システム開発で得たRedis利用ノウハウ](https://future-architect.github.io/articles/20190821/)
* [https://blog.yuuk.io/entry/redis-cpu-load](https://blog.yuuk.io/entry/redis-cpu-load)
* [いまさらGoでElasticache Redisの負荷・障害試験をがっつりしてみた](https://blog.applibot.co.jp/2020/02/13/redis-go-loadtest/)
* [Elasticache でのログ記録とモニタリング@AWS](https://docs.aws.amazon.com/ja_jp/AmazonElastiCache/latest/red-ug/MonitoringECMetrics.html)

#### (1) INFO command

```bash
#INFO command retrieves operatinal info and stats about Redis server
127.0.0.1:6379> info
#CPU
used_cpu_sys:145.841834
used_cpu_user:35.833458
:
: (snip)
:
#You can use INFO command with section specified
127.0.0.1:6379> info cpu
# CPU
used_cpu_sys:147.218890
used_cpu_user:36.197557
used_cpu_sys_children:0.057468
used_cpu_user_children:0.011944
```

#### (2) MEMORY commands

* `MEMORY STATS`
* `MEMORY USAGE`
* `MEMORY DOCTOR`

#### (3) redis-rdb-tools

* [redis-rdb-tools@GitHub](https://github.com/sripathikrishnan/redis-rdb-tools)

### 4.4 Do's and DONT's

* DO NOT use `keys` command (it's slow and heavy)
* DO NOT use multiple databases (it's deprecated)

---
%%%

* Master/Replica configuration
* 
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace misc
{
    public class Program
    {
        private static async Task runRedisBasic()
        {
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6380"))
            {
                IDatabase cache = redis.GetDatabase();

                await cache.StringSetAsync("/api/login", "logged in");
                await cache.StringSetAsync("/api/healthcheck", "ok");

                Console.WriteLine(await cache.StringGetAsync("/api/login"));
                Console.WriteLine(await cache.StringGetAsync("/api/healthcheck"));
            }
        }

        private static void runRedisPerf(bool async)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6380,allowAdmin=true");

            // データベースを取得
            IDatabase cache = redis.GetDatabase();

            var server = redis.GetServer("localhost", 6380);
            server.FlushAllDatabases();

            var range = Enumerable.Range(1, 10000);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (var index in range)
            {
                if (async)
                {
                    cache.StringSetAsync($"number:{index}", $"result:{index}");
                }
                else
                {
                    cache.StringSet($"number:{index}", $"result:{index}");
                }
            }

            if (async)
            {
                cache.WaitAll();
            }

            sw.Stop();

            var usedMemory = server.Info().Where(group => group.Key.Equals("Memory"))
            .First()
            .Where(c => c.Key.Equals("used_memory"))
            .First();
            Console.WriteLine($"used memory: {usedMemory.Key}:{usedMemory.Value} / elapsed: {sw.ElapsedMilliseconds} ms / Keys: {server.Keys().Count()}");
        }

        public static async Task Main(string[] args)
        {
            // CreateHostBuilder(args).Build().Run();

            // await runRedisBasic();
            runRedisPerf(false);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

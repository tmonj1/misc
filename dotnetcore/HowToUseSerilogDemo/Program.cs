using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace SerilogDemo
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        public static int Main(string[] args)
        {
            //Console.WriteLine(Environment.GetEnvironmentVariable("SEQ_URL"));

            // LOG_FORMATTER, LOG_MIN_LEVEL, LOG_MIN_LEVEL_MICROSOFT, LOG_MIN_LEVEL_SYSTEM, LOG_SEQ_URL
            var logFormatter = Environment.GetEnvironmentVariable("LOG_FORMATTER");
            var logMinLevel = Environment.GetEnvironmentVariable("LOG_MIN_LEVEL");
            var logMinLevelMicrosoft = Environment.GetEnvironmentVariable("LOG_MIN_LEVEL_MICROSOFT");
            var logMinLevelSystem = Environment.GetEnvironmentVariable("LOG_MIN_LEVEL_SYSTEM");
            var logSeqUrl = Environment.GetEnvironmentVariable("LOG_SEQ_URL");

            // emit log to console
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ConfiguredTo(logMinLevel)
                .MinimumLevel.ConfiguredTo("Microsoft", logMinLevelMicrosoft)
                .MinimumLevel.ConfiguredTo("System", logMinLevelSystem)
                .Enrich.WithProperty("AppName", AppAssemblyInfo.AppName)
                .Enrich.WithProperty("Version", AppAssemblyInfo.AppVersion)
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                //.WriteTo.Console(new CompactJsonFormatter())
                .WriteTo.ConsoleWithFormatter(logFormatter)
                .CreateLogger();

            try
            {
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
                  .UseSerilog()
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.UseStartup<Startup>();
                  });
    }
}

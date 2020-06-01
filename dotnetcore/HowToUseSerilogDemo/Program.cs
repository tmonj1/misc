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
            try
            {
                // setup logger
                SetupLogger();

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

        /// <summary>
        /// Setup console logger using Serilog.
        /// </summary>
        private static void SetupLogger()
        {
            // only supports LOG_[FORMATTER,MIN_LEVEL,MIN_LEVEL_MICROSOFT,MIN_LEVEL_SYSTEM]
            Func<string, string> env = (key) => Environment.GetEnvironmentVariable(key);

            // setup console logger only
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ConfiguredTo(env("LOG_IMN_LEVEL"))
                .MinimumLevel.ConfiguredTo("Microsoft", env("LOG_MIN_LEVEL_MICROSOFT"))
                .MinimumLevel.ConfiguredTo("System", env("LOG_MIN_LEVEL_SYSTEM"))
                .Enrich.WithProperty("AppName", AppAssemblyInfo.AppName)
                .Enrich.WithProperty("Version", AppAssemblyInfo.AppVersion)
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .WriteTo.ConsoleWithFormatter(env("LOG_FORMATTER"))
                .CreateLogger();
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

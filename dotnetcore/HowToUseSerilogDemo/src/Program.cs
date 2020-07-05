using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SerilogDemo.Logger;

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
                var versionInfo = Assembly.GetEntryAssembly().GetApplicationVersionInfo();
                Log.Logger = SerilogConfiguration.ConfigureDefault(loggerConfig => loggerConfig
                    .Enrich.WithProperty("AppName", versionInfo.Name)
                    .Enrich.WithProperty("Semver", versionInfo.SemanticVersion)
#if DEBUG
                    .WriteTo.SeqWithUrl(
                        Environment.GetEnvironmentVariable("LOG_SEQ_URL"),
                        Environment.GetEnvironmentVariable("LOG_SEQ_APIKEY")) // "9mjWXUIYPanPokRLE4B6"
#endif // DEBUG
                ).CreateLogger();

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

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
        CreateHostBuilder(args).Build().Run();
        return 0;
      }
      catch (Exception ex)
      {
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
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                  loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration);
                  //.Enrich.WithProperty("AppName", "demop2");
                  //.Enrich.WithMachineName();
                  //.Enrich.WithEnvironmentUserName();
                  //.Enrich.FromLogContext();
                });
  }
}

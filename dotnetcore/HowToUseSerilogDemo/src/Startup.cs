using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;

namespace SerilogDemo
{
  public interface ILogTest
  {
    public void Log();
  };
  public class LogTest : ILogTest
  {
    private readonly ILogger<LogTest> _logger;
    public LogTest(ILogger<LogTest> logger)
    {
      _logger = logger;
    }

    public void Log()
    {
      _logger.LogInformation("this is a LogTest message.");
    }
  }
  public class Startup
  {
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton(typeof(ILogTest), typeof(LogTest));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
        ILogTest logTest)
    {
      logTest.Log();

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseSerilogRequestLogging();

      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapGet("/", async context =>
              {
                Log.Information("/ called.");
                await context.Response.WriteAsync("Hello World!");
              });
      });
    }
  }
}

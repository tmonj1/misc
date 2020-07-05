using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogDemo.Logger;

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

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging(
              options =>
              {
                  // X-Request-Idヘッダを追加
                  options.EnrichDiagnosticContext =
                    RequestLoggingEnricher.EnrichWithCustomHeaders("X-Request-Id");
              }
            );

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/x", async context =>
                {
                    Log.Information("/x called.");
                    await context.Response.WriteAsync("Hello World!");
                });

                endpoints.MapGet("/index.html", async context =>
                {
                    await context.Response.WriteAsync(@"
                      <html>
                      <head><title>test</title>
                      <script>                                                                                               
                      function sendReq() {                                                                                   
                        var start = performance.now();
                        var data =
                          '{""@t"":""' + (new Date).toISOString() + '"",' +
                          '""@mt"":""Client Call"",' +
                          '""X-Request-Id"":""slkdjfsdl""' +
                          '}';
                        var r = new XMLHttpRequest();                                                                     
                        r.open('post', 'http://localhost:5341/api/events/raw?clef');
                        r.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');                          
                        r.send(data);
                        r.addEventListener('load', function(){
                          console.log(this.response);
                          var elapsed = (performance.now() - start).toString();
                          var data =
                            '{""@t"":""' + (new Date).toISOString() + '"",' +
                            '""@mt"":""Client Call"",' +
                            '""X-Request-Id"":""slkdjfsdl"",' +
                            '""Elapsed"":""' + elapsed + '""}';
                          var r2 = new XMLHttpRequest();                                                                     
                          r2.open('post', 'http://localhost:5341/api/events/raw?clef');
                          r2.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');                          
                          r2.send(data);
                        }, false); 
                      }
                      </script>
                      <body>
                        test                                                                                                
                        <button onclick='sendReq()'>test</button>   
                      </body>
                      </html>
                    ");
                });
            });
        }
    }
}

using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace demo1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.MapGet("/envs", async context =>
                {
                    var envDic = Environment.GetEnvironmentVariables();
                    string envs = string.Empty;
                    foreach (DictionaryEntry e in envDic)
                    {
                        envs += $"{e.Key}:{e.Value}{Environment.NewLine}";
                    }
                    await context.Response.WriteAsync(envs);
                });
                endpoints.MapGet("/headers", async context =>
                {
                    var headerDic = context.Request.Headers;
                    string headers = string.Empty;
                    foreach (var h in headerDic)
                    {
                        headers += $"{h.Key}:{h.Value}{Environment.NewLine}";
                    }
                    await context.Response.WriteAsync(headers);
                });
                endpoints.MapGet("/echo", async context =>
                {
                    await context.Response.WriteAsync(context.Request.QueryString.ToString());
                });
                endpoints.MapGet("/h", async context =>
                {
                    await context.Response.WriteAsync("ok");
                });
                endpoints.MapGet("/version", async context =>
                {
                    await context.Response.WriteAsync("v0.1.3");
                });
            });
        }
    }
}

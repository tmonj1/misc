using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using app2.Models;

namespace app2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;

            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet("/call1")]
        public async Task<JsonResult> call1Async()
        {
            // ローカル実行のときはこっち
            // http://app1:80/ を呼び出す
            // var client = _httpClientFactory.CreateClient("xray");
            // var result = await client.GetStringAsync("http://app1/");

            // EKS のときはこっち
            var client = new HttpClient();
            var result = await client.GetStringAsync("http://app1-svc:2080/");

            return Json(new
            {
                name = "call1",
                target = "app1",
                result = result
            });
        }

        [HttpGet("/health")]
        public ContentResult health()
        {
            return Content("ok");
        }
    }
}

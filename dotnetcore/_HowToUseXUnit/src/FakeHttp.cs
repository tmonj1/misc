using System.Threading.Tasks;

namespace HowToUseXUnit
{
    public interface IHttpClient
    {
        public string GetString(string url);
        public Task<string> GetStringAsync(string url);
    }

    public class MyHttpClient : IHttpClient
    {
        public string GetString(string url)
        {
            return url;
        }

        public async Task<string> GetStringAsync(string url)
        {
            return await Task.Run(() => url);
        }
    }
    public class FakeHttp
    {
        private IHttpClient _httpClient = null;

        public FakeHttp(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetClassName()
        {
            return typeof(FakeHttp).Name;
        }
        public string GetString(string url)
        {
            return _httpClient.GetString(url);
        }

        public async Task<string> GetStringAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        public int xxx()
        {
            return 3;
        }
    }
}
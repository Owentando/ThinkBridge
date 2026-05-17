using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ThinkBridge.Services
{
    public class WebResourceSearchService
    {
        private readonly string _apiKey = System.Configuration.ConfigurationManager.AppSettings["GoogleSearchApiKey"];
        private readonly string _cx =  System.Configuration.ConfigurationManager.AppSettings["GoogleSearchCx"];
        public async Task<List<WebResourceResult>> SearchAsync(string subject, string topic)
        {
            var results = new List<WebResourceResult>();

            if (string.IsNullOrWhiteSpace(subject) && string.IsNullOrWhiteSpace(topic))
                return results;

            if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_cx))
                return results;

            var query = (subject + " " + topic + " study notes tutorial article pdf")
                .Trim();

            var url =
    "https://customsearch.googleapis.com/customsearch/v1" +
    "?key=" + HttpUtility.UrlEncode(_apiKey) +
    "&cx=" + HttpUtility.UrlEncode(_cx) +
    "&q=" + HttpUtility.UrlEncode(query) +
    "&num=10";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine(json);

                if (!response.IsSuccessStatusCode)
                {
                    throw new System.Exception(json);
                }
                var root = JObject.Parse(json);
                var items = root["items"];

                if (items == null)
                    return results;

                foreach (var item in items)
                {
                    results.Add(new WebResourceResult
                    {
                        Title = item["title"]?.ToString(),
                        Link = item["link"]?.ToString(),
                        Snippet = item["snippet"]?.ToString(),
                        Source = item["displayLink"]?.ToString()
                    });
                }
            }

            return results;
        }
    }

    public class WebResourceResult
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Snippet { get; set; }
        public string Source { get; set; }
    }
}
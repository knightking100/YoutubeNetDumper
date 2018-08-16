using System.Net.Http;

namespace YoutubeNetDumper
{
    public class YoutubeExperimentalConfig
    {
        public string Geolocation { get; set; } = "US";
        public string Language { get; set; } = "en";
        public bool MeasureTime { get; set; } = true;
        public bool UseChromeUserAgent { get; set; } = false;
        public string UserAgent { get; set; }
        public HttpClient HttpClient { get; set; }
    }
}
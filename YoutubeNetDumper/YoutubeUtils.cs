using System;
using System.Web;

namespace YoutubeNetDumper
{
    public static class YoutubeUtils
    {
        public static string ParseVideoId(string url)
        {
            var uri = new UriBuilder(url).Uri;
            switch (uri.Host.ToLowerInvariant())
            {
                case "www.youtube.com":
                case "youtube.com":
                    if (uri.Segments[1].Trim('/') == "embed") return uri.Segments[2];
                    return HttpUtility.ParseQueryString(uri.Query).Get("v") ?? throw new FormatException("URL is not a YouTube video url");

                case "www.youtu.be":
                case "youtu.be":
                    return uri.AbsolutePath.Trim('/');

                default:
                    if (url.Length == 11)
                        return url;
                    throw new FormatException("URL is not a YouTube video url");
            }
        }
    }
}
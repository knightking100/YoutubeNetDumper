using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Formatting;
using System.Threading.Tasks;
using System.Web;

namespace YoutubeNetDumper
{
    /// <summary>
    /// Dumper for mobile version
    /// </summary>
    public class YoutubeMobileDumper : IYoutubeDumper
    {
        /// <summary>
        /// The location to search for videos
        /// </summary>
        public string Geolocation { get; set; } = "US";

        /// <summary>
        /// The language of result
        /// </summary>
        public string Language { get; set; } = "en";

        private readonly HttpClient _client;
        private readonly StringFormatter sb;

        private string PlayerSource { get; set; }
        private IReadOnlyList<UnscramblingInstruction> Instructions { get; set; }
        private readonly Stopwatch sw_parsing;
        private readonly Stopwatch _sw;

        public YoutubeMobileDumper()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 11_3 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A356 Safari/604.1");
            //_client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows Phone 8.0; Trident/6.0; IEMobile/10.0; ARM; Touch; NOKIA; Lumia 920)");
            sb = new StringFormatter();
            sw_parsing = new Stopwatch();
            _sw = new Stopwatch();
        }

        //TODO: Handle age-restricted and country-restricted videos
        public async Task<DumpResult> DumpAsync(string videoId)
        {
            sw_parsing.Reset();
            sw_parsing.Reset();

            _sw.Start();
            var content = await _client.GetStringAsync($"https://www.youtube.com/watch?v={videoId}");

            //Extract json
            var searchTerm = "ytInitialPlayerConfig =";
            var pos = content.LastIndexOf(searchTerm);

            for (int i = pos + searchTerm.Length; i < content.Length; i++)
            {
                if (content[i] == ';' && content[i - 1] == '}') break;
                sb.Append(content[i]);
            }

            var json = sb.ToString().Trim();
            sb.Clear();
            var obj = JObject.Parse(json);
            //var config = GenerateConfig(json); //This doesn't work somehow
            var views = (string)obj["args"]["view_count"];
            var video = new YoutubeVideo
            {
                Title = (string)obj["args"]["title"],
                Author = (string)obj["args"]["author"],
                Views = int.TryParse(views, out var v) ? (int?)v : null,
            };
            var player_url = "https://youtube.com" + (string)obj["assets"]["js"];

            var data =
                obj["args"]["url_encoded_fmt_stream_map"].Value<string>().Split(',');
            video.MediaStreams = await ParseMediaStreamsAsync(data, player_url);

            sw_parsing.Stop();
            _sw.Stop();

            return new DumpResult
            {
                ElapsedTime = _sw.Elapsed,
                ElapsedParsingTime = sw_parsing.Elapsed,
                Video = video
            };
        }

        /// <summary>
        /// Parse raw media stream
        /// </summary>
        /// <param name="raw">Raw data</param>
        /// <param name="assign"></param>
        private async Task<IReadOnlyList<YoutubeMediaStream>> ParseMediaStreamsAsync(string[] fmts, string player_url)
        {
            var media_streams = new YoutubeMediaStream[fmts.Length];

            for (int i = 0; i < fmts.Length; i++)
            {
                var fmt = fmts[i];
                var dict = SplitQuery(fmt);
                var url = dict.GetValueOrDefault("url");
                var sig = dict.GetValueOrDefault("s");

                //Decode
                if (!string.IsNullOrEmpty(sig))
                {
                    if (PlayerSource == null)
                    {
                        sw_parsing.Stop();
                        PlayerSource = await _client.GetStringAsync(player_url);
                        sw_parsing.Start();
                    }

                    if (Instructions == null)
                    {
                        var function = GetDecryptFunctionName(PlayerSource);
                        Instructions = GetDecryptInstructions(function, PlayerSource);
                    }

                    sig = SignatureDecoder.Decode(Instructions, sig);
                    url += $"&signature={sig}";
                }

                var p = dict.GetValueOrDefault("type").Split(';', '/', '=');
                var isMixed = dict.TryGetValue("quality", out var quality);
                var type = p[0] == "audio" ? MediaStreamType.Audio :
                     isMixed ? MediaStreamType.Mixed : MediaStreamType.Video;

                quality = quality ?? dict.GetValueOrDefault("quality_label");
                var size = dict.GetValueOrDefault("size");

                media_streams[i] = new YoutubeMediaStream
                {
                    Url = url,
                    Bitrate = dict.GetNullableIntValue("bitrate"),
                    Format = p[1],
                    Framerate = dict.GetNullableIntValue("fps"),
                    Itag = int.Parse(dict.GetValueOrDefault("itag")),
                    Quality = quality,
                    Type = type,
                    Codecs = p[p.Length - 1].Trim('"')
                };
            }
            return media_streams;
        }

        private static Dictionary<string, string> SplitQuery(string query)
        {
            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var rawParams = query.Split("&");
            for (int i = 0; i < rawParams.Length; i++)
            {
                var rawParam = rawParams[i];
                var param = HttpUtility.UrlDecode(rawParam);
                // Look for the equals sign
                var equalsPos = param.IndexOf('=');
                if (equalsPos <= 0)
                    continue;

                // Get the key and value
                var key = param.Substring(0, equalsPos);
                var value = equalsPos < param.Length
                    ? param.Substring(equalsPos + 1)
                    : string.Empty;

                // Add to dictionary
                dic[key] = value;
            }

            return dic;
        }

        private string GetDecryptFunctionName(string playerSource)
        {
            var searchTerm = "\"signature\",";
            var pos = playerSource.IndexOf(searchTerm);

            if (pos < 0) throw new Exception("Could not find the function for decoding");

            string result = "";
            for (int i = pos + searchTerm.Length; i < playerSource.Length; i++)
            {
                if (playerSource[i] == '(') break;
                result += playerSource[i];
            }

            return result.Trim();
        }

        private IReadOnlyList<UnscramblingInstruction> GetDecryptInstructions(string name, string playerSource)
        {
            sb.Clear(); //Just want to make sure
            var instructions = new List<UnscramblingInstruction>();
            var instrustions_map = new Dictionary<string, string>();
            //TODO: improve the parameter name (using regex maybe)
            var searchTerm = $"{name}=function(a){{";

            var pos = playerSource.IndexOf(searchTerm);

            for (int i = pos + searchTerm.Length; i < playerSource.Length; i++)
            {
                var c = playerSource[i];
                if (c == '}') break;

                if (c == ';')
                {
                    var line = sb.ToString();
                    sb.Clear();
                    if (line.Contains("split") || line.Contains("return")) continue;
                    //Example: AL.iy(a,9)
                    //Result:
                    // AL, iy, a, 9
                    var parts = line.Split('.', ',', '(', ')');
                    var func = parts[1];
                    var index = parts[parts.Length - 2];
                    //We haven't found the funtion
                    if (!instrustions_map.TryGetValue(func, out var value))
                    {
                        if (playerSource.Contains($"{func}:function(a,b){{var")) value = "swap";
                        else if (playerSource.Contains($"{func}:function(a){{")) value = "reverse";
                        else if (playerSource.Contains($"{func}:function(a,b){{")) value = "slice";
                        else throw new Exception($"Decoding instruction not found for function {func}");
                        instrustions_map[func] = value;
                    }
                    instructions.Add(new UnscramblingInstruction { Name = value, Index = int.Parse(index) });
                }
                else sb.Append(c);
            }
            return instructions;
        }

        private static YoutubeConfig GenerateConfig(string json)
        {
            var reader = new JsonTextReader(new System.IO.StringReader(json));
            string playerUrl = null;
            string raw_adaptive_streams = null;
            string title = null;
            string author = null;
            string raw_mixed_streams = null;
            string thumbnail_url = null;

            while (reader.Read())
            {
                if (playerUrl != null && raw_adaptive_streams != null
                    && title != null && author != null && raw_adaptive_streams != null
                    && raw_mixed_streams != null && thumbnail_url != null)
                    break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value as string;
                    switch (propertyName)
                    {
                        case "title":
                            title = reader.ReadAsString();
                            break;

                        case "author":
                            author = reader.ReadAsString();
                            break;

                        case "js":
                            playerUrl = reader.ReadAsString();
                            break;

                        case "adaptive_fmts":
                            raw_adaptive_streams = reader.ReadAsString();
                            break;

                        case "url_encoded_fmt_stream_map":
                            raw_mixed_streams = reader.ReadAsString();
                            break;

                        case "thumbnail_url":
                            thumbnail_url = reader.ReadAsString();
                            break;
                    }
                }
            }

            return new YoutubeConfig
            {
                PlayerUrl = playerUrl,
                Author = author,
                Title = title,
                ThumbnailUrl = thumbnail_url,
                RawAdaptiveStreams = raw_adaptive_streams,
                RawMixedStream = raw_mixed_streams
            };
        }

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
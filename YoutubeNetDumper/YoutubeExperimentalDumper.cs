using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Formatting;
using System.Text.JsonLab;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace YoutubeNetDumper
{
    public sealed class YoutubeExperimentalDumper : IYoutubeDumper
    {
        private readonly HttpClient _client;
        private readonly StringFormatter sb;

        private string PlayerSource { get; set; }
        private IReadOnlyList<UnscramblingInstruction> Instructions { get; set; }
        private readonly Stopwatch sw_parsing;
        private readonly Stopwatch sw;
        private readonly ArrayPool<byte> _pool;
        private readonly YoutubeExperimentalConfig _config;

        /// <summary>
        /// Create a new instance of <see cref="YoutubeDumper"/>
        /// </summary>
        public YoutubeExperimentalDumper(YoutubeExperimentalConfig config)
        {
            _config = config;
            _pool = ArrayPool<byte>.Shared;
            _client = config.HttpClient ?? new HttpClient();
            //Chrome User-Agent
            if (config.UseChromeUserAgent)
                _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");

            if (!string.IsNullOrWhiteSpace(config.UserAgent))
                _client.DefaultRequestHeaders.Add("User-Agent", config.UserAgent);
            sb = new StringFormatter();
            sw_parsing = config.MeasureTime ? new Stopwatch() : null;
            sw = config.MeasureTime ? new Stopwatch() : null;
        }

        public YoutubeExperimentalDumper() : this(new YoutubeExperimentalConfig())
        {
        }
        
        //TODO: Handle age-restricted and country-restricted videos
        public async Task<DumpResult> DumpAsync(string videoId)
        {
            sw?.Reset();
            sw_parsing?.Reset();

            sw?.Start();
            var buffer = _pool.Rent(60000);
            var content = await GetJsonAsync($"https://www.youtube.com/watch?v={videoId}" +
                $"&gl={_config.Geolocation}" +
                $"&hl={_config.Language}" + //Set the language
                $"&has_verified=1" +
                $"&bpctr=9999999999", buffer); //For videos that 'may be inappropriate or offensive for some users'
            
            if (content.IsEmpty)
                return new DumpResult() { Successful = false };
            sw_parsing?.Start();

            var config = ParseConfig(content);
            _pool.Return(buffer);
            var video = config.Video;

            var player_url = "https://www.youtube.com" + config.PlayerUrl;

            bool addComma = false;
            if (!string.IsNullOrEmpty(config.RawAdaptiveStreams))
            {
                sb.Append(config.RawAdaptiveStreams);
                addComma = true;
            }
            if (!string.IsNullOrEmpty(config.RawMixedStreams))
            {
                if (addComma) sb.Append(',');
                sb.Append(config.RawMixedStreams);
            }
            video.MediaStreams = await ParseMediaStreamsAsync(sb.ToString().Split(','), player_url);

            sw_parsing?.Stop();
            sw?.Stop();

            return new DumpResult
            {
                ElapsedTime = sw?.Elapsed,
                ElapsedParsingTime = sw_parsing?.Elapsed,
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
                        sw_parsing?.Stop();
                        PlayerSource = await _client.GetStringAsync(player_url);
                        sw_parsing?.Start();
                    }

                    if (Instructions == null)
                    {
                        var function = GetDecryptFunctionName(PlayerSource);
                        Instructions = GetDecryptInstructions(function, PlayerSource);
                    }

                    sig = SignatureDecoder.Decode(Instructions, sig);
                    url += $"&signature={sig}";
                }

                var p = dict.GetValueOrDefault("type")?.Split(';', '/', '=');
                var isMixed = dict.TryGetValue("quality", out var quality);
                var type = p[0] == "audio" ? MediaStreamType.Audio :
                     isMixed ? MediaStreamType.Mixed : MediaStreamType.Video;

                quality = quality ?? dict.GetValueOrDefault("quality_label");
                var size = dict.GetValueOrDefault("size");
                var itag = int.Parse(dict.GetValueOrDefault("itag"));
                var attributes = ItagMap.Get(itag);
                
                if (attributes.FPS == null)
                    attributes.FPS = dict.GetNullableIntValue("fps");

                media_streams[i] = new YoutubeMediaStream
                {
                    Url = url,
                    Bitrate = dict.GetNullableIntValue("bitrate"),
                    Itag = itag,
                    Quality = quality,
                    Type = type,
                    Attributes = attributes
                };
            }
            return media_streams;
        }

        private static (int Width, int Height) GetSize(string size)
        {
            var p = size.Split('x');
            return (int.Parse(p[0]), int.Parse(p[1]));
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
        static string[] _props = { "js" , "adaptive_fmts" , "url_encoded_fmt_stream_map" ,
        "video_id", "title", "author", "thumbnail_url", "live_content", "length_seconds", "avg_rating",
        "keywords", "view_count"};
        private static YoutubeConfig ParseConfig(ReadOnlyMemory<byte> json)
        {
            var reader = new Utf8JsonReader(json.Span);
            string playerUrl = null;
            string raw_adaptive_streams = null;
            string raw_mixed_streams = null;
            var video = new YoutubeVideo();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = Encodings.Utf8.ToString(reader.Value);
                    reader.Read();
                    if (_props.Contains(name))
                    {
                        var value = Regex.Unescape(Encodings.Utf8.ToString(reader.Value));
                        switch (name)
                        {
                            case "js": playerUrl = value; break;
                            case "adaptive_fmts": raw_adaptive_streams = value; break;
                            case "url_encoded_fmt_stream_map": raw_mixed_streams = value; break;
                            //Video info
                            case "video_id": video.Id = value; break;
                            case "title": video.Title = value; break;
                            case "author": video.Author = value; break;
                            case "thumbnail_url": video.ThumbnailUrl = value; break;
                            case "live_content": video.IsLiveStream = true; break;
                            case "length_seconds": video.Duration = TimeSpan.FromSeconds(double.Parse(value)); break;
                            case "avg_rating": video.AverageRating = double.Parse(value); break;
                            case "keywords": video.Keywords = value; break;
                            case "view_count": video.Views = long.Parse(value); break;
                        }
                    }
                    
                }
            }

            return new YoutubeConfig
            {
                PlayerUrl = playerUrl,
                RawAdaptiveStreams = raw_adaptive_streams,
                RawMixedStreams = raw_mixed_streams,
                Video = video
            };
        }

        static readonly byte[] end_pattern = Encoding.UTF8.GetBytes("tplayer.load");
        static readonly byte[] start_pattern = Encoding.UTF8.GetBytes("tplayer.config");

        private async Task<ReadOnlyMemory<byte>> GetJsonAsync(string url, byte[] buffer)
        {
            var memory = new Memory<byte>(buffer);
            int current_index = 0;

            bool found = false;
            bool startJson = false;

            using (var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var chars = _pool.Rent(start_pattern.Length + 1);
                var buff = new byte[1];
                while (stream.CanRead)
                {
                    int count = await stream.ReadAsync(new Memory<byte>(buff));
                    if (count == 0) break;
                    var value = buff[0];

                    if (value == ';')
                    {
                        await stream.ReadAsync(new Memory<byte>(buff));
                        if (buff[0] != 'y') continue;

                        await stream.ReadAsync(new Memory<byte>(chars));
                        found = found || chars.AsSpan(0, start_pattern.Length).SequenceEqual(start_pattern);
                        bool endOfJson = found && chars.AsSpan(0, end_pattern.Length).SequenceEqual(end_pattern);
                        if (endOfJson) break;
                    }

                    startJson = found && (startJson || value == '{');

                    if (startJson)
                    {
                        memory.Span[current_index] = value;
                        current_index++;
                    }
                }
                _pool.Return(chars);
            }

            return memory.Slice(0, current_index);
        }
        
    }
}
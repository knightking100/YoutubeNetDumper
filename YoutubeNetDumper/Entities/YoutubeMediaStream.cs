﻿using System.Collections.Generic;

namespace YoutubeNetDumper
{
    public class YoutubeMediaStream
    {
        public int Itag { get; internal set; }
        public string Url { get; internal set; }
        public string Quality { get; internal set; }//TODO: make this an enum
        public int? Bitrate { get; internal set; }
        public int? Framerate { get; internal set; }
        public string Format { get; internal set; }//TODO: make this an enum
        public MediaStreamType Type { get; internal set; }
        public string Codecs { get; internal set; }
        public string Size { get; internal set; }
    }

    public enum MediaStreamType
    {
        Video,
        Audio,
        Mixed
    }

    internal static class Extensions
    {
        public static int? GetNullableIntValue(this Dictionary<string, string> dict, string key)
        {
            var value = dict.GetValueOrDefault(key);
            if (!int.TryParse(value, out var s)) return null;
            return s;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeNetDumper
{
    public enum AudioCodec
    {
        Mp3,
        Aac,
        Vorbis,
        Opus
    }

    public enum VideoCodec
    {
        H263,
        H264,
        MP4V,
        VP8,
        VP9
    }

    public struct MediaStreamAttributes
    {
        public string Format { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public AudioCodec? AudioCodec { get; internal set; }
        public VideoCodec? VideoCodec { get; internal set; }
        public bool Is3D { get; internal set; }
        
    }
    internal static class ItagMap
    {
        //Thanks to https://github.com/rg3/youtube-dl/blob/master/youtube_dl/extractor/youtube.py#L380-L476
        private static readonly Dictionary<int, MediaStreamAttributes> _knownItags = new Dictionary<int, MediaStreamAttributes>
        {
            [5] = new MediaStreamAttributes(),
            [6] = new MediaStreamAttributes()
        };
    }
}

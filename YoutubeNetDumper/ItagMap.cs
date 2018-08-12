using System.Collections.Generic;

namespace YoutubeNetDumper
{
    public enum AudioCodec
    {
        Mp3,
        Aac,
        Vorbis,
        Opus,
        Dtse,
        Ec3,
		None
    }

    public enum VideoCodec
    {
        H263,
        H264,
        MP4V,
        VP8,
        VP9,
		None
    }

    public struct MediaStreamAttributes
    {
        public string Format { get; }
        public int? Width { get; internal set; }
        public int? Height { get; internal set; }
        public AudioCodec? AudioCodec { get; }
        public VideoCodec? VideoCodec { get; }
        public bool Is3D { get; }
		public int? FPS { get; internal set; }

        internal MediaStreamAttributes(string format, int? width, int? height, AudioCodec? aCodec,
            VideoCodec? vCodec, int? fps = null, bool is3D = false)
        {
            Format = format;
            Width = width;
            Height = height;
            AudioCodec = aCodec;
            VideoCodec = vCodec;
			FPS = fps;
            Is3D = is3D;
        }

        public string GetSize() => $"{Width}x{Height}";
        
    }
    internal static class ItagMap
    {
        //Code from https://github.com/rg3/youtube-dl/blob/master/youtube_dl/extractor/youtube.py#L380-L476
        private static readonly Dictionary<int, MediaStreamAttributes> _knownItags = new Dictionary<int, MediaStreamAttributes>
        {
            [5] = new MediaStreamAttributes("flv", 400, 240, AudioCodec.Mp3, VideoCodec.H263),
            [6] = new MediaStreamAttributes("flv", 450, 270, AudioCodec.Mp3, VideoCodec.H263),
            [13] = new MediaStreamAttributes("3gp", null, null, AudioCodec.Aac, VideoCodec.MP4V),
            [17] = new MediaStreamAttributes("3gp", 176, 144, AudioCodec.Aac, VideoCodec.MP4V),
            [18] = new MediaStreamAttributes("mp4", 640, 360, AudioCodec.Aac, VideoCodec.H264),
            [22] = new MediaStreamAttributes("mp4", 1280, 720, AudioCodec.Aac, VideoCodec.H264),
            [34] = new MediaStreamAttributes("flv", 640, 360, AudioCodec.Aac, VideoCodec.H264),
            [35] = new MediaStreamAttributes("flv", 854, 480, AudioCodec.Aac, VideoCodec.H264),
            // itag 36 videos are either 320x180 (BaW_jenozKc) or 320x240 (__2ABJjxzNo), abr varies as well
            [36] = new MediaStreamAttributes("3gp", 320, null, AudioCodec.Aac, VideoCodec.MP4V),
            [37] = new MediaStreamAttributes("mp4", 1920, 1080, AudioCodec.Aac, VideoCodec.H264),
            [38] = new MediaStreamAttributes("mp4", 4096, 3072, AudioCodec.Aac, VideoCodec.H264),
            [43] = new MediaStreamAttributes("webm", 640, 360, AudioCodec.Vorbis, VideoCodec.VP8),
            [44] = new MediaStreamAttributes("webm", 854, 480, AudioCodec.Vorbis, VideoCodec.VP8),
            [45] = new MediaStreamAttributes("webm", 1280, 720, AudioCodec.Vorbis, VideoCodec.VP8),
            [46] = new MediaStreamAttributes("webm", 1920, 1080, AudioCodec.Vorbis, VideoCodec.VP8),
            [59] = new MediaStreamAttributes("mp4", 854, 480, AudioCodec.Aac, VideoCodec.H264),
            [78] = new MediaStreamAttributes("mp4", 854, 480, AudioCodec.Aac, VideoCodec.H264),


            // 3D videos
            [82] = new MediaStreamAttributes("mp4", null, 360, AudioCodec.Aac, VideoCodec.H264, is3D: true),
            [83] = new MediaStreamAttributes("mp4", null, 480, AudioCodec.Aac, VideoCodec.H264, is3D: true),
            [84] = new MediaStreamAttributes("mp4", null, 720, AudioCodec.Aac, VideoCodec.H264, is3D: true),
            [85] = new MediaStreamAttributes("mp4", null, 1080, AudioCodec.Aac, VideoCodec.H264, is3D: true),
            [100] = new MediaStreamAttributes("webm", null, 360, AudioCodec.Vorbis, VideoCodec.VP8, is3D: true),
            [101] = new MediaStreamAttributes("webm", null, 480, AudioCodec.Vorbis, VideoCodec.VP8, is3D: true),
            [102] = new MediaStreamAttributes("webm", null, 720, AudioCodec.Vorbis, VideoCodec.VP8, is3D: true),

            // Apple HTTP Live Streaming
            [91] = new MediaStreamAttributes("mp4", null, 144, AudioCodec.Aac, VideoCodec.H264),
            [92] = new MediaStreamAttributes("mp4", null, 240, AudioCodec.Aac, VideoCodec.H264),
            [93] = new MediaStreamAttributes("mp4", null, 360, AudioCodec.Aac, VideoCodec.H264),
            [94] = new MediaStreamAttributes("mp4", null, 480, AudioCodec.Aac, VideoCodec.H264),
            [95] = new MediaStreamAttributes("mp4", null, 720, AudioCodec.Aac, VideoCodec.H264),
            [96] = new MediaStreamAttributes("mp4", null, 1080, AudioCodec.Aac, VideoCodec.H264),
            [132] = new MediaStreamAttributes("mp4", null, 240, AudioCodec.Aac, VideoCodec.H264),
            [151] = new MediaStreamAttributes("mp4", null, 72, AudioCodec.Aac, VideoCodec.H264),

            // DASH mp4 video
            [133] = new MediaStreamAttributes("mp4", null, 240, null, VideoCodec.H264),
            [134] = new MediaStreamAttributes("mp4", null, 360, null, VideoCodec.H264),
            [135] = new MediaStreamAttributes("mp4", null, 480, null, VideoCodec.H264),
            [136] = new MediaStreamAttributes("mp4", null, 720, null, VideoCodec.H264),
            [137] = new MediaStreamAttributes("mp4", null, 1080, null, VideoCodec.H264),
            [138] = new MediaStreamAttributes("mp4", null, null, null, VideoCodec.H264),
            [160] = new MediaStreamAttributes("mp4", null, 144, null, VideoCodec.H264),
            [212] = new MediaStreamAttributes("mp4", null, 480, null, VideoCodec.H264),
            [264] = new MediaStreamAttributes("mp4", null, 1440, null, VideoCodec.H264),
            [298] = new MediaStreamAttributes("mp4", null, 720, null, VideoCodec.H264, fps: 60),
            [299] = new MediaStreamAttributes("mp4", null, 1080, null, VideoCodec.H264, fps: 60),
            [266] = new MediaStreamAttributes("mp4", null, 2160, null, VideoCodec.H264),

            // Dash mp4 audio
            [139] = new MediaStreamAttributes("m4a", null, null, AudioCodec.Aac, null),
            [140] = new MediaStreamAttributes("m4a", null, null, AudioCodec.Aac, null),
            [141] = new MediaStreamAttributes("m4a", null, null, AudioCodec.Aac, null),
            [256] = new MediaStreamAttributes("m4a", null, null, AudioCodec.Aac, null),
            [258] = new MediaStreamAttributes("m4a", null, null, AudioCodec.Aac, null),
            [325] = new MediaStreamAttributes("m4a", null, null, AudioCodec.Dtse, null),
            [328] = new MediaStreamAttributes("m4a", null, null, AudioCodec.Ec3, null),

            // Dash webm
            [167] = new MediaStreamAttributes("webm", 640, 360, null, VideoCodec.VP8),
            [168] = new MediaStreamAttributes("webm", 854, 480, null, VideoCodec.VP8),
            [169] = new MediaStreamAttributes("webm", 1280, 720, null, VideoCodec.VP8),
            [170] = new MediaStreamAttributes("webm", 1920, 1080, null, VideoCodec.VP8),
            [218] = new MediaStreamAttributes("webm", 854, 480, null, VideoCodec.VP8),
            [219] = new MediaStreamAttributes("webm", 854, 480, null, VideoCodec.VP8),
            [278] = new MediaStreamAttributes("webm", null, 144, null, VideoCodec.VP9),
            [242] = new MediaStreamAttributes("webm", null, 240, null, VideoCodec.VP9),
            [243] = new MediaStreamAttributes("webm", null, 360, null, VideoCodec.VP9),
            [244] = new MediaStreamAttributes("webm", null, 480, null, VideoCodec.VP9),
            [245] = new MediaStreamAttributes("webm", null, 480, null, VideoCodec.VP9),
            [246] = new MediaStreamAttributes("webm", null, 480, null, VideoCodec.VP9),
            [247] = new MediaStreamAttributes("webm", null, 720, null, VideoCodec.VP9),
            [248] = new MediaStreamAttributes("webm", null, 1080, null, VideoCodec.VP9),
            [271] = new MediaStreamAttributes("webm", null, 1440, null, VideoCodec.VP9),
            // itag 272 videos are either 3840x2160 (e.g. RtoitU2A-3E) or 7680x4320 (sLprVF6d7Ug)
            [272] = new MediaStreamAttributes("webm", null, 2160, null, VideoCodec.VP9),
            [302] = new MediaStreamAttributes("webm", null, 720, null, VideoCodec.VP9, fps: 60),
            [303] = new MediaStreamAttributes("webm", null, 1080, null, VideoCodec.VP9, fps: 60),
            [308] = new MediaStreamAttributes("webm", null, 1440, null, VideoCodec.VP9, fps: 60),
            [313] = new MediaStreamAttributes("webm", null, 2160, null, VideoCodec.VP9),
            [315] = new MediaStreamAttributes("webm", null, 2160, null, VideoCodec.VP9, fps: 60),

            // Dash webm audio
            [171] = new MediaStreamAttributes("webm", null, null, AudioCodec.Vorbis, null),
            [172] = new MediaStreamAttributes("webm", null, null, AudioCodec.Vorbis, null),

            // Dash webm audio with opus inside
            [249] = new MediaStreamAttributes("webm", null, null, AudioCodec.Opus, null),
            [250] = new MediaStreamAttributes("webm", null, null, AudioCodec.Opus, null),
            [251] = new MediaStreamAttributes("webm", null, null, AudioCodec.Opus, null)
        };

        public static MediaStreamAttributes Get(int itag)
        {
            if (!_knownItags.TryGetValue(itag, out var attributes)) return new MediaStreamAttributes();
            return attributes;
        }
    }
}

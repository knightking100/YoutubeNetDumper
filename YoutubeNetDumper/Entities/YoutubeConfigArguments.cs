using System.Runtime.Serialization;

namespace YoutubeNetDumper
{
    //Not in use
    internal class YoutubeConfigArguments
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [DataMember(Name = "adaptive_fmts")]
        public string RawAdaptiveStreams { get; set; }

        [DataMember(Name = "url_encoded_fmt_stream_map")]
        public string RawMixedStream { get; set; }
    }
}
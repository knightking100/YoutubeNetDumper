namespace YoutubeNetDumper
{
    //Not in use
    internal class YoutubeConfig
    {
        //[DataMember(Name = "assets")]
        //public Dictionary<string, string> Assets { get; set; }

        //[IgnoreDataMember]
        //public string PlayerUrl => Assets["js"];

        //[DataMember(Name = "args")]
        //public YoutubeConfigArguments VideoData { get; set; }

        public string PlayerUrl { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string ThumbnailUrl { get; set; }
        public string RawAdaptiveStreams { get; set; }
        public string RawMixedStream { get; set; }
    }
}
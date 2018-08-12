namespace YoutubeNetDumper
{
    internal class YoutubeConfig
    {
        public string PlayerUrl { get; set; }
        public string RawAdaptiveStreams { get; set; }
        public string RawMixedStreams { get; set; }
        public YoutubeVideo Video { get; set; }
    }
}
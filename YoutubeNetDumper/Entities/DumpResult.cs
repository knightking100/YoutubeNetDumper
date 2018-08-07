using System;

namespace YoutubeNetDumper
{
    public class DumpResult
    {
        public YoutubeVideo Video { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan ElapsedParsingTime { get; set; }
    }
}
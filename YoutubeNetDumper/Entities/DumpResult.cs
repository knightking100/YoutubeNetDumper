using System;

namespace YoutubeNetDumper
{
    public class DumpResult
    {
        public bool Successful { get; internal set; } = true;
        public YoutubeVideo Video { get; internal set; }
        public TimeSpan? ElapsedTime { get; internal set; }
        public TimeSpan? ElapsedParsingTime { get; internal set; }

        internal DumpResult() { }
    }
}
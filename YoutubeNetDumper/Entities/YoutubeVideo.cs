using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeNetDumper
{
    public class YoutubeVideo
    {
        public string Title { get; internal set; }
        public string Author { get; internal set; }
        public int? Views { get; internal set; }
        public double AverageRating { get; internal set; }
        public string Keywords { get; internal set; }
        public bool IsLiveStream { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public string ThumbnailUrl { get; internal set; }

        public IReadOnlyList<YoutubeMediaStream> MediaStreams { get; internal set; }

        //These stuff doesn't work if you turn chrome User Agent off
        public bool Is360Degree => MediaStreams.Any(x => x.Type == MediaStreamType.Video && (x.Quality?.Contains("s") ?? false));

        public bool Is3D { get; }
        public bool Is4K => MediaStreams.Any(x => x.Quality?.StartsWith("2160") ?? false);
        public bool Is8K => MediaStreams.Any(x => x.Quality?.StartsWith("4320") ?? false);
    }
}
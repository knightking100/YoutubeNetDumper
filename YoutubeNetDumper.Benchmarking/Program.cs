using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Threading.Tasks;
using VideoLibrary;
using YoutubeExplode;

namespace YoutubeNetDumper.Benchmarking
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<YoutubeLibsBenchmarks>();
            Console.ReadLine();
        }
    }

    [MemoryDiagnoser]
    public class YoutubeLibsBenchmarks
    {
        private readonly string VIDEO_ID = "kJQP7kiw5Fk";
        private readonly YoutubeClient ytExplode = new YoutubeClient();
        private readonly YoutubeDumper ytNetDumper = new YoutubeDumper();
        private readonly YoutubeExperimentalDumper ytNetDumperNew = new YoutubeExperimentalDumper();

        [Benchmark]
        public async Task YoutubeExplode() => await ytExplode.GetVideoMediaStreamInfosAsync(VIDEO_ID);

        [Benchmark]
        public async Task YoutubeNetDumper() => await ytNetDumper.DumpAsync(VIDEO_ID);

        [Benchmark]
        public async Task libvideo()
        {
            var video = await YouTube.Default.GetVideoAsync($"https://www.youtube.com/watch?v={VIDEO_ID}");
            await video.GetUriAsync();// libvideo doesn't decrypt URL when you get the video
        }

        [Benchmark]
        public async Task YoutubeNetDumperNew() => await ytNetDumperNew.DumpAsync(VIDEO_ID);
    }
    
}
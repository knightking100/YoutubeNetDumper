using System;
using System.Diagnostics;
using System.Linq;

namespace YoutubeNetDumper.Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Enter youtube video url:");
            string videoId = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(videoId))
                videoId = "RBumgq5yVrA";
            videoId = YoutubeUtils.ParseVideoId(videoId);
            var result = new YoutubeExperimentalDumper().DumpAsync(videoId).GetAwaiter().GetResult();

            Console.WriteLine($"\n{result.Video.MediaStreams.Count} streams were obtained");
            foreach (var stream in result.Video.MediaStreams)
            {
                Console.WriteLine($"\nUrl: {stream.Url}");
                Console.WriteLine($"Type: {stream.Type}");
                if (stream.Bitrate != null)
                    Console.WriteLine($"Bitrate: {stream.Bitrate}");
                if (stream.Size != null)
                    Console.WriteLine($"Size: {stream.Size}");
                if (stream.Quality != null)
                    Console.WriteLine($"Quality: {stream.Quality}");
                if (stream.Framerate != null)
                    Console.WriteLine($"FPS: {stream.Framerate}");
                Console.WriteLine($"Format: {stream.Format}");
                Console.WriteLine($"Codecs: {stream.Codecs}");
            }

            Console.WriteLine($"{result.Video.Title} by {result.Video.Author}");

            if (result.Video.Is360Degree)
                Console.WriteLine($"This is a 360° video");
            if (result.Video.Is8K)
                Console.WriteLine($"This is a 8K video");
            else if (result.Video.Is4K)
                Console.WriteLine($"This is a 4K video");
            if (result.Video.Is3D)
                Console.WriteLine($"This is a 3D video");
            Console.WriteLine($"Total time: {result.ElapsedTime.TotalMilliseconds}ms");
            Console.WriteLine($"Parsing time: {result.ElapsedParsingTime.TotalMilliseconds}ms");

            Console.WriteLine($"Peak Working Set: {Process.GetCurrentProcess().PeakWorkingSet64}");

            for (int index = 0; index <= GC.MaxGeneration; index++)
            {
                Console.WriteLine($"Gen {index} collections: {GC.CollectionCount(index)}");
            }
            Console.WriteLine("Would you like to play the video in ffplay??");
            string option = Console.ReadLine();
            switch (option.ToLower())
            {
                case "y":
                case "yes":
                    //var proc = new ProcessStartInfo
                    //{
                    //    FileName = "ffplay.exe",
                    //    Arguments = $"\"{mixed_streams[0].Url}\" -window_title \"{result.Video.Title}\" -autoexit",
                    //    CreateNoWindow = false,
                    //    RedirectStandardOutput = false,
                    //};
                    Process.Start(new ProcessStartInfo("ffplay.exe", $"\"{result.Video.MediaStreams.FirstOrDefault(x => x.Type == MediaStreamType.Mixed).Url}\" -window_title \"{result.Video.Title}\" -autoexit"));
                    break;
            }
            Console.ReadLine();
        }
    }
}
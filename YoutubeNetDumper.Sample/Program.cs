﻿using System;
using System.Diagnostics;
using System.IO;
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
                if (stream.Quality != null)
                    Console.WriteLine($"Quality: {stream.Quality}");
                if (stream.Attributes.FPS != null)
                    Console.WriteLine($"FPS: {stream.Attributes.FPS}");
                Console.WriteLine($"Format: {stream.Attributes.Format}");
                if (stream.Attributes.AudioCodec != null)
                    Console.WriteLine($"Audio Codecs: {stream.Attributes.AudioCodec}");
                if (stream.Attributes.VideoCodec != null)
                    Console.WriteLine($"Video Codecs: {stream.Attributes.VideoCodec}");
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

            if (File.Exists("ffplay.exe"))
            {
                Console.WriteLine("Would you like to play the video in ffplay??");
                string option = Console.ReadLine();
                switch (option.ToLower())
                {
                    case "y":
                    case "yes":
                        Process.Start(new ProcessStartInfo("ffplay.exe", $"\"{result.Video.MediaStreams.FirstOrDefault(x => x.Type == MediaStreamType.Mixed).Url}\" -window_title \"{result.Video.Title}\" -autoexit"));
                        break;
                }
            }
            
            Console.ReadLine();
        }
    }
}
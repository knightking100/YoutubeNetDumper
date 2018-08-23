using System;
using System.Threading.Tasks;

namespace YoutubeNetDumper
{
    public interface IYoutubeDumper : IDisposable
    {
        Task<DumpResult> DumpAsync(string videoId);
    }
}
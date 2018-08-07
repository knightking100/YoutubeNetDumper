using System.Threading.Tasks;

namespace YoutubeNetDumper
{
    public interface IYoutubeDumper
    {
        Task<DumpResult> DumpAsync(string videoId);
    }
}
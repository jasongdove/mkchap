using System.Globalization;
using System.Text;

namespace MkChap.Models;

public record Chapter(TimeSpan Start, TimeSpan Finish)
{
    public string GetMetadata(int num)
    {
        var sb = new StringBuilder();

        sb.Append("[CHAPTER]\n");
        sb.Append("TIMEBASE=1/1000\n");
        sb.Append($"START={Start.TotalMilliseconds.ToString(NumberFormatInfo.InvariantInfo)}\n");
        sb.Append($"END={Finish.TotalMilliseconds.ToString(NumberFormatInfo.InvariantInfo)}\n");
        sb.Append($"title=Chapter {num}\n");
        sb.Append('\n');
            
        return sb.ToString();
    }
}
using System.Globalization;
using LanguageExt;

namespace MkChap;

public class Duration : FFprobeBase
{
    public static async Task<Option<TimeSpan>> GetDuration(string? inputFile)
    {
        if (string.IsNullOrWhiteSpace(inputFile))
        {
            return Option<TimeSpan>.None;
        }

        var output = await GetFFprobeOutput(new List<string>
        {
            "-v", "panic",
            "-show_entries", "format=duration",
            "-of", "default=nw=1:nokey=1",
            inputFile
        });

        if (double.TryParse(output, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var value))
        {
            return TimeSpan.FromSeconds(value);
        }

        return Option<TimeSpan>.None;
    }
}
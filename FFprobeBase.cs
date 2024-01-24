using CliWrap;
using CliWrap.Buffered;

namespace MkChap;

public abstract class FFprobeBase
{
    protected static async Task<string> GetFFprobeOutput(IEnumerable<string> arguments)
    {
        var result = await Cli.Wrap("ffprobe")
            .WithArguments(arguments)
            .WithValidation(CommandResultValidation.None)
            .WithStandardErrorPipe(PipeTarget.ToStream(Stream.Null))
            .ExecuteBufferedAsync();
        
        return result.StandardOutput;
    }
}
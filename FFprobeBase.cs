using System.Diagnostics;

namespace MkChap;

public abstract class FFprobeBase
{
    protected static async Task<string> GetFFprobeOutput(IEnumerable<string> arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                UseShellExecute = false,
                RedirectStandardError = false,
                RedirectStandardOutput = true
            }
        };

        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();
        
        await process.WaitForExitAsync();
        
        // ReSharper disable once MethodHasAsyncOverload
        process.WaitForExit();

        return await process.StandardOutput.ReadToEndAsync();
    }
}
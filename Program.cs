using System.Diagnostics;
using System.Globalization;
using CommandLine;
using LanguageExt;
using static LanguageExt.Prelude;

namespace MkChap;

public static class Program
{
    public static async Task<int> Main(string[] args) =>
        await Parser.Default.ParseArguments<CommandLineOptions>(args)
            .MapResult(async opts =>
                {
                    try
                    {
                        // TODO: get duration
                        // TODO: get chapters
                        var maybeDuration = await GetDuration(opts.Input);
                        Console.WriteLine(maybeDuration);
                        return 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return -2;
                    }
                },
                _ => Task.FromResult(-1));

    private static async Task<Option<TimeSpan>> GetDuration(string inputFile)
    {
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

        return None;
    }

    private static async Task<string> GetFFprobeOutput(IEnumerable<string> arguments)
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
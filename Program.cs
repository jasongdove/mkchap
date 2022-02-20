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
                        var maybeDuration = await GetDuration(opts.Input);
                        foreach (var duration in maybeDuration)
                        {
                            var blackDetectOutput = await BlackDetect(opts.Input, opts.MinBlackSeconds, opts.RatioBlackPixels,
                                opts.BlackPixelThreshold);
                            var blackSections = GetBlackSections(blackDetectOutput, opts.MinBlackSeconds);
                            foreach (var section in blackSections)
                            {
                                Console.WriteLine(section);
                            }

                            return 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    return -2;
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

    private static async Task<string> BlackDetect(string inputFile, double minBlackSeconds, double ratioBlackPixels,
        double blackPixelThreshold)
    {
        // rework filename in a format that works on windows
        if (OperatingSystem.IsWindows())
        {
            // \ is escape, so use / for directory separators
            inputFile = inputFile.Replace(@"\", @"/");

            // colon after drive letter needs to be escaped
            inputFile = inputFile.Replace(@":/", @"\:/");
        }

        return await GetFFprobeOutput(new List<string>
        {
            "-f", "lavfi",
            "-i",
            $"movie={inputFile},blackdetect=d={minBlackSeconds}:pic_th={ratioBlackPixels}:pix_th={blackPixelThreshold}[out0]",
            "-show_entries", "frame_tags=lavfi.black_start,lavfi.black_end",
            "-of", "default=nw=1",
            "-v", "panic"
        });
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

    private static List<BlackSection> GetBlackSections(string blackDetectOutput, double minBlackSeconds) =>
        blackDetectOutput.Split("\n")
            .Map(s => s.Trim())
            .Filter(s => !string.IsNullOrWhiteSpace(s))
            .Map(s => TimeSpan.FromSeconds(double.Parse(s.Split("=")[1], NumberStyles.Number,
                NumberFormatInfo.InvariantInfo)))
            .Chunk(2)
            .Filter(c => c.Length == 2)
            .Map(c => new BlackSection(c[0], c[1]))
            .Filter(bs => bs.Start.TotalSeconds >= minBlackSeconds && bs.Duration.TotalSeconds >= minBlackSeconds)
            .ToList();

    private record BlackSection(TimeSpan Start, TimeSpan Finish)
    {
        public TimeSpan Duration => Finish - Start;
    };

    private record Chapter(TimeSpan Start, TimeSpan Finish);
}
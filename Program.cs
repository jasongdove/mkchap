using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using MkChap.Models;

namespace MkChap;

public static class Program
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };
    
    public static async Task<int> Main(string[] args) =>
        await Parser.Default.ParseArguments<CommandLineOptions>(args)
            .MapResult(async opts =>
                {
                    try
                    {
                        var outputFile = opts.Output ?? string.Empty;
                        
                        var maybeDuration = await Duration.GetDuration(opts.Input);
                        foreach (var duration in maybeDuration)
                        {
                            var windows = GetWindows(opts.Windows, duration);
                            var blackDetectOutput = await BlackDetect.Detect(
                                opts.Input, opts.MinBlackSeconds,
                                opts.RatioBlackPixels,
                                opts.BlackPixelThreshold);
                            var blackSections = GetBlackSections(blackDetectOutput, opts.MinBlackSeconds, windows);

                            var okSections = blackSections.Filter(bs => bs.State == State.Ok).ToList();
                            var doubled = new List<BlackSection>();
                            doubled.AddRange(okSections);
                            doubled.AddRange(okSections);

                            var chapters = GetChapters(doubled, duration);

                            var analysisResult = new AnalysisResult(blackSections, chapters);
                            Console.WriteLine(JsonSerializer.Serialize(analysisResult, Options));
                            
                            if (!string.IsNullOrWhiteSpace(outputFile))
                            {
                                await ChapterWriter.WriteToFile(opts.Input, outputFile, chapters);
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
    
    private static List<BlackSection> GetBlackSections(string blackDetectOutput, double minBlackSeconds,
        List<Window> windows) =>
        blackDetectOutput.Split("\n")
            .Map(s => s.Trim())
            .Filter(s => !string.IsNullOrWhiteSpace(s))
            .Map(s => TimeSpan.FromSeconds(double.Parse(s.Split("=")[1], NumberStyles.Number,
                NumberFormatInfo.InvariantInfo)))
            .Chunk(2)
            .Filter(c => c.Length == 2)
            .Map(c =>
            {
                var start = c[0];
                var finish = c[1];
                var state = State.Ok;

                var midpoint = start + (finish - start) / 2.0;

                if (start.TotalSeconds < minBlackSeconds || (finish - start).TotalSeconds < minBlackSeconds)
                {
                    state = State.TooShort;
                }
                else if (windows.All(w => !w.Contains(midpoint)))
                {
                    state = State.OutsideOfWindows;
                }

                return new BlackSection(c[0], c[1], state);
            })
            .ToList();

    private static List<Window> GetWindows(string? windowsOption, TimeSpan duration) =>
        (windowsOption ?? string.Empty).Split(",")
            .Map(s => s.Trim())
            .Filter(s => !string.IsNullOrWhiteSpace(s))
            .Map(s =>
            {
                var split = s.Split(":");
                var startVal = double.Parse(split[0]);
                if (startVal < 0)
                {
                    startVal += duration.TotalSeconds;
                }

                var finishVal = double.Parse(split[1]);
                if (finishVal < 0)
                {
                    finishVal += duration.TotalSeconds;
                }

                var start = TimeSpan.FromSeconds(startVal);
                var finish = TimeSpan.FromSeconds(finishVal);
                return new Window(start, finish);
            })
            .DefaultIfEmpty(new Window(TimeSpan.Zero, duration))
            .ToList();

    private static List<Chapter> GetChapters(List<BlackSection> blackSections, TimeSpan duration)
    {
        var markers = new List<TimeSpan> { TimeSpan.Zero };
        markers.AddRange(blackSections.OrderBy(bs => bs.Start).Map(bs => bs.Midpoint()));
        markers.Add(duration);
        return markers.Chunk(2).Map(c => new Chapter(c[0], c[1])).ToList();
    }
}
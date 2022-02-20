﻿using System.Diagnostics;
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
                            var windows = GetWindows(opts.Windows, duration);
                            var blackDetectOutput = await BlackDetect(opts.Input, opts.MinBlackSeconds, opts.RatioBlackPixels,
                                opts.BlackPixelThreshold);
                            var blackSections = GetBlackSections(blackDetectOutput, opts.MinBlackSeconds, windows);
                            foreach (var section in blackSections)
                            {
                                Console.WriteLine(section);
                            }

                            var okSections = blackSections.Filter(bs => bs.State == State.Ok).ToList();
                            var doubled = new List<BlackSection>();
                            doubled.AddRange(okSections);
                            doubled.AddRange(okSections);

                            var chapters = GetChapters(doubled, duration);
                            foreach (var chapter in chapters)
                            {
                                Console.WriteLine(chapter);
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

                if (start.TotalSeconds < minBlackSeconds || (finish - start).TotalSeconds < minBlackSeconds)
                {
                    state = State.TooShort;
                }
                
                if (windows.All(w => !w.Contains(start) && !w.Contains(finish)))
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
                var split = s.Split("-");
                var start = TimeSpan.FromSeconds(double.Parse(split[0]));
                var finish = TimeSpan.FromSeconds(double.Parse(split[1]));
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
    
    private enum State
    {
        TooShort,
        OutsideOfWindows,
        Ok
    }

    private record BlackSection(TimeSpan Start, TimeSpan Finish, State State)
    {
        public TimeSpan Duration => Finish - Start;
        public TimeSpan Midpoint() => Start + (Finish - Start) / 2.0;
    };

    private record Chapter(TimeSpan Start, TimeSpan Finish);

    private record Window(TimeSpan Start, TimeSpan Finish)
    {
        public bool Contains(TimeSpan time) => time >= Start && time <= Finish;
    }
}
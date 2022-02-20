using CommandLine;

namespace MkChap;

public class CommandLineOptions
{
    [Value(index: 0, Required = true, HelpText = "Input file")]
    public string Input { get; set; }

    [Option(shortName: 's', longName: "seconds", Required = false, Default = 1.0d)]
    public double MinBlackSeconds { get; set; }

    [Option(shortName: 'r', longName: "ratio", Required = false, Default = 0.9d)]
    public double RatioBlackPixels { get; set; }

    [Option(shortName: 'b', longName: "black", Required = false, Default = 0.1d)]
    public double BlackPixelThreshold { get; set; }

    [Option(shortName: 'w', longName: "windows", Required = false)]
    public string Windows { get; set; }
}
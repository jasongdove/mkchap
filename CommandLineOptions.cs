using CommandLine;

namespace MkChap;

public class CommandLineOptions
{
    [Value(index: 0, Required = true, HelpText = "Input file")]
    public string Input { get; set; }
}
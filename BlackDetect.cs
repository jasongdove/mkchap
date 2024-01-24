namespace MkChap;

public class BlackDetect : FFprobeBase
{
    public static async Task<string> Detect(string? inputFile, double minBlackSeconds, double ratioBlackPixels,
        double blackPixelThreshold)
    {
        if (string.IsNullOrWhiteSpace(inputFile))
        {
            return string.Empty;
        }
        
        inputFile = FixFileName(inputFile);
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
    
    private static string FixFileName(string inputFile)
    {
        // rework filename in a format that works on windows
        if (OperatingSystem.IsWindows())
        {
            // \ is escape, so use / for directory separators
            inputFile = inputFile.Replace(@"\", @"/");

            // colon after drive letter needs to be escaped
            inputFile = inputFile.Replace(@":/", @"\:/");
        }

        return inputFile;
    }
}
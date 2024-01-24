using System.Diagnostics;
using System.Text;
using MkChap.Models;

namespace MkChap;

public class ChapterWriter
{
    public static async Task WriteToFile(string? inputFile, string outputFile, List<Chapter> chapters)
    {
        if (string.IsNullOrWhiteSpace(inputFile))
        {
            return;
        }
        
        var ffMetadata = GetFFMetadata(chapters);
        var metadataFile = string.Empty;

        try
        {
            metadataFile = await WriteFFMetadata(ffMetadata);
            await WriteMetadataToFile(inputFile, outputFile, metadataFile);
        }
        finally
        {
            try
            {
                if (File.Exists(metadataFile))
                {
                    File.Delete(metadataFile);
                }
            }
            catch
            {
                // do nothing
            }
        }
    }
    
    private static string GetFFMetadata(List<Chapter> chapters)
    {
        var sb = new StringBuilder();

        sb.Append(";FFMETADATA1\n");
        sb.Append('\n');

        for (var i = 0; i < chapters.Count; i++)
        {
            sb.Append(chapters[i].GetMetadata(i + 1));
        }

        return sb.ToString();
    }

    private static async Task<string> WriteFFMetadata(string ffMetadata)
    {
        var file = Path.GetTempFileName();
        await File.WriteAllTextAsync(file, ffMetadata);
        return file;
    }

    private static async Task WriteMetadataToFile(string inputFile, string outputFile, string metadataFile)
    {
        if (inputFile == outputFile)
        {
            var extension = Path.GetExtension(inputFile);
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), extension);
            await PerformWrite(inputFile, tempFile, metadataFile);
            File.Move(tempFile, outputFile, true);
        }
        else
        {
            await PerformWrite(inputFile, outputFile, metadataFile);
        }
    }
    
    private static async Task PerformWrite(string inputFile, string outputFile, string metadataFile)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                UseShellExecute = false,
                RedirectStandardError = false,
                RedirectStandardOutput = false
            }
        };

        var arguments = new List<string>
        {
            "-hide_banner",
            "-v", "error",
            "-i", inputFile,
            "-i", metadataFile,
            "-map_metadata", "1",
            "-map_chapters", "1",
            "-codec", "copy",
            "-y", outputFile
        };

        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();
        
        await process.WaitForExitAsync();
        
        // ReSharper disable once MethodHasAsyncOverload
        process.WaitForExit();
    }
}
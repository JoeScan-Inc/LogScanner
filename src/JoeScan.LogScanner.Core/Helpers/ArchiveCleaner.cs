using JoeScan.LogScanner.Core.Models;
using NLog;

namespace JoeScan.LogScanner.Core.Helpers;

public static class ArchiveCleaner
{
    private static Logger Logger = LogManager.GetCurrentClassLogger();
    public static void CleanupDirectory(string directory, int maxCount)
    {
        try
        {
            // string[] filePaths = Directory.GetFiles(directory, ,
            //     SearchOption.TopDirectoryOnly);
            DirectoryInfo info = new DirectoryInfo(directory);
            if (info.Exists)
            {
                var files = info.GetFiles($"*.{RawLogReaderWriter.DefaultExtension}", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(p => p.CreationTime).Skip(maxCount).ToList();
                foreach (var file in files)
                {
                    file.Delete();
                }
                if (files.Count > 0)
                {
                    Logger.Debug($"Cleaned up in: {directory}. Removed {files.Count} files.");
                }
            }
            else
            {
                Logger.Debug($"Directory to clean up: {directory} not found.");
            }
        }
        catch (Exception e)
        {

        }
    }
}

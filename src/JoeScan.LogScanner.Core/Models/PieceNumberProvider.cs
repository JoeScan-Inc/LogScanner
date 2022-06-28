using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.Text;

namespace JoeScan.LogScanner.Core.Models;

class PieceNumberProvider : IPieceNumberProvider
{
    private ILogger Logger { get; }
    private static  int nextLogNumber = 0;
    private readonly FileStream? stream;

    public int GetNextPieceNumber()
    {
        nextLogNumber++;
        SaveLastNumber(nextLogNumber);
        return nextLogNumber;
    }

    private void SaveLastNumber(int i)
    {
        try
        {
            if (stream != null)
            {
                stream.Seek(0L, SeekOrigin.Begin);
                using (var bw = new BinaryWriter(stream, Encoding.Default, true))
                {
                    bw.Write(i);
                }
                stream.Flush();
                Logger.Debug($"Saved last log number successfully.");
            }
        }
        catch (Exception e)
        {
            Logger.Warn($"Failed to save last log number: {e}");
        }
    }

    public PieceNumberProvider(ILogger logger)
    {
        Logger = logger;
        // get last number from file
        try
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dirName = Path.Combine(folderPath, "JoeScan", "LogScanner");
            var fileName = Path.Combine(dirName, "lastPieceNumber.txt");

            logger.Debug($"Trying to read last log number from file: {fileName}");
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
            stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite); // don't dispose, we keep it open
            using BinaryReader br = new BinaryReader(stream, Encoding.Default, true);
            nextLogNumber = br.ReadInt32() ;
        }
        catch (Exception e)
        {
            logger.Warn($"Reading last log number failed: {e.Message}. Restarting at log #1");
            nextLogNumber = 0;
        }
    }

    public void Dispose()
    {
        // we get disposed when the container goes out of scope, i.e. at
        // application shutdown. 
        stream?.Flush();
        stream?.Dispose();
    }
}

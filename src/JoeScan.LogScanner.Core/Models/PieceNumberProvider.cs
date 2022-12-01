using JoeScan.LogScanner.Core.Interfaces;
using NLog;
using System.Text;

namespace JoeScan.LogScanner.Core.Models;

class PieceNumberProvider : IPieceNumberProvider
{
    public event EventHandler<PieceNumberChangedEventArgs> PieceNumberChanged;
    #region Injected Properties

    private ILogger Logger { get; }

    #endregion

    #region Private Fields

    private static int currentLogNumber = 0;
    private readonly string filePath;
    private readonly FileSystemWatcher? watcher;
    private const string fileName = "lastPieceNumber.txt" ;
    private object locker = new object();

    #endregion

    #region IPieceNumberProvider

    public int GetNextPieceNumber()
    {
        currentLogNumber++;
        SaveLastNumber(currentLogNumber);
        return currentLogNumber;
    }

    public int PeekNextPieceNumber()
    {
        return currentLogNumber+1;
    }

    public void SetNextPieceNumber(int pieceNumber)
    {
        currentLogNumber = pieceNumber;
        SaveLastNumber(pieceNumber);
    }

    #endregion

    #region Private Methods

    private void SaveLastNumber(int i)
    {
        try
        {
            lock (locker)
            {
                File.WriteAllText(filePath, i.ToString());
            }
            Logger.Debug($"Saved last log number {i} successfully.");
        }
        catch (Exception e)
        {
            Logger.Warn($"Failed to save last log number: {e}");
        }
    }

    private int GetNumberFromFile()
    {
        try
        {
            lock (locker)
            {
                var str = File.ReadAllText(filePath);
                if (int.TryParse(str, out int res))
                {
                    return res;
                }
            }
        }
        catch (Exception e)
        {
            Logger.Debug(e);
        }

        currentLogNumber = 0;
        return currentLogNumber;
    }

    #endregion

    #region Lifecycle

    public PieceNumberProvider(ILogger logger)
    {
        Logger = logger;
        try
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var dirName = Path.Combine(folderPath, "JoeScan", "LogScanner");
            filePath = Path.Combine(dirName, fileName);
           
            logger.Debug($"Trying to read last log number from file: {filePath}");
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
                SaveLastNumber(0);
            }

            if (!File.Exists(filePath))
            {
                SaveLastNumber(0);
            }

            if (File.Exists(filePath))
            {
                watcher = new FileSystemWatcher(dirName);
                watcher.IncludeSubdirectories = false;
                watcher.Filter = fileName;
                watcher.EnableRaisingEvents = true;
                watcher.Changed += WatcherOnChanged;
                watcher.Deleted += WatcherOnChanged;
            }

            currentLogNumber = GetNumberFromFile();
        }
        catch (Exception e)
        {
            logger.Warn($"Reading last log number failed: {e.Message}. Restarting at log #1");
            currentLogNumber = 0;
        }
    }

    private void WatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        OnPieceNumberChanged(new PieceNumberChangedEventArgs(GetNumberFromFile()));
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        // we get disposed when the container goes out of scope, i.e. at
        // application shutdown. 
        if (watcher != null)
        {
            watcher.Changed -= WatcherOnChanged;
            watcher.Deleted -= WatcherOnChanged;
            watcher.Dispose();
        }
    }

    #endregion

    protected virtual void OnPieceNumberChanged(PieceNumberChangedEventArgs e)
    {
        PieceNumberChanged?.Invoke(this, e);
    }
}

public class PieceNumberChangedEventArgs
{
    public int NextNumber { get; }

    public PieceNumberChangedEventArgs(int nextNumber)
    {
        NextNumber = nextNumber;
    }
}

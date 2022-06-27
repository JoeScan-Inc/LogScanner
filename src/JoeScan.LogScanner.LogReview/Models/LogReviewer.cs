
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System;

namespace JoeScan.LogScanner.LogReview.Models;

public class LogReviewer : PropertyChangedBase
{
    public ILogger Logger { get; }

    #region Lifecycle

    public LogReviewer(ILogger logger)
    {
        Logger = logger;
    }

    #endregion

    #region Event Handlers

    public event EventHandler? LogChanged;

    #endregion

    #region Backing Properties

    private RawLog? currentLog;

    #endregion

    #region Bound Properties

    public RawLog? CurrentLog
    {
        get => currentLog;
        set
        {
            if (Equals(value, currentLog)) return;
            currentLog = value;
            NotifyOfPropertyChange(() => CurrentLog);
            OnLogChanged();
        }
    }

    public string CurrentFile { get; set; }
    #endregion

    #region Event Invocation

    protected virtual void OnLogChanged()
    {
        LogChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    public void Load(string fileName)
    {
        // do the actual load
        try
        {
            CurrentLog = RawLogReaderWriter.Read(fileName);
            CurrentFile = fileName;
            NotifyOfPropertyChange(()=>CurrentFile);
            OnLogChanged();
        }
        catch (Exception e)
        {
            var msg = $"Failed to read raw log from file \"{fileName}\". Error was: {e.Message}";
            Logger.Error(msg);
            
        }
    }
}

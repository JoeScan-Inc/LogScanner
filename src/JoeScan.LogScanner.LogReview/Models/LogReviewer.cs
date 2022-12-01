
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Interfaces;
using NLog;
using System;

namespace JoeScan.LogScanner.LogReview.Models;

public class LogReviewer : PropertyChangedBase, ILogModelObservable
{
    public ILogger Logger { get; }
    public LogModelBuilder ModelBuilder { get; }

    #region Bound Properties

    public RawLog? CurrentRawLog
    {
        get => currentRawLog;
        set
        {
            if (Equals(value, currentRawLog))
            {
                return;
            }
            currentRawLog = value;
            NotifyOfPropertyChange(() => CurrentRawLog);
            if (currentRawLog != null)
            {
                CurrentLogModel = ModelBuilder.Build(currentRawLog).LogModel;
            }
        }
    }

    public LogModel? CurrentLogModel
    {
        get => currentLogModel;
        set
        {
            if (Equals(value, currentLogModel))
            {
                return;
            }
            currentLogModel = value;
            NotifyOfPropertyChange(() => CurrentLogModel);
            Sections.Clear();
            if (currentLogModel != null)
            {
                Sections.AddRange(currentLogModel.Sections);
                CurrentSection = Sections[0];
            }
            
        }
    }

    public LogSection? CurrentSection
    {
        get => currentSection;
        set
        {
            if (Equals(value, currentSection))
            {
                return;
            }
            currentSection = value;
            NotifyOfPropertyChange(() => CurrentSection);
        }
    }

    public IObservableCollection<LogSection> Sections { get; } = new BindableCollection<LogSection>();
    public string CurrentFile { get; set; } = String.Empty;
    #endregion

    #region Lifecycle

    public LogReviewer(ILogger logger,
        LogModelBuilder modelBuilder)
    {
        Logger = logger;
        ModelBuilder = modelBuilder;
    }

    #endregion

    #region Backing Properties

    private RawLog? currentRawLog;
    private LogModel? currentLogModel;
    private LogSection? currentSection;

    #endregion

    public void Load(string fileName)
    {
        // do the actual load
        try
        {
            CurrentRawLog = RawLogReaderWriter.Read(fileName);
            CurrentFile = fileName;
            NotifyOfPropertyChange(() => CurrentFile);
        }
        catch (Exception e)
        {
            var msg = $"Failed to read raw log from file \"{fileName}\". Error was: {e.Message}";
            Logger.Error(msg);
        }
    }
}

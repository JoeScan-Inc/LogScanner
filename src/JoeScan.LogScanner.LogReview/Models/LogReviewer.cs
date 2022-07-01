
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.CrossSection;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace JoeScan.LogScanner.LogReview.Models;

public class LogReviewer : PropertyChangedBase
{
    public ILogger Logger { get; }
    public LogModelBuilder ModelBuilder { get; }
    public CrossSectionViewModel CrossSectionViewModel { get; }

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
            if (currentRawLog != null)
            {

                CurrentLogModel = ModelBuilder.Build(currentRawLog);
            }
            NotifyOfPropertyChange(() => CurrentRawLog);
            OnLogChanged();
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
            if (currentLogModel != null)
            {
                CurrentSection = currentLogModel.Sections[0];
                Sections = currentLogModel.Sections;
            }
            else
            {
                CurrentSection = null;
                Sections = null;
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
            CrossSectionViewModel.CurrentSection = currentSection;
        }
    }

    public string CurrentFile { get; set; } = String.Empty;
    public List<LogSection>? Sections { get; private set; }
    #endregion

    #region Lifecycle

    public LogReviewer(ILogger logger,
        LogModelBuilder modelBuilder,
        CrossSectionViewModel crossSectionViewModel)
    {
        Logger = logger;
        ModelBuilder = modelBuilder;
        CrossSectionViewModel = crossSectionViewModel;
    }

    #endregion

    #region Event Handlers

    public event EventHandler? LogChanged;

    #endregion

    #region Backing Properties

    private RawLog? currentRawLog;
    private LogModel? currentLogModel;
    private LogSection? currentSection;

    #endregion

    #region Event Invocation

    private void OnLogChanged()
    {
        LogChanged?.Raise(this, EventArgs.Empty);
        
    }

    #endregion

    public void Load(string fileName)
    {
        // do the actual load
        try
        {
            CurrentRawLog = RawLogReaderWriter.Read(fileName);
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

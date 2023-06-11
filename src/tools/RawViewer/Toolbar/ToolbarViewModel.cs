using Caliburn.Micro;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs;
using Nini.Config;
using NLog;
using RawViewer.Helpers;
using RawViewer.Shell;
using RawViewer.Timeline;
using System;
using System.IO;
using JoeScan.LogScanner.Core.Models;

namespace RawViewer.Toolbar;

public class ToolbarViewModel : Screen
{
    private readonly IRawViewerConfig config;
    private readonly IDialogService dialogService;
    public DataManager Data { get; }
    public TimelinePlotViewModel TimelinePlot { get; }
    public IEventAggregator EventAggregator { get; }
    public ILogger Logger { get; }

    public ToolbarViewModel(DataManager data,
        TimelinePlotViewModel timelinePlot, IRawViewerConfig config, IEventAggregator eventAggregator,
        IDialogService dialogService, ILogger logger)
    {
        this.config = config;
        this.dialogService = dialogService;
        Data = data;
        TimelinePlot = timelinePlot;
        EventAggregator = eventAggregator;
        Logger = logger;
    }

    public async void Load()
    {
        var initialDirectory = config.LastFileBrowserLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var openFileDialogSettings = new OpenFileDialogSettings()
        {
            Title = "Open raw log archive file",
            InitialDirectory = initialDirectory,
            Filter = $"Raw LogScanner Recording (*.raw)|*.raw|All Files (*.*)|*.*",
            CheckFileExists = true
        };

        bool? success = dialogService.ShowOpenFileDialog(this, openFileDialogSettings);
        if (success == true)
        {
            try
            {
                EventAggregator.PublishOnUIThreadAsync(true);
                var newProfiles = await DataReader.ReadFromFileAsync(openFileDialogSettings.FileName);
                int count = 0;
                Data.SetProfiles(newProfiles);
                
            }
            catch (Exception e)
            {

            }
            finally
            {
                EventAggregator.PublishOnUIThreadAsync(false);
            }
            config.LastFileBrowserLocation = Path.GetDirectoryName(openFileDialogSettings.FileName);
        }
    }
    public async void LoadLogFile()
    {
        var initialDirectory = config.LastFileBrowserLocation ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var openFileDialogSettings = new OpenFileDialogSettings()
        {
            Title = "Open raw log archive file",
            InitialDirectory = initialDirectory,
            Filter = $"Log Archive File (*.{RawLogReaderWriter.DefaultExtension})|*.{RawLogReaderWriter.DefaultExtension}|All Files (*.*)|*.*",
            CheckFileExists = true

        };

        bool? success = dialogService.ShowOpenFileDialog(this, openFileDialogSettings);
        if (success == true)
        {
            try
            {
                EventAggregator.PublishOnUIThreadAsync(true);
                
                var newProfiles = await DataReader.ReadFromLogModelAsync(openFileDialogSettings.FileName);
                int count = 0;
                Data.SetProfiles(newProfiles);

            }
            catch (Exception e)
            {

            }
            finally
            {
                EventAggregator.PublishOnUIThreadAsync(false);
            }
            config.LastFileBrowserLocation = Path.GetDirectoryName(openFileDialogSettings.FileName);
        }
    }
}

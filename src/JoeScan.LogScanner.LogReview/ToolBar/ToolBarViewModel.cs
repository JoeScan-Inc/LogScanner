using Accessibility;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Config;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.LogReview.Models;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

namespace JoeScan.LogScanner.LogReview.ToolBar;

public class ToolBarViewModel : Screen
{
    #region Injected

    public ILogModelObservable Model { get; }

    #region Private fields

    private ILogReviewConfig config;
    private IDialogService dialogService;

    #endregion

    #endregion

    #region Lifecycle

    public ToolBarViewModel(ILogModelObservable model,
        ILogReviewConfig config,
        IDialogService dialogService)
    {
        this.config = config;
        this.dialogService = dialogService;
        Model = model;
        Model.PropertyChanged += (_, _) => Refresh();
    }

    #endregion

    #region UI Callbacks

    public void Load()
    {

        var initialDirectory = String.IsNullOrEmpty(this.config.FileBrowserLastFolder)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : this.config.FileBrowserLastFolder;
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
            Model.Load(openFileDialogSettings.FileName);
            config.FileBrowserLastFolder = Path.GetDirectoryName(openFileDialogSettings.FileName);
        }
    }

    public bool CanLoadNext =>
        !String.IsNullOrEmpty(Model.CurrentFile) &&
        !String.IsNullOrEmpty(GetNextFile(Model.CurrentFile));



    public bool CanLoadPrevious =>
        !String.IsNullOrEmpty(Model.CurrentFile) &&
        !String.IsNullOrEmpty(GetNextFile(Model.CurrentFile, -1));


    #endregion

    #region UI Bound Properties

    public string CurrentFileName => Model.CurrentFile;
    public string RawLogId => Model.CurrentRawLog != null ? Model.CurrentRawLog.LogNumber.ToString() : "n/a";

    public string LogScannedDate => Model.CurrentRawLog != null
        ? Model.CurrentRawLog.TimeScanned.ToString(CultureInfo.CurrentUICulture)
        : "";

    #endregion

    public void LoadNext()
    {
        Model.Load(GetNextFile(Model.CurrentFile, 1));
    }

    public void LoadPrevious()
    {
        Model.Load(GetNextFile(Model.CurrentFile, -1));

    }

    private string GetNextFile(string reviewerCurrentFile, int direction = 1)
    {
        if (!String.IsNullOrEmpty(reviewerCurrentFile))
        {
            var path = Path.GetDirectoryName(reviewerCurrentFile);
            var files = Directory.GetFiles(path!, $"*.{RawLogReaderWriter.DefaultExtension}").ToList();
            var idx = files.IndexOf(reviewerCurrentFile);
            if (idx > -1)
            {
                if (idx + direction > 0 && idx + direction < files.Count)
                {
                    return files[idx + direction];
                }
            }
        }
        return String.Empty;
    }
}

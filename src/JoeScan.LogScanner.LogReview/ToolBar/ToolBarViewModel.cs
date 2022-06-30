using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.Settings;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;

namespace JoeScan.LogScanner.LogReview.ToolBar;

public class ToolBarViewModel : Screen
{
    #region Injected

    public LogReviewer Reviewer { get; }

    #region Private fields

    private ILogReviewSettings settings;
    private IDialogService dialogService;

    #endregion

    #endregion

    #region Lifecycle

    public ToolBarViewModel(LogReviewer reviewer,
        ILogReviewSettings settings,
        IDialogService dialogService)
    {
        this.settings = settings;
        Reviewer = reviewer;
        this.dialogService = dialogService;
        Reviewer.LogChanged += (_, _) => Refresh();
    }

    #endregion

    #region UI Callbacks

    public void Load()
    {
       
        var initialDirectory = String.IsNullOrEmpty(this.settings.FileBrowserLastFolder)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : this.settings.FileBrowserLastFolder;
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
            Reviewer.Load(openFileDialogSettings.FileName);
            settings.FileBrowserLastFolder = Path.GetDirectoryName(openFileDialogSettings.FileName);
        }
    }

    public bool CanLoadNext =>
        !String.IsNullOrEmpty(Reviewer.CurrentFile) &&
        !String.IsNullOrEmpty(GetNextFile(Reviewer.CurrentFile));

    

    public bool CanLoadPrevious =>
        !String.IsNullOrEmpty(Reviewer.CurrentFile) &&
        !String.IsNullOrEmpty(GetNextFile(Reviewer.CurrentFile,-1));


    #endregion

    #region UI Bound Properties

    public string CurrentFileName => Reviewer.CurrentFile;
    public string RawLogId => Reviewer.CurrentRawLog != null ? Reviewer.CurrentRawLog.LogNumber.ToString() : "n/a";

    public string LogScannedDate => Reviewer.CurrentRawLog != null
        ? Reviewer.CurrentRawLog.TimeScanned.ToString(CultureInfo.CurrentUICulture)
        : "";

    #endregion

    public void LoadNext()
    {
        Reviewer.Load(GetNextFile(Reviewer.CurrentFile,1));
    }

    public void LoadPrevious()
    {
        Reviewer.Load(GetNextFile(Reviewer.CurrentFile, -1));

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

using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.CrossSection;
using JoeScan.LogScanner.LogReview.Log3D;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.Navigator;
using JoeScan.LogScanner.LogReview.SectionTable;
using JoeScan.LogScanner.LogReview.ToolBar;
using NLog;
using NLog.Fluent;
using System.Windows.Interop;
using System.Windows;
using System;

namespace JoeScan.LogScanner.LogReview.Shell;

public class ShellViewModel : Screen
{
    public ILogger Logger { get; }
    public ToolBarViewModel ToolBar { get; }
    public CrossSectionViewModel CrossSection { get; }
    public NavigatorViewModel Navigator { get; }
    public Log3DWrapperViewModel Log3D { get; }
    public SectionTableViewModel SectionTable { get; }
    public LogReviewer Reviewer { get; }

    public ShellViewModel(ILogger logger,
        ToolBarViewModel toolBar,
        CrossSectionViewModel crossSection,
        NavigatorViewModel navigator,
        Log3DWrapperViewModel log3D,
        SectionTableViewModel sectionTable,
        LogReviewer reviewer)
    {
        Logger = logger;
        ToolBar = toolBar;
        CrossSection = crossSection;
        Navigator = navigator;
        Log3D = log3D;
        SectionTable = sectionTable;
        Reviewer = reviewer;
        Logger.Debug("Started LogReview tool.");
    }

    // handle command line args
    protected override void OnViewLoaded(object view)
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            Reviewer.Load(args[1]);
        }
    }

   
}

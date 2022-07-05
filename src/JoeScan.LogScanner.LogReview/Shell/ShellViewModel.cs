using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.CrossSection;
using JoeScan.LogScanner.LogReview.Log3D;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.Navigator;
using JoeScan.LogScanner.LogReview.SectionTable;
using JoeScan.LogScanner.LogReview.ToolBar;

namespace JoeScan.LogScanner.LogReview.Shell;

public class ShellViewModel : Screen
{
    public ToolBarViewModel ToolBar { get; }
    public CrossSectionViewModel CrossSection { get; }
    public LogReviewer Reviewer { get; }
    public NavigatorViewModel Navigator { get; }
    public Log3DViewModel Log3D { get; }
    public SectionTableViewModel SectionTable { get; }

    public ShellViewModel(ToolBarViewModel toolBar,
        CrossSectionViewModel crossSection,
        LogReviewer reviewer,
        NavigatorViewModel navigator,
        Log3DViewModel log3D,
        SectionTableViewModel sectionTable)
    {
        ToolBar = toolBar;
        CrossSection = crossSection;
        Reviewer = reviewer;
        Navigator = navigator;
        Log3D = log3D;
        SectionTable = sectionTable;
    }
}

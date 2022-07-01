using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.CrossSection;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.Navigator;
using JoeScan.LogScanner.LogReview.ToolBar;

namespace JoeScan.LogScanner.LogReview.Shell;

public class ShellViewModel : Screen
{
    public ToolBarViewModel ToolBar { get; }
    public CrossSectionViewModel CrossSection { get; }
    public LogReviewer Reviewer { get; }
    public NavigatorViewModel Navigator { get; }

    public ShellViewModel(ToolBarViewModel toolBar,
        CrossSectionViewModel crossSection,
        LogReviewer reviewer,
        NavigatorViewModel navigator)
    {
        ToolBar = toolBar;
        CrossSection = crossSection;
        Reviewer = reviewer;
        Navigator = navigator;
    }
}

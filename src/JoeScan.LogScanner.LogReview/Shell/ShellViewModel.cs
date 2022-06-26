using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.CrossSection;
using JoeScan.LogScanner.LogReview.ToolBar;

namespace JoeScan.LogScanner.LogReview.Shell;

public class ShellViewModel : Screen
{
    public ToolBarViewModel ToolBar { get; }
    public CrossSectionViewModel CrossSection { get; }

    public ShellViewModel(ToolBarViewModel toolBar,
        CrossSectionViewModel crossSection)
    {
        ToolBar = toolBar;
        CrossSection = crossSection;
    }
}

using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.ToolBar;

namespace JoeScan.LogScanner.LogReview.Shell;

public class ShellViewModel : Screen
{
    public ToolBarViewModel ToolBar { get; }

    public ShellViewModel(ToolBarViewModel toolBar)
    {
        ToolBar = toolBar;
    }
}

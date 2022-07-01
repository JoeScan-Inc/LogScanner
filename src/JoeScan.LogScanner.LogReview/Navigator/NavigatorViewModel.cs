using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.Models;

namespace JoeScan.LogScanner.LogReview.Navigator;

public class NavigatorViewModel : Screen
{
    public LogReviewer Reviewer { get; }

    public NavigatorViewModel(LogReviewer reviewer)
    {
        Reviewer = reviewer;
        Reviewer.LogChanged += (_, _) => Refresh();
    }

    public void NextSection()
    {
        Reviewer.CurrentSection = Reviewer.Sections![Reviewer.Sections.IndexOf(Reviewer.CurrentSection!) + 1];
        Refresh();
    }
    public void PreviousSection()
    {
        Reviewer.CurrentSection = Reviewer.Sections![Reviewer.Sections.IndexOf(Reviewer.CurrentSection!) - 1];
        Refresh();
    }
    public void FirstSection()
    {
        Reviewer.CurrentSection = Reviewer.Sections![0];
        Refresh();

    }
    public void LastSection()
    {
        Reviewer.CurrentSection = Reviewer.Sections![^1];
        Refresh();

    }

    public bool CanNextSection =>
        Reviewer.Sections != null
        && Reviewer.Sections.IndexOf(Reviewer.CurrentSection!) <
        Reviewer.Sections.Count - 1;

    public bool CanPreviousSection =>
        Reviewer.Sections != null
        && Reviewer.Sections.IndexOf(Reviewer.CurrentSection!) > 0;
    public bool CanFirstSection =>
        Reviewer.Sections != null
        && Reviewer.Sections.IndexOf(Reviewer.CurrentSection!) != 0;
    public bool CanLastSection =>
        Reviewer.Sections != null
        && Reviewer.Sections.IndexOf(Reviewer.CurrentSection!) != Reviewer.Sections.Count-1;

    public string SectionInfoLabel {
        get
        {
            if (Reviewer.CurrentLogModel == null)
            {
                return "No Section.";
            }
            var index = Reviewer.Sections!.IndexOf(Reviewer.CurrentSection!);
            var count = Reviewer.Sections.Count;
            var pos = Reviewer.CurrentSection!.SectionCenter;
            return $"Section {index+1} of {count}, Center at {pos:F2} mm";
        }
    }
}

using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.Models;
using System;
using System.Windows.Input;

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
            return $"Section {index+1} of {count}";
        }
    }

    public string Position => 
        Reviewer.CurrentSection != null ? $"{Reviewer.CurrentSection!.SectionCenter:F} mm": "n/a";

    public string FitError =>
        Reviewer.CurrentSection != null ? $"{Reviewer.CurrentSection.FitError:F1} mm" : "n/a";

    public string SectionWidth => 
        Reviewer.CurrentLogModel != null ? $"{Reviewer.CurrentLogModel.Interval:F1} mm" : "";

    

}

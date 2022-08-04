using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.LogReview.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JoeScan.LogScanner.LogReview.Navigator;

public class NavigatorViewModel : Screen
{
    public ILogModelObservable Model { get; }
    
    public NavigatorViewModel(ILogModelObservable model)
    {
        Model = model;
        Model.PropertyChanged += (_, _) => Refresh();
    }

    public void NextSection()
    {
        Model.CurrentSection = Model.Sections![Model.Sections.IndexOf(Model.CurrentSection!) + 1];
        Refresh();
    }
    public void PreviousSection()
    {
        Model.CurrentSection = Model!.Sections![Model!.Sections.IndexOf(Model.CurrentSection!) - 1];
        Refresh();
    }
    public void FirstSection()
    {
        Model.CurrentSection = Model!.Sections![0];
        Refresh();
    }

    public void LastSection()
    {
        Model.CurrentSection = Model!.Sections![^1];
        Refresh();
    }

    public bool CanNextSection =>
        Model.CurrentLogModel != null && Model.CurrentSection != null 
        && Model.Sections!.IndexOf(Model.CurrentSection) <
        Model.Sections.Count - 1;

    public bool CanPreviousSection =>
        Model.CurrentLogModel != null && Model.CurrentSection != null
        && Model.Sections!.IndexOf(Model.CurrentSection) > 0;

    public bool CanFirstSection =>
        Model.CurrentLogModel != null && Model.CurrentSection != null
                                && Model.Sections!.IndexOf(Model.CurrentSection!) != 0;
    public bool CanLastSection =>
        Model.CurrentLogModel != null && Model.CurrentSection != null
                                && Model.Sections!.IndexOf(Model.CurrentSection!) != Model.Sections.Count-1;

    public string SectionInfoLabel {
        get
        {
            if (Model.CurrentLogModel == null)
            {
                return "No Section.";
            }
            var index = Model.Sections!.IndexOf(Model.CurrentSection!);
            var count = Model.Sections.Count;
            return $"Section {index+1} of {count}";
        }
    }

    public string Position => 
        Model.CurrentSection != null ? $"{Model.CurrentSection!.SectionCenter:F} mm": "n/a";

    public string FitError =>
        Model.CurrentSection != null ? $"{Model.CurrentSection.FitError:F1} mm" : "n/a";

    public string SectionWidth => 
        Model.CurrentLogModel != null ? $"{Model.CurrentLogModel.Interval:F1} mm" : "";


   
}

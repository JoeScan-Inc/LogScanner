using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Config;
using JoeScan.LogScanner.LogReview.Interfaces;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.Shared.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UnitsNet.Units;

namespace JoeScan.LogScanner.LogReview.Navigator;

public class NavigatorViewModel : Screen
{
    private readonly LengthUnit targetUnit;
    public ILogModelObservable Model { get; }
    public ILogReviewConfig Config { get; }

    public NavigatorViewModel(ILogModelObservable model,
        ILogReviewConfig config)
    {
        Model = model;
        Config = config;
        Model.PropertyChanged += (_, _) => Refresh();
        targetUnit = config.Units == DisplayUnits.Inches ? LengthUnit.Inch : LengthUnit.Millimeter;
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
        Model.CurrentSection != null ?
            ConvertUnit(Model.CurrentSection!.SectionCenter):"";
            

    public string FitError =>
        Model.CurrentSection != null ? ConvertUnit(Model.CurrentSection.FitError) : "n/a";

    public string SectionWidth => 
        Model.CurrentLogModel != null ? ConvertUnit(Model.CurrentLogModel.Interval) : "";

    private string ConvertUnit(double val)
    {
        return $"{UnitsNet.Length.FromMillimeters(val).ToUnit(targetUnit):F2}";
    }
   
}

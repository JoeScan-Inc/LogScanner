using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Shared.Enums;
using NLog;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using UnitsNet;
using UnitsNet.Units;

namespace JoeScan.LogScanner.Shared.LogProperties;

public class LogPropertiesViewModel : Screen
{
   
    public IObservableCollection<LogPropertyItemViewModel> Items { get; set; }
        = new BindableCollection<LogPropertyItemViewModel>();
    public LogPropertiesViewModel(
        LogScannerEngine engine)
    {

        engine.LogModelBroadcastBlock.LinkTo(new ActionBlock<LogModelResult>(result =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                UpdateWith(result.LogModel);
            });
        }));
    }

    public LogPropertiesViewModel()
    {

    }

    public void UpdateWith(LogModel? model)
    {
        foreach (var logPropertyItemViewModel in Items)
        {
            logPropertyItemViewModel.UpdateWith(model);
        }
    }
    public void SetDisplayUnits(DisplayUnits du)
    {
        Items.Clear();
        var targetUnit = du == DisplayUnits.Inches ? LengthUnit.Inch : LengthUnit.Millimeter;
        var targetUnitLarge = du == DisplayUnits.Inches ? LengthUnit.Foot : LengthUnit.Meter;
        Items.Add(new LogPropertyItemViewModel("LogNumber", "", (logModel) => logModel.LogNumber.ToString()));
        Items.Add(new LogPropertyItemViewModel("Length", "", (logModel) => Length.FromMillimeters(logModel.Length).ToUnit(targetUnitLarge).ToString("F3")));
        Items.Add(new LogPropertyItemViewModel("Length", "", (logModel) => Length.FromMillimeters(logModel.Length).ToUnit(targetUnit).ToString("F1")));
        Items.Add(new LogPropertyItemViewModel("SED", "", (logModel) => Length.FromMillimeters(logModel.SmallEndDiameter).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("SED X", "", (logModel) => Length.FromMillimeters(logModel.SmallEndDiameterX).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("SED Y", "", (logModel) => Length.FromMillimeters(logModel.SmallEndDiameterY).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("LED", "", (logModel) => Length.FromMillimeters(logModel.LargeEndDiameter).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("LED X", "", (logModel) => Length.FromMillimeters(logModel.LargeEndDiameterX).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("LED Y", "", (logModel) => Length.FromMillimeters(logModel.LargeEndDiameterY).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("Max ⌀", "", (logModel) => Length.FromMillimeters(logModel.MaxDiameter).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("Max ⌀ at Pos", "", (logModel) => Length.FromMillimeters(logModel.MaxDiameterZ).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("Min ⌀", "", (logModel) => Length.FromMillimeters(logModel.MinDiameter).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("Min ⌀ at Pos", "", (logModel) => Length.FromMillimeters(logModel.MinDiameterZ).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("Sweep", "", (logModel) => Length.FromMillimeters(logModel.Sweep).ToUnit(targetUnit).ToString("F2")));
        Items.Add(new LogPropertyItemViewModel("Sweep Angle", "", (logModel) => Angle.FromRadians(logModel.Sweep).ToUnit(AngleUnit.Degree).ToString("F1")));
        Items.Add(new LogPropertyItemViewModel("Butt End First", "", (logModel) => logModel.ButtEndFirst ? "True" : "False"));
        Items.Add(new LogPropertyItemViewModel("Volume", "", (logModel)
            => Volume.FromCubicMillimeters(logModel.Volume).ToUnit(du == DisplayUnits.Inches ? VolumeUnit.CubicFoot : VolumeUnit.CubicMeter).ToString("F3")));
        Items.Add(new LogPropertyItemViewModel("Bark Volume", "", (logModel)
            => Volume.FromCubicMillimeters(logModel.BarkVolume).ToUnit(du == DisplayUnits.Inches ? VolumeUnit.CubicFoot : VolumeUnit.CubicMeter).ToString("F3")));
        Items.Add(new LogPropertyItemViewModel("Taper", "", (logModel) => logModel.Taper.ToString("F3")));
        Items.Add(new LogPropertyItemViewModel("Time Scanned", "", (logModel) => logModel.TimeScanned.ToShortTimeString()));
        Items.Add(new LogPropertyItemViewModel("Date Scanned", "", (logModel) => logModel.TimeScanned.ToShortDateString()));
    }
}

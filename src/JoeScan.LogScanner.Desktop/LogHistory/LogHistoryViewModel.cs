using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.Engine;
using JoeScan.LogScanner.Desktop.Enums;
using NLog;
using System;
using System.ComponentModel;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using UnitsNet.Units;

// ReSharper disable UnusedMember.Global

namespace JoeScan.LogScanner.Desktop.LogHistory;

public class LogHistoryViewModel : Screen
{
    public ILogScannerConfig Config { get; }
    public ILogger Logger { get; }
    public EngineViewModel Model { get; }

    public IObservableCollection<LogHistoryEntry> Items { get; } = new BindableCollection<LogHistoryEntry>();

    public LogHistoryViewModel(ILogScannerConfig config,
        ILogger logger,
        EngineViewModel model)
    {
        Config = config;
        Logger = logger;
        Model = model;
        model.LogModelBroadcastBlock.LinkTo(new ActionBlock<LogModel>(logModel =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
               Items.Insert(0,new LogHistoryEntry(Config.Units, logModel));
               if (Items.Count > Config.LogHistoryConfig.MaxLength)
               {
                   Items.RemoveAt(Config.LogHistoryConfig.MaxLength);
               }
            });
        }));
    }

   
}

public record LogHistoryEntry
{
    public LogHistoryEntry(DisplayUnits units, LogModel logModel)
    {
        
        var targetUnit = units == DisplayUnits.Inches ? LengthUnit.Inch : LengthUnit.Millimeter;
        var targetUnitLarge = units == DisplayUnits.Inches ? LengthUnit.Foot : LengthUnit.Meter;
        LogNumber = logModel.LogNumber.ToString();
        TimeScanned = logModel.TimeScanned;
        Length = UnitsNet.Length.FromMillimeters(logModel.Length).ToUnit(targetUnit).ToString("F2");
        SED = UnitsNet.Length.FromMillimeters(logModel.SmallEndDiameter).ToUnit(targetUnit).ToString("F2");
        SEDX = UnitsNet.Length.FromMillimeters(logModel.SmallEndDiameterX).ToUnit(targetUnit).ToString("F2");
        SEDY = UnitsNet.Length.FromMillimeters(logModel.SmallEndDiameterY).ToUnit(targetUnit).ToString("F2");
        LED = UnitsNet.Length.FromMillimeters(logModel.LargeEndDiameter).ToUnit(targetUnit).ToString("F2");
        LEDX = UnitsNet.Length.FromMillimeters(logModel.LargeEndDiameterX).ToUnit(targetUnit).ToString("F2");
        LEDY = UnitsNet.Length.FromMillimeters(logModel.LargeEndDiameterY).ToUnit(targetUnit).ToString("F2");
        MaxDiameter = UnitsNet.Length.FromMillimeters(logModel.MaxDiameter).ToUnit(targetUnit).ToString("F2");
        MaxDiameterZ = UnitsNet.Length.FromMillimeters(logModel.MaxDiameterZ).ToUnit(targetUnit).ToString("F2");
        MinDiameter = UnitsNet.Length.FromMillimeters(logModel.MinDiameter).ToUnit(targetUnit).ToString("F2");
        MinDiameterZ = UnitsNet.Length.FromMillimeters(logModel.MinDiameterZ).ToUnit(targetUnit).ToString("F2");
        Sweep = UnitsNet.Length.FromMillimeters(logModel.Sweep).ToUnit(targetUnit).ToString("F2");
        SweepAngle = UnitsNet.Angle.FromRadians(logModel.SweepAngleRad).ToUnit(AngleUnit.Degree).ToString("F1");
        ButtEndFirst = logModel.ButtEndFirst ? "True" : "False";
        Volume = UnitsNet.Volume.FromCubicMillimeters(logModel.Volume).ToUnit(units==DisplayUnits.Millimeters?VolumeUnit.CubicMeter:VolumeUnit.CubicFoot).ToString("F2");
        BarkVolume = UnitsNet.Volume.FromCubicMillimeters(logModel.BarkVolume).ToUnit(units==DisplayUnits.Millimeters?VolumeUnit.CubicMeter:VolumeUnit.CubicFoot).ToString("F2");
        Taper = logModel.Taper.ToString("F3");
        FileName = logModel.RawLog.ArchiveFileName;
    }
    [DisplayName("#")]
    public string LogNumber { get; }
    [DisplayName("Time/Date")]
    public DateTime TimeScanned { get; }
    
    public string Length { get; }
    public string SED { get; }
    public string SEDX { get; }
    public string SEDY { get; }
    public string LED { get; }
    public string LEDX { get; }
    public string LEDY { get; }
    public string Volume { get; }
    public string BarkVolume { get; }
    public string Taper { get; }
    [DisplayName("Max ⌀")]
    public string MaxDiameter { get; }
    [DisplayName("Max ⌀ Pos")]
    public string MaxDiameterZ { get; }
    [DisplayName("Min ⌀")]
    public string MinDiameter { get; }
    [DisplayName("Min ⌀ Pos")]
    public string MinDiameterZ { get; }
    public string Sweep { get; }
    [DisplayName("Sweep Angle")]
    public string SweepAngle { get; }
    [DisplayName("Butt End First")]
    public string ButtEndFirst { get; }
    private string FileName { get; }


}

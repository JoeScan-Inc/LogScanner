using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.Desktop.Config;
using JoeScan.LogScanner.Desktop.Engine;
using JoeScan.LogScanner.Desktop.Enums;
using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        model.LogModelBroadcastBlock.LinkTo(new ActionBlock<LogModelResult>(result =>
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Items.Insert(0, new LogHistoryEntry(Config.Units, result));
                if (Items.Count > Config.LogHistoryConfig.MaxLength)
                {
                    Items.RemoveAt(Config.LogHistoryConfig.MaxLength);
                }
            });
        }));
    }

    public void RowDoubleClicked(MouseButtonEventArgs e)
    {
        if (e.Source is DataGrid grid)
        {
            if (grid.CurrentItem is LogHistoryEntry entry)
            {
                //TODO: kick off Review
            }
        }
    }
}

public record LogHistoryEntry
{
    public LogHistoryEntry(DisplayUnits units, LogModelResult res)
    {
        LogNumber = res.LogNumber.ToString();
        TimeScanned = res.TimeScanned;
        

        if (res.IsValidModel)
        {
            var targetUnit = units == DisplayUnits.Inches ? LengthUnit.Inch : LengthUnit.Millimeter;
            var targetUnitLarge = units == DisplayUnits.Inches ? LengthUnit.Foot : LengthUnit.Meter;
            LogNumber = res.LogNumber.ToString();
            TimeScanned = res.LogModel.TimeScanned;
            Length = UnitsNet.Length.FromMillimeters(res.LogModel.Length).ToUnit(targetUnit).ToString("F2");
            SED = UnitsNet.Length.FromMillimeters(res.LogModel.SmallEndDiameter).ToUnit(targetUnit).ToString("F2");
            SEDX = UnitsNet.Length.FromMillimeters(res.LogModel.SmallEndDiameterX).ToUnit(targetUnit).ToString("F2");
            SEDY = UnitsNet.Length.FromMillimeters(res.LogModel.SmallEndDiameterY).ToUnit(targetUnit).ToString("F2");
            LED = UnitsNet.Length.FromMillimeters(res.LogModel.LargeEndDiameter).ToUnit(targetUnit).ToString("F2");
            LEDX = UnitsNet.Length.FromMillimeters(res.LogModel.LargeEndDiameterX).ToUnit(targetUnit).ToString("F2");
            LEDY = UnitsNet.Length.FromMillimeters(res.LogModel.LargeEndDiameterY).ToUnit(targetUnit).ToString("F2");
            MaxDiameter = UnitsNet.Length.FromMillimeters(res.LogModel.MaxDiameter).ToUnit(targetUnit).ToString("F2");
            MaxDiameterZ = UnitsNet.Length.FromMillimeters(res.LogModel.MaxDiameterZ).ToUnit(targetUnit).ToString("F2");
            MinDiameter = UnitsNet.Length.FromMillimeters(res.LogModel.MinDiameter).ToUnit(targetUnit).ToString("F2");
            MinDiameterZ = UnitsNet.Length.FromMillimeters(res.LogModel.MinDiameterZ).ToUnit(targetUnit).ToString("F2");
            Sweep = UnitsNet.Length.FromMillimeters(res.LogModel.Sweep).ToUnit(targetUnit).ToString("F2");
            SweepAngle = UnitsNet.Angle.FromRadians(res.LogModel.SweepAngleRad).ToUnit(AngleUnit.Degree).ToString("F1");
            ButtEndFirst = res.LogModel.ButtEndFirst ? "True" : "False";
            Volume = UnitsNet.Volume.FromCubicMillimeters(res.LogModel.Volume)
                .ToUnit(units == DisplayUnits.Millimeters ? VolumeUnit.CubicMeter : VolumeUnit.CubicFoot)
                .ToString("F2");
            BarkVolume = UnitsNet.Volume.FromCubicMillimeters(res.LogModel.BarkVolume)
                .ToUnit(units == DisplayUnits.Millimeters ? VolumeUnit.CubicMeter : VolumeUnit.CubicFoot)
                .ToString("F2");
            Taper = res.LogModel.Taper.ToString("F3");
            IsValid = true;
        }
    }

    [DisplayName("#")] public string LogNumber { get; }
    [DisplayName("Time/Date")] public DateTime TimeScanned { get; }

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
    [DisplayName("Max ⌀")] public string MaxDiameter { get; }
    [DisplayName("Max ⌀ Pos")] public string MaxDiameterZ { get; }
    [DisplayName("Min ⌀")] public string MinDiameter { get; }
    [DisplayName("Min ⌀ Pos")] public string MinDiameterZ { get; }
    public string Sweep { get; }
    [DisplayName("Sweep Angle")] public string SweepAngle { get; }
    [DisplayName("Butt End First")] public string ButtEndFirst { get; }

    public bool IsValid { get; }

    private RawLog RawLog { get; }
}

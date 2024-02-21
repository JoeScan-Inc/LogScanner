using Config.Net;
using JoeScan.Pinchot;
using System.ComponentModel;

namespace JoeScan.LogScanner.Core.Adapters.JS50;

public interface IJs50AdapterConfig
{
    public uint ScanPeriodUs { get; set; }
    [DefaultValue("XYBrightnessQuarter")] public DataFormat DataFormat { get; set; }
    public string ScanSystemDefinition { get; set; }
}


using System.ComponentModel;
using Config.Net;
using JoeScan.LogScanner.Js25.Enums;

namespace JoeScan.LogScanner.Js25;

public interface IJs25AdapterConfig
{
    [Option(Alias = "ScanThread.InternalProfileQueueLength", DefaultValue = 10)]
    int InternalProfileQueueLength { get; }
    [Option(Alias = "ScanThread.BaseAddress")]
    string BaseAddress { get; }

    [Option(Alias = "ScanThread.EncoderUpdateIncrement")]
    int EncoderUpdateIncrement { get; }

    [Option(Alias = "ScanThread.MaxRequestedProfileCount")]
    int MaxRequestedProfileCount { get; }

    [Option(Alias = "ScanThread.CableIdList")]
    string CableIdList { get; }

    [Option(Alias = "ScanThread.ParamFile")]
    string ParamFile { get; }

    [Option(Alias = "ScanThread.SyncMode")]
    SyncMode SyncMode { get; }
    // in microseconds, between 200 and 5,000,000 for JS-20/25
    [Option(Alias = "ScanThread.PulseInterval")]
    int PulseInterval { get; }

    [Option(Alias = "ScanThread.PulseMasterId")]
    int PulseMasterId { get; }
}


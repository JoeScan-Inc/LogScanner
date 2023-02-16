using Config.Net;

namespace PlcConnector;

public interface IPlcConnectorConfig
{
    [Option(Alias = "PlcConnector.IpAddress")]
    string IpAddress { get; }
    [Option(Alias = "PlcConnector.CpuSlot")]
    int CpuSlot { get; }
    [Option(Alias = "PlcConnector.RackNumber")]
    int RackNumber { get; }
    [Option(Alias = "PlcConnector.HeartbeatIntervalS")]
    int HeartbeatIntervalS { get; }
    [Option(Alias = "PlcConnector.SolutionTagName")]
    string SolutionTagName { get; }
    [Option(Alias = "PlcConnector.WatchdogTagName")]
    string WatchdogTagName { get; }
    [Option(Alias = "PlcConnector.EndOfLogTagName")]
    string EndOfLogTagName { get; }
}

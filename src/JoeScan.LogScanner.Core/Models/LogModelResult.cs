namespace JoeScan.LogScanner.Core.Models;

public class LogModelResult
{
    public RawLog RawLog { get; }
    public LogModel? LogModel { get; }
    public IEnumerable<string> Messages { get; }

    public LogModelResult(RawLog rawLog, LogModel? logModel, IEnumerable<string> messages)
    {
        RawLog = rawLog;
        LogModel = logModel;
        Messages = messages;
    }

    public bool IsValidModel => LogModel != null;
    public int LogNumber => RawLog.LogNumber;

    public DateTime TimeScanned => RawLog.TimeScanned;

}

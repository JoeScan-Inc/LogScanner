using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Interfaces;

public interface ILogDataConsumer : IHandle<LogData>
{
    LogData CurrentLog { get; set; }
}

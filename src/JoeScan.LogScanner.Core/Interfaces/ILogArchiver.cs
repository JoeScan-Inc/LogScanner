using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface ILogArchiver
{
    public void ArchiveLog(RawLog rawLog);
    public RawLog UnarchiveLog(Guid logId);
}

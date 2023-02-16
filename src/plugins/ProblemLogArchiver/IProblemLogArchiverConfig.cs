using System.ComponentModel;

namespace ProblemLogArchiver;

public interface IProblemLogArchiverConfig
{
    [DefaultValue(false)]
    public bool Enabled { get; }
    public string ArchiveLocation { get; }
    public int MaxCount { get; }
}

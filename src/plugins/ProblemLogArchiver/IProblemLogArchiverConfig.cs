using System.ComponentModel;

namespace ProblemLogArchiver;

public interface IProblemLogArchiverConfig
{
    [DefaultValue(false)]
    public bool Enabled { get; }
    public string ArchiveLocation { get; }
    [DefaultValue(100)]
    public int MaxCount { get; }
}

using JoeScan.LogScanner.Core.Events;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IPlugin
{
    string Name { get; }
    public uint VersionMajor { get; }
    public uint VersionMinor { get; }
    public uint VersionPatch { get; }
    public Guid Id { get; }

    public event EventHandler<PluginMessageEventArgs>? PluginMessage;
}

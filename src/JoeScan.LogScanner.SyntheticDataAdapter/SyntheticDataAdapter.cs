using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.SyntheticDataAdapter;

public class SyntheticDataAdapter : IScannerAdapter
{
    public UnitSystem Units => UnitSystem.Inches;
    public BufferBlock<Profile> AvailableProfiles { get; } = new BufferBlock<Profile>(new DataflowBlockOptions
    {
        // unlimited capacity, otherwise the BufferBlock will decline profiles
        // when subsequent steps can't keep up
        BoundedCapacity = -1
    });
    public bool IsRunning { get; private set; }
    public void Configure()
    {
        IsConfigured = true;
    }

    public bool IsConfigured { get; private set; }
    public void Start()
    {
        OnScanningStarted();
        IsRunning = true;
    }

    public Task StartAsync()
    {
        return Task.Run(Start);
    }

    public void Stop()
    {
        IsRunning = false;
        OnScanningStopped();
    }

    public Task StopAsync()
    {
        return Task.Run(Stop);
    }

    public string Name => "Synthetic Data";
    public event EventHandler? ScanningStarted;
    public event EventHandler? ScanningStopped;
    public event EventHandler? ScanErrorEncountered;
    public event EventHandler<EncoderUpdateArgs>? EncoderUpdated;

    protected virtual void OnScanningStarted()
    {
        ScanningStarted?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnScanningStopped()
    {
        ScanningStopped?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnScanErrorEncountered()
    {
        ScanErrorEncountered?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnEncoderUpdated(EncoderUpdateArgs e)
    {
        EncoderUpdated?.Invoke(this, e);
    }
}

using CellWinNet;
using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using UnitsNet;
using UnitsNet.Units;

namespace PlcConnector;
public class PlcConnectorPlugin : ILogModelConsumerPlugin, IHeartBeatSubscriber
{
    public IPlcConnectorConfig Config { get; }
    public ILogger Logger { get; }
    public string PluginName { get; } = "PlcConnector";
    public int VersionMajor { get; } = 1;
    public int VersionMinor { get; } = 0;
    public int VersionPatch { get; } = 0;
    public Guid Id { get; } = Guid.Parse("{2AB395EE-5181-4B4F-93F2-0DB8CAF36894}");
    public async void Initialize()
    {
        await Task.Run(CellWinConfiguration.Initialize);
        IsInitialized = CellWinConfiguration.IsInitialized;
        Logger.Debug("CellWin is " + (IsInitialized ?"initialized":"not initialized."));
        if (IsInitialized)
        {
            Logger.Debug("Connecting...");
            try
            {
                IsConnected = await Connect(Config.IpAddress, Config.CpuSlot, Config.RackNumber);
            }
            catch (Exception )
            {
                IsConnected = false;
            }
            Logger.Debug("PLC is " + (IsConnected ? "connected" : "not connected."));
        }
    }

    public bool IsConnected { get; set; }

    public bool IsInitialized { get; private set; }

    public void Cleanup()
    {
        
    }

    public void Consume(LogModel logModel)
    {
        // create the solution tag array and send
        if (!IsConnected)
        {
            Logger.Trace("Cannot send solution: PLC not connected.");
            return;
        }

        var values = new int[28];
        values[0] = (int)(Volume.FromCubicMillimeters(logModel.BarkVolume).ToUnit(VolumeUnit.CubicInch).Value * 1000);
        values[1] = 1; //TODO: implement ButEndFirst
        values[2] = (int)(logModel.CompoundSweep90 * 1000);
        values[3] = (int)(logModel.CompoundSweep * 1000);
        values[4] = (int)(Length.FromMillimeters(logModel.LargeEndDiameter).ToUnit(LengthUnit.Inch).Value * 1000);
        values[5] = (int)(Length.FromMillimeters(logModel.LargeEndDiameterX).ToUnit(LengthUnit.Inch).Value * 1000);
        values[6] = (int)(Length.FromMillimeters(logModel.LargeEndDiameterY).ToUnit(LengthUnit.Inch).Value * 1000);
        values[7] = (int)(Length.FromMillimeters(logModel.Length).ToUnit(LengthUnit.Inch).Value * 1000);
        values[8] = logModel.LogNumber;
        // values[9] = (int)(Length.FromMillimeters(logModel.MaxDiameter).ToUnit(LengthUnit.Inch).Value * 1000);
        // values[10] = (int)(Length.FromMillimeters(logModel.MaxDiameter).ToUnit(LengthUnit.Inch).Value * 1000);
        // values[11] = (int)(Length.FromMillimeters(logModel.MaxDiameter).ToUnit(LengthUnit.Inch).Value * 1000);
        values[12] = (int)(Length.FromMillimeters(logModel.SmallEndDiameter).ToUnit(LengthUnit.Inch).Value * 1000);
        values[13] = (int)(Length.FromMillimeters(logModel.SmallEndDiameterX).ToUnit(LengthUnit.Inch).Value * 1000);
        values[14] = (int)(Length.FromMillimeters(logModel.SmallEndDiameterY).ToUnit(LengthUnit.Inch).Value * 1000);

    }

    public void Dispose()
    {

    }

    public TimeSpan RequestedInterval { get; } = TimeSpan.FromSeconds(5);
    public void Callback(bool isRunning)
    {
        var tagValue = isRunning ? 1 : 0;
        if (IsConnected)
        {
            Logger.Trace("Setting Heartbeat Tag");
            Task.Run(() =>
            {
                lock (this)
                {
                    var res = NativeMethods.js_write_tag_32(Config.WatchdogTagName, tagValue);
                    if (res == 0)
                    {
                        Logger.Trace($"Successfully set tag {Config.WatchdogTagName} to {tagValue}.");
                    }
                    else
                    {
                        Logger.Trace($"Failed to set tag {Config.WatchdogTagName} to {tagValue}.");
                    }
                }
            }).Forget();
        }
        else
        {
            Logger.Trace("Heartbeat: not sending Tag because PLC is not connected.");
        }
    }

    public PlcConnectorPlugin(IPlcConnectorConfig config,ILogger logger)
    {
        Config = config;
        Logger = logger;

    }

    private Task<bool> Connect(string ip, int cpu, int rack)
    {
        return Task.Run(() =>
        {
            Logger.Trace("Invoking NativeMethods.js_connect()");
            int result = NativeMethods.js_connect(ip, cpu, rack);
            Logger.Trace("NativeMethods.js_connect() returned status {0}", result);
            return result == 0;
        });
    }
}

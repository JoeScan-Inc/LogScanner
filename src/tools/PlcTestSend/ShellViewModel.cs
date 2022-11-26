using Caliburn.Micro;
using CellWinNet;
using System.Net;
using AdonisUI.Controls;
using System.Threading.Tasks;
using System;
using System.Threading;
// ReSharper disable MemberCanBePrivate.Global

namespace PlcTestSend;

public class ShellViewModel : Screen
{
    private string ipAddress;
    private int cpuSlot;
    private int rackNumber;
    private bool isConnected;
    private bool isBusy;
    private System.Threading.Timer watchdogTimer;
    private long watchdogInterval = 5000;

    private const string SolutionTagName = "CommJSSolution";
    private const string WatchdogTagName = "JSWatchDog";
    private const string EndOfLogTagName = "JsEndLog";

    #region Bound Properties

    public IObservableCollection<Tag> Tags { get; set; } = new BindableCollection<Tag>();

    public string IpAddress 
    {
        get => ipAddress;
        set
        {
            if (value == ipAddress) return;
            ipAddress = value;
            NotifyOfPropertyChange(() => IpAddress);
            NotifyOfPropertyChange(() => CanConnect);
        }
    }

    public int CpuSlot
    {
        get => cpuSlot;
        set
        {
            if (value == cpuSlot) return;
            cpuSlot = value;
            NotifyOfPropertyChange(() => CpuSlot);
            NotifyOfPropertyChange(() => CanConnect);
        }
    }

    public int RackNumber
    {
        get => rackNumber;
        set
        {
            if (value == rackNumber) return;
            rackNumber = value;
            NotifyOfPropertyChange(() => RackNumber);
            NotifyOfPropertyChange(() => CanConnect);
        }
    }

    #endregion

    #region Lifecycle

    public ShellViewModel()
    {
        GenerateTagsList();
        IpAddress = "10.201.64.3";
        CpuSlot = 0;
        RackNumber = 1;
        watchdogTimer = new Timer(WatchDogTimerElapsed, null, -1, -1);
    }

    #endregion

    #region Guards

    public bool CanConnect => !IsConnected && CpuSlot >= 0 && RackNumber >= 0 && IPAddress.TryParse(IpAddress, out IPAddress? tmp);
    public bool CanDisconnect => IsConnected;
    public bool CanSend => IsConnected;
    public bool CanSendLogEnd => IsConnected;

    public bool IsConnected
    {
        get => isConnected;
        set
        {
            isConnected = value;
            if (isConnected)
            {
                StartWatchdogTimer();
            }
            else
            {
                StopWatchdogTimer();
            }
            Refresh();
        }
    }

    public bool IsDisconnected => !IsConnected;

    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (value == isBusy) return;
            isBusy = value;
            NotifyOfPropertyChange(() => IsBusy);
        }
    }
    public bool HeartbeatEnabled { get; set; }
    public bool HeartBeatVisible { get; set; }
    #endregion

    #region UI Callbacks

    public async void Connect()
    {

        CellWinConfiguration.Initialize();
        if (!CellWinConfiguration.IsInitialized)
        {
            MessageBox.Show("Internal Error: Failed to initialize CellWinNet library.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        IsBusy = true;
        IsConnected = await Connect(IpAddress, CpuSlot, RackNumber);
        IsBusy = false;
        if (!IsConnected)
        {
            MessageBox.Show("Failed to connect to PLC." 
                + " Check IP Address and CPU Slot/Rack.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void Disconnect()
    {
        IsConnected = false;
    }
    
    public void SendLogEnd()
    {
        try
        {
            if (!SendArchiveValue(EndOfLogTagName, 1))
            {
                throw new Exception("PLCSender failed to send tag.");
            }
        }
        catch (Exception )
        {
            MessageBox.Show("Setting JsEndLog tag to \"1\" failed","Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void Send()
    {
       
        // build solution packet

        var data = new int[28];
        for (int i = 0; i < 28; i++)
        {
            data[i] = (int)(Tags[i].Value * Tags[i].Scaler);
        }

        var res = SendArchiveValueArray(SolutionTagName, data);
        if (!res)
        {
            MessageBox.Show($"Sending data packet to PLC tag {SolutionTagName} failed","Error",MessageBoxButton.OK,MessageBoxImage.Error);
        }
       
    }

    #endregion
    #region Private Methods

    public void StopWatchdogTimer()
    {
        watchdogTimer.Change(-1, -1);
    }

    private async void WatchDogTimerElapsed(object state)
    {
        if (HeartbeatEnabled)
        {
            HeartBeatVisible = true;
            NotifyOfPropertyChange(()=> HeartBeatVisible);
            await Task.Delay(500);
            HeartBeatVisible = false;
            NotifyOfPropertyChange(() => HeartBeatVisible);
          //  SendArchiveValue(WatchdogTagName, 1);
        }
    }

    public void StartWatchdogTimer()
    {
        watchdogTimer.Change(0, watchdogInterval);
    }

    private  Task<bool> Connect(string ip, int cpu, int rack)
    {
        return Task.Run(() =>
        {
            // log.Debug("Invoking NativeMethods.js_connect()");
            int result = NativeMethods.js_connect(ip, cpu, rack);
            // log.DebugFormat("NativeMethods.js_connect() returned status {0}", result);
            return result == 0;
        });
    }

    private bool SendArchiveValue(string tag, int value)
    {
        if (!IsConnected)
        {
            return false;
        }
        lock (this)
        {
            var res = NativeMethods.js_write_tag_32(tag, value);
            return res == 0;
        }
    }
    public bool SendArchiveValueArray(string tag, int[] values)
    {
        if (!IsConnected)
        {
            return false;
        }

        lock (this)
        {
            var res = NativeMethods.js_write_tag_array_32(tag, values, values.Length);
            return (res == 0);
        }
    }
    private void GenerateTagsList()
    {
        Tags.Add(new Tag(){Name = "BarkVolume", Value = 1200, Description = "Bark Volume in cu in"});
        Tags.Add(new Tag(){Name = "ButtEndFirst", Value = 1, Scaler = 1, Description = "1=true, 2=false"} );
        Tags.Add(new Tag(){Name = "CompoundSweep90", Value = 1});
        Tags.Add(new Tag(){Name = "CompoundSweepS", Value = 1});
        Tags.Add(new Tag(){Name = "LED", Value = 25});
        Tags.Add(new Tag(){Name = "LEDX", Value = 23.1});
        Tags.Add(new Tag(){Name = "LEDY", Value = 25.5});
        Tags.Add(new Tag(){Name = "Length", Value = 240, Description = "inches"});
        Tags.Add(new Tag(){Name = "LogNumber", Value = 1, Scaler = 1});
        Tags.Add(new Tag(){Name = "MaxDiameter", Value = 21});
        Tags.Add(new Tag(){Name = "MaxDiameterZ", Value = 118});
        Tags.Add(new Tag(){Name = "MinDiameterZ", Value = 0});
        Tags.Add(new Tag(){Name = "SED", Value = 18.01});
        Tags.Add(new Tag(){Name = "SEDX", Value = 18.47});
        Tags.Add(new Tag(){Name = "SEDY", Value = 17.33});
        Tags.Add(new Tag(){Name = "Sweep", Value = 1});
        Tags.Add(new Tag(){Name = "SweepAngle", Value = 22, Description = " 1/1000 degrees"});
        Tags.Add(new Tag(){Name = "SweepPercent", Value = 0.03, Description = "Percent * 1000"});
        Tags.Add(new Tag(){Name = "Taper", Value = 1});
        Tags.Add(new Tag(){Name = "TaperX", Value = 1});
        Tags.Add(new Tag(){Name = "Volume", Value = 1});
        Tags.Add(new Tag(){Name = "LeadingFaceAngle", Value = 1, IsEnabled = false, Description = "n/a"});
        Tags.Add(new Tag(){Name = "TrailingFaceAngle", Value = 1, IsEnabled = false, Description = "n/a" });
        Tags.Add(new Tag(){Name = "LeadingFaceDefectPresent", Value = 1, IsEnabled = false, Description = "n/a" });
        Tags.Add(new Tag(){Name = "TrailingFaceDefectPresent", Value = 1, IsEnabled = false, Description = "n/a", Scaler = 1});
        Tags.Add(new Tag(){Name = "TrailingFaceDefectPresent", Value = 1, IsEnabled = false, Scaler = 1});
        Tags.Add(new Tag(){Name = "LeadingFaceOffset", Value = 1, IsEnabled = false, Description = "n/a" });
        Tags.Add(new Tag(){Name = "TrailingFaceOffset", Value = 1, IsEnabled = false, Description = "n/a" });

        for (int i = 0; i < Tags.Count; i++)
        {
            Tags[i].Index = i;
        }
    }

    #endregion
}

public class Tag
{
    
    public string Name { get;  set; }
    public double Value { get; set; }
    public bool IsEnabled { get;  set; } = true;
    public double Scaler { get; set; } = 1000.0;
    public string Description { get; set; }
    public int Index { get;  set; }
}

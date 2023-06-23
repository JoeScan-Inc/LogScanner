using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using System.Globalization;

namespace SamplePlugin;
public class SamplePlugin: ILogModelConsumerPlugin, IDisposable
{
    public ILogger Logger { get; }
    // A plugin for the LogScannerEngine implements multiple interfaces:
    // IPlugin is the base interface that each plugin needs to provide. IPlugin here 
    // is implicitly declared via ILogModelConsumerPlugin, which itself is inheriting IPlugin, 
    // so the above class declaration could also be written as
    // public class SamplePlugin : IPlugin, ILogModelConsumerPlugin, IDisposable but that would be
    // redundant. 

    // You can also implement other interfaces here, such as ILogStatusEventConsumer or IHeartbeatSubscriber
    // see SamplePluginModule for the registration in this case. 

    #region IPlugin Implementation

    /// <summary>
    /// The user-visible name of the plugin, it will be only used
    /// for diagnostic messages and can be anything you choose. It should
    /// convey the exact purpose of this plugin, e.g. DatabaseWriterPostgres 
    /// </summary>
    public string Name { get; } = "SamplePlugin";

    /// <summary>
    /// The Major version of the plugin. This is currently not used by the Engine but
    /// is here for future expandability. 
    /// </summary>
    public uint VersionMajor { get; } = 1;
    /// <summary>
    /// The Minor version of the plugin. This is currently not used by the Engine but
    /// is here for future expandability. 
    /// </summary>
    public uint VersionMinor { get; } = 0;
    /// <summary>
    /// The patch version of the plugin. This is currently not used by the Engine but
    /// is here for future expandability. 
    /// </summary>
    public uint VersionPatch { get; } = 0;

    /// <summary>
    /// The Id is a unique identifier for the plugin. Create a new GUID when
    /// implementing a new plugin. 
    /// </summary>
    public Guid Id { get; } = Guid.Parse("{D81834CA-0ED3-4AB7-9FC4-A8D81D1A08D4}");
    /// <summary>
    /// A plugin can choose to implement this event in order to send user messages back to the Engine,
    /// where they are either displayed in a UI or used otherwise. 
    /// </summary>
    public event EventHandler<PluginMessageEventArgs>? PluginMessage;
    #endregion

    // The ILogModelConsumer plugin is a type of plugin that gets called by the Engine
    // with the results of LogModelBuilder for each log (stem) that was processed. 

    // The interface consists of four methods. In this example, we will simply write the log number
    // and the log volume to a text file. You can use this interface for anything, like driving a sorter, 
    // insert data into a database etc. 

    // The Consume() call is executed on a threadpool thread and therefore asynchronous. You should take 
    // precautions only if the call is taking longer than the typical time between log scans, otherwise you 
    // will get an ever expanding queue to work on. 

    // In a future version, Consume() will become ConsumeAsync() and return a task.

    #region ILogModelConsumer Implementation
    /// <summary>
    /// Initialize() is called when the Engine loads the plugin. This is the place to
    /// create files, database connections or PLC connections
    /// </summary>
    public void Initialize()
    {
        // for the sample implementation, we open a text file 
        // for writing or create a new one if it does not exist. 

        // no error checking here for brevity
        if (File.Exists(samplePluginFileName))
        {
            myStream = new StreamWriter(File.Open(samplePluginFileName, FileMode.Append));
        }
        else
        {
            myStream = new StreamWriter(File.OpenWrite(samplePluginFileName));
        }
        OnPluginMessage(new PluginMessageEventArgs(LogLevel.Info, "Initialized SamplePlugin!"));
    }
    /// <summary>
    /// The Engine will not call your plugin if it is not initialized,
    /// i.e. when you're not ready to receive calls. 
    /// </summary>
    public bool IsInitialized => myStream != null;

    public SamplePlugin(ILogger logger)
    {
        Logger = logger;
        Logger.Debug("Created SamplePlugin Instance");
    }

    public void Cleanup()
    {
        // currently not called by the Engine
    }
    /// <summary>
    /// called for each log model
    /// </summary>
    /// <param name="logModel"></param>
    public void Consume(LogModelResult logModel)
    {
        
        if (logModel.IsValidModel)
        {
            if (myStream != null)
            {
                
                myStream.WriteLine($"{logModel.LogNumber},{logModel.LogModel!.Volume:F1}");
                OnPluginMessage(new PluginMessageEventArgs(LogLevel.Trace, $"Successfully wrote Volume for log {logModel.LogNumber} to database."));
                Logger.Trace("Successfully wrote Volume for log {logModel.LogNumber} to database.");
            }
        }
        else
        {
            OnPluginMessage(new PluginMessageEventArgs(LogLevel.Warn, "LogModel is invalid. Cannot get Volume."));
            Logger.Error("LogModel is invalid. Cannot get Volume.");
        }
        Thread.Sleep(1000);
    }

    #endregion

    #region IDisposable Implementation
    /// <summary>
    /// IDisposable implementation. The Engine will Dispose all plugins when shutting down.
    /// </summary>
    public void Dispose()
    {
        if (myStream != null)
        {
            // closes underlying stream
            myStream.Dispose();

        }
    }

    #endregion

    #region Event Invocation

    protected virtual void OnPluginMessage(PluginMessageEventArgs e)
    {
        PluginMessage?.Invoke(this, e);
    }

    #endregion

    #region Sample plugin private fields

    private const string samplePluginFileName = "SamplePluginOutput.txt";
    private StreamWriter? myStream = null;


    #endregion
}

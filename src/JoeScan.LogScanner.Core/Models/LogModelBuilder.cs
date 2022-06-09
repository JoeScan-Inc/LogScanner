using NLog;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Models;

public class LogModelBuilder
{
    #region Injected Properties

    public ICoreConfig Config { get; }
    public ILogger Logger { get; }

    #endregion

    public TransformBlock<RawLog, LogModel> BuilderBlock { get; } 

    #region Lifecycle

    public LogModelBuilder(ICoreConfig config, ILogger logger)
    {
        Config = config;
        Logger = logger;
        BuilderBlock = new TransformBlock<RawLog, LogModel>(Build, 
            new ExecutionDataflowBlockOptions(){EnsureOrdered = true, SingleProducerConstrained = true, MaxDegreeOfParallelism = 2});
    }
    
    #endregion

    public LogModel Build(RawLog log)
    {
        Logger.Debug($"Building new LogModel from RawLog #{log.LogNumber}");
        Logger.Debug($"Using SectionInterval {Config.LogModelBuilderConfig.SectionInterval} {Config.Units}");

        var model =  new LogModel { LogNumber = log.LogNumber, TimeScanned = log.TimeScanned };

        double nextSection = Config.LogModelBuilderConfig.SectionInterval;
        // startoffset is index into keys where real data begins
        var sections = log.ProfileData.GroupBy( q=>q.)
        return model;
    }


}

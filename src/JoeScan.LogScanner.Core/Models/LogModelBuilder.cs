using NLog;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace JoeScan.LogScanner.Core.Models;

public class LogModelBuilder
{
    private readonly ICoreConfig config;
    private readonly LogSectionBuilder sectionBuilder;
    private readonly ILogger logger;

    #region Injected Properties



    #endregion

    public TransformBlock<RawLog, LogModel> BuilderBlock { get; } 

    #region Lifecycle

    public LogModelBuilder(ICoreConfig config,
        LogSectionBuilder sectionBuilder,
        ILogger logger)
    {
        this.config = config;
        this.sectionBuilder = sectionBuilder;
        this.logger = logger;
        BuilderBlock = new TransformBlock<RawLog, LogModel>(Build, 
            new ExecutionDataflowBlockOptions(){EnsureOrdered = true, SingleProducerConstrained = true, MaxDegreeOfParallelism = 2});
    }
    
    #endregion

    public LogModel Build(RawLog log)
    {
        var sw = Stopwatch.StartNew();
        var interval = config.LogModelBuilderConfig.SectionInterval;
        var encoderPulseInterval = config.SingleZoneLogAssemblerConfig.EncoderPulseInterval;
        logger.Debug($"Building new LogModel from RawLog #{log.LogNumber}");
        logger.Debug($"Using SectionInterval {interval} {config.Units}");
        
        var model =  new LogModel { LogNumber = log.LogNumber, TimeScanned = log.TimeScanned, Interval = interval};
        var firstEncVal = log.ProfileData[0].EncoderValues[0];

        // this will contain the z position of each profile - for the JS-25 we will need more work as the encoder may be de-synced
        // but for the JS-50 series we have always synchronized encoder positions for all heads
        var zPositions = log.ProfileData.Select(q =>
            (q.EncoderValues[0] - firstEncVal) * encoderPulseInterval).ToArray();

        double nextSection = interval;
        var startOffset = 0;
        double firstZ = zPositions[startOffset];
        double maxZ = zPositions[^1];
        var currentRawSection = new List<Profile>();
        for (int i = startOffset; i < zPositions.Length; i++)
        {
            if ((zPositions[i] - firstZ) > nextSection)
            {

                if (currentRawSection.Count > 0)
                {
                    var s = sectionBuilder.Build(currentRawSection, nextSection - interval / 2.0);
                    if (s.IsValid)
                    {
                        model.Sections.Add(s);
                    }
                    else
                    {
                        model.RejectedSections.Add(s);
                    }
                }
                do
                {
                    nextSection += interval;
                } while ((zPositions[i] - firstZ) > nextSection);

                if ((maxZ > nextSection) && ((maxZ - nextSection) < (interval / 2)))
                {
                    interval += (maxZ - nextSection);
                    nextSection = maxZ + 1; // make sure last profile makes it into the bin
                }

                currentRawSection = new List<Profile>();
            }
            currentRawSection.Add(log.ProfileData[i]);

        }
        var s2 = sectionBuilder.Build(currentRawSection, nextSection - interval / 2);
        if (s2.IsValid)
        {
            model.Sections.Add(s2);
        }
        else
        {
            model.RejectedSections.Add(s2);
        }

        var elapsed = sw.ElapsedMilliseconds;
        logger.Debug($"Log Model Generation took: {elapsed} ms");
        return model;

    }


}

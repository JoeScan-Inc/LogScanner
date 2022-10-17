using NLog;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using UnitsNet;

namespace JoeScan.LogScanner.Core.Models;

public class LogModelBuilder
{
    #region Injected Fields

    private readonly ILogger logger;
    private readonly ICoreConfig config;
    private readonly LogSectionBuilder sectionBuilder;

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
            new ExecutionDataflowBlockOptions() { EnsureOrdered = true, SingleProducerConstrained = true, MaxDegreeOfParallelism = 2 });
    }

    #endregion

    public LogModel Build(RawLog log)
    {
        var sw = Stopwatch.StartNew();
        var interval = config.LogModelBuilderConfig.SectionInterval;
        var encoderPulseInterval = config.SingleZoneLogAssemblerConfig.EncoderPulseInterval;
        logger.Debug($"Building new LogModel from RawLog #{log.LogNumber}");
        logger.Debug($"Using SectionInterval {interval} {config.Units}");
        List<LogSection> sections = new List<LogSection>();
        List<LogSection> rejectedSections = new List<LogSection>();
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
                        sections.Add(s);
                    }
                    else
                    {
                        rejectedSections.Add(s);
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
            sections.Add(s2);
        }
        else
        {
            rejectedSections.Add(s2);
        }

        var elapsed = sw.ElapsedMilliseconds;
        logger.Debug($"Log Model Generation took: {elapsed} ms");
        // var fitErrors = model.Sections.Select(s => s.FitError).ToArray();
        var model = new LogModel(log.LogNumber, config.LogModelBuilderConfig.SectionInterval, log.TimeScanned, config.SectionBuilderConfig.MaxFitError,
            config.SingleZoneLogAssemblerConfig.EncoderPulseInterval, log.Units) { Sections = sections, RejectedSections = rejectedSections };
        MeasureModel(model);
        return model;

    }

    private void MeasureModel(LogModel model)
    {
        // instead of doing this inside of the LogModel instance, we pretend LogModel is immutable
        // and do all calculations here

        // Measure diameters at a configurable offset from each end. This helps with 
        // models where the first few profiles are incomplete or on the cut face of the log
        double beginZ = config.LogModelBuilderConfig.DiameterEndOffset;
        double endZ = model.Length - config.LogModelBuilderConfig.DiameterEndOffset;

        LogSection beginSection = model.Sections[0];
        LogSection endSection = model.Sections[0];
        double n = model.Sections.Count;

        var Volume = 0.0;
        var BarkVolume = 0.0;
        var MaxDiameter = 0.0;
        var MinDiameter = Double.MaxValue;
        var MaxDiameterPos = 0.0;
        var MinDiameterPos = 0.0;

        double sumX = 0.0;
        double sumY = 0.0;
        double sumZ = 0.0;
        double sumXZ = 0.0;
        double sumYZ = 0.0;
        double sumZ2 = 0.0;
        foreach (LogSection sm in model.Sections)
        {
            // find the section closest to offset locations the SED/LED measurements
            if (Math.Abs(sm.SectionCenter - beginZ) < Math.Abs(beginSection.SectionCenter - beginZ))
            {
                beginSection = sm;
            }

            if (Math.Abs(sm.SectionCenter - endZ) < Math.Abs(endSection.SectionCenter - endZ))
            {
                endSection = sm;
            }
            Volume += sm.WoodArea * model.Length / model.Sections.Count;
            BarkVolume += sm.BarkArea * model.Length / model.Sections.Count;

            //find the largest diameter in the log and it's Z location
            if (MaxDiameter < sm.DiameterMax)
            {
                MaxDiameter = sm.DiameterMax;
                MaxDiameterPos = sm.SectionCenter;
            }
            // minimum diameter
            if (MinDiameter > sm.DiameterMin)
            {
                MinDiameter = sm.DiameterMin;
                MinDiameterPos = sm.SectionCenter;
            }

            // Find sums used to calculate best fit center line
            sumX += sm.CentroidX;
            sumY += sm.CentroidY;
            sumZ += sm.SectionCenter;
            sumZ2 += sm.SectionCenter * sm.SectionCenter;
            sumXZ += sm.CentroidX * sm.SectionCenter;
            sumYZ += sm.CentroidY * sm.SectionCenter;
        }

        model.Volume = Volume;
        model.BarkVolume = BarkVolume;

        // this computes a best fit line through all centers
        model.CenterLineSlopeX = (n * sumXZ - sumZ * sumX) / (n * sumZ2 - sumZ * sumZ);
        model.CenterLineInterceptXZ = sumX / n - model.CenterLineSlopeX * sumZ / n;
        model.CenterLineSlopeY = (n * sumYZ - sumZ * sumY) / (n * sumZ2 - sumZ * sumZ);
        model.CenterLineInterceptYZ = sumY / n - model.CenterLineSlopeY * sumZ / n;

        // Check which end is large
        if (endSection.DiameterMin + endSection.DiameterMax < beginSection.DiameterMin + beginSection.DiameterMax)
        {
            // beginning is larger
            model.SmallEndDiameter = (endSection.DiameterMin + endSection.DiameterMax) / 2.0;
            model.SmallEndDiameterX = endSection.DiameterX;
            model.SmallEndDiameterY = endSection.DiameterY;
            model.LargeEndDiameter = (beginSection.DiameterMin + beginSection.DiameterMax) / 2.0;
            model.LargeEndDiameterX = beginSection.DiameterX;
            model.LargeEndDiameterY = beginSection.DiameterY;
        }
        else
        {
            // end is larger
            model.LargeEndDiameter = (endSection.DiameterMin + endSection.DiameterMax) / 2.0;
            model.LargeEndDiameterX = endSection.DiameterX;
            model.LargeEndDiameterY = endSection.DiameterY;
            model.SmallEndDiameter = (beginSection.DiameterMin + beginSection.DiameterMax) / 2.0;
            model.SmallEndDiameterX = beginSection.DiameterX;
            model.SmallEndDiameterY = beginSection.DiameterY;
        }

        // Based on a straight line between the begin and end centroids 
        // find the section with maximum deviation.

        // First calculate the parameters of the centerline 
        double xSlope = (endSection.CentroidX - beginSection.CentroidX) /
                        (endSection.SectionCenter - beginSection.SectionCenter);
        double ySlope = (endSection.CentroidY - beginSection.CentroidY) /
                        (endSection.SectionCenter - beginSection.SectionCenter);

        // initialize sweeps to zero
       
        foreach (var sm in model.Sections)
        {
            sm.CenterLineX = sm.SectionCenter * model.CenterLineSlopeX + model.CenterLineInterceptXZ;
            sm.CenterLineY = sm.SectionCenter * model.CenterLineSlopeY + model.CenterLineInterceptYZ;
        }
        var Sweep = 0.0;
        var SweepAngle = 0.0;
        var midSections = model.Sections.Where(s => s.SectionCenter > beginSection.SectionCenter && s.SectionCenter < endSection.SectionCenter);

        double[] devX = midSections.Select((sm) => (sm.CentroidX - ((sm.SectionCenter - beginSection.SectionCenter) * xSlope + beginSection.CentroidX))).ToArray();
        double[] devY = midSections.Select((sm) => (sm.CentroidY - ((sm.SectionCenter - beginSection.SectionCenter) * ySlope + beginSection.CentroidY))).ToArray();

        double[] avg3x = WeightedMovingAverage(devX);
        double[] avg3y = WeightedMovingAverage(devY);

        for (int i = 0; i < avg3x.Length; i++)
        {
            double deviation = Math.Sqrt(avg3x[i] * avg3x[i] + avg3y[i] * avg3y[i]);
            if (deviation >= Sweep)
            {
                Sweep = deviation;
                SweepAngle = Math.Atan2(avg3y[i], avg3x[i]);
            }
        }

        model.Sweep = Sweep;
        model.SweepAngle = SweepAngle;
        var CompoundSweep90 = 0.0;
        var CompoundSweepS = 0.0;

        // Check center deviation at 180 and 90/270 from SweepAngle
        foreach (LogSection sm in midSections)
        {
            // calculate current centerline location for the current Z
            double endToEndcenterLineX = (sm.SectionCenter - beginSection.SectionCenter) * xSlope + beginSection.CentroidX;
            double endToEndcenterLineY = (sm.SectionCenter - beginSection.SectionCenter) * ySlope + beginSection.CentroidY;

            // calculate the current deviation
            double deviationX = sm.CentroidX - endToEndcenterLineX;
            double deviationY = sm.CentroidY - endToEndcenterLineY;

            // rotate deviations by -SweepAngle
            double deviationParalell = deviationX * Math.Cos(SweepAngle) + deviationY * Math.Sin(SweepAngle);
            double deviationPerpendicular = deviationX * -Math.Sin(SweepAngle) + deviationY * Math.Cos(SweepAngle);

            // Set sweep values to the largest devation found.
            CompoundSweepS = Math.Max(CompoundSweepS, -deviationParalell);
            CompoundSweep90 = Math.Max(CompoundSweep90, Math.Abs(deviationPerpendicular));
        }

        model.CompoundSweep = CompoundSweepS;
        model.CompoundSweep90 = CompoundSweep90;
    }

    private static  double[] WeightedMovingAverage(double[] v)
    {
        //TODO: test and verify
        double[] avg = new double[v.Length];
        avg[0] = v[0];
        avg[1] = v[1];

        for (int i = 2; i < v.Length - 2; i++)
        {
            avg[i] = (v[i - 2] / 4 + v[i - 1] / 2 + v[i] + v[i + 1] / 2 + v[i + 2] / 4) / 2.5;
        }

        avg[v.Length - 2] = v[^2];
        avg[v.Length - 1] = v.Last();
        return avg;
    }
}

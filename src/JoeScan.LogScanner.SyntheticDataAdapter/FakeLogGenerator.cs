using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.SyntheticDataAdapter;

public class FakeLogGenerator
{
    public ISyntheticDataAdapterConfig Config { get; }

    public record struct HeadCamPair(uint head, uint cam, double angle);

    private Random random = new Random();

    private bool inLog = false;
    private double dataStart;
    private double logStart;
    private double logEnd;
    private double dataEnd;

    private const double coverageAngle = 130;
    private const double RHO = Math.PI / 180.0;
    private const int numPointsPerProfile = 300;

    private List<HeadCamPair> heads = new List<HeadCamPair>()
    {
        new HeadCamPair(0, 0, 90),
        new HeadCamPair(1, 0, -30),
        new HeadCamPair(2, 0,210),
        new HeadCamPair(0, 1,90),
        new HeadCamPair(1, 1,-30),
        new HeadCamPair(2, 1,210),
    };

    private Dictionary<HeadCamPair, uint> seqNum;
    private long dataStartsAtEncoderValue;
    private readonly double encoderPulseInterval;
    private uint sequenceNumber = 0;
    private UnitSystem units;
    private double startDiameter;
    private double endDiameter;

    public FakeLogGenerator(ISyntheticDataAdapterConfig config)
    {
        Config = config;
        encoderPulseInterval = Config.EncoderPulseInterval;
        units = Config.Units;

    }

    public IEnumerable<Profile> ProfileForEncoderValue(long encVal, long timeStampNs)
    {
        if (!inLog)
        {
            GenerateNextLog(encVal);
        }

        return GetNextProfile(encVal, timeStampNs);
    }

    private IEnumerable<Profile> GetNextProfile(long encVal, long timeStampNs)
    {
        // calculate profile position relative to dataStart in Unit
        var pos = (encVal - dataStartsAtEncoderValue) * encoderPulseInterval;
        if (pos > dataEnd)
        {
            inLog = false;
            return new List<Profile>(); // no profiles
        }

        if (pos < logStart || (pos > logEnd))
        {
            // send empty (maybe noisy) profiles
            return CreateEmptyProfiles(encVal, timeStampNs);
        }
        else
        {
            // send real profile based on pos within log
            var scaler = (pos - logStart) / (logEnd - logStart);
            return CreateValidProfiles(scaler, encVal, timeStampNs);
        }
    }

    private IEnumerable<Profile> CreateValidProfiles(double scaler, long encVal, long timeStampNs)
    {
        foreach (var headCamPair in heads)
        {
            var pts = CreatePointsForProfile(scaler, headCamPair);
            yield return new Profile()
            {
                Camera = headCamPair.cam,
                Data = pts,
                EncoderValues = new Dictionary<uint, long>() { { 0, encVal + 10 * headCamPair.cam } },
                Inputs = InputFlags.None,
                LaserIndex = 1,
                LaserOnTimeUs = 200,
                ScanHeadId = headCamPair.head,
                ScanningFlags = ScanFlags.None,
                SequenceNumber = sequenceNumber,
                TimeStampNs = (ulong)timeStampNs,
                Units = units
            };
        }
    }

    private Point2D[] CreatePointsForProfile(double scaler, HeadCamPair headCamPair)
    {
        var currentDiameter = startDiameter + scaler * (endDiameter - startDiameter);
        return Circle.MakeSectionPoints(new Point2D(0, currentDiameter / 2, 0)
            , currentDiameter
            , (headCamPair.angle - coverageAngle / 2) * RHO
            , (headCamPair.angle + coverageAngle / 2) * RHO
            , numPointsPerProfile
            , true);
    }

    private IEnumerable<Profile> CreateEmptyProfiles(long encVal, long timeStampNs)
    {
        foreach (var headCamPair in heads)
        {
            yield return new Profile()
            {
                Camera = headCamPair.cam,
                Data = Array.Empty<Point2D>(),
                EncoderValues = new Dictionary<uint, long>() { { 0, encVal } },
                Inputs = InputFlags.None,
                LaserIndex = 1,
                LaserOnTimeUs = 200,
                ScanHeadId = headCamPair.head,
                ScanningFlags = ScanFlags.None,
                SequenceNumber = sequenceNumber,
                TimeStampNs = (ulong)timeStampNs,
                Units = units
            };
        }
        sequenceNumber++;
    }

    private void GenerateNextLog(long encVal)
    {
        // generate log sizes in units
        dataStart = 0;
        logStart = Config.LeadingGap;
        var length = Config.MinLogLength + random.NextDouble() * (Config.MaxLogLength - Config.MinLogLength);
        logEnd = logStart + length;
        dataEnd = logEnd + Config.TrailingGap;
        dataStartsAtEncoderValue = encVal;

        startDiameter = Config.MinLogDiameter + random.NextDouble() * (Config.MaxLogDiameter - Config.MinLogDiameter);
        endDiameter = startDiameter + random.NextDouble() * Config.MaxDiameterVariation;
        (startDiameter, endDiameter) = random.Next(100)%2 > 0 ? (startDiameter, endDiameter) : (endDiameter, startDiameter); // swap randomly

        inLog = true;

    }
}

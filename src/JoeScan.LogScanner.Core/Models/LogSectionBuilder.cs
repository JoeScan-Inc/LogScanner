using JoeScan.LogScanner.Core.Extensions;
using JoeScan.LogScanner.Core.Geometry;
using NLog;

namespace JoeScan.LogScanner.Core.Models;

public class LogSectionBuilder
{
    private readonly ICoreConfig config;
    private readonly ILogger logger;

    public LogSectionBuilder(ICoreConfig config, ILogger logger)
    {
        this.config = config;
        this.logger = logger;
    }
    //TODO: make a section truly immutable by keeping all the modelling here in the builder and then just 
    // calling the section constructor with all values
    public LogSection Build(IReadOnlyList<Profile> profiles, double sectionCenter)
    {
        //TODO: replace with array.copy?
        var filteredPoints = new List<Point2D>();
        foreach (var prof in profiles)
        {
            foreach (var point in prof.Data)
            {
                filteredPoints.Add(point);
            }
        }

        var boundingBox = CreateRobustBoundingBox(filteredPoints);
        var rejectedPoints = new List<Point2D>();

        if (config.SectionBuilderConfig.FilterOutliers)
        {
            filteredPoints = FilterOutliers(profiles, filteredPoints,rejectedPoints, config.SectionBuilderConfig.OutlierFilterMaxNumIterations, 
                config.SectionBuilderConfig.OutlierFilterMaxDistance);
        }

        var ellipseModel = EllipseFit.EllipseFitDirect(filteredPoints.ToFloatArray());
        var modeledProfile = GenerateModeledSection(filteredPoints,
            config.SectionBuilderConfig.ModelPointCount, ellipseModel, config.SectionBuilderConfig.BarkAllowance);


        var section = new LogSection(profiles, sectionCenter)
        {
            BarkAllowance = config.SectionBuilderConfig.BarkAllowance,
            BoundingBox = boundingBox,
            EllipseModel = ellipseModel,
            FilteredPoints = filteredPoints,
            RejectedPoints = rejectedPoints,
            ModeledProfile = modeledProfile
        };
        section.IsValid = ValidateSection(section);
        return section;

    }

    private List<Point2D> FilterOutliers(IReadOnlyList<Profile> profiles, List<Point2D> filteredPoints, List<Point2D> rejectedPoints, int numIterations, double maxDistance)
    {
        var results = new List<Circle>(numIterations);
        var r1 = new Random(42);
        var c = filteredPoints.Count;

        for (int i = 0; i < numIterations; i++)
        {
            var randomIndices = new int[3];
            var numSelected = 0;
            do
            {
                var l = r1.Next(c);
                var p = filteredPoints[l];
                randomIndices[numSelected++] = l;
                
            } while (numSelected < 3);
            // find the circle that fits these 3 inputpoints
            var candidate = CircleFit.FitThreePoints(randomIndices, filteredPoints);
            results.Add(candidate);
        }

        var medianRadius = results.OrderBy(s => s.r).Skip(numIterations / 3).Take(numIterations / 3).Average(p => p.r);
        var medianX = results.OrderBy(p => p.x).Skip(numIterations / 3).Take(numIterations / 3).Average(p => p.x);
        var medianY = results.OrderBy(p => p.y).Skip(numIterations / 3).Take(numIterations / 3).Average(p => p.y);

        var bestModel = new Circle() { r = medianRadius, x = medianX, y = medianY };
        var consensusSet = new List<Point2D>(c);
        foreach (var p in filteredPoints)
        {
            double dist = CircleFit.DistanceSquare(bestModel.x, bestModel.y, p.X, p.Y);
            if ((bestModel.r + maxDistance) * (bestModel.r + maxDistance) > dist)
            {
                consensusSet.Add(p);
            }
            else
            {
                rejectedPoints.Add(p);
            }
        }
        return consensusSet;
    }

    private FilteredBoundingBox CreateRobustBoundingBox(IEnumerable<Point2D> points)
    {
        var bb = new FilteredBoundingBox()
        {
            MinX = Double.MaxValue,
            MaxX = Double.MinValue,
            MinY = Double.MaxValue,
            MaxY = Double.MinValue
        };
        const int binCount = 1000;

        var xBins = new int[binCount];
        var yBins = new int[binCount];

        var rangeLow = -500.0;
        var rangeHigh = 500.0;

        int xcount = 0;
        int ycount = 0;

        var scaler = (rangeHigh - rangeLow) / binCount;
        foreach (var p in points)
        {
            bb.MinX = Math.Min(p.X, bb.MinX);
            bb.MinY = Math.Min(p.Y, bb.MinY);
            bb.MaxX = Math.Max(p.X, bb.MaxX);
            bb.MaxY = Math.Max(p.Y, bb.MaxY);

            var index = (int)((p.X - rangeLow) / (scaler));
            if (index >= 0 && index < binCount)
            {
                xBins[index]++;
                xcount++;
            }
            index = (int)((p.Y - rangeLow) / (scaler));
            if (index >= 0 && index < binCount)
            {
                yBins[index]++;
                ycount++;
            }
        }
        var skipPercent = 3.0;
        var lowerThreshold = (skipPercent * xcount) / 100.0;
        var upperThreshold = xcount - lowerThreshold;
        var sumX = 0;
        var sumY = 0;
        for (int i = 0; i < binCount; i++)
        {
            sumX += xBins[i];
            if (sumX > lowerThreshold && bb.FilteredMinX == 0.0)
            {
                bb.FilteredMinX = i * scaler + rangeLow;
            }
            if (sumX < upperThreshold)
            {
                bb.FilteredMaxX = i * scaler + rangeLow;
            }

            sumY += yBins[i];
            if (sumY > lowerThreshold && bb.FilteredMinY == 0.0)
            {
                bb.FilteredMinY = i * scaler + rangeLow;
            }
            if (sumY < upperThreshold)
            {
                bb.FilteredMaxY = i * scaler + rangeLow;
            }
        }
        return bb;
    }

    public List<Point2D> GenerateModeledSection(IList<Point2D> points, int pointCount, Ellipse ell, double barkAllowance)
    {
        // Each point in the FilteredProfile contributes to one or more modeled point.
        // 
        double[] weights = new double[pointCount];
        double[] radiiWeightedSum = new double[pointCount];
        double[] brightnessWeightedSum = new double[pointCount];

        double localCentroidY =  ell.Y;

        AddPointsToWeightArrays(points.ToArray(),new Point2D(ell.X,ell.Y,0), pointCount, weights, radiiWeightedSum, brightnessWeightedSum);
        

        var modeledProfile = new List<Point2D>();

        for (int i = 0; i < pointCount; ++i)
        {
            double angle = ((double)i / pointCount) * 2 * Math.PI;
            if (weights[i] > 0)
            {
                double radius = radiiWeightedSum[i] / weights[i];
                var y = radius * Math.Sin(angle) + localCentroidY;
                var x = radius * Math.Cos(angle) + ell.X;
                var b = brightnessWeightedSum[i] / weights[i];
                modeledProfile.Add(new Point2D(x, y, b));
            }
            else
            {
                double rotSin = Math.Sin(ell.Theta);
                double rotCos = Math.Cos(ell.Theta);
                angle -= ell.Theta;
                double x = (ell.A - barkAllowance) * Math.Cos(angle) * rotCos - (ell.B - barkAllowance) * Math.Sin(angle) * rotSin + ell.X;
                double y = (ell.A - barkAllowance) * Math.Cos(angle) * rotSin + (ell.B - barkAllowance) * Math.Sin(angle) * rotCos + localCentroidY;


                modeledProfile.Add(new Point2D(x, y, 1.0F));
            }
        }

        return modeledProfile;
    }
    private static void AddPointsToWeightArrays(Point2D[] pts, Point2D center, int pointCount,  double[] weights, double[] radiiWeightedSum, double[] brightnessWeightedSum)
    {
        foreach (var pt in pts)
        {
            double x = pt.X - center.X;
            double y = pt.Y - center.Y;
            double radius = Math.Sqrt(x * x + y * y);
            double angle = Math.Atan2(y, x);

            double fractionalIndex = pointCount * angle / (Math.PI * 2);
            //TODO: unit dependent?
            const double smoothingRegion = 2.1;

            for (var index = (int)Math.Ceiling(fractionalIndex); index - fractionalIndex < smoothingRegion; ++index)
            {
                int wrappedIndex = (index + pointCount) % pointCount;
                double weight = (smoothingRegion - (index - fractionalIndex));
                weights[wrappedIndex] += weight;
                radiiWeightedSum[wrappedIndex] += weight * radius;
                brightnessWeightedSum[wrappedIndex] += weight * pt.B;
            }

            for (int index = (int)Math.Floor(fractionalIndex); fractionalIndex - index < smoothingRegion; --index)
            {
                int wrappedIndex = (index + pointCount) % pointCount;
                double weight = (smoothingRegion - (fractionalIndex - index));
                weights[wrappedIndex] += weight;
                radiiWeightedSum[wrappedIndex] += weight * radius;
                brightnessWeightedSum[wrappedIndex] += weight * pt.B;
            }
        }
    }

    private bool ValidateSection(LogSection section)
    {

        //assume section is valid until we find a problem
        bool valid = true;
        
        if (section.DiameterMax / section.DiameterMin > config.SectionBuilderConfig.MaxOvality)
        {
            logger.Debug($"Excessively oval section {section.DiameterMax}/{section.DiameterMin} = {section.Ovality}");
            valid = false;
        }

        if (section.DiameterMin < config.SectionBuilderConfig.MinimumLogDiameter)
        {
            logger.Debug($"Unreasonably small diameter {section.DiameterMin}");
            valid = false;
        }

        if (section.DiameterMax > config.SectionBuilderConfig.MaximumLogDiameter)
        {
            logger.Debug($"Unreasonably large diameter {section.DiameterMax}");
            valid = false;
        }

        if (section.CentroidX > config.SectionBuilderConfig.LogMaximumPositionX 
            || section.CentroidX < config.SectionBuilderConfig.LogMinimumPositionX 
            || section.CentroidY > config.SectionBuilderConfig.LogMaximumPositionY 
            || section.CentroidY < config.SectionBuilderConfig.LogMinimumPositionY)
        {
            logger.Debug($"Out of range centroid ({section.CentroidX},{section.CentroidY}");
            valid = false;
        }

        return valid;
    }



}

namespace JoeScan.LogScanner.Core.Geometry;

public struct FilteredBoundingBox
{
    public double MinX;
    public double FilteredMinX;
    public double MaxX;
    public double FilteredMaxX;
    public double MinY;
    public double FilteredMinY;
    public double MaxY;
    public double FilteredMaxY;

    public double FilteredWidth()
    {
        return FilteredMaxX - FilteredMinX;
    }
    public double FilteredHeight()
    {
        return FilteredMaxY - FilteredMinY;
    }
}

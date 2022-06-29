using JoeScan.LogScanner.Core.Geometry;

namespace JoeScan.LogScanner.Core.Models;

public enum UnitSystem : byte
{
    Inches = 0,
    Millimeters = 1,
}

public static class UnitConverter
{
    public static Profile Convert(UnitSystem from, UnitSystem to, Profile p)
    {
        if (from == to)
        {
            return p;
        }

        if (from == UnitSystem.Inches)
        {
            p.Data = p.Data.Select(q => new Point2D(q.X * 25.4, q.Y * 25.4, q.B)).ToArray();
            p.Units = UnitSystem.Millimeters;
        }
        else
        {
            p.Data = p.Data.Select(q => new Point2D(q.X / 25.4, q.Y / 25.4, q.B)).ToArray();
            p.Units = UnitSystem.Inches;

        }

        return p;

    }
}

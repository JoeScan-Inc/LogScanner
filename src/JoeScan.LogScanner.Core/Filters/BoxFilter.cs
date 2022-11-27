using JoeScan.LogScanner.Core.Geometry;
using System.Runtime.Serialization;
using UnitsNet;
using UnitsNet.Units;

namespace JoeScan.LogScanner.Core.Filters;

public class BoxFilter : FilterBase
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }
    public override bool IsValid => isValid;

    private bool isValid = false;
    public override IReadOnlyList<Point2D> Outline => new List<Point2D>()
    {
        new Point2D(Left, Bottom, 0),
        new Point2D(Left, Top, 0),
        new Point2D(Right, Top, 0),
        new Point2D(Right, Bottom, 0)
    };

    public override string Kind => "BoxFilter";

    public override bool Contains(Point2D p)
    {
        if (!isValid)
        {
            // an invalid filter will let all points go through
            return true;
        }
        return p.X > Left && p.X < Right && p.Y > Bottom && p.Y < Top;
    }

    // this is called after Json.NET created the object, we check here if 
    // it is a valid filter
    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        if (!String.IsNullOrEmpty(Units))
        {
            if (LengthUnit.TryParse(Units, out LengthUnit lu))
            {
                Left = UnitConverter.Convert(Left, lu, LengthUnit.Millimeter);
                Right = UnitConverter.Convert(Right, lu, LengthUnit.Millimeter);
                Top = UnitConverter.Convert(Top, lu, LengthUnit.Millimeter);
                Bottom = UnitConverter.Convert(Bottom, lu, LengthUnit.Millimeter);
            }
        }
        isValid = Left < Right && Top > Bottom;
    }


}

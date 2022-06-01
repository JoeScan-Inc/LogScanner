using JoeScan.LogScanner.Core.Filters;
using JoeScan.LogScanner.Core.Geometry;
using JoeScan.LogScanner.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace JoeScan.LogScanner.Core.Tests.Models;

[TestClass]
public class FlightsAndWindowFilterTests
{
    [TestMethod]
    public void SaveToJson()
    {
        var sut = new FlightsAndWindowFilter();
        sut.Filters.Add(0, new PolygonFilter()
        {
            Vertices = new Point2D[]
        {
            new Point2D(10F,20F,0F),
            new Point2D(20F,30F,0F)
        }
        });
        sut.Filters.Add(1, new PolygonFilter()
        {
            Vertices = new Point2D[]
            {
                new Point2D(110F,220F,0F),
                new Point2D(220F,300F,0F)
            }
        });

        var s = JsonConvert.SerializeObject(sut, Formatting.Indented);

    }
}

using JoeScan.LogScanner.Core.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JoeScan.LogScanner.Core.Tests.Geometry;

[TestClass]
public class RectTests
{
    [TestMethod]
    public void IntersectionTests()
    {
        Rect A = new Rect();
        Rect B = new Rect(-100, -100, 200, 200);
        Assert.AreEqual(true, A.IsEmpty);
        Assert.AreEqual(false, B.IsEmpty);
        Assert.AreEqual(-100, B.Left);
        Assert.AreEqual(100, B.Right);
        Assert.AreEqual(100, B.Top);
        Assert.AreEqual(-100, B.Bottom);

        Rect C = new Rect(-10, -10, 20, 20);
        Assert.AreEqual(true, C.IntersectsWith(B));
        Assert.AreEqual(true, B.IntersectsWith(C));
        Assert.AreEqual(true, B.Contains(C));
        Assert.AreEqual(false, C.Contains(B));
    }
}

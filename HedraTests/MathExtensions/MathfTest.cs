using Hedra.Numerics;
using NUnit.Framework;

namespace HedraTests.MathExtensions;

public class MathfTest
{
    [Test]
    public void Test_LinearInterpolate3D_Centered()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 0.5f, 0.5f, 0.5f);
        Assert.AreEqual(result, 4.5f);
    }

    [Test]
    public void Test_LinearInterpolate3D_BottomLeftFront()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0);
        Assert.AreEqual(result, 1);
    }

    [Test]
    public void Test_LinearInterpolate3D_BottomRightFront()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 1, 0, 0);
        Assert.AreEqual(result, 2);
    }

    [Test]
    public void Test_LinearInterpolate3D_TopLeftFront()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 0, 1, 0);
        Assert.AreEqual(result, 3);
    }

    [Test]
    public void Test_LinearInterpolate3D_TopRightFront()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 1, 1, 0);
        Assert.AreEqual(result, 4);
    }

    [Test]
    public void Test_LinearInterpolate3D_BottomLeftBack()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 1);
        Assert.AreEqual(result, 5);
    }

    [Test]
    public void Test_LinearInterpolate3D_BottomRightBack()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 1, 0, 1);
        Assert.AreEqual(result, 6);
    }

    [Test]
    public void Test_LinearInterpolate3D_TopLeftBack()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 0, 1, 1);
        Assert.AreEqual(result, 7);
    }

    [Test]
    public void Test_LinearInterpolate3D_TopRightBack()
    {
        var result = Mathf.LinearInterpolate3D(1, 2, 3, 4, 5, 6, 7, 8, 1, 1, 1);
        Assert.AreEqual(result, 8);
    }
}
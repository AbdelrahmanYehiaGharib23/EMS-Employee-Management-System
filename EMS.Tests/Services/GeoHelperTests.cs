using EMS.BLL.Services.Location;

namespace EMS.Tests.Services;

public class GeoHelperTests
{
    [Fact]
    public void CalculateDistance_WhenCoordinatesAreEqual_ReturnsZero()
    {
        var distance = GeoHelper.CalculateDistance(30.0444, 31.2357, 30.0444, 31.2357);

        Assert.Equal(0d, distance);
    }

    [Fact]
    public void CalculateDistance_OneDegreeAlongEquator_ReturnsAbout111Kilometers()
    {
        var distance = GeoHelper.CalculateDistance(0, 0, 0, 1);

        Assert.InRange(distance, 111_190, 111_200);
    }

    [Fact]
    public void CalculateDistance_IsSymmetric()
    {
        var forward = GeoHelper.CalculateDistance(30.0444, 31.2357, 29.9792, 31.1342);
        var reverse = GeoHelper.CalculateDistance(29.9792, 31.1342, 30.0444, 31.2357);

        Assert.Equal(forward, reverse, precision: 8);
    }
}

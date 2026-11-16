using ThuyetMinh.Api.Services;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class GeoFencePolicyTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FreshPositionInsideRadius_IsAllowed() =>
        Assert.True(GeoFencePolicy.IsAllowed(
            10.7743, 106.6978, 50,
            10.7743, 106.6978, 10, Now, Now));

    [Fact]
    public void OutsideRadius_IsRejected() =>
        Assert.False(GeoFencePolicy.IsAllowed(
            10.7743, 106.6978, 50,
            10.78, 106.6978, 10, Now, Now));

    [Fact]
    public void MissingOrStalePosition_IsRejected()
    {
        Assert.False(GeoFencePolicy.IsAllowed(
            10.7743, 106.6978, 50,
            null, null, null, null, Now));

        Assert.False(GeoFencePolicy.IsAllowed(
            10.7743, 106.6978, 50,
            10.7743, 106.6978, 10,
            Now.AddMinutes(-2), Now));
    }

    [Fact]
    public void InaccuratePosition_IsRejected() =>
        Assert.False(GeoFencePolicy.IsAllowed(
            10.7743, 106.6978, 50,
            10.7743, 106.6978, 300, Now, Now));
}
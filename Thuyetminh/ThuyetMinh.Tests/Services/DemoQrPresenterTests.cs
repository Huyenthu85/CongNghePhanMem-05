using ThuyetMinh.Api.Services;
using Xunit;

namespace ThuyetMinh.Tests.Services;

// The former "phone visitor + presenter credit" flow is replaced by a QR payment
// tied directly to the desktop buyer's PaymentOrder.
public sealed class DemoQrPresenterTests
{
    [Fact]
    public void PaymentQr_IsBoundToOneBuyerAndOrder()
    {
        var store = new DemoQrTokenStore();
        var first = store.Issue(11, "TM-ONE", DateTimeOffset.UtcNow.AddMinutes(10));
        var second = store.Issue(22, "TM-TWO", DateTimeOffset.UtcNow.AddMinutes(10));
        var one = store.Begin(first.Code);
        var two = store.Begin(second.Code);
        Assert.Equal(11, one.VisitorId);
        Assert.Equal("TM-ONE", one.OrderCode);
        Assert.Equal(22, two.VisitorId);
        Assert.Equal("TM-TWO", two.OrderCode);
    }
}

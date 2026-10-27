using ThuyetMinh.Api.Services;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public sealed class DemoQrTokenStoreTests
{
    [Fact]
    public void ScanOfOnePayment_CanBeConfirmedOnlyOnce()
    {
        var store = new DemoQrTokenStore();
        var qr = store.Issue(11, "TM-ORDER-1", DateTimeOffset.UtcNow.AddMinutes(10));
        var first = store.Begin(qr.Code);
        Assert.Equal(DemoQrTokenStore.RedeemState.Reserved, first.State);
        Assert.Equal(11, first.VisitorId);
        Assert.Equal("TM-ORDER-1", first.OrderCode);
        Assert.Equal(DemoQrTokenStore.RedeemState.Busy, store.Begin(qr.Code).State);
        store.Finish(qr.Code, succeeded: true);
        Assert.Equal(DemoQrTokenStore.RedeemState.AlreadyCompleted, store.Begin(qr.Code).State);
    }

    [Fact]
    public void FailedConfirmation_CanBeRetried()
    {
        var store = new DemoQrTokenStore();
        var code = store.Issue(11, "TM-ORDER-1", DateTimeOffset.UtcNow.AddMinutes(10)).Code;
        Assert.Equal(DemoQrTokenStore.RedeemState.Reserved, store.Begin(code).State);
        store.Finish(code, succeeded: false);
        Assert.Equal(DemoQrTokenStore.RedeemState.Reserved, store.Begin(code).State);
    }

    [Fact]
    public void InvalidOrExpiredCode_IsRejected()
    {
        var clock = new ManualClock();
        var store = new DemoQrTokenStore(clock);
        Assert.Equal(DemoQrTokenStore.RedeemState.Invalid, store.Begin(null).State);
        Assert.Equal(DemoQrTokenStore.RedeemState.Invalid, store.Begin("not-a-code").State);
        var code = store.Issue(7, "TM-ORDER", clock.GetUtcNow().AddMinutes(20)).Code;
        clock.Advance(TimeSpan.FromMinutes(6));
        Assert.Equal(DemoQrTokenStore.RedeemState.Invalid, store.Begin(code).State);
    }

    private sealed class ManualClock : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan span) => _now += span;
    }
}

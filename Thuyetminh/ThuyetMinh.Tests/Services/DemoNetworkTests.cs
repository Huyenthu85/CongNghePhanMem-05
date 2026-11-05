using System.Net;
using ThuyetMinh.Api.Services;
using Xunit;

namespace ThuyetMinh.Tests.Services;

public class DemoNetworkTests
{
    [Theory]
    [InlineData("127.0.0.1", true)]
    [InlineData("192.168.1.12", true)]
    [InlineData("10.0.0.5", true)]
    [InlineData("172.20.0.7", true)]
    [InlineData("8.8.8.8", false)]
    public void PrivateLanCheck(string address, bool expected)
    {
        Assert.Equal(expected, DemoNetwork.IsLocalOrPrivate(IPAddress.Parse(address)));
    }
}

using System.Net;
using System.Net.Sockets;

namespace ThuyetMinh.Api.Services;

public static class DemoNetwork
{
    // Only for previewing unapproved sample locations on a developer's LAN.
    public static bool IsLocalOrPrivate(IPAddress? ip)
    {
        if (ip == null) return false;
        if (IPAddress.IsLoopback(ip)) return true;
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
        var b = ip.GetAddressBytes();
        return b[0] == 10 ||
            (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
            (b[0] == 192 && b[1] == 168);
    }
    // External tunnel traffic also arrives from a loopback connection. Check the Host
    // as well, or remote users could be mistaken for administrators on localhost.
    public static bool IsDirectLocalRequest(Microsoft.AspNetCore.Http.HttpContext context)
    {
        string host = context.Request.Host.Host;
        bool localHost = string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "::1", StringComparison.OrdinalIgnoreCase);
        return localHost && IPAddress.IsLoopback(context.Connection.RemoteIpAddress ?? IPAddress.None);
    }
}

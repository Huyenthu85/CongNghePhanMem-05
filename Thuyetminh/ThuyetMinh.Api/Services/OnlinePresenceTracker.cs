using System.Collections.Concurrent;

namespace ThuyetMinh.Api.Services;

// Dem session dang hoat dong trong MOT tien trinh API; khong phai so nguoi online tuyet doi.
public static class OnlinePresenceTracker
{
    public sealed record OnlineSession(string Username, DateTimeOffset LastSeenUtc);
    private static readonly ConcurrentDictionary<string, OnlineSession> Sessions = new();
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(45);

    public static void Touch(string sessionId, string username)
    {
        Sessions[sessionId] = new OnlineSession(username, DateTimeOffset.UtcNow);
    }

    public static OnlineSession[] GetOnline()
    {
        var cutoff = DateTimeOffset.UtcNow - Timeout;
        foreach (var item in Sessions)
        {
            if (item.Value.LastSeenUtc < cutoff)
                Sessions.TryRemove(item.Key, out _);
        }
        return Sessions.Values.OrderByDescending(x => x.LastSeenUtc).ToArray();
    }
}
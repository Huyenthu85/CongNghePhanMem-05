using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Business.Services
{
    public class OnlineUserService
    {
        private readonly ConcurrentDictionary<string, OnlineUserInfo> _users = new();

        public void Track(string sessionId, string username, string role, string ip, string userAgent, bool isListening = false, string? listeningShop = null)
        {
            _users[sessionId] = new OnlineUserInfo
            {
                SessionId = sessionId,
                Username = username,
                Role = role,
                IpAddress = ip,
                UserAgent = userAgent,
                IsListening = isListening,
                ListeningShop = listeningShop,
                LastSeen = DateTime.UtcNow
            };
        }

        public void Untrack(string sessionId)
        {
            _users.TryRemove(sessionId, out _);
        }

        public List<OnlineUserInfo> GetOnlineUsers()
        {
            var cutoff = DateTime.UtcNow.AddSeconds(-45);
            foreach (var kv in _users.Where(x => x.Value.LastSeen < cutoff))
            {
                _users.TryRemove(kv.Key, out _);
            }
            return _users.Values.OrderByDescending(u => u.LastSeen).ToList();
        }

        public List<OnlineUserInfo> GetActualOnlineUsers(AppDbContext db)
        {
            var now = DateTime.UtcNow;
            var activeIds = db.Users.AsNoTracking().Where(u => u.Active)
                .Select(u => u.Id).ToList().Select(id => $"account-{id}").ToHashSet();
            var guestIds = db.AnonymousVisitors.AsNoTracking().Where(v => v.IsActive)
                .Select(v => v.Id).ToList().Select(id => $"visitor-{id}").ToHashSet();
            var online = GetOnlineUsers()
                .Where(u => u.Role == "Guest" ? guestIds.Contains(u.SessionId) : activeIds.Contains(u.SessionId))
                .ToDictionary(u => u.SessionId, u => new OnlineUserInfo
                {
                    SessionId = u.SessionId, Username = u.Username, Role = u.Role,
                    IpAddress = u.IpAddress, UserAgent = u.UserAgent, LastSeen = u.LastSeen
                });
            var cutoff = now.AddSeconds(-ListeningService.MaxHeartbeatAgeSeconds);
            var sessions = db.ListeningSessions.AsNoTracking()
                .Where(s => s.Status == ListeningSessionStatus.Playing && s.AudioId != null &&
                    s.AnonymousVisitor.IsActive && s.LastHeartbeatAt >= cutoff &&
                    db.ListeningBalances.Any(b => b.AnonymousVisitorId == s.AnonymousVisitorId && b.RemainingSeconds > 0))
                .OrderBy(s => s.LastHeartbeatAt)
                .Select(s => new { s.AnonymousVisitorId, s.LastHeartbeatAt, s.Shop.Name })
                .ToList();
            foreach (var session in sessions)
            {
                var key = $"visitor-{session.AnonymousVisitorId}";
                if (!online.TryGetValue(key, out var user))
                {
                    user = new OnlineUserInfo
                    {
                        SessionId = key, Username = $"Khách-{session.AnonymousVisitorId}",
                        Role = "Guest", LastSeen = session.LastHeartbeatAt
                    };
                    online[key] = user;
                }
                user.IsListening = true;
                user.ListeningShop = session.Name;
                if (user.LastSeen < session.LastHeartbeatAt)
                    user.LastSeen = session.LastHeartbeatAt;
            }
            return online.Values.OrderByDescending(u => u.LastSeen).ToList();
        }

        public bool IsUserOnline(string username) =>
            _users.Values.Any(u => u.Username == username);

        public int CountOnline => GetOnlineUsers().Count;
    }

    public class OnlineUserInfo
    {
        public string SessionId { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public bool IsListening { get; set; }
        public string? ListeningShop { get; set; }
        public DateTime LastSeen { get; set; }
    }
}

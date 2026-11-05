using System.Collections.Concurrent;

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
            var cutoff = DateTime.UtcNow.AddMinutes(-5);
            foreach (var kv in _users.Where(x => x.Value.LastSeen < cutoff))
            {
                _users.TryRemove(kv.Key, out _);
            }
            return _users.Values.OrderByDescending(u => u.LastSeen).ToList();
        }

        public bool IsUserOnline(string username) =>
            _users.Values.Any(u => u.Username == username);

        public int CountOnline => _users.Count;
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
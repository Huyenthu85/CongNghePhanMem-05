using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;

namespace ThuyetMinh.Api.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly OnlineUserService _onlineUsers;

        public UsersModel(AppDbContext db, OnlineUserService onlineUsers)
        {
            _db = db;
            _onlineUsers = onlineUsers;
        }

        public List<User> Users { get; set; } = new();
        public List<OnlineUserInfo> OnlineUsers { get; set; } = new();
        public HashSet<string> OnlineUsernames { get; set; } = new();
        public HashSet<string> ListeningUsernames { get; set; } = new();
        public Dictionary<string, DateTime> LastSeen { get; set; } = new();
        public int OnlineCount { get; set; }
        public int ListeningCount { get; set; }
        public int TotalVisitorProfiles { get; set; }
        [TempData] public string? Message { get; set; }

        public void OnGet() => LoadData();

        public IActionResult OnPostToggleActive(long id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                Message = "Lỗi: Không tìm thấy người dùng.";
                return RedirectToPage("/Admin/Users");
            }
            if (user.Username == "admin")
            {
                Message = "Lỗi: Không thể khoá tài khoản admin gốc.";
                return RedirectToPage("/Admin/Users");
            }
            user.Active = !user.Active;
            user.SessionVersion++;
            _db.SaveChanges();
            _onlineUsers.Untrack($"account-{user.Id}");
            Message = user.Active ? $"Đã mở khoá {user.Username}." : $"Đã khoá {user.Username}.";
            return RedirectToPage("/Admin/Users");
        }

        private void LoadData()
        {
            Users = _db.Users.OrderByDescending(u => u.Id).ToList();
            TotalVisitorProfiles = _db.AnonymousVisitors.Count() ;
            OnlineUsers = _onlineUsers.GetActualOnlineUsers(_db);
            OnlineCount = OnlineUsers.Count;
            ListeningCount = OnlineUsers.Count(u => u.IsListening);
            OnlineUsernames = new HashSet<string>(OnlineUsers.Where(u => u.Role != "Guest").Select(u => u.Username));
            ListeningUsernames = new HashSet<string>(OnlineUsers.Where(u => u.Role != "Guest" && u.IsListening).Select(u => u.Username));
            LastSeen = OnlineUsers.GroupBy(u => u.Username)
                .ToDictionary(g => g.Key, g => g.Max(u => u.LastSeen));
        }
    }
}

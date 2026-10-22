using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using ThuyetMinh.Data;

namespace ThuyetMinh.Api.Pages;

public class RegisterModel : PageModel
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _hasher;
    private readonly AppDbContext _db;

    public RegisterModel(IUserRepository users, IPasswordHasher<User> hasher, AppDbContext db)
    {
        _users = users;
        _hasher = hasher;
        _db = db;
    }

    [BindProperty] public RegisterInput Input { get; set; } = new();
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var existingUser = await _users.FindByUsernameAsync(Input.Username);
        if (existingUser != null)
        {
            Error = "Tên đăng nhập này đã có người sử dụng.";
            return Page();
        }

        if (Input.Password != Input.ConfirmPassword)
        {
            Error = "Mật khẩu xác nhận không khớp.";
            return Page();
        }

        var newUser = new User
        {
            Username = Input.Username,
            FullName = Input.FullName,
            PhoneNumber = Input.PhoneNumber,
            Address = $"{Input.AddressLine}, {Input.Ward}, {Input.City}",
            Role = Role.Owner,
            Active = true
        };
        
        newUser.PasswordHash = _hasher.HashPassword(newUser, Input.Password);

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Login");
    }
}

public class RegisterInput
{
    public string Username { get; set; } = "";
    public string? FullName { get; set; }
    public string PhoneNumber { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    
    public string AddressLine { get; set; } = "";
    public string Ward { get; set; } = "";
    public string City { get; set; } = "";
}
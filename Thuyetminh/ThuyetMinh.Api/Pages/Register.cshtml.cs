using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        Error = Input.GetValidationError();

        if (!ModelState.IsValid || Error != null)
        {
            Error ??= "Vui lòng kiểm tra dữ liệu đăng ký.";
            return Page();
        }

        Input.Username = Input.Username.Trim();

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
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            if (!await _db.Users.AnyAsync(
                u => u.Username == Input.Username))
                throw;

            Error = "Tên đăng nhập này đã có người sử dụng.";
            return Page();
        }

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

    public string? GetValidationError()
    {
        if (string.IsNullOrWhiteSpace(Username)
            || Username.Trim().Length is < 3 or > 100)
            return "Tên đăng nhập phải có từ 3 đến 100 ký tự.";

        if (string.IsNullOrWhiteSpace(Password)
            || Password.Length is < 6 or > 128)
            return "Mật khẩu phải có từ 6 đến 128 ký tự.";

        if (Password != ConfirmPassword)
            return "Mật khẩu xác nhận không khớp.";

        if (string.IsNullOrWhiteSpace(PhoneNumber)
            || !System.Text.RegularExpressions.Regex.IsMatch(
                PhoneNumber.Trim(),
                @"^(0|\+84)(\s|\.)?((3[2-9])|(5[689])|(7[06-9])|(8[1-689])|(9[0-46-9]))(\d)(\s|\.)?(\d{3})(\s|\.)?(\d{3})$"))
            return "Vui lòng nhập số điện thoại hợp lệ.";

        if (string.IsNullOrWhiteSpace(AddressLine)
            || string.IsNullOrWhiteSpace(Ward)
            || string.IsNullOrWhiteSpace(City))
            return "Vui lòng nhập đầy đủ địa chỉ.";

        if (AddressLine.Length > 250
            || Ward.Length > 100
            || City.Length > 100
            || FullName?.Length > 200)
            return "Thông tin cá nhân quá dài.";

        return null;
    }
}
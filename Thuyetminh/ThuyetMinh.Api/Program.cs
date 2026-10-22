using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ThuyetMinh.Data;
using ThuyetMinh.Data.Models;
using ThuyetMinh.Data.Repositories;
using ThuyetMinh.Business.Services;
using ThuyetMinh.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Chua cau hinh SQLite.");
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "congnghephanmem.db");
var sqliteConnection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString)
{
    DataSource = databasePath
};
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(sqliteConnection.ToString()));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IShopSubmissionRepository, ShopSubmissionRepository>();
builder.Services.AddScoped<TranslationRepository>();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddScoped<LanguageRepository>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<AudioRepository>();
builder.Services.AddScoped<AudioService>();
builder.Services.AddScoped<ProcessingJobRepository>();
builder.Services.AddScoped<ProcessingJobService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "SmartScheme";
    options.DefaultChallengeScheme = "SmartScheme";
})
.AddPolicyScheme("SmartScheme", "JWT or Cookie", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
            return JwtBearerDefaults.AuthenticationScheme;
        return "Cookies";
    };
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
    o.Events = new JwtBearerEvents
    {
        OnTokenValidated = ctx =>
        {
            var raw = ctx.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var svc = ctx.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            if (svc.IsBlacklisted(raw)) ctx.Fail("Token đã bị thu hồi");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập: Bearer {token} (không có dấu ngoặc nhọn)"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();
app.UseStaticFiles();

// ============ SEED DATA ============
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    db.Database.Migrate();

    // 1. Seed Users + 5 quán ăn TP.HCM
    if (!db.Users.Any())
    {
        // 1. Tạo tài khoản
        var admin = new User { Username = "admin", Role = Role.Admin, FullName = "Quản trị viên", Active = true };
        admin.PasswordHash = hasher.HashPassword(admin, "admin123");
        db.Users.Add(admin);

        var owner = new User { Username = "owner1", Role = Role.Owner, FullName = "Chủ quán 1", Active = true };
        owner.PasswordHash = hasher.HashPassword(owner, "owner123");
        db.Users.Add(owner);

        db.SaveChanges();

<<<<<<< Updated upstream
        // 2. Tạo 5 Quán ăn ảo với địa chỉ thật
        var shops = new List<Shop>
        {
            new Shop
            {
                OwnerId = owner.Id,
                Name = "Quán Ăn Gia Đình Phố",
                Address = "123 Lý Tự Trọng, Phường Bến Thành, Quận 1, TP.HCM",
                Latitude = 10.7743,
                Longitude = 106.6978,
                ActivationRadiusMeters = 50,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "Bún Chả Hà Nội 36 Phố",
                Address = "36 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP.HCM",
                Latitude = 10.7712,
                Longitude = 106.6935,
                ActivationRadiusMeters = 60,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "Phở Bò Sài Gòn Xưa",
                Address = "150 Pasteur, Phường 6, Quận 3, TP.HCM",
                Latitude = 10.7823,
                Longitude = 106.6930,
                ActivationRadiusMeters = 50,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "Ốc Đêm Sài Thành",
                Address = "45 Võ Văn Tần, Phường 6, Quận 3, TP.HCM",
                Latitude = 10.7790,
                Longitude = 106.6915,
                ActivationRadiusMeters = 80,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "Cơm Tấm Ba Ghiền",
                Address = "84 Đặng Văn Ngữ, Phường 10, Phú Nhuận, TP.HCM",
                Latitude = 10.7996,
                Longitude = 106.6790,
                ActivationRadiusMeters = 100,
                PublishStatus = PublishStatus.Approved
            }
=======
        var shops = new List<Shop>
        {
            new Shop { OwnerId = owner.Id, Name = "Quán Ăn Gia Đình Phố", Address = "123 Lý Tự Trọng, Phường Bến Thành, Quận 1, TP.HCM", Latitude = 10.7743, Longitude = 106.6978, ActivationRadiusMeters = 50, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Bún Chả Hà Nội 36 Phố", Address = "36 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP.HCM", Latitude = 10.7712, Longitude = 106.6935, ActivationRadiusMeters = 60, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Phở Bò Sài Gòn Xưa", Address = "150 Pasteur, Phường 6, Quận 3, TP.HCM", Latitude = 10.7823, Longitude = 106.6930, ActivationRadiusMeters = 50, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Ốc Đêm Sài Thành", Address = "45 Võ Văn Tần, Phường 6, Quận 3, TP.HCM", Latitude = 10.7790, Longitude = 106.6915, ActivationRadiusMeters = 80, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Cơm Tấm Ba Ghiền", Address = "84 Đặng Văn Ngữ, Phường 10, Phú Nhuận, TP.HCM", Latitude = 10.7996, Longitude = 106.6790, ActivationRadiusMeters = 100, PublishStatus = PublishStatus.Approved }
>>>>>>> Stashed changes
        };
        db.Shops.AddRange(shops);
        db.SaveChanges();

<<<<<<< Updated upstream
        // 3. Tạo sẵn 1 ShopSubmission đã duyệt cho mỗi quán (để API lấy content trả về dữ liệu)
        foreach (var shop in shops)
        {
            var sub = new ShopSubmission
            {
                ShopId = shop.Id,
                SubmittedById = owner.Id,
                Name = shop.Name,
                Address = shop.Address,
                Latitude = shop.Latitude,
                Longitude = shop.Longitude,
                ActivationRadiusMeters = shop.ActivationRadiusMeters,
                Description = "Quán ăn ngon, không gian sạch sẽ.",
                Status = SubmissionStatus.Approved,
                ReviewedById = admin.Id,
                ReviewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.ShopSubmissions.Add(sub);
            db.SaveChanges();
            
            // Gắn version đã duyệt vào Shop
            shop.PublishedVersionId = sub.Id;
=======
        foreach (var s in shops)
        {
            var sub = new ShopSubmission
            {
                ShopId = s.Id, SubmittedById = owner.Id,
                Name = s.Name, Address = s.Address,
                Latitude = s.Latitude, Longitude = s.Longitude,
                ActivationRadiusMeters = s.ActivationRadiusMeters,
                Description = "Quán ăn ngon, không gian sạch sẽ.",
                Status = SubmissionStatus.Approved,
                ReviewedById = admin.Id, ReviewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.ShopSubmissions.Add(sub);
            db.SaveChanges();
            s.PublishedVersionId = sub.Id;
        }
        db.SaveChanges();
    }

    // 2. Seed Languages cho Module B
    if (!db.Languages.Any())
    {
        db.Languages.AddRange(
            new Language { Code = "vi", Name = "Tiếng Việt", IsActive = true },
            new Language { Code = "en", Name = "Tiếng Anh", IsActive = true },
            new Language { Code = "ja", Name = "Tiếng Nhật", IsActive = true }
        );
        db.SaveChanges();
    }

    // 3. Seed Translation + Audio cho Module B
    if (!db.Translations.Any())
    {
        var firstApprovedSub = db.ShopSubmissions
            .FirstOrDefault(s => s.Status == SubmissionStatus.Approved);
        var viLang = db.Languages.FirstOrDefault(l => l.Code == "vi");
        
        if (firstApprovedSub != null && viLang != null)
        {
            var translation = new Translation
            {
                ShopSubmissionId = firstApprovedSub.Id,
                LanguageId = viLang.Id,
                TranslatedText = "Chào mừng đến với Quán Ăn Gia Đình Phố. Quán chuyên các món ăn Việt truyền thống, không gian ấm cúng.",
                Status = "Success"
            };
            db.Translations.Add(translation);
            db.SaveChanges();

            var audio = new Audio
            {
                TranslationId = translation.Id,
                FilePath = "/audio/quan-mau.mp3",
                DurationSeconds = 5,
                Status = "Success"
            };
            db.Audios.Add(audio);
>>>>>>> Stashed changes
            db.SaveChanges();
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.Run();
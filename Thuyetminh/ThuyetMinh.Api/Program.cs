<<<<<<< HEAD
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
using ThuyetMinh.Api.Services;

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

builder.Services.AddSingleton<DemoQrTokenStore>();
builder.Services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
builder.Services.AddScoped<IPaymentConfirmationService, PaymentConfirmationService>();
builder.Services.AddScoped<IListeningService, ListeningService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShopRepository, ShopRepository>();
builder.Services.AddScoped<IShopSubmissionRepository, ShopSubmissionRepository>();
builder.Services.AddScoped<IAnonymousVisitorRepository, AnonymousVisitorRepository>();
builder.Services.AddScoped<IAnonymousVisitorService, AnonymousVisitorService>();
builder.Services.AddScoped<IListeningBalanceRepository, ListeningBalanceRepository>();
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
        // Nếu request bắt đầu bằng /api/ → dùng JWT
        if (context.Request.Path.StartsWithSegments("/api"))
            return JwtBearerDefaults.AuthenticationScheme;
        // Ngược lại → dùng Cookie
        return "Cookies";
    };
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.
    AddJwtBearer(o =>
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

// DEMO classroom: expose only the visitor-facing routes through a public Quick Tunnel.
// The tunnel forwards requests from localhost, so RemoteIpAddress alone CANNOT protect
// local administrator endpoints. Public Host values are restricted before Swagger/auth.
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (!DemoNetwork.IsDirectLocalRequest(context))
        {
            string host = context.Request.Host.Host;
            if (!host.EndsWith(".trycloudflare.com", StringComparison.OrdinalIgnoreCase) && !string.Equals(host, "glory-exact-employer.ngrok-free.dev", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            var path = context.Request.Path;
            bool isGet = HttpMethods.IsGet(context.Request.Method);
            bool isPost = HttpMethods.IsPost(context.Request.Method);
            bool permitted = (isGet && (
                path == "/" || path.StartsWithSegments("/payment") ||
                path.StartsWithSegments("/Narration") ||
                path.StartsWithSegments("/demo/qr/pay") ||
                path.StartsWithSegments("/api/visitor/me") ||
                path.StartsWithSegments("/api/public/shops") ||
                path.StartsWithSegments("/css") || path.StartsWithSegments("/js") ||
                path.StartsWithSegments("/lib") || path.StartsWithSegments("/images") ||
                path.StartsWithSegments("/favicon.ico"))) ||
                (isPost && (path.StartsWithSegments("/api/listening") ||
                    path == "/api/demo/qr/redeem"));
            if (!permitted)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
        }
        await next();
    });
}


// Seed dữ liệu mẫu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        var admin = new User { Username = "admin", Role = Role.Admin, FullName = "Quản trị viên", Active = true };
        admin.PasswordHash = hasher.HashPassword(admin, "admin123");
        db.Users.Add(admin);

        var owner = new User { Username = "owner1", Role = Role.Owner, FullName = "Chủ quán 1", Active = true };
        owner.PasswordHash = hasher.HashPassword(owner, "owner123");
        db.Users.Add(owner);

        db.SaveChanges();

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
        };
        db.Shops.AddRange(shops);
        db.SaveChanges();

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
// Do not expose paid MP3/audio from wwwroot before balance verification.
// Production should stream private audio with authorization and metering.
app.Use(async (context, next) =>
{
    string path = context.Request.Path.Value ?? string.Empty;
    if (new[] { ".mp3", ".wav", ".ogg", ".m4a" }
        .Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
    {
        var visitors = context.RequestServices.GetRequiredService<IAnonymousVisitorService>();
        var db = context.RequestServices.GetRequiredService<AppDbContext>();
        var visitor = await visitors.FindExistingAsync(context.Request.Cookies["tm_visitor"]);
        bool allowed = visitor != null && await db.ListeningBalances
            .AnyAsync(x => x.AnonymousVisitorId == visitor.Id && x.RemainingSeconds > 0);
        if (!allowed)
        {
            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
            return;
        }
    }
    await next();
});
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/payment"));
app.Run();
=======
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

        // 2. Tạo 5 Quán ăn ảo với địa chỉ thật
        var shops = new List<Shop>
        {
            new Shop { OwnerId = owner.Id, Name = "Quán Ăn Gia Đình Phố", Address = "123 Lý Tự Trọng, Phường Bến Thành, Quận 1, TP.HCM", Latitude = 10.7743, Longitude = 106.6978, ActivationRadiusMeters = 50, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Bún Chả Hà Nội 36 Phố", Address = "36 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP.HCM", Latitude = 10.7712, Longitude = 106.6935, ActivationRadiusMeters = 60, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Phở Bò Sài Gòn Xưa", Address = "150 Pasteur, Phường 6, Quận 3, TP.HCM", Latitude = 10.7823, Longitude = 106.6930, ActivationRadiusMeters = 50, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Ốc Đêm Sài Thành", Address = "45 Võ Văn Tần, Phường 6, Quận 3, TP.HCM", Latitude = 10.7790, Longitude = 106.6915, ActivationRadiusMeters = 80, PublishStatus = PublishStatus.Approved },
            new Shop { OwnerId = owner.Id, Name = "Cơm Tấm Ba Ghiền", Address = "84 Đặng Văn Ngữ, Phường 10, Phú Nhuận, TP.HCM", Latitude = 10.7996, Longitude = 106.6790, ActivationRadiusMeters = 100, PublishStatus = PublishStatus.Approved }
        };
        db.Shops.AddRange(shops);
        db.SaveChanges();

        // 3. Tạo sẵn 1 ShopSubmission đã duyệt cho mỗi quán
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
>>>>>>> origin/feature/truc-module-a

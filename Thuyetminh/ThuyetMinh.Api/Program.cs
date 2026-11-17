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


var connectionString = builder.Configuration.GetConnectionString("SqlServerTest");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Chua cau hinh SQL Server.");
}

builder.Services.AddDbContext<AppDbContext, SqlServerAppDbContext>(o =>
    o.UseSqlServer(connectionString));

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
builder.Services.AddScoped<IMachineTranslationService, MachineTranslationService>();
builder.Services.AddScoped<NarrationProcessingService>();
builder.Services.AddHostedService<ThuyetMinh.Api.Workers.NarrationProcessingWorker>();
builder.Services.AddScoped<ITtsService, TtsService>();
builder.Services.AddScoped<LanguageRepository>();
builder.Services.AddScoped<LanguageService>();
builder.Services.AddScoped<AudioRepository>();
builder.Services.AddScoped<AudioService>();
builder.Services.AddScoped<ProcessingJobRepository>();
builder.Services.AddScoped<ProcessingJobService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddSingleton<OnlineUserService>();
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
        // Náº¿u request báº¯t Ä‘áº§u báº±ng /api/ â†’ dĂ¹ng JWT
        if (context.Request.Path.StartsWithSegments("/api"))
            return JwtBearerDefaults.AuthenticationScheme;
        // NgÆ°á»£c láº¡i â†’ dĂ¹ng Cookie
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
                if (svc.IsBlacklisted(raw)) ctx.Fail("Token Ä‘Ă£ bá»‹ thu há»“i");
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
        Description = "Nháº­p: Bearer {token} (khĂ´ng cĂ³ dáº¥u ngoáº·c nhá»n)"
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
            if (!host.EndsWith(".trycloudflare.com", StringComparison.OrdinalIgnoreCase) && !string.Equals(host, app.Configuration["Demo:PublicHost"], StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }
            // FULL-WEBSITE-NGROK-GROUP-TEST: allow the trusted ngrok host to reach
            // Razor Pages and APIs, while retaining the site's normal authentication.
            // Only enabled during a controlled group test with changed demo passwords.
            if (app.Configuration.GetValue<bool>("Demo:AllowFullWebsiteOverNgrok") &&
                string.Equals(host, app.Configuration["Demo:PublicHost"], StringComparison.OrdinalIgnoreCase))
            {
                // Developer tools are not required for group website testing.
                if (context.Request.Path.StartsWithSegments("/swagger") ||
                    context.Request.Path.StartsWithSegments("/Error"))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                await next();
                return;
            }
            // NGROK-PUBLIC-QR-20261027: Dev-only public demo endpoints.
            // The controller still validates each pending order and tm_visitor ownership.
            if (app.Configuration.GetValue<bool>("Demo:EnablePublicQr") &&
                string.Equals(host, app.Configuration["Demo:PublicHost"], StringComparison.OrdinalIgnoreCase) &&
                ((context.Request.Path.StartsWithSegments("/api/payment") &&
                  (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsPost(context.Request.Method))) ||
                 (context.Request.Path == "/api/demo/qr/issue" &&
                  HttpMethods.IsPost(context.Request.Method))))
            {
                await next();
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
                path.StartsWithSegments("/api/listening/sessions") ||
                path == "/api/Language" ||
                path == "/api/Translation/search" ||
                path.StartsWithSegments("/api/Audio/translation") ||
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


// Seed dá»¯ liá»‡u máº«u
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        var admin = new User { Username = "admin", Role = Role.Admin, FullName = "Quáº£n trá»‹ viĂªn", Active = true };
        admin.PasswordHash = hasher.HashPassword(admin, "admin123");
        db.Users.Add(admin);

        var owner = new User { Username = "owner1", Role = Role.Owner, FullName = "Chá»§ quĂ¡n 1", Active = true };
        owner.PasswordHash = hasher.HashPassword(owner, "owner123");
        db.Users.Add(owner);

        db.SaveChanges();

        var shops = new List<Shop>
        {
            new Shop
            {
                OwnerId = owner.Id,
                Name = "QuĂ¡n Ä‚n Gia ÄĂ¬nh Phá»‘",
                Address = "123 LĂ½ Tá»± Trá»ng, PhÆ°á»ng Báº¿n ThĂ nh, Quáº­n 1, TP.HCM",
                Latitude = 10.7743,
                Longitude = 106.6978,
                ActivationRadiusMeters = 50,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "BĂºn Cháº£ HĂ  Ná»™i 36 Phá»‘",
                Address = "36 Nguyá»…n TrĂ£i, PhÆ°á»ng Báº¿n ThĂ nh, Quáº­n 1, TP.HCM",
                Latitude = 10.7712,
                Longitude = 106.6935,
                ActivationRadiusMeters = 60,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "Phá»Ÿ BĂ² SĂ i GĂ²n XÆ°a",
                Address = "150 Pasteur, PhÆ°á»ng 6, Quáº­n 3, TP.HCM",
                Latitude = 10.7823,
                Longitude = 106.6930,
                ActivationRadiusMeters = 50,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "á»c ÄĂªm SĂ i ThĂ nh",
                Address = "45 VĂµ VÄƒn Táº§n, PhÆ°á»ng 6, Quáº­n 3, TP.HCM",
                Latitude = 10.7790,
                Longitude = 106.6915,
                ActivationRadiusMeters = 80,
                PublishStatus = PublishStatus.Approved
            },
            new Shop
            {
                OwnerId = owner.Id,
                Name = "CÆ¡m Táº¥m Ba Ghiá»n",
                Address = "84 Äáº·ng VÄƒn Ngá»¯, PhÆ°á»ng 10, PhĂº Nhuáº­n, TP.HCM",
                Latitude = 10.7996,
                Longitude = 106.6790,
                ActivationRadiusMeters = 100,
                PublishStatus = PublishStatus.Approved
            }
        };
        db.Shops.AddRange(shops);
        db.SaveChanges();

        // 3. Táº¡o sáºµn 1 ShopSubmission Ä‘Ă£ duyá»‡t cho má»—i quĂ¡n (Ä‘á»ƒ API láº¥y content tráº£ vá» dá»¯ liá»‡u)
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
                Description = "QuĂ¡n Äƒn ngon, khĂ´ng gian sáº¡ch sáº½.",
                Status = SubmissionStatus.Approved,
                ReviewedById = admin.Id,
                ReviewedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.ShopSubmissions.Add(sub);
            db.SaveChanges();

            // Gáº¯n version Ä‘Ă£ duyá»‡t vĂ o Shop
            shop.PublishedVersionId = sub.Id;
            db.SaveChanges();
        }
    }

    // Module B (Truc/Nhong): seed languages independently of existing users.
    // Idempotent: do not overwrite translations or audio already created in the database.
    if (!db.Languages.Any(x => x.Code == "vi"))
        db.Languages.Add(new Language { Code = "vi", Name = "Tiáº¿ng Viá»‡t", IsActive = true });
    if (!db.Languages.Any(x => x.Code == "en"))
        db.Languages.Add(new Language { Code = "en", Name = "Tiáº¿ng Anh", IsActive = true });
    if (!db.Languages.Any(x => x.Code == "zh"))
        db.Languages.Add(new Language { Code = "zh", Name = "Tiáº¿ng Trung", IsActive = true });
    if (!db.Languages.Any(x => x.Code == "km"))
        db.Languages.Add(new Language { Code = "km", Name = "Tiáº¿ng Khmer", IsActive = true });
    db.SaveChanges();

    // Attach the audio demo only to a real, published shop version (never to an arbitrary Id).
    var demoShop = db.Shops.FirstOrDefault(x =>
        x.Name == "QuĂ¡n Ä‚n Gia ÄĂ¬nh Phá»‘" && x.PublishStatus == PublishStatus.Approved);
    var publishedVersionId = demoShop?.PublishedVersionId;
    var viLanguage = db.Languages.FirstOrDefault(x => x.Code == "vi");
    if (publishedVersionId != null && viLanguage != null)
    {
        var translation = db.Translations.FirstOrDefault(x =>
            x.ShopSubmissionId == publishedVersionId && x.LanguageId == viLanguage.Id);
        if (translation == null)
        {
            translation = new Translation
            {
                ShopSubmissionId = publishedVersionId.Value,
                LanguageId = viLanguage.Id,
                TranslatedText = "ChĂ o má»«ng Ä‘áº¿n vá»›i QuĂ¡n Ä‚n Gia ÄĂ¬nh Phá»‘. QuĂ¡n chuyĂªn cĂ¡c mĂ³n Äƒn Viá»‡t truyá»n thá»‘ng, khĂ´ng gian áº¥m cĂºng.",
                Status = "Success"
            };
            db.Translations.Add(translation);
            db.SaveChanges();
        }

        var demoAudioFile = Path.Combine(app.Environment.WebRootPath ??
            Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "audio", "quan-mau.mp3");
        if (File.Exists(demoAudioFile) && !db.Audios.Any(x => x.TranslationId == translation.Id))
        {
            db.Audios.Add(new Audio
            {
                TranslationId = translation.Id,
                FilePath = "/audio/quan-mau.mp3",
                DurationSeconds = 5,
                Status = "Success"
            });
            db.SaveChanges();
        }
    }
}

GpsTestSeeder.Run(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
// Paid media is served only through the authorized listening-session endpoint.
app.Use(async (context, next) =>
{
    string path = context.Request.Path.Value ?? string.Empty;
    if (context.Request.Path.StartsWithSegments("/audio") ||
        new[] { ".mp3", ".wav", ".ogg", ".m4a" }
        .Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next();
});
app.UseStaticFiles();
app.UseAuthentication();
app.UseMiddleware<OnlineUserMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/payment"));
app.Run();

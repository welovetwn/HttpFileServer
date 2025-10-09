using HttpFileServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// 設定 Form 上傳限制
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5L * 1024 * 1024 * 1024; // 5 GB
    // 你也可以設定 ValueLengthLimit, MemoryBufferThreshold 等，視需要而定
});

// 設定 Kestrel 限制
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5L * 1024 * 1024 * 1024; // 5 GB
});

// 加入 MVC (Controller + Views)
builder.Services.AddControllersWithViews();

// 註冊 ConfigService，注入 IConfiguration 與 IWebHostEnvironment
builder.Services.AddSingleton<ConfigService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var env = provider.GetRequiredService<IWebHostEnvironment>();
    return new ConfigService(config, env);
});

// 假設 AuthSessionTracker 是你自己管理 session 的服務
builder.Services.AddSingleton<AuthSessionTracker>();

// 加入 Cookie 認證
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";             // 未登入時跳轉
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/denied";     // 權限不足時跳轉
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
using HttpFileServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// 加入自訂設定檔
builder.Configuration
    .AddJsonFile("user_settings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("folder_settings.json", optional: false, reloadOnChange: true);

// 其他設定（Form 限制、Kestrel、MVC）
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5L * 1024 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5L * 1024 * 1024 * 1024;
});

builder.Services.AddControllersWithViews();

// 註冊 ConfigService（下一步會修改）
builder.Services.AddSingleton<ConfigService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var env = provider.GetRequiredService<IWebHostEnvironment>();
    return new ConfigService(config, env);
});

// 認證
builder.Services.AddSingleton<AuthSessionTracker>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/denied";
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

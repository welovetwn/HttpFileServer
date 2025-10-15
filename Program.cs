// C:\Projects\HttpFileServer\Program.cs

using HttpFileServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
using System.Diagnostics;
using System.Text;

string userSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "user_settings.json");

if (!File.Exists(userSettingsPath))
{
    Console.WriteLine("🔧 尚未設定 user_settings.json，將執行 GUI 設定工具...");

    string ps1Path = Path.Combine(Directory.GetCurrentDirectory(), "admin_setup.ps1");

    // 如果腳本尚不存在就建立
    if (!File.Exists(ps1Path))
    {
        string ps1Content = @"
# \HttpFileServer\admin_setup.ps1
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# 建立表單
$form = New-Object System.Windows.Forms.Form
$form.Text = 'Admin 管理者初始設定'
$form.Width = 420
$form.Height = 250
$form.StartPosition = 'CenterScreen'
$form.Font = New-Object System.Drawing.Font(""Segoe UI"", 12)
$form.KeyPreview = $true

# 定義共用的 KeyDown 處理器：Enter 鍵當作 Tab 鍵
$handleEnterAsTab = {
    if ($_.KeyCode -eq 'Enter') {
        $_.SuppressKeyPress = $true  # 防止叮咚聲
        $form.SelectNextControl($form.ActiveControl, $true, $true, $true, $false)
    }
}

# Admin Username Label
$labelUser = New-Object System.Windows.Forms.Label
$labelUser.Text = '管理者帳號:'
$labelUser.Top = 30
$labelUser.Left = 20
$labelUser.Width = 150
$form.Controls.Add($labelUser)

# Admin Username TextBox
$textUser = New-Object System.Windows.Forms.TextBox
$textUser.Top = 25
$textUser.Left = 180
$textUser.Width = 200
$textUser.Font = $form.Font
$textUser.Add_KeyDown($handleEnterAsTab)
$form.Controls.Add($textUser)

# Admin Password Label
$labelPass = New-Object System.Windows.Forms.Label
$labelPass.Text = '管理者密碼:'
$labelPass.Top = 80
$labelPass.Left = 20
$labelPass.Width = 150
$form.Controls.Add($labelPass)

# Admin Password TextBox
$textPass = New-Object System.Windows.Forms.TextBox
$textPass.Top = 75
$textPass.Left = 180
$textPass.Width = 200
$textPass.UseSystemPasswordChar = $true
$textPass.Font = $form.Font
$textPass.Add_KeyDown($handleEnterAsTab)
$form.Controls.Add($textPass)

# 建立按鈕
$btnOK = New-Object System.Windows.Forms.Button
$btnOK.Text = '建立'
$btnOK.Top = 130
$btnOK.Left = 180
$btnOK.Width = 100
$btnOK.Height = 40
$btnOK.Font = $form.Font
$btnOK.Add_KeyDown($handleEnterAsTab)
$form.Controls.Add($btnOK)

# Click 行為
$btnOK.Add_Click({
    if (-not $textUser.Text -or -not $textPass.Text) {
        [System.Windows.Forms.MessageBox]::Show('請輸入帳號與密碼', '錯誤', 'OK', 'Error')
        return
    }

    $json = @{
        Users = @(
            @{
                Username   = $textUser.Text
                Password   = $textPass.Text
                Role       = 'Admin'
                Permission = '99'
            }
        )
    } | ConvertTo-Json -Depth 3

    Set-Content -Path 'user_settings.json' -Value $json -Encoding UTF8
    [System.Windows.Forms.MessageBox]::Show(""按【確定】鍵,程式將自動重新執行！"", ""✅ 帳號設定完成！"", ""OK"", ""Information"")
    $form.Close()
})

$form.Topmost = $true
[void]$form.ShowDialog()
# === 關閉對話框後：延遲3秒並啟動 HttpFileServer.exe ===
Start-Sleep -Seconds 3

$exePath = Join-Path -Path $PSScriptRoot -ChildPath ""HttpFileServer.exe""
if (Test-Path $exePath) {
    Start-Process -FilePath $exePath
} else {
    [System.Windows.Forms.MessageBox]::Show(""⚠ 找不到 HttpFileServer.exe"", ""啟動失敗"", ""OK"", ""Warning"")
}
";

        File.WriteAllText(ps1Path, ps1Content, Encoding.UTF8);
    }

    // 執行 PowerShell UI
    Process.Start(new ProcessStartInfo
    {
        FileName = "powershell.exe",
        Arguments = $"-ExecutionPolicy Bypass -File \"{ps1Path}\"",
        UseShellExecute = true
    });

    Console.WriteLine("⚠️ 請完成帳號設定後重新啟動應用程式。");
    return;
}

// 開始正常啟動流程
var builder = WebApplication.CreateBuilder(args);

// 加入設定檔
builder.Configuration
    .AddJsonFile("user_settings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("folder_settings.json", optional: true, reloadOnChange: true); // 可選（已自動產生）

// 表單大小與 Kestrel 限制
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5L * 1024 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 5L * 1024 * 1024 * 1024;
    serverOptions.ListenAnyIP(5000);
});

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ConfigService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var env = provider.GetRequiredService<IWebHostEnvironment>();
    return new ConfigService(config, env);
});

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

PrintLocalIPv4Addresses(5000);

// 自動開啟瀏覽器
bool autoOpenBrowser = true;
if (autoOpenBrowser)
{
    var first = GetLocalIPv4Addresses().FirstOrDefault();
    if (!string.IsNullOrEmpty(first))
    {
        var url = $"http://{first}:5000/";
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch
        {
            // 忽略瀏覽器錯誤
        }
    }
}

app.Run();

static void PrintLocalIPv4Addresses(int port)
{
    var ips = GetLocalIPv4Addresses();
    Console.WriteLine("--------------------------------------------------");
    Console.WriteLine("Server 已啟動 (Kestrel 綁定 0.0.0.0)。可在區網內以以下位址連線：");
    if (ips.Count == 0)
    {
        Console.WriteLine($"  (未偵測到 IPv4 位址，請確認網路介面是否啟用)");
    }
    else
    {
        foreach (var ip in ips)
        {
            Console.WriteLine($"  http://{ip}:{port}/");
        }
    }
    Console.WriteLine("若防火牆阻擋請允許此程式的連接埠（預設: 5000）");
    Console.WriteLine("--------------------------------------------------");
}

static List<string> GetLocalIPv4Addresses()
{
    var result = new List<string>();
    try
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                result.Add(ip.ToString());
            }
        }
    }
    catch
    {
        // 忽略
    }

    if (result.Count == 0)
    {
        result.Add("127.0.0.1");
    }

    return result;
}
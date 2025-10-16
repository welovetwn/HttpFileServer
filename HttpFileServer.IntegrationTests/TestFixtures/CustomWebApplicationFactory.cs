// HttpFileServer.IntegrationTests/TestFixtures/CustomWebApplicationFactory.cs
using HttpFileServer.Models;
using HttpFileServer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace HttpFileServer.IntegrationTests.TestFixtures;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> 
    where TProgram : class
{
    private readonly string _testDirectory;

    public CustomWebApplicationFactory()
    {
        // 建立測試用的臨時目錄
        _testDirectory = Path.Combine(Path.GetTempPath(), $"HttpFileServerIntegrationTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        // 建立測試用的設定檔
        InitializeTestData();
    }

    private void InitializeTestData()
    {
        // 建立測試用的 user_settings.json
        var userSettings = new UserSettings
        {
            Users = new List<User>
            {
                new User 
                { 
                    Username = "testadmin", 
                    Password = "testpass", 
                    Permission = PermissionLevel.Admin.ToString()  
                },
                new User 
                { 
                    Username = "testuser", 
                    Password = "userpass", 
                    Permission = PermissionLevel.ReadOnly.ToString()  
                }
            }
        };
        
        var userSettingsPath = Path.Combine(_testDirectory, "user_settings.json");
        System.IO.File.WriteAllText(userSettingsPath, System.Text.Json.JsonSerializer.Serialize(userSettings, 
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // 建立測試用的 folder_settings.json
        var folderSettings = new FolderSettings
        {
            SharedFolders = new List<SharedFolder>
            {
                new SharedFolder
                {
                    Name = "TestFolder",
                    Path = Path.Combine(_testDirectory, "TestFolder"),
                    AccessList = new List<FolderAccess>
                    {
                        new FolderAccess { Username = "testadmin", Permission = PermissionLevel.Admin },
                        new FolderAccess { Username = "testuser", Permission = PermissionLevel.ReadOnly }
                    }
                }
            }
        };
        
        Directory.CreateDirectory(Path.Combine(_testDirectory, "TestFolder"));
        
        var folderSettingsPath = Path.Combine(_testDirectory, "folder_settings.json");
        System.IO.File.WriteAllText(folderSettingsPath, System.Text.Json.JsonSerializer.Serialize(folderSettings,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // 建立測試用的 config.json
        var config = new Config
        {
            BaseFolderPath = _testDirectory
        };
        
        var configPath = Path.Combine(_testDirectory, "config.json");
        System.IO.File.WriteAllText(configPath, System.Text.Json.JsonSerializer.Serialize(config,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 移除真實的 ConfigService
            services.RemoveAll<ConfigService>();
            
            // 使用測試目錄建立新的 ConfigService
            services.AddSingleton<ConfigService>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var mockEnv = new TestWebHostEnvironment(_testDirectory);
                return new ConfigService(configuration, mockEnv);
            });
        });

        // 使用測試環境的內容根路徑
        builder.UseContentRoot(_testDirectory);
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 清理測試目錄
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch { /* 忽略清理錯誤 */ }
        }
        base.Dispose(disposing);
    }
}

// 測試用的 IWebHostEnvironment 實作
public class TestWebHostEnvironment : IWebHostEnvironment
{
    public TestWebHostEnvironment(string contentRootPath)
    {
        ContentRootPath = contentRootPath;
        WebRootPath = Path.Combine(contentRootPath, "wwwroot");
        EnvironmentName = "Testing";
        ApplicationName = "HttpFileServer";
    }

    public string WebRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; }
}
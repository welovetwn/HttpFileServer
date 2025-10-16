# fix-tests-for-real-code.ps1
# 根據實際的 ConfigService 修正測試程式碼
# 設定控制台編碼為 UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 | Out-Null

Write-Host "測試中文顯示" -ForegroundColor Green

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "修正測試程式碼以符合實際專案結構" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# 定義修正後的測試檔案
$files = @{
    "HttpFileServer.Tests/Services/ConfigServiceTests.cs" = @"
// HttpFileServer.Tests/Services/ConfigServiceTests.cs
using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using HttpFileServer.Services;
using HttpFileServer.Models;

namespace HttpFileServer.Tests.Services;

public class ConfigServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ConfigService _configService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public ConfigServiceTests()
    {
        // 建立測試用的臨時目錄
        _testDirectory = Path.Combine(Path.GetTempPath(), `$"HttpFileServerTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);

        // 設定 Mock
        _mockConfiguration = new Mock<IConfiguration>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(_testDirectory);

        // 建立 ConfigService
        _configService = new ConfigService(_mockConfiguration.Object, _mockEnvironment.Object);
    }

    [Fact]
    public void GetUsers_應該回傳使用者列表()
    {
        // Act
        var users = _configService.GetUsers();

        // Assert
        users.Should().NotBeNull();
        users.Should().BeOfType<List<User>>();
    }

    [Fact]
    public void AddUser_應該成功新增使用者()
    {
        // Arrange
        var newUser = new User
        {
            Username = "testuser",
            Password = "testpass",
            Permission = PermissionLevel.ReadOnly.ToString()
        };

        // Act
        _configService.AddUser(newUser);
        var users = _configService.GetUsers();

        // Assert
        users.Should().Contain(u => u.Username == "testuser");
    }

    [Fact]
    public void GetUser_當使用者存在_應該回傳該使用者()
    {
        // Arrange
        var user = new User
        {
            Username = "existinguser",
            Password = "password",
            Permission = PermissionLevel.ReadOnly.ToString()
        };
        _configService.AddUser(user);

        // Act
        var result = _configService.GetUser("existinguser");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("existinguser");
    }

    [Fact]
    public void GetUser_當使用者不存在_應該回傳Null()
    {
        // Act
        var result = _configService.GetUser("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserExists_當使用者存在_應該回傳True()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Password = "pass",
            Permission = PermissionLevel.ReadOnly.ToString()
        };
        _configService.AddUser(user);

        // Act
        var result = _configService.UserExists("testuser");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void UserExists_當使用者不存在_應該回傳False()
    {
        // Act
        var result = _configService.UserExists("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateUserPassword_應該成功更新密碼()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Password = "oldpass",
            Permission = PermissionLevel.ReadOnly.ToString()
        };
        _configService.AddUser(user);

        // Act
        var result = _configService.UpdateUserPassword("testuser", "newpass");
        var updatedUser = _configService.GetUser("testuser");

        // Assert
        result.Should().BeTrue();
        updatedUser!.Password.Should().Be("newpass");
    }

    [Fact]
    public void UpdateUserPassword_當使用者不存在_應該回傳False()
    {
        // Act
        var result = _configService.UpdateUserPassword("nonexistent", "newpass");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DeleteUser_應該成功刪除使用者()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Password = "pass",
            Permission = PermissionLevel.ReadOnly.ToString()
        };
        _configService.AddUser(user);

        // Act
        var result = _configService.DeleteUser("testuser");

        // Assert
        result.Should().BeTrue();
        _configService.UserExists("testuser").Should().BeFalse();
    }

    [Fact]
    public void DeleteUser_當使用者不存在_應該回傳False()
    {
        // Act
        var result = _configService.DeleteUser("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateUser_當帳號密碼正確_應該回傳使用者()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Password = "correctpass",
            Permission = PermissionLevel.ReadOnly.ToString()
        };
        _configService.AddUser(user);

        // Act
        var result = _configService.ValidateUser("testuser", "correctpass");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public void ValidateUser_當密碼錯誤_應該回傳Null()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Password = "correctpass",
            Permission = PermissionLevel.ReadOnly.ToString()
        };
        _configService.AddUser(user);

        // Act
        var result = _configService.ValidateUser("testuser", "wrongpass");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFolders_應該回傳資料夾列表()
    {
        // Act
        var folders = _configService.GetFolders();

        // Assert
        folders.Should().NotBeNull();
        folders.Should().BeOfType<List<SharedFolder>>();
    }

    [Fact]
    public void AddFolder_應該成功新增資料夾()
    {
        // Arrange
        var folder = new SharedFolder
        {
            Name = "TestFolder",
            Path = "C:\\Test",
            AccessList = new List<FolderAccess>()
        };

        // Act
        _configService.AddFolder(folder);
        var folders = _configService.GetFolders();

        // Assert
        folders.Should().Contain(f => f.Name == "TestFolder");
    }

    [Fact]
    public void GetFolderByName_當資料夾存在_應該回傳該資料夾()
    {
        // Arrange
        var folder = new SharedFolder
        {
            Name = "TestFolder",
            Path = "C:\\Test",
            AccessList = new List<FolderAccess>()
        };
        _configService.AddFolder(folder);

        // Act
        var result = _configService.GetFolderByName("TestFolder");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestFolder");
    }

    [Fact]
    public void UpdateFolderAccessList_應該成功更新權限清單()
    {
        // Arrange
        var folder = new SharedFolder
        {
            Name = "TestFolder",
            Path = "C:\\Test",
            AccessList = new List<FolderAccess>()
        };
        _configService.AddFolder(folder);

        var newAccessList = new List<FolderAccess>
        {
            new FolderAccess { Username = "user1", Permission = PermissionLevel.ReadOnly }
        };

        // Act
        var result = _configService.UpdateFolderAccessList("TestFolder", newAccessList);
        var updatedFolder = _configService.GetFolderByName("TestFolder");

        // Assert
        result.Should().BeTrue();
        updatedFolder!.AccessList.Should().HaveCount(1);
        updatedFolder.AccessList[0].Username.Should().Be("user1");
    }

    [Fact]
    public void DeleteFolder_應該成功刪除資料夾()
    {
        // Arrange
        var folder = new SharedFolder
        {
            Name = "TestFolder",
            Path = "C:\\Test",
            AccessList = new List<FolderAccess>()
        };
        _configService.AddFolder(folder);

        // Act
        var result = _configService.DeleteFolder("TestFolder");

        // Assert
        result.Should().BeTrue();
        _configService.GetFolderByName("TestFolder").Should().BeNull();
    }

    [Fact]
    public void GetAccessibleFolders_Admin權限_應該看到所有資料夾()
    {
        // Arrange
        var folder1 = new SharedFolder
        {
            Name = "Folder1",
            Path = "C:\\Folder1",
            AccessList = new List<FolderAccess>()
        };
        var folder2 = new SharedFolder
        {
            Name = "Folder2",
            Path = "C:\\Folder2",
            AccessList = new List<FolderAccess>()
        };
        _configService.AddFolder(folder1);
        _configService.AddFolder(folder2);

        // Act
        var result = _configService.GetAccessibleFolders("admin", (int)PermissionLevel.Admin);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(f => f.Permission == PermissionLevel.Admin);
    }

    [Fact]
    public void GetAccessibleFolders_一般使用者_應該只看到有權限的資料夾()
    {
        // Arrange
        var folder1 = new SharedFolder
        {
            Name = "Folder1",
            Path = "C:\\Folder1",
            AccessList = new List<FolderAccess>
            {
                new FolderAccess { Username = "user1", Permission = PermissionLevel.ReadOnly }
            }
        };
        var folder2 = new SharedFolder
        {
            Name = "Folder2",
            Path = "C:\\Folder2",
            AccessList = new List<FolderAccess>
            {
                new FolderAccess { Username = "user2", Permission = PermissionLevel.ReadOnly }
            }
        };
        _configService.AddFolder(folder1);
        _configService.AddFolder(folder2);

        // Act
        var result = _configService.GetAccessibleFolders("user1", (int)PermissionLevel.ReadOnly);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Folder1");
    }

    [Fact]
    public void GetBaseFolderPath_應該回傳基礎資料夾路徑()
    {
        // Act
        var result = _configService.GetBaseFolderPath();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be(_testDirectory);
    }

    public void Dispose()
    {
        // 清理測試產生的檔案
        if (Directory.Exists(_testDirectory))
        {
            try
            {
                Directory.Delete(_testDirectory, true);
            }
            catch { /* 忽略清理錯誤 */ }
        }
    }
}
"@

    "HttpFileServer.Tests/Services/PermissionServiceTests.cs" = @"
// HttpFileServer.Tests/Services/PermissionServiceTests.cs
using Xunit;
using FluentAssertions;
using HttpFileServer.Models;

namespace HttpFileServer.Tests.Services;

public class PermissionLevelTests
{
    [Theory]
    [InlineData(PermissionLevel.Admin, PermissionLevel.ReadOnly, true)]
    [InlineData(PermissionLevel.FullAccess, PermissionLevel.ReadOnly, true)]
    [InlineData(PermissionLevel.ReadOnly, PermissionLevel.FullAccess, false)]
    [InlineData(PermissionLevel.ReadOnly, PermissionLevel.ReadOnly, true)]
    [InlineData(PermissionLevel.None, PermissionLevel.ReadOnly, false)]
    public void PermissionLevel_比較測試(PermissionLevel userPerm, PermissionLevel requiredPerm, bool expected)
    {
        // Act
        var result = (int)userPerm >= (int)requiredPerm;

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void PermissionLevel_Admin應該是最高權限()
    {
        // Assert
        ((int)PermissionLevel.Admin).Should().BeGreaterThan((int)PermissionLevel.FullAccess);
        ((int)PermissionLevel.Admin).Should().BeGreaterThan((int)PermissionLevel.ReadOnly);
        ((int)PermissionLevel.Admin).Should().BeGreaterThan((int)PermissionLevel.None);
    }

    [Fact]
    public void PermissionLevel_FullAccess應該大於ReadOnly()
    {
        // Assert
        ((int)PermissionLevel.FullAccess).Should().BeGreaterThan((int)PermissionLevel.ReadOnly);
    }

    [Fact]
    public void PermissionLevel_None應該是最低權限()
    {
        // Assert
        ((int)PermissionLevel.None).Should().BeLessThan((int)PermissionLevel.ReadOnly);
        ((int)PermissionLevel.None).Should().BeLessThan((int)PermissionLevel.FullAccess);
        ((int)PermissionLevel.None).Should().BeLessThan((int)PermissionLevel.Admin);
    }
}
"@

    "HttpFileServer.Tests/TestUtilities/TestDataBuilder.cs" = @"
// HttpFileServer.Tests/TestUtilities/TestDataBuilder.cs
using HttpFileServer.Models;

namespace HttpFileServer.Tests.TestUtilities;

public class TestDataBuilder
{
    public static User CreateUser(
        string username = "testuser",
        string password = "testpass",
        PermissionLevel permission = PermissionLevel.ReadOnly)
    {
        return new User
        {
            Username = username,
            Password = password,
            Permission = permission.ToString()
        };
    }

    public static List<User> CreateUserList(int count = 3)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateUser(
                username: $"user{i}",           // ✅ 使用冒號
                password: $"pass{i}",           // ✅ 使用冒號
                permission: i == 0 ? PermissionLevel.Admin : PermissionLevel.ReadOnly  // ✅ 使用冒號
            ));
        }
        return users;
    }

    public static SharedFolder CreateSharedFolder(
        string name = "TestFolder",
        string path = "C:\\Test",
        params (string Username, PermissionLevel Permission)[] accessList)
    {
        return new SharedFolder
        {
            Name = name,
            Path = path,
            AccessList = accessList.Select(a => new FolderAccess
            {
                Username = a.Username,
                Permission = a.Permission
            }).ToList()
        };
    }

    public static List<SharedFolder> CreateFolderList()
    {
        return new List<SharedFolder>
        {
            CreateSharedFolder("Public", "C:\\Public", ("admin", PermissionLevel.Admin), ("user1", PermissionLevel.ReadOnly)),
            CreateSharedFolder("Private", "C:\\Private", ("admin", PermissionLevel.Admin)),
            CreateSharedFolder("Shared", "C:\\Shared", ("admin", PermissionLevel.Admin), ("user1", PermissionLevel.FullAccess))
        };
    }
}
"@
    "HttpFileServer.IntegrationTests/TestFixtures/CustomWebApplicationFactory.cs" = @"
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
        _testDirectory = Path.Combine(Path.GetTempPath(), `$"HttpFileServerIntegrationTests_{Guid.NewGuid()}");
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
"@

    "HttpFileServer.IntegrationTests/Api/AdminApiTests.cs" = @"
// HttpFileServer.IntegrationTests/Api/AdminApiTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using HttpFileServer.Models;
using HttpFileServer.IntegrationTests.TestFixtures;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HttpFileServer.IntegrationTests.Api;

public class AdminApiTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AdminApiTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetUsers_未登入_應該重導向到登入頁()
    {
        // Act
        var response = await _client.GetAsync("/admin/users");

        // Assert
        // 根據您的認證設定，可能是 Unauthorized 或 Redirect
        (response.StatusCode == HttpStatusCode.Unauthorized || 
         response.StatusCode == HttpStatusCode.Redirect).Should().BeTrue();
    }

    // 注意: 以下測試需要實際的 Controller 和認證機制才能正確運作
    // 暫時註解掉，等您提供 Controller 程式碼後再啟用

    /*
    [Fact]
    public async Task Login_使用正確帳密_應該成功()
    {
        // Arrange
        var loginData = new { username = "testadmin", password = "testpass" };

        // Act
        var response = await _client.PostAsJsonAsync("/account/login", loginData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    */
}
"@

    "HttpFileServer.IntegrationTests/Auth/AuthenticationTests.cs" = @"
// HttpFileServer.IntegrationTests/Auth/AuthenticationTests.cs
using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using HttpFileServer.IntegrationTests.TestFixtures;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HttpFileServer.IntegrationTests.Auth;

public class AuthenticationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthenticationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Application_應該可以正常啟動()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        // 只要不是 500 錯誤就算成功啟動
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // 注意: 認證測試需要實際的 AccountController
    // 請提供您的認證 Controller 後再完善這些測試
}
"@

    "HttpFileServer.IntegrationTests/Scenarios/UserWorkflowTests.cs" = @"
// HttpFileServer.IntegrationTests/Scenarios/UserWorkflowTests.cs
using System.Net;
using Xunit;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using HttpFileServer.IntegrationTests.TestFixtures;

namespace HttpFileServer.IntegrationTests.Scenarios;

public class UserWorkflowTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UserWorkflowTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task 首頁應該可以存取()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }

    // 完整的工作流程測試需要：
    // 1. AccountController (登入/登出)
    // 2. DashboardController (儀表板)
    // 3. FileController (檔案操作)
    // 請提供這些 Controller 的程式碼後，我會補充完整的測試
}
"@

    "HttpFileServer.IntegrationTests/Performance/LoadTests.cs" = @"
// HttpFileServer.IntegrationTests/Performance/LoadTests.cs
using System.Diagnostics;
using Xunit;
using FluentAssertions;
using HttpFileServer.IntegrationTests.TestFixtures;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace HttpFileServer.IntegrationTests.Performance;

public class LoadTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LoadTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task 並行首頁請求_應該在合理時間內完成()
    {
        // Arrange
        var concurrentRequests = 20;
        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => _client.GetAsync("/"))
            .ToArray();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5秒內完成
        tasks.All(t => t.Result.StatusCode != System.Net.HttpStatusCode.InternalServerError).Should().BeTrue();
    }
}
"@
}

Write-Host "開始修正測試檔案..." -ForegroundColor Yellow
Write-Host ""

$updatedFiles = 0
foreach ($filePath in $files.Keys) {
    $updatedFiles++
    $fullPath = Join-Path $PWD $filePath
    $directory = Split-Path $fullPath -Parent
    
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    
    $content = $files[$filePath]
    [System.IO.File]::WriteAllText($fullPath, $content, [System.Text.Encoding]::UTF8)
    
    Write-Host "✓ 已更新: $filePath" -ForegroundColor Green
}

# 移除不適用的 Controller 測試（因為需要更多實際程式碼才能正確實作）
Write-Host ""
Write-Host "移除暫時不適用的 Controller 測試..." -ForegroundColor Yellow

$controllersToRemove = @(
    "HttpFileServer.Tests/Controllers/AdminControllerTests.cs",
    "HttpFileServer.Tests/Controllers/DashboardControllerTests.cs",
    "HttpFileServer.Tests/Controllers/FileDownloadTests.cs"
)

foreach ($file in $controllersToRemove) {
    $fullPath = Join-Path $PWD $file
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Force
        Write-Host "✓ 已移除: $file" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "測試檔案修正完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "已完成:" -ForegroundColor Yellow
Write-Host "  ✓ 更新 ConfigServiceTests.cs (基於實際的 ConfigService)" -ForegroundColor Green
Write-Host "  ✓ 更新 PermissionLevelTests.cs (測試權限列舉)" -ForegroundColor Green
Write-Host "  ✓ 更新 TestDataBuilder.cs (使用正確的模型)" -ForegroundColor Green
Write-Host "  ✓ 移除不適用的 Controller 測試" -ForegroundColor Gray
Write-Host ""
Write-Host "注意事項:" -ForegroundColor Yellow
Write-Host "  - Controller 測試需要您提供實際的 Controller 程式碼後再建立" -ForegroundColor Gray
Write-Host "  - 目前的測試已可以測試 ConfigService 的核心功能" -ForegroundColor Gray
Write-Host ""
$response = Read-Host "是否立即執行測試? (Y/N)"
if ($response -eq 'Y' -or $response -eq 'y') {
    Write-Host ""
    Write-Host "執行單元測試..." -ForegroundColor Cyan
    Write-Host ""
    dotnet test HttpFileServer.Tests/HttpFileServer.Tests.csproj --verbosity normal 
}
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
        _testDirectory = Path.Combine(Path.GetTempPath(), $"HttpFileServerTests_{Guid.NewGuid()}");
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
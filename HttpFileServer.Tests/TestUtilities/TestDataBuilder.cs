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
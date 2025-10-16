//\Models\PermissionLevel.cs
namespace HttpFileServer.Models
{
    public enum PermissionLevel
    {
        Admin = 99,
        FullAccess = 9,
        ReadOnly = 1,
        DownloadOnly = 2,
        None = 0
    }
}

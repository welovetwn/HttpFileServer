// Extensions/UserExtensions.cs
using HttpFileServer.Models;

namespace HttpFileServer.Extensions
{
    public static class UserExtensions
    {
        public static int GetPermissionLevel(this User user)
        {
            return int.TryParse(user.Permission, out var level) ? level : 0;
        }
    }
}
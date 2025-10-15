//Models/DashboardViewModel.cs
namespace HttpFileServer.Models
{
    public class DashboardViewModel
    {
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public int PermissionLevel { get; set; }  // ✅ 加入這一行

        public List<FolderAccessViewModel> AccessibleFolders { get; set; } = new();
    }
}


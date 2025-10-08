//Models/DashboardViewModel.cs
namespace HttpFileServer.Models
{
    public class DashboardViewModel
    {
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public List<SharedFolder> AccessibleFolders { get; set; } = new();
    }
}
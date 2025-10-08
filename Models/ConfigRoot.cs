//Models\ConfigRoot.cs
namespace HttpFileServer.Models
{
    public class ConfigRoot
    {
        public List<User> Users { get; set; } = new();
        public List<SharedFolder> SharedFolders { get; set; } = new();
    }
}
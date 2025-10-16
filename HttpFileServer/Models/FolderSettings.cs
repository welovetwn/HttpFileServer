//Models/FolderSettings.cs
namespace HttpFileServer.Models
{
    public class FolderSettings
    {
        public List<SharedFolder> SharedFolders { get; set; } = new();
    }
}


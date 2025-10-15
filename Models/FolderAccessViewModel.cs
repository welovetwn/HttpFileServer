//Models\FolderAccessViewModel.cs
namespace HttpFileServer.Models
{
    public class FolderAccessViewModel
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public PermissionLevel Permission { get; set; } = PermissionLevel.None;
    }
}
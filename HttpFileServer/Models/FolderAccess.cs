namespace HttpFileServer.Models
{
    public class FolderAccess
    {
        public string Username { get; set; } = "";
        public PermissionLevel Permission { get; set; } = PermissionLevel.None;
    }
}

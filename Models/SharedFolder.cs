//Models\SharedFolder.cs
namespace HttpFileServer.Models
{
    public class SharedFolder
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public List<string> AllowedUsers { get; set; } = new();
    }
}
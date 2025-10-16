//Models\User.cs
namespace HttpFileServer.Models
{
    public class User
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public string? Permission { get; set; }
    }
}

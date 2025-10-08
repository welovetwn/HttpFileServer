//Models/LoginViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace HttpFileServer.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }
    }
}

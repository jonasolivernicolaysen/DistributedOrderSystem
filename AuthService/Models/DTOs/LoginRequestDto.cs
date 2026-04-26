using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
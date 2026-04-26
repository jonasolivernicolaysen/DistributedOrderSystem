using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs
{
    public class ErrorResponseDto
    {
        [Required]
        public string Message { get; set; }
    }
}

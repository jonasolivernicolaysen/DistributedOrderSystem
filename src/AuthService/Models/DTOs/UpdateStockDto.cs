using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs
{
    public class UpdateStockDto
    {
        [Required]
        public int UpdatedStock { get; set; }
    }
}

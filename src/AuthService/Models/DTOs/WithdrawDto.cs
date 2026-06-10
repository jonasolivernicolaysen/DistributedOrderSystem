namespace AuthService.Models.DTOs
{
    public class WithdrawDto
    {
        public required string UserId { get; set; }
        public required decimal Amount { get; set; }
    }
}

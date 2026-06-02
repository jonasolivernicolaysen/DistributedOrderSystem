namespace PaymentService.Models.DTOs
{
    public class WithdrawDto
    {
        public required string Username { get; set; }
        public required double Amount { get; set; }
    }
}

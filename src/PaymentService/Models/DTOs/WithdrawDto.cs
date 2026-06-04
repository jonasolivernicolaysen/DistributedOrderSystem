namespace PaymentService.Models.DTOs
{
    public class WithdrawDto
    {
        public required string UserId { get; set; }
        public required double Amount { get; set; }
    }
}

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Exceptions;
using PaymentService.Models;
using PaymentService.Models.DTOs;
using SharedContracts;
using System.Text;
using System.Text.Json;

namespace PaymentService.Services
{
    public class PaymentLogic
    {
        private readonly PaymentDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentLogic(
            PaymentDbContext context,
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(PaymentModel payment, bool shouldBeProcessed)> ProcessOrderPayment(PaymentDto dto, string currentUserId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId);

            if (payment == null)
                throw new NotFoundException(dto.PaymentId.ToString());

            if (payment.UserId != currentUserId)
                throw new UnauthorizedException("User is not authorized to process this payment.");

            if (payment.Status == PaymentStatus.Completed)
                return (payment, false);

            // withdraw here
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://localhost:7144/api/auth/withdraw");

            var token = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["Authorization"]
                .ToString();

            // forward the body
            var withdrawDto = new WithdrawDto
            {
                UserId = currentUserId,
                Amount = payment.TotalAmount
            };
            var json = JsonSerializer.Serialize(withdrawDto);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // forward jwt token
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new BadRequestException(error);
            }

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.PayingAccount = dto.PayingAccount;

            var evt = new PaymentCompletedEvent 
            {
                OrderId = payment.OrderId,
                PaymentId = payment.PaymentId,
                ProductId = payment.ProductId,
                Quantity = payment.Quantity
            };

            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(PaymentCompletedEvent),
                Payload = JsonSerializer.Serialize(evt),
                CreatedAt = DateTime.UtcNow,
                Processed = false
            });

            await _context.SaveChangesAsync();

            return (payment, true);
        }
    }
}

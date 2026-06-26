using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentService.Data;
using PaymentService.Exceptions;
using PaymentService.Messaging;
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
        private readonly ILogger<PaymentLogic> _logger;

        public PaymentLogic(
            PaymentDbContext context,
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PaymentLogic> logger
            )
        {
            _context = context;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(PaymentModel payment, bool shouldBeProcessed)> ProcessOrderPayment(PaymentDto dto, string currentUserId)
        {
            try
            {
                
                var payment = await _context.Payments
                    .Include(p => p.Items)
                    .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId);

                if (payment == null)
                    throw new NotFoundException(dto.PaymentId.ToString());

                if (payment.UserId != currentUserId)
                    throw new UnauthorizedException("User is not authorized to process this payment.");

                if (payment.Status == PaymentStatus.Paid)
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
                    Amount = payment.TotalPrice
                };
                var json = JsonSerializer.Serialize(withdrawDto);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // forward jwt token
                request.Headers.Add("Authorization", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(content);

                    var detail = doc.RootElement
                        .GetProperty("detail")
                        .GetString();

                    throw new BadRequestException(detail ?? "Payment failed");
                }

                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = DateTime.UtcNow;

                var evt = new PaymentCompletedEvent 
                {
                    OrderId = payment.OrderId,
                    PaymentId = payment.PaymentId,
                    TotalPrice = payment.TotalPrice,
                    UserId = payment.UserId,
                    Items = payment.Items.Select(i => new PaymentCompletedItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList(),
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
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        public async Task<PaymentModel> GetPaymentDetailsAsync(Guid paymentId)
        {
            var payment = await _context
                .Payments
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null)
                throw new NotFoundException($"Payment with id {paymentId} not found");
            return payment;
        }
    }
}

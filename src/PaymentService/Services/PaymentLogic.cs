using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Exceptions;
using PaymentService.Models;
using PaymentService.Models.DTOs;
using SharedContracts;
using System.Text.Json;

namespace PaymentService.Services
{
    public class PaymentLogic
    {
        private readonly PaymentDbContext _context;
        private readonly HttpClient _httpClient;

        public PaymentLogic(
            PaymentDbContext context,
            HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<(PaymentModel payment, bool shouldBeProcessed)> ProcessOrderPayment(PaymentDto model)
        {
            // call authservice withdraw endpoint here



            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == model.PaymentId);

            if (payment == null)
                throw new NotFoundException(model.PaymentId.ToString());

            if (payment.Status == PaymentStatus.Completed)
                return (payment, false);

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.PayingAccount = model.PayingAccount;

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

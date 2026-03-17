using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Models.DTOs;
using SharedContracts;
using System.Text.Json;

namespace PaymentService.Services
{
    public class PaymentLogic
    {
        private readonly PaymentDbContext _context;

        public PaymentLogic(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<(PaymentModel payment, bool shouldBeProcessed)> ProcessOrderPayment(PaymentDto model)
        {
            // ignores checking if account number exists or has the neccessary balance, this is outside scope

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == model.PaymentId);

            if (payment == null)
                throw new Exception($"Payment with id {model.PaymentId} not found");

            if (payment.Status == PaymentStatus.Completed)
                return (payment, false);

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.PayingAccount = model.PayingAccount;

            var evt = new PaymentCompletedEvent // orderid paymentid prodcutid quanitty
            {
                OrderId = payment.OrderId,
                PaymentId = payment.PaymentId,
                ProductId = payment.ProductId,
                Quantity = payment.Quantity
            };

            _context.OutBoxMessages.Add(new OutBoxMessage
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

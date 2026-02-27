using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Messaging;
using PaymentService.Models;
using PaymentService.Models.DTOs;
using PaymentService.Services;
using SharedContracts;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentLogic _paymentLogic;
        private readonly PaymentPublisher _publisher;

        public PaymentController(
            PaymentLogic paymentLogic, 
            PaymentPublisher publisher)
        {
            _paymentLogic = paymentLogic;
            _publisher = publisher;
        }


        [HttpPost]
        public async Task<IActionResult> Pay(PaymentDto model)
        {
            var (payment, shouldBeProcessed) = await _paymentLogic.ProcessOrderPayment(model);

            // publish event if not already published
            if (shouldBeProcessed)
            {
                var evt = new PaymentCompletedEvent
                {
                    OrderId = payment.OrderId,
                    PaymentId = payment.PaymentId,
                    ProductId = payment.ProductId,
                    Quantity = payment.Quantity
                };
                _publisher.Publish(evt);
            }

            return Ok(payment);
        }
    }
}
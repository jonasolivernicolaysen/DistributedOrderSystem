using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Messaging;
using PaymentService.Models;
using PaymentService.Models.DTOs;
using PaymentService.Services;
using SharedContracts;

namespace PaymentService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentLogic _paymentLogic;
        private readonly RabbitMQPublisher _publisher;

        public PaymentController(
            PaymentLogic paymentLogic,
            RabbitMQPublisher publisher)
        {
            _paymentLogic = paymentLogic;
            _publisher = publisher;
        }


        [HttpPost]
        public async Task<IActionResult> Pay(PaymentDto model)
        {
            var (payment, shouldBeProcessed) = await _paymentLogic.ProcessOrderPayment(model);
            return Ok(payment);
        }
    }
}
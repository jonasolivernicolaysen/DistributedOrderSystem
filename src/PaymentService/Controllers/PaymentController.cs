using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Data;
using PaymentService.Exceptions;
using PaymentService.Messaging;
using PaymentService.Models;
using PaymentService.Models.DTOs;
using PaymentService.Services;
using SharedContracts;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace PaymentService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentLogic _paymentLogic;
        private readonly RabbitMQPublisher _publisher;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            PaymentLogic paymentLogic,
            RabbitMQPublisher publisher,
            ILogger<PaymentController> logger)
        {
            _paymentLogic = paymentLogic;
            _publisher = publisher;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] PaymentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                throw new UnauthorizedException("User must be authenticated");

            var (payment, shouldBeProcessed) = await _paymentLogic.ProcessOrderPayment(dto, userId);
            return Ok(payment);
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentDetails([FromRoute] Guid paymentId)
        {
            var payment = await _paymentLogic.GetPaymentDetailsAsync(paymentId);
            _logger.LogInformation(payment.ToString());
            return Ok(payment);
        }
    }
}
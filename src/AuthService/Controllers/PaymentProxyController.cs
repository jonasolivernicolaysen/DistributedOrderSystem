using AuthService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AuthService.Controllers
{

    [Authorize]
    [Route("api/payments")]
    public class PaymentProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private ILogger<PaymentProxyController> _logger;
        private readonly string _paymentServiceUrl;

        public PaymentProxyController(
            HttpClient httpClient,
            ILogger<PaymentProxyController> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _paymentServiceUrl = configuration["Services:PaymentService"];
        }


        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] PaymentDto dto)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_paymentServiceUrl}/api/payments");

            // forward the body
            var json = JsonSerializer.Serialize(dto);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentDetails([FromRoute] Guid paymentId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_paymentServiceUrl}/api/payments/{paymentId}");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(content);

            return StatusCode((int)response.StatusCode, content);
        }
    }
}
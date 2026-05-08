using AuthService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AuthService.Controllers
{

    [Authorize]
    [Route("api/payments")]
    public class PaymentProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public PaymentProxyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] PaymentDto dto)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://localhost:7068/api/payments");

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
    }
}
using AuthService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AuthService.Controllers
{

    [Authorize]
    [Route("api/inventory")]
    public class InventoryProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public InventoryProxyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateStock([FromBody] Guid productId, int updatedStock)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://localhost:7248/api/inventory/{productId}");

            // forward the body
            var json = JsonSerializer.Serialize(updatedStock);
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
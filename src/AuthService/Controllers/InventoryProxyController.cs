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
        private readonly string _inventoryServiceUrl;

        public InventoryProxyController(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _inventoryServiceUrl = configuration["Services:InventoryService"];
        }


        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateStock([FromRoute] Guid productId, [FromBody] UpdateStockDto dto)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"{_inventoryServiceUrl}/api/inventory/{productId}");

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

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_inventoryServiceUrl}/api/inventory/");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetInventoryItemById([FromRoute] Guid productId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_inventoryServiceUrl}/api/inventory/{productId}");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        
    }
}
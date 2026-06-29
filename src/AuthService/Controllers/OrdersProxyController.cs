using AuthService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AuthService.Controllers
{
 
    [Authorize]
    [Route("api/orders")]
    public class OrdersProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _orderServiceUrl;

        public OrdersProxyController(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _orderServiceUrl = configuration["Services:OrderService"];
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_orderServiceUrl}/api/orders");

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

        [HttpPost("cart")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddToCartDto dto)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_orderServiceUrl}/api/orders/cart");

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

        [HttpGet("cart")]
        public async Task<IActionResult> GetCart()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_orderServiceUrl}/api/orders/cart");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        [HttpDelete("cart/{productId}")]
        public async Task<IActionResult> DeleteItemFromCart(Guid productId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{_orderServiceUrl}/api/orders/cart/{productId}");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        [HttpPost("cart/checkout")]
        public async Task<IActionResult> CheckoutCart()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_orderServiceUrl}/api/orders/cart/checkout");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }
    }
}   
using AuthService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AuthService.Controllers
{

    [Authorize]
    [Route("api/products")]
    public class ProductProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ProductProxyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        [HttpGet]
        public async Task<IActionResult> ListProducts()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://localhost:7165/api/products");


            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById(Guid Id)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://localhost:7165/api/products/{Id}");

            // forward jwt token
            var token = Request.Headers["Authorization"].ToString();
            request.Headers.Add("Authorization", token);

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            return StatusCode((int)response.StatusCode, content);
        }

        [HttpPost]
        public async Task<IActionResult> CreateListing(CreateProductListingDto dto)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://localhost:7165/api/products");

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

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateListing(Guid productId, UpdateProductListingDto dto)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"https://localhost:7165/api/products/{productId}");

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
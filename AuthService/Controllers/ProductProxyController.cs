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
    }
}
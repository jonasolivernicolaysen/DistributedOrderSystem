using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Models.DTOs;
using ProductService.Data;
using ProductService.Mappers;

using SharedContracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProductService.Services;

namespace ProductService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/products")]
    public class OrderController : ControllerBase
    {
        private readonly ProductLogic _productLogic;
        public OrderController(
            ProductLogic productLogic)
        {
            _productLogic = productLogic;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _productLogic.GetProductsAsync();
            return Ok(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductById(Guid Id)
        {
            var product = await _productLogic.GetProductByIdAsync(Id);
            if (product == null)
                return NotFound($"Product with id {Id} not found.");
            
            return Ok(product);
        }
    }
}

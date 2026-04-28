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
    public class ProductController : ControllerBase
    {
        private readonly ProductLogic _productLogic;
        public ProductController(
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

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(Guid productId)
        {
            var product = await _productLogic.GetProductByIdAsync(productId);            
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductListing(CreateProductListingDto dto)
        {
            var product = await _productLogic.CreateProductListingAsync(dto);
            return Ok(product);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProductListing(Guid productId, UpdateProductListingDto dto)
        {
            var product = await _productLogic.UpdateProductListingAsync(productId, dto);
            return Ok(product);
        }
    }
}

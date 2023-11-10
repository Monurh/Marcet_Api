using Marcet_DB.Models;
using Marcet_DB.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;

namespace Marcet_DB.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductControllers : ControllerBase
    {
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;
        private readonly PaginationParameters paginationParameters;

        public ProductControllers(ApplicationContext dbContext, IConfiguration configuration)
        {
            db = dbContext;
            _configuration = configuration;
            paginationParameters = new PaginationParameters();
        }
        [HttpPost("add")]
        public async Task<ActionResult> AddProduct([FromBody]Product productData)
        {
            try
            {
                productData.ProductId = Guid.NewGuid();

                await db.Products.AddAsync(productData);
                await db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = productData.ProductId }, productData);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize(Roles ="Admin")]
        [HttpGet("{id:guid}")]

        public async Task<ActionResult> GetProduct(Guid id)
        {
            var product = await db.Products.FirstOrDefaultAsync(u => u.ProductId == id);
            if (product == null) 
            { 
                return NotFound("Товар не знайден");
            }
            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("edit")]
        public async Task<ActionResult> EditProduct([FromBody] Product productData)
        {
            try
            {
                var product = await db.Products.FirstOrDefaultAsync(u => u.ProductId == productData.ProductId);
                if (product == null)
                {
                    return NotFound("Товар не знайдено");
                }
                UpdateProductData(product, productData);
                await db.SaveChangesAsync();
                return Ok(productData);
              
            }
            catch (Exception ex)
            {
                return BadRequest($"Помилка під час редагування: {ex.Message}");
            }
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("delete")]
        public async Task<ActionResult>DeleteProduct(Guid id)
        {
            try
            {
                var product = await db.Products.FirstOrDefaultAsync(u =>u.ProductId==id);
                if(product == null)
                {
                    return NotFound("Товар не знайдено");
                }
                db.Products.Remove(product);
                await db.SaveChangesAsync();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest($"Помилка при видаленні товару");
            }
        }
        private void UpdateProductData(Product product, Product prodctData)
        {
            product.ProductName = prodctData.ProductName;
            product.Description = prodctData.Description;
            product.Price = prodctData.Price;
            product.StockQuantity = prodctData.StockQuantity;
            product.Category= prodctData.Category;
            product.Manufacturer = prodctData.Manufacturer;
            product.Photo= prodctData.Photo;
        }


        // Пагинация

        [HttpGet("Pagination")]
        [SwaggerOperation(Summary = "Get paginated items", Description = "Get a paginated list of items.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successful operation", typeof(IEnumerable<Product>))]
        public async Task<IActionResult> GetItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var items = await db.Products
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

    }
}

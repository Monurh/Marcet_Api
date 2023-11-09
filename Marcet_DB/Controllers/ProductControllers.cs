using Marcet_DB.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Marcet_DB.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductControllers : ControllerBase
    {
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;

        public ProductControllers(ApplicationContext dbContext, IConfiguration configuration)
        {
            db = dbContext;
            _configuration = configuration;
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[HttpGet("{id:product}")]
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
                return Ok(productData);
              
            }
            catch (Exception ex)
            {
                return BadRequest($"Помилка під час редагування: {ex.Message}");
            }
        }

    }
}

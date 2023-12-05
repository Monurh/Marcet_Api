using Marcet_DB.Models;
using Marcet_DB.Pagination;
using Marcet_Log.ErrorHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Runtime.Serialization;

namespace Marcet_DB.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductControllers : ControllerBase
    {
        private readonly ILogger<ProductControllers> _logger;
        private readonly ApplicationContext db;
        private readonly IConfiguration _configuration;
        private readonly PaginationParameters paginationParameters;

        public ProductControllers(ApplicationContext dbContext, IConfiguration configuration, ILogger<ProductControllers> logger)
        {
            db = dbContext;
            _configuration = configuration;
            paginationParameters = new PaginationParameters();
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddProduct([FromBody] Product productData)
        {
            try
            {
                _logger.LogInformation($"Executing AddProduct method for product with ID: {productData.ProductId}");

                productData.ProductId = Guid.NewGuid();

                await db.Products.AddAsync(productData);
                await db.SaveChangesAsync();

                _logger.LogInformation($"AddProduct method executed successfully for product with ID: {productData.ProductId}");
                return CreatedAtAction(nameof(GetProduct), new { id = productData.ProductId }, productData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing AddProduct for product with ID: {productData.ProductId}", ex);
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult> GetProduct(Guid id)
        {
            try
            {
                _logger.LogInformation($"Executing GetProduct method for product with ID: {id}");

                var product = await db.Products.FirstOrDefaultAsync(u => u.ProductId == id);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found.");
                    return NotFound("Товар не знайден");
                }

                _logger.LogInformation($"GetProduct method executed successfully for product with ID: {id}");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing GetProduct for product with ID: {id}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }
        //Get Name
        [HttpGet("{name}")]
        public async Task<ActionResult>GetProduct(String name)
        {
            try
            {
                _logger.LogInformation($"Executing GetProduct method for product with Name:{name}");

                var product = await db.Products.Where(u=>u.ProductName==name).ToListAsync();
                if (product == null || !product.Any()) 
                {
                    _logger.LogWarning($"Product with  {name} not found.");
                    return NotFound("Товар не знайден");
                }
                _logger.LogInformation($"GetProduct method executed successfully for product with ProductName: {name}");
                return Ok(product);
            }
            catch(Exception ex) 
            {
                _logger.LogError($"An error occurred while processing GetProduct for product with ProductName: {name}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("edit")]
        public async Task<ActionResult> EditProduct([FromBody] Product productData)
        {
            try
            {
                _logger.LogInformation($"Executing EditProduct method for product with ID: {productData.ProductId}");

                var product = await db.Products.FirstOrDefaultAsync(u => u.ProductId == productData.ProductId);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {productData.ProductId} not found.");
                    throw new NotFoundException("Товар не знайдено");
                }

                UpdateProductData(product, productData);
                await db.SaveChangesAsync();

                _logger.LogInformation($"EditProduct method executed successfully for product with ID: {productData.ProductId}");
                return Ok(productData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing EditProduct for product with ID: {productData.ProductId}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            try
            {
                _logger.LogInformation($"Executing DeleteProduct method for product with ID: {id}");

                var product = await db.Products.FirstOrDefaultAsync(u => u.ProductId == id);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found.");
                    throw new NotFoundException("Товар не знайдено");
                }

                db.Products.Remove(product);
                await db.SaveChangesAsync();

                _logger.LogInformation($"DeleteProduct method executed successfully for product with ID: {id}");
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing DeleteProduct for product with ID: {id}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }
        private void UpdateProductData(Product product, Product prodctData)
        {
            product.ProductName = prodctData.ProductName;
            product.Description = prodctData.Description;
            product.Price = prodctData.Price;
            product.StockQuantity = prodctData.StockQuantity;
            product.Category = prodctData.Category;
            product.Manufacturer = prodctData.Manufacturer;
            product.Photo = prodctData.Photo;
        }


        // Pagination
        [HttpGet("Pagination")]
        [SwaggerOperation(Summary = "Get paginated items", Description = "Get a paginated list of items.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Successful operation", typeof(IEnumerable<Product>))]
        public async Task<IActionResult> GetItems([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation($"Executing GetItems method for page {pageNumber} with page size {pageSize}");

                var items = await db.Products
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (items == null || items.Count == 0)
                {
                    _logger.LogWarning("No items found during pagination.");
                    throw new NotFoundException("Paginated items", "No items found");
                }

                _logger.LogInformation($"GetItems method executed successfully for page {pageNumber} with page size {pageSize}");
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while processing GetItems", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }

        //Sort
        [HttpGet("Sort")]
        public async Task<IActionResult> GetSortedProducts(SortProduct sortOption)
        {
            try
            {
                _logger.LogInformation($"Executing GetSortedProducts method with sort option: {sortOption}");

                IQueryable<Product> query = db.Products;

                switch (sortOption)
                {
                    //По алфавиту
                    case SortProduct.NameAsc:
                        query = query.OrderBy(p => p.ProductName);
                        break;
                    // начиная с конца алфавита
                    case SortProduct.NameDesc:
                        query = query.OrderByDescending(p => p.ProductName);
                        break;
                    //возрастание по цене
                    case SortProduct.PriceAsc:
                        query = query.OrderBy(p => p.Price);
                        break;
                    // убывание по цене
                    case SortProduct.PriceDesc:
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    // по категории
                    case SortProduct.Category:
                        query = query.OrderBy(p => p.Category);
                        break;
                }

                string sortingName = Enum.GetName(typeof(SortProduct), sortOption);
                string sortingDisplayName = typeof(SortProduct)
                    .GetField(sortingName)
                    .GetCustomAttributes(typeof(SortProduct), false)
                    .Cast<EnumMemberAttribute>()
                    .FirstOrDefault()?.Value;

                var sortedProducts = await query.ToListAsync();

                _logger.LogInformation($"GetSortedProducts method executed successfully with sort option: {sortOption}");
                return Ok(sortedProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing GetSortedProducts with sort option: {sortOption}", ex);
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
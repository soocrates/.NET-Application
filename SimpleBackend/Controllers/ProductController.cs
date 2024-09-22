using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleBackend.Data;
using SimpleBackend.Models;
using Microsoft.Extensions.Logging;
using Amazon.XRay.Recorder.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly SimpleDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(SimpleDbContext context, ILogger<ProductController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all products with category names
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    // Explicitly handle null by checking the Category
                    CategoryName = p.Category != null ? p.Category.Name : "No Category"
                })
                .ToListAsync();

            _logger.LogInformation("200 OK: Fetched all products.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "200");

            return Ok(products);
        }

        // Get a single product by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    // Explicitly handle null by checking the Category
                    CategoryName = p.Category != null ? p.Category.Name : "No Category"
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("404 Not Found: Product not found.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "404");
                return NotFound(new { Message = "Product not found" });
            }

            _logger.LogInformation("200 OK: Fetched product.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "200");

            return Ok(product);
        }

        // Create a new product
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("201 Created: Product created.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "201");

                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "500 Internal Server Error: Error creating product.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "500");
                return StatusCode(500, new { Message = "Error creating product" });
            }
        }

        // Update an existing product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                _logger.LogWarning("400 Bad Request: Product ID mismatch.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "400");
                return BadRequest(new { Message = "Product ID mismatch" });
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                _logger.LogInformation("204 No Content: Product updated.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "204");

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(id))
                {
                    _logger.LogWarning("404 Not Found: Product not found.");
                    AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "404");
                    return NotFound(new { Message = "Product not found" });
                }
                else
                {
                    _logger.LogError(ex, "500 Internal Server Error: Error updating product.");
                    AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "500");
                    return StatusCode(500, new { Message = "Error updating product" });
                }
            }
        }

        // Delete a product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("404 Not Found: Product not found.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "404");
                return NotFound(new { Message = "Product not found" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("204 No Content: Product deleted.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "204");

            return NoContent();
        }

        // Get products by category name
        [HttpGet("category/{categoryName}")]
        public async Task<IActionResult> GetProductsByCategoryName(string categoryName)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.Name == categoryName)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    // Explicitly handle null by checking the Category
                    CategoryName = p.Category != null ? p.Category.Name : "No Category"
                })
                .ToListAsync();

            if (!products.Any())
            {
                _logger.LogWarning("404 Not Found: No products found for category.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "404");
                return NotFound(new { Message = "No products found for the category" });
            }

            _logger.LogInformation("200 OK: Fetched products by category.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "200");

            return Ok(products);
        }

        // Helper method to check if a product exists
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}

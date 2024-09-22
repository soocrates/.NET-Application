using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleBackend.Data;
using SimpleBackend.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Amazon.XRay.Recorder.Core;

namespace SimpleBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly SimpleDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(SimpleDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            _logger.LogInformation("200 OK: Fetched all categories.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "200");
            return Ok(categories);
        }

        // Get a single category by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("404 Not Found: Category not found.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "404");
                return NotFound(new { Message = "Category not found" });
            }
            _logger.LogInformation("200 OK: Fetched category.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "200");
            return Ok(category);
        }

        // Create a new category
        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("201 Created: Category created.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "201");
            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryId }, category);
        }

        // Update an existing category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                _logger.LogWarning("400 Bad Request: Category ID mismatch.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "400");
                return BadRequest(new { Message = "Category ID mismatch" });
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("204 No Content: Category updated.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "204");
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "500 Internal Server Error: Error updating category.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "500");
                return StatusCode(500, new { Message = "Error updating category" });
            }
        }

        // Delete a category
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("404 Not Found: Category not found.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "404");
                return NotFound(new { Message = "Category not found" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("204 No Content: Category deleted.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "204");
            return NoContent();
        }

        // Get products by category name
        [HttpGet("name/{categoryName}/products")]
        public async Task<IActionResult> GetProductsByCategoryName(string categoryName)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Name == categoryName);

            if (category == null)
            {
                _logger.LogWarning("404 Not Found: Category not found.");
                AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "404");
                return NotFound(new { Message = "Category not found" });
            }

            _logger.LogInformation("200 OK: Fetched products for category.");
            AWSXRayRecorder.Instance.AddAnnotation("ResponseCode", "200");
            return Ok(category.Products);
        }
    }
}

namespace SimpleBackend.Models
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Instead of CategoryId, we now include the CategoryName
        public string CategoryName { get; set; } = string.Empty;
    }
}

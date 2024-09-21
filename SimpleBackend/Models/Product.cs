namespace SimpleBackend.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        // Ensure Category is in the same namespace or add a proper using directive
        public Category? Category { get; set; }
    }
}

namespace SimpleBackend.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;  // Initialize with default value
        public string ImageUrl { get; set; } = string.Empty;  // Initialize with default value
        public string Description { get; set; } = string.Empty;  // Initialize with default value

        public ICollection<Product> Products { get; set; } = new List<Product>();  // Initialize with empty list
    }
}

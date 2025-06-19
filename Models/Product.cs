using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Models
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(100,ErrorMessage ="product Name is limited to 100 characters only")]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "product Brand is limited to 100 characters only")]
        public string Brand { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "product category is limited to 100 characters only")]
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Precision(16,2)] // Precision for decimal values
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}

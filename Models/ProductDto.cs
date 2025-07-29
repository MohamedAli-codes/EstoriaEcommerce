using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace E_commerce.Models
{
    public class ProductDto
    {
        [Required,MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Brand { get; set; } = string.Empty;
        [Required,MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        [Required]
        public int Stock { get; set; } 
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; } 
    }
}

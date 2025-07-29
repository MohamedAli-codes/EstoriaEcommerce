using System.Net.Http.Json;
using E_commerce.Models;
using E_commerce.Services;

namespace E_commerce.Seeding
{
    public class ProductSeeder
    {
        private readonly ApplicationContext _context;
        private readonly HttpClient _httpClient;

        public ProductSeeder(ApplicationContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        public async Task SeedAsync()
        {
            if (_context.Products.Any()) return;

            var fakeProducts = await _httpClient.GetFromJsonAsync<List<FakeProduct>>("https://fakestoreapi.com/products");

            var mapped = fakeProducts.Select(p => new Product
            {
                Name = p.title,
                Description = p.description,
                Price = (decimal)p.price,
                Stock = new Random().Next(0, 20),
                Category = p.category,
                ImageUrl = p.image,
                Brand = "Generic"
            }).ToList();

            _context.Products.AddRange(mapped);
            await _context.SaveChangesAsync();
        }

        // Inner DTO for JSON mapping
        private class FakeProduct
        {
            public int id { get; set; }
            public string title { get; set; }
            public double price { get; set; }
            public string description { get; set; }
            public string category { get; set; }
            public string image { get; set; }
        }
    }
}

using Bogus;
using E_commerce.Models;

public class ProductSeeder
{
    public static List<Product> GenerateFakeProducts(int count = 30)
    {
        var faker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Brand, f => f.Company.CompanyName())
            .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(10, 1000)))
            .RuleFor(p => p.ImageUrl, f => f.Image.PicsumUrl())
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1)); // Products created within the last year

        return faker.Generate(count);
    }
}

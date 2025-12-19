using ProductService.Domain;

namespace ProductService.Infrastructure;

public static class DbInitializer
{
    public static void Seed(ProductDbContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Mouse",
                Description = "Ergonomic mouse with adjustable DPI",
                Price = 29.99m,
                Stock = 120
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Mechanical Keyboard",
                Description = "Backlit keyboard with blue switches",
                Price = 89.5m,
                Stock = 75
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "USB-C Hub",
                Description = "7-in-1 hub with HDMI and Ethernet",
                Price = 54.0m,
                Stock = 200
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}

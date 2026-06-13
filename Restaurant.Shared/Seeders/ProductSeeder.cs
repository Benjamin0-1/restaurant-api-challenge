using Microsoft.EntityFrameworkCore;
using Restaurant.Shared.Entities;

namespace Restaurant.Shared.Seeders;

public class ProductSeeder : ISeeder
{
    private readonly DatabaseContext _context;

    public ProductSeeder(DatabaseContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Products.AnyAsync(cancellationToken))
            return;

        var user = await _context.Users.FirstOrDefaultAsync(cancellationToken); // va a traer el primer usuario, que es el admin creado por el UserSeeder
        if (user is null) return;

        var categories = await _context.Categories.ToListAsync(cancellationToken);
        if (categories.Count == 0) return;

        int Cid(string name) => categories.First(c => c.Name == name).Id;

        var now = DateTime.UtcNow;

        var products = new List<ProductEntity>
        {
            new() { Name = "Lemonade",       Description = "Fresh squeezed lemonade",       Price = 3.50m,  Sku = "BEV-001", IsAvailable = true, CategoryId = Cid("Beverages"),   UserId = user.Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Iced Coffee",     Description = "Cold brew with milk",           Price = 4.00m,  Sku = "BEV-002", IsAvailable = true, CategoryId = Cid("Beverages"),   UserId = user.Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Spring Rolls",    Description = "Crispy vegetable spring rolls", Price = 6.00m,  Sku = "APP-001", IsAvailable = true, CategoryId = Cid("Appetizers"),  UserId = user.Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Garlic Bread",    Description = "Toasted bread with garlic butter", Price = 4.50m, Sku = "APP-002", IsAvailable = true, CategoryId = Cid("Appetizers"), UserId = user.Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Grilled Chicken", Description = "Grilled chicken with vegetables", Price = 14.99m, Sku = "MNC-001", IsAvailable = true, CategoryId = Cid("Main Course"), UserId = user.Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Beef Burger",     Description = "Angus beef burger with fries",  Price = 12.99m, Sku = "MNC-002", IsAvailable = true, CategoryId = Cid("Main Course"), UserId = user.Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Chocolate Cake",  Description = "Rich dark chocolate cake",      Price = 5.50m,  Sku = "DES-001", IsAvailable = true, CategoryId = Cid("Desserts"),    UserId = user.Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Cheesecake",      Description = "Classic New York cheesecake",   Price = 6.00m,  Sku = "DES-002", IsAvailable = true, CategoryId = Cid("Desserts"),    UserId = user.Id, CreatedAt = now, UpdatedAt = now },
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

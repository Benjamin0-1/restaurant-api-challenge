using Microsoft.EntityFrameworkCore;
using Restaurant.Shared.Entities;

namespace Restaurant.Shared.Seeders;

public class CategorySeeder : ISeeder
{
    private readonly DatabaseContext _context;

    public CategorySeeder(DatabaseContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Categories.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;

        var categories = new List<CategoryEntity>
        {
            new() { Name = "Beverages",   Description = "Drinks and beverages",  IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Appetizers",  Description = "Starters and snacks",   IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Main Course", Description = "Main dishes",            IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Desserts",    Description = "Sweet treats",           IsActive = true, CreatedAt = now, UpdatedAt = now },
        };

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

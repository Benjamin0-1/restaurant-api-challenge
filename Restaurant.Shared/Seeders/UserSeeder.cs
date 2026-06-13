using Microsoft.EntityFrameworkCore;
using Restaurant.Shared.Entities;

namespace Restaurant.Shared.Seeders;

public class UserSeeder : ISeeder
{
    private readonly DatabaseContext _context;

    public UserSeeder(DatabaseContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(cancellationToken))
            return;

        var now = DateTime.UtcNow;

        var user = new UserEntity
        {
            Name = "Admin",
            Email = "admin@restaurant.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

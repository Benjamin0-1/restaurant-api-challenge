namespace Restaurant.Shared.Seeders;

public interface ISeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Shared;
using Restaurant.Shared.Entities;
using Restaurant.Shared.Seeders;
using Xunit;

namespace Restaurant.Tests;


public class RestaurantWebApplicationFactory : WebApplicationFactory<Program> // <- WebApplicationFactory tambien se puede configurar en otro archivo, es comun.
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // cambie sqlite por in-memory-db
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DatabaseContext>));
            if (dbDescriptor is not null)
                services.Remove(dbDescriptor);

            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<DatabaseContext>(options =>
                options
                    .UseInMemoryDatabase(_dbName)
                    .UseInternalServiceProvider(inMemoryServiceProvider));

            // Esto detiene a los seeders
            var seederDescriptors = services
                .Where(d => d.ServiceType == typeof(ISeeder))
                .ToList();
            foreach (var d in seederDescriptors)
                services.Remove(d);
        });
    }
}


file record PaginatedResponse<T>(List<T> Data, int PageNumber, int PageSize, int TotalPages);
file record ProductResponse(int Id, string Name, string Description, decimal Price, string Sku, bool IsAvailable, int CategoryId);
file record LoginResponse(string Token);


public class ProductControllerTests : IAsyncLifetime
{
    private readonly RestaurantWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private int _lemonadeId;
    private int _cheesecakeId;
    private int _unavailableId;
    private int _beveragesCategoryId;
    private int _dessertsCategoryId;

    public ProductControllerTests()
    {
        _factory = new RestaurantWebApplicationFactory();
        _client = _factory.CreateClient();
    }


    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await db.Database.EnsureCreatedAsync();

        var now = DateTime.UtcNow;

        var user = new UserEntity
        {
            Name = "Test Admin",
            Email = "test@restaurant.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678A"),
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Users.Add(user);

        var beverages = new CategoryEntity { Name = "Beverages", Description = "Drinks", IsActive = true, CreatedAt = now, UpdatedAt = now };
        var desserts  = new CategoryEntity { Name = "Desserts",  Description = "Sweets", IsActive = true, CreatedAt = now, UpdatedAt = now };
        db.Categories.AddRange(beverages, desserts);
        await db.SaveChangesAsync();

        _beveragesCategoryId = beverages.Id;
        _dessertsCategoryId  = desserts.Id;

        var lemonade = new ProductEntity
        {
            Name = "Lemonade", Description = "Fresh squeezed", Price = 3.50m,
            Sku = "BEV-001", IsAvailable = true,
            CategoryId = beverages.Id, UserId = user.Id, CreatedAt = now, UpdatedAt = now
        };
        var cheesecake = new ProductEntity
        {
            Name = "Cheesecake", Description = "NY style", Price = 6.00m,
            Sku = "DES-001", IsAvailable = true,
            CategoryId = desserts.Id, UserId = user.Id, CreatedAt = now, UpdatedAt = now
        };
        var unavailable = new ProductEntity
        {
            Name = "Unavailable Item", Description = "Not in stock", Price = 1.00m,
            Sku = "NA-001", IsAvailable = false,
            CategoryId = beverages.Id, UserId = user.Id, CreatedAt = now, UpdatedAt = now
        };
        db.Products.AddRange(lemonade, cheesecake, unavailable);
        await db.SaveChangesAsync();

        _lemonadeId    = lemonade.Id;
        _cheesecakeId  = cheesecake.Id;
        _unavailableId = unavailable.Id;

        // Obtain JWT and attach to every subsequent request
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { email = "test@restaurant.com", password = "12345678A" });

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.Token);
    }


    public async Task DisposeAsync()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            await db.Database.EnsureDeletedAsync();
        }

        await _factory.DisposeAsync();
    }

    // test de GET products

    [Fact]
    public async Task Get_NoFilters_ReturnsAllSeededProducts()
    {
        var response = await _client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<ProductResponse>>();
        Assert.Equal(3, body!.Data.Count);
    }

    [Fact]
    public async Task Get_FilterByName_ReturnsMatchingProducts()
    {
        var response = await _client.GetAsync("/api/v1/products?name=Lemon");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<ProductResponse>>();
        Assert.Single(body!.Data);
        Assert.Equal("Lemonade", body.Data[0].Name);
    }

    [Fact]
    public async Task Get_FilterByIsAvailableFalse_ReturnsOnlyUnavailable()
    {
        var response = await _client.GetAsync("/api/v1/products?isAvailable=false");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<ProductResponse>>();
        Assert.Single(body!.Data);
        Assert.False(body.Data[0].IsAvailable);
    }

    [Fact]
    public async Task Get_FilterByCategoryId_ReturnsOnlyThatCategory()
    {
        var response = await _client.GetAsync($"/api/v1/products?categoryId={_dessertsCategoryId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<ProductResponse>>();
        Assert.Single(body!.Data);
        Assert.Equal("Cheesecake", body.Data[0].Name);
    }

    [Fact]
    public async Task Get_FilterByMinAndMaxPrice_ReturnsProductsInRange()
    {
        var response = await _client.GetAsync("/api/v1/products?minPrice=3&maxPrice=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<ProductResponse>>();
        Assert.All(body!.Data, p =>
        {
            Assert.True(p.Price >= 3m);
            Assert.True(p.Price <= 5m);
        });
    }

    [Fact]
    public async Task Get_Pagination_ReturnsCorrectPage()
    {
        var response = await _client.GetAsync("/api/v1/products?pageSize=2&pageNumber=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PaginatedResponse<ProductResponse>>();
        Assert.Single(body!.Data);
        Assert.Equal(2, body.PageSize);
        Assert.Equal(2, body.TotalPages);
    }

    [Fact]
    public async Task Get_NoToken_Returns401()
    {
        using var unauthClient = _factory.CreateClient();

        var response = await unauthClient.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // POST de Products

    [Fact]
    public async Task Post_ValidProduct_Returns201WithCreatedProduct()
    {
        var payload = new
        {
            name = "Iced Coffee",
            description = "Cold brew",
            price = 4.00m,
            sku = "BEV-002",
            isAvailable = true,
            categoryId = _beveragesCategoryId
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal("Iced Coffee", product!.Name);
        Assert.Equal(4.00m, product.Price);
    }

    [Fact]
    public async Task Post_NoToken_Returns401()
    {
        using var unauthClient = _factory.CreateClient();

        var response = await unauthClient.PostAsJsonAsync("/api/v1/products",
            new { name = "x", description = "x", price = 1m, sku = "XX-001", isAvailable = true, categoryId = _beveragesCategoryId });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_DuplicateSku_Returns400()
    {
        var payload = new
        {
            name = "Duplicate",
            description = "x",
            price = 1m,
            sku = "BEV-001", // already seeded
            isAvailable = true,
            categoryId = _beveragesCategoryId
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_NonexistentCategoryId_Returns400()
    {
        var payload = new
        {
            name = "Bad Cat",
            description = "x",
            price = 1m,
            sku = "BC-001",
            isAvailable = true,
            categoryId = 9999
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // PATCH de Products

    [Fact]
    public async Task Patch_ValidUpdate_Returns200WithUpdatedProduct()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{_lemonadeId}",
            new { name = "Fresh Lemonade", price = 4.00m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal("Fresh Lemonade", product!.Name);
        Assert.Equal(4.00m, product.Price);
    }

    [Fact]
    public async Task Patch_EmptyBody_Returns200WithNoChanges()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{_cheesecakeId}",
            new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal("Cheesecake", product!.Name);
    }

    [Fact]
    public async Task Patch_NonexistentId_Returns404()
    {
        var response = await _client.PatchAsJsonAsync(
            "/api/v1/products/99999",
            new { name = "Ghost" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Patch_InvalidCategoryId_Returns400()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{_lemonadeId}",
            new { categoryId = 9999 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Patch_NegativePrice_Returns400()
    {
        var response = await _client.PatchAsJsonAsync(
            $"/api/v1/products/{_lemonadeId}",
            new { price = -1m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // DELETE de Products

    [Fact]
    public async Task Delete_OwnProduct_Returns204AndProductDisappearsFromGet()
    {
        var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{_unavailableId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Soft-deleted product should no longer appear in GET
        var getResponse = await _client.GetAsync("/api/v1/products");
        var body = await getResponse.Content.ReadFromJsonAsync<PaginatedResponse<ProductResponse>>();
        Assert.DoesNotContain(body!.Data, p => p.Id == _unavailableId);
    }

    [Fact]
    public async Task Delete_AlreadyDeletedProduct_Returns404()
    {
        // First delete
        await _client.DeleteAsync($"/api/v1/products/{_unavailableId}");

        // Second delete on the same product
        var response = await _client.DeleteAsync($"/api/v1/products/{_unavailableId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonexistentId_Returns404()
    {
        var response = await _client.DeleteAsync("/api/v1/products/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NoToken_Returns401()
    {
        using var unauthClient = _factory.CreateClient();

        var response = await unauthClient.DeleteAsync($"/api/v1/products/{_lemonadeId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

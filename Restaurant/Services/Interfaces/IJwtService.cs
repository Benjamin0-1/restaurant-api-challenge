using Restaurant.Shared.Entities;

namespace Restaurant.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(UserEntity user);
}

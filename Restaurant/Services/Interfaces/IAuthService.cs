using Restaurant.Dtos;
using Restaurant.Requests;
using Restaurant.Shared.Entities;

namespace Restaurant.Services.Interfaces;

public interface IAuthService
{
    Task<JwtDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<UserEntity> SignupAsync(SignupRequest request, CancellationToken cancellationToken);
}
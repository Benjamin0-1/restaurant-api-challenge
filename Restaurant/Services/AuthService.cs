using Microsoft.EntityFrameworkCore;
using Restaurant.Dtos;
using Restaurant.Requests;
using Restaurant.Services.Interfaces;
using Restaurant.Shared;
using Restaurant.Shared.Entities;

namespace Restaurant.Services;

public class AuthService : IAuthService
{
    private readonly DatabaseContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(DatabaseContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<JwtDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = _jwtService.GenerateToken(user);

        return new JwtDto
        {
            Token = token,
            // Expiration = DateTime.UtcNow.AddMinutes(60) 
        };
    }

    public async Task<UserEntity> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default)
    {
        var user = new UserEntity
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}

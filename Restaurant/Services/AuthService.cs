using FluentValidation;
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
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<SignupRequest> _signupValidator;

    public AuthService(DatabaseContext context, IJwtService jwtService,
        IValidator<LoginRequest> loginValidator, IValidator<SignupRequest> signupValidator)
    {
        _context = context;
        _jwtService = jwtService;
        _loginValidator = loginValidator;
        _signupValidator = signupValidator;
    }

    public async Task<JwtDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = _jwtService.GenerateToken(user);

        return new JwtDto
        {
            Token = token,
        };
    }

    public async Task<UserEntity> SignupAsync(SignupRequest request, CancellationToken cancellationToken = default)
    {
        await _signupValidator.ValidateAndThrowAsync(request, cancellationToken);
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

using Microsoft.AspNetCore.Mvc;
using Restaurant.Requests;
using Restaurant.Services.Interfaces;

namespace Restaurant.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result is null)
            return Unauthorized();
        return Ok(result);
    }
    [HttpPost("signup")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request, CancellationToken cancellationToken)
    {
        await _authService.SignupAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }
}

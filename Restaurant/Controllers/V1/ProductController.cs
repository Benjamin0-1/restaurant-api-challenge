using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Filters;
using Restaurant.Requests;
using Restaurant.Services.Interfaces;

namespace Restaurant.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService  _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] ProductFilter? filters, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _productService.GetAsync(filters, userId, cancellationToken);
        return Ok(result);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] ProductCreateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _productService.CreateAsync(
            request,
            userId,
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}

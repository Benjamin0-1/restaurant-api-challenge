using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Filters;
using Restaurant.Requests;
using Restaurant.Services.Interfaces;

namespace Restaurant.Controllers.V1;

[Authorize]
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
        var result = _productService.GetAsync(filters, cancellationToken).Result;
        return Ok(result);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest request, CancellationToken cancellationToken)
    {
        request.USerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); // se asigna automaticamente.
        var result = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Middlewares;

public class KeyNotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not KeyNotFoundException notFoundException)
            return false;

        var problemDetails = new ProblemDetails()
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = notFoundException.Message
        };

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}


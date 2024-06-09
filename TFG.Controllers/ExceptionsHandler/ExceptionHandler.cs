using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TFG.Services.Exceptions;

namespace TFG.Controllers.ExceptionsHandler;

internal sealed class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not HttpException httpException)
        {
            logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Status = httpException.Code,
            Title = httpException.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
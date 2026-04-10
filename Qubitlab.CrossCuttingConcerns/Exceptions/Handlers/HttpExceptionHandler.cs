using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;
using Qubitlab.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

namespace Qubitlab.CrossCuttingConcerns.Exceptions.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    private HttpResponse response;

    public HttpResponse Response
    {
        get => response ?? throw new ArgumentNullException(nameof(response));
        set => response = value;
    }

    protected override Task HandleException(AuthorizationException authorizationException)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        var details = new AuthorizationProblemDetails(authorizationException.Message);
        var json = JsonSerializer.Serialize(details);
        return Response.WriteAsync(json);
    }

    protected override Task HandleException(ValidationException validationException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        var details = new ValidationProblemDetails(validationException.Errors);

        var json = JsonSerializer.Serialize(details);
        return Response.WriteAsync(json);
    }

    protected override Task HandleException(BusinessException businessException)
    {
        Response.StatusCode = StatusCodes.Status400BadRequest;
        var details = new BusinessProblemDetails(businessException.Message);
        var json = JsonSerializer.Serialize(details);
        return Response.WriteAsync(json);
    }

    protected override  Task HandleException(NotFoundException notFoundException)
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        var details = new NotFoundProblemDetails(notFoundException.Message);
        var json = JsonSerializer.Serialize(details);
        return Response.WriteAsync(json);
    }

    protected override Task HandleException(Exception exception)
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        var details = new InternalServerErrorProblemDetails(exception.Message);
        var json = JsonSerializer.Serialize(details);
        return Response.WriteAsync(json);
    }
}
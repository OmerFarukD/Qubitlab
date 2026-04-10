using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;

namespace Qubitlab.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class ValidationProblemDetails : ProblemDetails
{
    public IEnumerable<ValidationExceptionModel> Errors { get; set; }

    public ValidationProblemDetails(IEnumerable<ValidationExceptionModel> errors)
    {
        Title = "Validation error(s)";
        Errors = errors;
        Status = StatusCodes.Status400BadRequest;
        Type = "https://example.com/problems/validation";
    }
}
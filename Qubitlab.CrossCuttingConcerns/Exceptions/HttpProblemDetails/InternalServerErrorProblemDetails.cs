using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Qubitlab.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class InternalServerErrorProblemDetails : ProblemDetails
{
    public InternalServerErrorProblemDetails(string detail = "Internal Server Error")
    {
        Title = "Internal Server Error";
        Detail = detail;
        Status = StatusCodes.Status500InternalServerError;
        Type = "https://example.com/problems/internal-server";
    }
}
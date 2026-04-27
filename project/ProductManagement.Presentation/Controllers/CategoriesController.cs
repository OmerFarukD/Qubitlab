using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.Features.Categories.Commands.Create;
using ProductManagement.Application.Features.Categories.Commands.Delete;
using ProductManagement.Application.Features.Categories.Commands.Update;
using ProductManagement.Application.Features.Categories.Queries.GetById;
using ProductManagement.Application.Features.Categories.Queries.GetList;
using ProductManagement.Application.Features.Categories.Queries.GetListByPaginate;
using Qubitlab.Persistence.EFCore.Entities;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetByIdCategoryQuery { Id = id }, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetListCategoryQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("paginate")]
    public async Task<IActionResult> GetListByPaginate([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(
            new GetListByPaginateCategoryQuery { PageIndex = pageIndex, PageSize = pageSize },
            cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CategoryAddCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CategoryUpdateCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new CategoryDeleteCommand { Id = id }, cancellationToken);
        return Ok(response);
    }
}

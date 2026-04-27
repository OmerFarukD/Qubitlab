using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.Features.Products.Commands.Create;
using ProductManagement.Application.Features.Products.Commands.Delete;
using ProductManagement.Application.Features.Products.Commands.Update;
using ProductManagement.Application.Features.Products.Queries.GetById;
using ProductManagement.Application.Features.Products.Queries.GetList;
using ProductManagement.Application.Features.Products.Queries.GetListByPaginate;

namespace ProductManagement.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetByIdProductQuery { Id = id }, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int? categoryId,
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? inStock,
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetListProductQuery
        {
            CategoryId = categoryId,
            Name = name,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            InStock = inStock
        }, cancellationToken);
        return Ok(response);
    }

    [HttpGet("paginate")]
    public async Task<IActionResult> GetListByPaginate(
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? name = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? inStock = null,
        CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(
            new GetListByPaginateProductQuery
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                CategoryId = categoryId,
                Name = name,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStock = inStock
            },
            cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] ProductAddCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProductUpdateCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new ProductDeleteCommand { Id = id }, cancellationToken);
        return Ok(response);
    }
}

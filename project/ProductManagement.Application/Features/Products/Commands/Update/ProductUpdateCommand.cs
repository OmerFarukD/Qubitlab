using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Products.Rules;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Products.Commands.Update;

public sealed class ProductUpdateCommand : IRequest<ProductUpdatedResponseDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }

    public sealed class ProductUpdateCommandHandler(
        IMapper _mapper,
        IProductRepository _productRepository,
        ProductBusinessRules _businessRules
    ) : IRequestHandler<ProductUpdateCommand, ProductUpdatedResponseDto>
    {
        public async Task<ProductUpdatedResponseDto> Handle(ProductUpdateCommand request, CancellationToken cancellationToken)
        {
            await _businessRules.ProductIsPresentAsync(request.Id, cancellationToken);
            await _businessRules.ProductNameMustBeUniqueWhenUpdatingAsync(request.Id, request.Name, cancellationToken);

            Product? product = await _productRepository.GetAsync(
                predicate: x => x.Id == request.Id,
                cancellationToken: cancellationToken
            );

            product = _mapper.Map(request, product);
            Product updatedProduct = await _productRepository.UpdateAsync(product,cancellationToken);
            ProductUpdatedResponseDto response = _mapper.Map<ProductUpdatedResponseDto>(updatedProduct);

            return response;
        }
    }
}

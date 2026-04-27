using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Products.Rules;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Products.Commands.Create;

public sealed class ProductAddCommand : IRequest<ProductAddedResponseDto>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }

    public sealed class ProductAddCommandHandler(
        IMapper _mapper,
        IProductRepository _productRepository,
        ProductBusinessRules _businessRules
    ) : IRequestHandler<ProductAddCommand, ProductAddedResponseDto>
    {
        public async Task<ProductAddedResponseDto> Handle(ProductAddCommand request, CancellationToken cancellationToken)
        {
            await _businessRules.ProductNameMustBeUniqueAsync(request.Name, cancellationToken);

            Product product = _mapper.Map<Product>(request);
            Product addedProduct = await _productRepository.AddAsync(product);
            ProductAddedResponseDto response = _mapper.Map<ProductAddedResponseDto>(addedProduct);

            return response;
        }
    }
}

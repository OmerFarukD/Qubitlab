using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Products.Rules;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Products.Commands.Delete;

public sealed class ProductDeleteCommand : IRequest<ProductDeletedResponseDto>
{
    public Guid Id { get; set; }

    public sealed class ProductDeleteCommandHandler(
        IMapper _mapper,
        IProductRepository _productRepository,
        ProductBusinessRules _businessRules
    ) : IRequestHandler<ProductDeleteCommand, ProductDeletedResponseDto>
    {
        public async Task<ProductDeletedResponseDto> Handle(ProductDeleteCommand request, CancellationToken cancellationToken)
        {
            await _businessRules.ProductIsPresentAsync(request.Id, cancellationToken);

            Product? product = await _productRepository.GetAsync(
                predicate: x => x.Id == request.Id,
                cancellationToken: cancellationToken
            );

            await _productRepository.DeleteAsync(product);
            ProductDeletedResponseDto response = _mapper.Map<ProductDeletedResponseDto>(product);

            return response;
        }
    }
}

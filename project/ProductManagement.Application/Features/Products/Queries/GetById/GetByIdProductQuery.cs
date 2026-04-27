using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Products.Rules;
using ProductManagement.Application.Features.Products.Specifications;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Extensions;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Queries.GetById;

public sealed class GetByIdProductQuery : IRequest<GetByIdProductResponseDto>
{
    public Guid Id { get; set; }

    public sealed class GetByIdProductQueryHandler(
        IMapper _mapper,
        IProductRepository _productRepository,
        ProductBusinessRules _businessRules
    ) : IRequestHandler<GetByIdProductQuery, GetByIdProductResponseDto>
    {
        public async Task<GetByIdProductResponseDto> Handle(GetByIdProductQuery request, CancellationToken cancellationToken)
        {
            await _businessRules.ProductIsPresentAsync(request.Id, cancellationToken);

            ISpecification<Product> spec = new ProductWithCategorySpecification()
                .And(new ProductByIdSpecification(request.Id));

            Product? product = await _productRepository.GetAsync(spec, cancellationToken);

            GetByIdProductResponseDto response = _mapper.Map<GetByIdProductResponseDto>(product);

            return response;
        }
    }
}

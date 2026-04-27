using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Products.Specifications;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Extensions;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Queries.GetList;

public sealed class GetListProductQuery : IRequest<List<GetListProductResponseDto>>
{
    public int? CategoryId { get; set; }
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }

    public sealed class GetListProductQueryHandler(
        IMapper _mapper,
        IProductRepository _productRepository
    ) : IRequestHandler<GetListProductQuery, List<GetListProductResponseDto>>
    {
        public async Task<List<GetListProductResponseDto>> Handle(GetListProductQuery request, CancellationToken cancellationToken)
        {
            ISpecification<Product> spec = new ProductWithCategorySpecification();

            if (request.CategoryId.HasValue)
                spec = spec.And(new ProductByCategorySpecification(request.CategoryId.Value));

            if (!string.IsNullOrWhiteSpace(request.Name))
                spec = spec.And(new ProductByNameSpecification(request.Name));

            if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
                spec = spec.And(new ProductByPriceRangeSpecification(
                    request.MinPrice, request.MaxPrice));

            if (request.InStock.HasValue && request.InStock.Value)
                spec = spec.And(new ProductInStockSpecification());

            List<Product> products = await _productRepository.GetListAsync(spec, cancellationToken);

            List<GetListProductResponseDto> response = _mapper.Map<List<GetListProductResponseDto>>(products);

            return response;
        }
    }
}

using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Products.Specifications;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Extensions;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Queries.GetListByPaginate;

public sealed class GetListByPaginateProductQuery : IRequest<Paginate<GetListByPaginateProductResponseDto>>
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public int? CategoryId { get; set; }
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }

    public sealed class GetListByPaginateProductQueryHandler(
        IMapper _mapper,
        IProductRepository _productRepository
    ) : IRequestHandler<GetListByPaginateProductQuery, Paginate<GetListByPaginateProductResponseDto>>
    {
        public async Task<Paginate<GetListByPaginateProductResponseDto>> Handle(
            GetListByPaginateProductQuery request,
            CancellationToken cancellationToken)
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

            Paginate<Product> products = await _productRepository.GetPaginateAsync(
                spec,
                index: request.PageIndex,
                size: request.PageSize,
                cancellationToken: cancellationToken);

            Paginate<GetListByPaginateProductResponseDto> response =
                _mapper.Map<Paginate<GetListByPaginateProductResponseDto>>(products);

            return response;
        }
    }
}

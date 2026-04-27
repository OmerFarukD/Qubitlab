using AutoMapper;
using ProductManagement.Application.Features.Products.Commands.Create;
using ProductManagement.Application.Features.Products.Commands.Delete;
using ProductManagement.Application.Features.Products.Commands.Update;
using ProductManagement.Application.Features.Products.Queries.GetById;
using ProductManagement.Application.Features.Products.Queries.GetList;
using ProductManagement.Application.Features.Products.Queries.GetListByPaginate;
using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Entities;

namespace ProductManagement.Application.Features.Products.Profiles;

public class ProductsMapper : Profile
{
    public ProductsMapper()
    {
        CreateMap<ProductAddCommand, Product>();
        CreateMap<Product, ProductAddedResponseDto>();

        CreateMap<ProductUpdateCommand, Product>();
        CreateMap<Product, ProductUpdatedResponseDto>();

        CreateMap<Product, ProductDeletedResponseDto>();

        CreateMap<Product, GetByIdProductResponseDto>();

        CreateMap<Product, GetListProductResponseDto>();

        CreateMap<Product, GetListByPaginateProductResponseDto>();
        CreateMap<Paginate<Product>, Paginate<GetListByPaginateProductResponseDto>>();
    }
}

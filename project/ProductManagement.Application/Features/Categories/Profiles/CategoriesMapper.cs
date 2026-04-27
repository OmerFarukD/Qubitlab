using AutoMapper;
using ProductManagement.Application.Features.Categories.Commands.Create;
using ProductManagement.Application.Features.Categories.Commands.Delete;
using ProductManagement.Application.Features.Categories.Commands.Update;
using ProductManagement.Application.Features.Categories.Queries.GetById;
using ProductManagement.Application.Features.Categories.Queries.GetList;
using ProductManagement.Application.Features.Categories.Queries.GetListByPaginate;
using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Entities;

namespace ProductManagement.Application.Features.Categories.Profiles;

public class CategoriesMapper : Profile
{
    public CategoriesMapper()
    {
        CreateMap<CategoryAddCommand, Category>();
        CreateMap<Category, CategoryAddedResponseDto>();

        CreateMap<CategoryUpdateCommand, Category>();
        CreateMap<Category, CategoryUpdatedResponseDto>();

        CreateMap<Category, CategoryDeletedResponseDto>();

        CreateMap<Category, GetByIdCategoryResponseDto>();

        CreateMap<Category, GetListCategoryResponseDto>();

        CreateMap<Category, GetListByPaginateCategoryResponseDto>();
        CreateMap<Paginate<Category>, Paginate<GetListByPaginateCategoryResponseDto>>();
    }
}

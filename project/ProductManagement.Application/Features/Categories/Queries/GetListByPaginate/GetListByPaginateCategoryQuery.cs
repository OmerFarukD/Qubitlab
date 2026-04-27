using AutoMapper;
using MediatR;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Entities;

namespace ProductManagement.Application.Features.Categories.Queries.GetListByPaginate;

public sealed class GetListByPaginateCategoryQuery : IRequest<Paginate<GetListByPaginateCategoryResponseDto>>
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;

    public sealed class GetListByPaginateCategoryQueryHandler(
        IMapper _mapper,
        ICategoryRepository _categoryRepository
    ) : IRequestHandler<GetListByPaginateCategoryQuery, Paginate<GetListByPaginateCategoryResponseDto>>
    {
        public async Task<Paginate<GetListByPaginateCategoryResponseDto>> Handle(
            GetListByPaginateCategoryQuery request,
            CancellationToken cancellationToken)
        {
            Paginate<Category> categories = await _categoryRepository.GetPaginateAsync(
                index: request.PageIndex,
                size: request.PageSize,
                cancellationToken: cancellationToken
            );

            Paginate<GetListByPaginateCategoryResponseDto> response =
                _mapper.Map<Paginate<GetListByPaginateCategoryResponseDto>>(categories);

            return response;
        }
    }
}

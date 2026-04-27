using AutoMapper;
using MediatR;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Categories.Queries.GetList;

public sealed class GetListCategoryQuery : IRequest<List<GetListCategoryResponseDto>>
{
    public sealed class GetListCategoryQueryHandler(
        IMapper _mapper,
        ICategoryRepository _categoryRepository
    ) : IRequestHandler<GetListCategoryQuery, List<GetListCategoryResponseDto>>
    {
        public async Task<List<GetListCategoryResponseDto>> Handle(GetListCategoryQuery request, CancellationToken cancellationToken)
        {
            List<Category> categories = await _categoryRepository.GetListAsync(
                cancellationToken: cancellationToken
            );

            List<GetListCategoryResponseDto> response = _mapper.Map<List<GetListCategoryResponseDto>>(categories);

            return response;
        }
    }
}

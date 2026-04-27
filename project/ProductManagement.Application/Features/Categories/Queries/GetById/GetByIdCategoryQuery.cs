using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Categories.Rules;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Categories.Queries.GetById;

public sealed class GetByIdCategoryQuery : IRequest<GetByIdCategoryResponseDto>
{
    public int Id { get; set; }

    public sealed class GetByIdCategoryQueryHandler(
        IMapper _mapper,
        ICategoryRepository _categoryRepository,
        CategoryBusinessRules _businessRules
    ) : IRequestHandler<GetByIdCategoryQuery, GetByIdCategoryResponseDto>
    {
        public async Task<GetByIdCategoryResponseDto> Handle(GetByIdCategoryQuery request, CancellationToken cancellationToken)
        {
            await _businessRules.CategoryIsPresentAsync(request.Id, cancellationToken);

            Category? category = await _categoryRepository.GetAsync(
                predicate: x => x.Id == request.Id,
                cancellationToken: cancellationToken
            );

            GetByIdCategoryResponseDto response = _mapper.Map<GetByIdCategoryResponseDto>(category);

            return response;
        }
    }
}

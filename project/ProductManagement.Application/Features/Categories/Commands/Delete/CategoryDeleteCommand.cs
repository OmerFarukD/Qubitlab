using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Categories.Rules;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Categories.Commands.Delete;

public sealed class CategoryDeleteCommand : IRequest<CategoryDeletedResponseDto>
{
    public int Id { get; set; }

    public sealed class CategoryDeleteCommandHandler(
        IMapper _mapper,
        ICategoryRepository _categoryRepository,
        CategoryBusinessRules _businessRules
    ) : IRequestHandler<CategoryDeleteCommand, CategoryDeletedResponseDto>
    {
        public async Task<CategoryDeletedResponseDto> Handle(CategoryDeleteCommand request, CancellationToken cancellationToken)
        {
            await _businessRules.CategoryIsPresentAsync(request.Id, cancellationToken);

            Category? category = await _categoryRepository.GetAsync(
                predicate: x => x.Id == request.Id,
                cancellationToken: cancellationToken
            );

            await _categoryRepository.DeleteAsync(category);
            CategoryDeletedResponseDto response = _mapper.Map<CategoryDeletedResponseDto>(category);

            return response;
        }
    }
}

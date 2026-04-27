using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Categories.Rules;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Features.Categories.Commands.Update;

public sealed class CategoryUpdateCommand : IRequest<CategoryUpdatedResponseDto>
{
    public int Id { get; set; }
    public string Name { get; set; }

    public sealed class CategoryUpdateCommandHandler(
        IMapper _mapper,
        ICategoryRepository _categoryRepository,
        CategoryBusinessRules _businessRules
    ) : IRequestHandler<CategoryUpdateCommand, CategoryUpdatedResponseDto>
    {
        public async Task<CategoryUpdatedResponseDto> Handle(CategoryUpdateCommand request, CancellationToken cancellationToken)
        {
            await _businessRules.CategoryIsPresentAsync(request.Id, cancellationToken);
            await _businessRules.CategoryNameMustBeUniqueWhenUpdatingAsync(request.Id, request.Name, cancellationToken);

            Category? category = await _categoryRepository.GetAsync(
                predicate: x => x.Id == request.Id,
                cancellationToken: cancellationToken
            );

            category = _mapper.Map(request, category);
            Category updatedCategory = await _categoryRepository.UpdateAsync(category);
            CategoryUpdatedResponseDto response = _mapper.Map<CategoryUpdatedResponseDto>(updatedCategory);

            return response;
        }
    }
}

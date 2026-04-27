using ProductManagement.Application.Features.Categories.Constants;
using ProductManagement.Application.Services.Repositories;
using Qubitlab.Application.BaseBusiness;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;

namespace ProductManagement.Application.Features.Categories.Rules;

public class CategoryBusinessRules(ICategoryRepository _categoryRepository) : BaseBusinessRules
{
    public async Task CategoryIsPresentAsync(int id, CancellationToken cancellationToken)
    {
        bool isPresent = await _categoryRepository.AnyAsync(
            predicate: x => x.Id == id,
            cancellationToken: cancellationToken
        );

        if (!isPresent)
            throw new BusinessException(CategoryMessages.CategoryNotFoundMessage);
    }

    public async Task CategoryNameMustBeUniqueAsync(string name, CancellationToken cancellationToken)
    {
        bool isPresent = await _categoryRepository.AnyAsync(
            predicate: x => x.Name == name,
            cancellationToken: cancellationToken
        );

        if (isPresent)
            throw new BusinessException(CategoryMessages.CategoryNameMustBeUniqueMessage);
    }

    public async Task CategoryNameMustBeUniqueWhenUpdatingAsync(int id, string name, CancellationToken cancellationToken)
    {
        bool isPresent = await _categoryRepository.AnyAsync(
            predicate: x => x.Name == name && x.Id != id,
            cancellationToken: cancellationToken
        );

        if (isPresent)
            throw new BusinessException(CategoryMessages.CategoryNameMustBeUniqueMessage);
    }
}

using ProductManagement.Application.Features.Products.Constants;
using ProductManagement.Application.Services.Repositories;
using Qubitlab.Application.BaseBusiness;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;

namespace ProductManagement.Application.Features.Products.Rules;

public class ProductBusinessRules : BaseBusinessRules
{
    private readonly IProductRepository _productRepository;

    public ProductBusinessRules(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task ProductIsPresentAsync(Guid id, CancellationToken cancellationToken)
    {
        bool isPresent = await _productRepository.AnyAsync(
            predicate: x => x.Id == id,
            cancellationToken: cancellationToken
        );

        if (!isPresent)
            throw new BusinessException(ProductMessages.ProductNotFoundMessage);
    }

    public async Task ProductNameMustBeUniqueAsync(string name, CancellationToken cancellationToken)
    {
        bool isPresent = await _productRepository.AnyAsync(
            predicate: x => x.Name == name,
            cancellationToken: cancellationToken
        );

        if (isPresent)
            throw new BusinessException(ProductMessages.ProductNameMustBeUniqueMessage);
    }

    public async Task ProductNameMustBeUniqueWhenUpdatingAsync(Guid id, string name, CancellationToken cancellationToken)
    {
        bool isPresent = await _productRepository.AnyAsync(
            predicate: x => x.Name == name && x.Id != id,
            cancellationToken: cancellationToken
        );

        if (isPresent)
            throw new BusinessException(ProductMessages.ProductNameMustBeUniqueMessage);
    }
}

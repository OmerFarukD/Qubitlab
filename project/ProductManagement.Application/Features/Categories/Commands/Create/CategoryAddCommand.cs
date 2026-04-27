using AutoMapper;
using MediatR;
using ProductManagement.Application.Features.Categories.Rules;
using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using Qubitlab.Application.Pipelines.Authorization;

namespace ProductManagement.Application.Features.Categories.Commands.Create;

public sealed class CategoryAddCommand : IRequest<CategoryAddedResponseDto> , IAuthRequired
{ 
    public string Name { get; set; }

    public string[] Roles => ["Admin"];
    
    public sealed class CategoryAddCommandHandler(
        IMapper _mapper, ICategoryRepository _categoryRepository,CategoryBusinessRules _businessRules
    )
        
        : IRequestHandler<CategoryAddCommand, CategoryAddedResponseDto>
    {

        public async Task<CategoryAddedResponseDto> Handle(CategoryAddCommand request, CancellationToken cancellationToken)
        {

            await _businessRules.CategoryNameMustBeUniqueAsync(request.Name,cancellationToken);

            Category category = _mapper.Map<Category>(request);

            Category addedCategory = await _categoryRepository.AddAsync(category);
           
            CategoryAddedResponseDto response = _mapper.Map<CategoryAddedResponseDto>(addedCategory);

            return response;
        }
    }


   
}
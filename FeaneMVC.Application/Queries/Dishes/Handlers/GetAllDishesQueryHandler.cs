using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Application.DTOs.Dishes;
using FeaneMVC.Application.Mapping;
using MediatR;

namespace FeaneMVC.Application.Queries.Dishes.Handlers;

public class GetAllDishesQueryHandler : IRequestHandler<GetAllDishesQuery, IReadOnlyList<DishDto>>
{
    private readonly IDishReadRepository _dishReadRepository;

    public GetAllDishesQueryHandler(IDishReadRepository dishReadRepository)
    {
        _dishReadRepository = dishReadRepository;
    }

    public async Task<IReadOnlyList<DishDto>> Handle(GetAllDishesQuery request, CancellationToken cancellationToken)
    {
        var dishes = await _dishReadRepository.GetAllAsync(cancellationToken);
        return dishes.ToDishDtoList();
    }
}

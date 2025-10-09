using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Dishes.Handlers;

public class SeedSampleDishesCommandHandler : IRequestHandler<SeedSampleDishesCommand, OperationResult<BulkSeedSummary>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SeedSampleDishesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<OperationResult<BulkSeedSummary>> Handle(SeedSampleDishesCommand request, CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.DishWriter.SeedRandomAsync(request.Count, cancellationToken);
        if (result.Status)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}

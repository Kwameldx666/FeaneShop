using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Dishes.Handlers
{
    public class DeleteDishCommandHandler : IRequestHandler<DeleteDishCommand, OperationResult>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteDishCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<OperationResult> Handle(DeleteDishCommand request, CancellationToken cancellationToken)
        {
            if (request.DishId == Guid.Empty)
            {
                return OperationResult.Failure("Invalid dish ID");
            }

            var result = await _unitOfWork.DishWriter.DeleteAsync(request.DishId, cancellationToken);
            if (result.Status)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return result;
        }
    }
}

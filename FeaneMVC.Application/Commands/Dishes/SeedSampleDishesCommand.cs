using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Dishes
{
    public sealed record SeedSampleDishesCommand(int Count) : IRequest<OperationResult<BulkSeedSummary>>;
}

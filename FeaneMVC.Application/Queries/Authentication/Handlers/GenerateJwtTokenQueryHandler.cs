using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Queries.Authentication.Handlers;

public class GenerateJwtTokenQueryHandler : IRequestHandler<GenerateJwtTokenQuery, string>
{
    private readonly IJwtTokenService _jwtTokenService;

    public GenerateJwtTokenQueryHandler(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    public Task<string> Handle(GenerateJwtTokenQuery request, CancellationToken cancellationToken)
    {
        var token = _jwtTokenService.GenerateToken(request.User);
        return Task.FromResult(token);
    }
}

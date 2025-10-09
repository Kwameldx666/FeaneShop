using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using MediatR;

namespace FeaneMVC.Application.Commands.Sessions.Handlers;

public class UpsertSessionCommandHandler : IRequestHandler<UpsertSessionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpsertSessionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(UpsertSessionCommand request, CancellationToken cancellationToken)
    {
        var sessionRepository = _unitOfWork.Sessions;

        var session = await sessionRepository.FindByCredentialAsync(request.Credential, request.IsEmail, cancellationToken);

        if (session != null)
        {
            session.CookieString = request.CookieValue;
            session.ExpireTime = request.ExpireTime;

            if (request.IsEmail)
            {
                session.Email = request.Credential;
            }
            else
            {
                session.Username = request.Credential;
            }
        }
        else
        {
            session = new Session
            {
                CookieString = request.CookieValue,
                ExpireTime = request.ExpireTime,
                Username = request.IsEmail ? null : request.Credential,
                Email = request.IsEmail ? request.Credential : null
            };

            await sessionRepository.AddSessionAsync(session, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

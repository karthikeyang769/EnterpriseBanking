using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IdentityService.Domain.Repositories;
using SharedKernel.Domain;

namespace IdentityService.Application.Commands;

public record RevokeTokenCommand(string Email, string RefreshToken) : IRequest<bool>;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new DomainException("Invalid email.", "INVALID_USER");
        }

        user.RevokeRefreshToken(request.RefreshToken);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using IdentityService.Domain.Repositories;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using SharedKernel.Domain;

namespace IdentityService.Application.Commands;

public record RefreshTokenCommand(string Email, string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new DomainException("Invalid email.", "INVALID_USER");
        }

        var activeToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken && t.IsActive);
        if (activeToken == null)
        {
            throw new DomainException("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");
        }

        // Revoke current token
        user.RevokeRefreshToken(request.RefreshToken);

        // Generate new tokens
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();
        user.AddRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newAccessToken = _jwtProvider.GenerateToken(user);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }
}

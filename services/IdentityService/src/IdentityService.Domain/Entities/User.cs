using System;
using System.Collections.Generic;
using SharedKernel.Domain;

namespace IdentityService.Domain.Entities;

public class User : Entity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Default Role

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public void AddRefreshToken(string token, DateTime expiresAt)
    {
        _refreshTokens.Add(new RefreshToken
        {
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            UserId = this.Id
        });
    }

    public void RevokeRefreshToken(string token)
    {
        var existing = _refreshTokens.Find(t => t.Token == token);
        if (existing != null)
        {
            existing.RevokedAt = DateTime.UtcNow;
        }
    }
}

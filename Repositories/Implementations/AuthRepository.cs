using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repositories.Interfaces;

namespace Billing_Software_Api.Repositories.Implementations;

/// <summary>
/// Repository implementation managing User data access directly via ApplicationDbContext.
/// </summary>
public class AuthRepository : GenericRepository<User>, IAuthRepository
{
    public AuthRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

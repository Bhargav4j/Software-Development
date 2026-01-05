using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Infrastructure.Data;

namespace TourManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserInfo entity
/// </summary>
public class UserInfoRepository : IUserInfoRepository
{
    private readonly TourManagementDbContext _context;
    private readonly ILogger<UserInfoRepository> _logger;

    public UserInfoRepository(TourManagementDbContext context, ILogger<UserInfoRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<UserInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserInfos
                .AsNoTracking()
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users from database");
            throw;
        }
    }

    public async Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email: {Email} from database", email);
            throw;
        }
    }

    public async Task<UserInfo> AddAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.UserInfos.AddAsync(userInfo, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user to database");
            throw;
        }
    }

    public async Task UpdateAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.UserInfos.Update(userInfo);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with email: {Email} in database", userInfo.Email);
            throw;
        }
    }

    public async Task DeleteAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var userInfo = await _context.UserInfos.FindAsync(new object[] { email }, cancellationToken);
            if (userInfo != null)
            {
                userInfo.IsActive = false;
                userInfo.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with email: {Email} from database", email);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserInfos.AnyAsync(u => u.Email == email && u.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of user with email: {Email}", email);
            throw;
        }
    }

    public async Task<UserInfo?> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user with email: {Email}", email);
            throw;
        }
    }
}

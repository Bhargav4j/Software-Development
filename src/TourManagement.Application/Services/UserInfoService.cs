using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service for user information business operations
/// </summary>
public class UserInfoService : IUserInfoService
{
    private readonly IUserInfoRepository _userInfoRepository;
    private readonly ILogger<UserInfoService> _logger;

    public UserInfoService(IUserInfoRepository userInfoRepository, ILogger<UserInfoService> logger)
    {
        _userInfoRepository = userInfoRepository ?? throw new ArgumentNullException(nameof(userInfoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<UserInfo>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all users");
            return await _userInfoRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<UserInfo?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user with email: {Email}", email);
            return await _userInfoRepository.GetByEmailAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with email: {Email}", email);
            throw;
        }
    }

    public async Task<UserInfo> CreateUserAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new user: {Email}", userInfo.Email);

            var exists = await _userInfoRepository.ExistsAsync(userInfo.Email, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"User with email {userInfo.Email} already exists");
            }

            userInfo.CreatedDate = DateTime.UtcNow;
            userInfo.IsActive = true;

            var result = await _userInfoRepository.AddAsync(userInfo, cancellationToken);
            _logger.LogInformation("User created successfully with email: {Email}", result.Email);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", userInfo.Email);
            throw;
        }
    }

    public async Task UpdateUserAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating user with email: {Email}", userInfo.Email);

            var exists = await _userInfoRepository.ExistsAsync(userInfo.Email, cancellationToken);
            if (!exists)
            {
                throw new InvalidOperationException($"User with email {userInfo.Email} not found");
            }

            userInfo.ModifiedDate = DateTime.UtcNow;

            await _userInfoRepository.UpdateAsync(userInfo, cancellationToken);
            _logger.LogInformation("User updated successfully with email: {Email}", userInfo.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with email: {Email}", userInfo.Email);
            throw;
        }
    }

    public async Task DeleteUserAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting user with email: {Email}", email);

            var exists = await _userInfoRepository.ExistsAsync(email, cancellationToken);
            if (!exists)
            {
                throw new InvalidOperationException($"User with email {email} not found");
            }

            await _userInfoRepository.DeleteAsync(email, cancellationToken);
            _logger.LogInformation("User deleted successfully with email: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with email: {Email}", email);
            throw;
        }
    }

    public async Task<UserInfo?> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating user with email: {Email}", email);
            return await _userInfoRepository.ValidateUserAsync(email, password, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user with email: {Email}", email);
            throw;
        }
    }
}

using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service implementation for UserInfo operations
/// </summary>
public class UserInfoService : IUserInfoService
{
    private readonly IUserInfoRepository _repository;
    private readonly ILogger<UserInfoService> _logger;

    public UserInfoService(IUserInfoRepository repository, ILogger<UserInfoService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<UserInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all users");
            return await _repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<UserInfo?> GetByIdAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user by email: {Email}", email);
            return await _repository.GetByIdAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<UserInfo> CreateAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new user: {Email}", userInfo.Email);

            if (await _repository.ExistsAsync(userInfo.Email, cancellationToken))
            {
                throw new InvalidOperationException($"User with email {userInfo.Email} already exists");
            }

            userInfo.CreatedDate = DateTime.UtcNow;
            userInfo.IsActive = true;

            await _repository.AddAsync(userInfo, cancellationToken);
            _logger.LogInformation("User created successfully: {Email}", userInfo.Email);

            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", userInfo.Email);
            throw;
        }
    }

    public async Task UpdateAsync(string email, UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating user: {Email}", email);

            var existingUser = await _repository.GetByIdAsync(email, cancellationToken);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with email {email} not found");
            }

            userInfo.ModifiedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(userInfo, cancellationToken);

            _logger.LogInformation("User updated successfully: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Email}", email);
            throw;
        }
    }

    public async Task DeleteAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting user: {Email}", email);
            await _repository.DeleteAsync(email, cancellationToken);
            _logger.LogInformation("User deleted successfully: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {Email}", email);
            throw;
        }
    }

    public async Task<UserInfo?> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating user: {Email}", email);
            return await _repository.ValidateUserAsync(email, password, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<UserInfo>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching users with term: {SearchTerm}", searchTerm);
            return await _repository.SearchAsync(searchTerm, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
            throw;
        }
    }
}

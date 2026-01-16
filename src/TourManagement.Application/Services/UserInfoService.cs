using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class UserInfoService : IUserInfoService
{
    private readonly IUserInfoRepository _repository;
    private readonly ILogger<UserInfoService> _logger;

    public UserInfoService(IUserInfoRepository repository, ILogger<UserInfoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserInfo>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all users");
            return await _repository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<UserInfo?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving user with email: {Email}", email);
            return await _repository.GetByEmailAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with email: {Email}", email);
            throw;
        }
    }

    public async Task<UserInfo?> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating user with email: {Email}", email);

            var user = await _repository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found with email: {Email}", email);
                return null;
            }

            // In production, use proper password hashing (BCrypt, Argon2, etc.)
            if (user.Password == password)
            {
                _logger.LogInformation("User validated successfully: {Email}", email);
                return user;
            }

            _logger.LogWarning("Invalid password for user: {Email}", email);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user with email: {Email}", email);
            throw;
        }
    }

    public async Task RegisterUserAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registering new user with email: {Email}", userInfo.Email);

            if (await _repository.ExistsAsync(userInfo.Email, cancellationToken))
            {
                _logger.LogWarning("User already exists with email: {Email}", userInfo.Email);
                throw new InvalidOperationException($"User with email {userInfo.Email} already exists");
            }

            userInfo.CreatedDate = DateTime.UtcNow;
            userInfo.IsActive = true;

            // In production, hash the password before storing
            // userInfo.Password = BCrypt.Net.BCrypt.HashPassword(userInfo.Password);

            await _repository.AddAsync(userInfo, cancellationToken);
            _logger.LogInformation("User registered successfully: {Email}", userInfo.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with email: {Email}", userInfo.Email);
            throw;
        }
    }

    public async Task UpdateUserAsync(UserInfo userInfo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating user with email: {Email}", userInfo.Email);

            var existingUser = await _repository.GetByEmailAsync(userInfo.Email, cancellationToken);
            if (existingUser == null)
            {
                _logger.LogWarning("User not found with email: {Email}", userInfo.Email);
                throw new InvalidOperationException($"User with email {userInfo.Email} not found");
            }

            userInfo.ModifiedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(userInfo, cancellationToken);
            _logger.LogInformation("User updated successfully: {Email}", userInfo.Email);
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

            if (!await _repository.ExistsAsync(email, cancellationToken))
            {
                _logger.LogWarning("User not found with email: {Email}", email);
                throw new InvalidOperationException($"User with email {email} not found");
            }

            await _repository.DeleteAsync(email, cancellationToken);
            _logger.LogInformation("User deleted successfully: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with email: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<UserInfo>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
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

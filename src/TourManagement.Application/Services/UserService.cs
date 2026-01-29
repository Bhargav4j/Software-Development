using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service implementation for User business operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all users");
            return await _userRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user with ID: {UserId}", id);
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user with email: {Email}", email);
            return await _userRepository.GetByEmailAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with email: {Email}", email);
            throw;
        }
    }

    public async Task<User> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new user: {Email}", user.Email);

            var existingUser = await _userRepository.GetByEmailAsync(user.Email, cancellationToken);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {user.Email} already exists");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;

            var createdUser = await _userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);

            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating user with ID: {UserId}", user.Id);

            var existingUser = await _userRepository.GetByIdAsync(user.Id, cancellationToken);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found");
            }

            user.ModifiedDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);
            _logger.LogInformation("User updated successfully: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", user.Id);
            throw;
        }
    }

    public async Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID: {UserId}", id);

            var exists = await _userRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                throw new InvalidOperationException($"User with ID {id} not found");
            }

            await _userRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("User deleted successfully: {UserId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Authenticating user: {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Email}", email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Email}", email);
                return null;
            }

            _logger.LogInformation("User authenticated successfully: {Email}", email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user: {Email}", email);
            throw;
        }
    }
}

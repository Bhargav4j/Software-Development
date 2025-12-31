using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// User service implementation with authentication and password hashing
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
            _logger.LogInformation("Fetching all users");
            var users = await _userRepository.GetAllAsync(cancellationToken);
            _logger.LogInformation("Successfully fetched {Count} users", users.Count());
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching user with ID {UserId}", id);
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with ID {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching user with email {Email}", email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with email {Email}", email);
            throw;
        }
    }

    public async Task<User> RegisterUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(user);

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty", nameof(password));
            }

            _logger.LogInformation("Registering new user with email {Email}", user.Email);

            var emailExists = await _userRepository.EmailExistsAsync(user.Email, cancellationToken);
            if (emailExists)
            {
                _logger.LogWarning("Email {Email} already exists", user.Email);
                throw new InvalidOperationException($"Email {user.Email} is already registered");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.CreatedDate = DateTime.UtcNow;
            user.IsActive = true;
            user.CreatedBy = user.Email;

            var createdUser = await _userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("Successfully registered user with ID {UserId}", createdUser.UserId);

            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user with email {Email}", user?.Email);
            throw;
        }
    }

    public async Task<User?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Authenticating user with email {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User with email {Email} not found", email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed: Invalid password for email {Email}", email);
                return null;
            }

            _logger.LogInformation("Successfully authenticated user with email {Email}", email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user with email {Email}", email);
            throw;
        }
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(user);

            _logger.LogInformation("Updating user with ID {UserId}", user.UserId);

            var existingUser = await _userRepository.GetByIdAsync(user.UserId, cancellationToken);
            if (existingUser == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update", user.UserId);
                throw new InvalidOperationException($"User with ID {user.UserId} not found");
            }

            user.ModifiedDate = DateTime.UtcNow;
            user.ModifiedBy = user.Email;

            await _userRepository.UpdateAsync(user, cancellationToken);
            _logger.LogInformation("Successfully updated user with ID {UserId}", user.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", user?.UserId);
            throw;
        }
    }

    public async Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting user with ID {UserId}", id);

            var exists = await _userRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                throw new InvalidOperationException($"User with ID {id} not found");
            }

            await _userRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Successfully deleted user with ID {UserId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            throw;
        }
    }
}

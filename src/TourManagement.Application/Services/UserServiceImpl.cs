using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Exceptions;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class UserServiceImpl : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserServiceImpl> _logger;

    public UserServiceImpl(IUserRepository userRepository, ILogger<UserServiceImpl> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _userRepository.GetAllAsync(cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _userRepository.GetByIdAsync(id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _userRepository.GetByEmailAsync(email, cancellationToken);

    public async Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userRepository.EmailExistsAsync(user.Email, cancellationToken);
        if (emailExists)
        {
            throw new ValidationException($"User with email '{user.Email}' already exists");
        }
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        _userRepository.UpdateAsync(user, cancellationToken);

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default) =>
        _userRepository.DeleteAsync(id, cancellationToken);

    public Task<IEnumerable<User>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default) =>
        _userRepository.SearchAsync(searchTerm, cancellationToken);

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return false;
        }
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}

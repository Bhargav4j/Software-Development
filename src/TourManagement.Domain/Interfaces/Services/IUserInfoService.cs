using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Services;

public interface IUserInfoService
{
    Task<IEnumerable<UserInfo>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserInfo?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserInfo?> ValidateUserAsync(string email, string password, CancellationToken cancellationToken = default);
    Task RegisterUserAsync(UserInfo userInfo, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(UserInfo userInfo, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserInfo>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default);
}

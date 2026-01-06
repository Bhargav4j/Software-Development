using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Services;

/// <summary>
/// Service interface for Admin business operations
/// </summary>
public interface IAdminService
{
    Task<IEnumerable<Admin>> GetAllAdminsAsync(CancellationToken cancellationToken = default);
    Task<Admin?> GetAdminByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Admin?> GetAdminByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Admin> CreateAdminAsync(Admin admin, string password, CancellationToken cancellationToken = default);
    Task UpdateAdminAsync(int id, Admin admin, CancellationToken cancellationToken = default);
    Task DeleteAdminAsync(int id, CancellationToken cancellationToken = default);
    Task<Admin?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}

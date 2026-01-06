using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Services;

/// <summary>
/// Service interface for Admin operations
/// </summary>
public interface IAdminService
{
    Task<Admin?> AuthenticateAdminAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Admin?> GetAdminByIdAsync(int id, CancellationToken cancellationToken = default);
}

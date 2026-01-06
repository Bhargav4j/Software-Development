using TourManagement.Domain.Entities;

namespace TourManagement.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Admin entity
/// </summary>
public interface IAdminRepository
{
    Task<IEnumerable<Admin>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Admin?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Admin?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Admin> AddAsync(Admin admin, CancellationToken cancellationToken = default);
    Task UpdateAsync(Admin admin, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Admin?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}

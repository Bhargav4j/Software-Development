using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Infrastructure.Data;

namespace TourManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Admin entity
/// </summary>
public class AdminRepository : IAdminRepository
{
    private readonly TourManagementDbContext _context;
    private readonly ILogger<AdminRepository> _logger;

    public AdminRepository(TourManagementDbContext context, ILogger<AdminRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Admin>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .Where(a => a.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Admin?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);
    }

    public async Task<Admin?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == email && a.IsActive, cancellationToken);
    }

    public async Task<Admin> AddAsync(Admin admin, CancellationToken cancellationToken = default)
    {
        await _context.Admins.AddAsync(admin, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return admin;
    }

    public async Task UpdateAsync(Admin admin, CancellationToken cancellationToken = default)
    {
        _context.Admins.Update(admin);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var admin = await _context.Admins.FindAsync(new object[] { id }, cancellationToken);
        if (admin != null)
        {
            admin.IsActive = false;
            admin.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Admin?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Email == email && a.Password == password && a.IsActive, cancellationToken);
    }
}

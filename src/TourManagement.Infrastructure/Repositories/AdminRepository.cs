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
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Admin>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Admins
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.Username)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all admins from database");
            throw;
        }
    }

    public async Task<Admin?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin with ID: {AdminId} from database", id);
            throw;
        }
    }

    public async Task<Admin?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Username == username && a.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin with username: {Username} from database", username);
            throw;
        }
    }

    public async Task<Admin> AddAsync(Admin admin, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Admins.AddAsync(admin, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return admin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding admin to database");
            throw;
        }
    }

    public async Task UpdateAsync(Admin admin, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin in database");
            throw;
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var admin = await _context.Admins.FindAsync(new object[] { id }, cancellationToken);
            if (admin != null)
            {
                admin.IsActive = false;
                admin.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting admin from database");
            throw;
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Admins.AnyAsync(a => a.Id == id && a.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if admin exists in database");
            throw;
        }
    }
}

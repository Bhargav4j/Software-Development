using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service implementation for Admin business operations
/// </summary>
public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IAdminRepository adminRepository, ILogger<AdminService> logger)
    {
        _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Admin>> GetAllAdminsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all admins");
            return await _adminRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all admins");
            throw;
        }
    }

    public async Task<Admin?> GetAdminByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving admin with ID: {AdminId}", id);
            return await _adminRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin with ID: {AdminId}", id);
            throw;
        }
    }

    public async Task<Admin?> GetAdminByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving admin with username: {Username}", username);
            return await _adminRepository.GetByUsernameAsync(username, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin with username: {Username}", username);
            throw;
        }
    }

    public async Task<Admin> CreateAdminAsync(Admin admin, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new admin: {Username}", admin.Username);

            var existingAdmin = await _adminRepository.GetByUsernameAsync(admin.Username, cancellationToken);
            if (existingAdmin != null)
            {
                _logger.LogWarning("Admin already exists with username: {Username}", admin.Username);
                throw new InvalidOperationException($"Admin with username {admin.Username} already exists");
            }

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            admin.CreatedDate = DateTime.UtcNow;
            admin.IsActive = true;
            admin.CreatedBy = "System";

            var result = await _adminRepository.AddAsync(admin, cancellationToken);
            _logger.LogInformation("Admin created successfully with ID: {AdminId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin: {Username}", admin.Username);
            throw;
        }
    }

    public async Task UpdateAdminAsync(int id, Admin admin, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating admin with ID: {AdminId}", id);

            var existingAdmin = await _adminRepository.GetByIdAsync(id, cancellationToken);
            if (existingAdmin == null)
            {
                _logger.LogWarning("Admin not found with ID: {AdminId}", id);
                throw new InvalidOperationException($"Admin with ID {id} not found");
            }

            existingAdmin.Username = admin.Username;
            existingAdmin.Email = admin.Email;
            existingAdmin.FullName = admin.FullName;
            existingAdmin.IsActive = admin.IsActive;
            existingAdmin.ModifiedDate = DateTime.UtcNow;
            existingAdmin.ModifiedBy = "System";

            await _adminRepository.UpdateAsync(existingAdmin, cancellationToken);
            _logger.LogInformation("Admin updated successfully with ID: {AdminId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating admin with ID: {AdminId}", id);
            throw;
        }
    }

    public async Task DeleteAdminAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting admin with ID: {AdminId}", id);

            var exists = await _adminRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("Admin not found with ID: {AdminId}", id);
                throw new InvalidOperationException($"Admin with ID {id} not found");
            }

            await _adminRepository.DeleteAsync(id, cancellationToken);
            _logger.LogInformation("Admin deleted successfully with ID: {AdminId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting admin with ID: {AdminId}", id);
            throw;
        }
    }

    public async Task<Admin?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Authenticating admin: {Username}", username);

            var admin = await _adminRepository.GetByUsernameAsync(username, cancellationToken);
            if (admin == null)
            {
                _logger.LogWarning("Admin not found: {Username}", username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            {
                _logger.LogWarning("Invalid password for admin: {Username}", username);
                return null;
            }

            if (!admin.IsActive)
            {
                _logger.LogWarning("Admin is inactive: {Username}", username);
                return null;
            }

            _logger.LogInformation("Admin authenticated successfully: {Username}", username);
            return admin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating admin: {Username}", username);
            throw;
        }
    }
}

using Microsoft.Extensions.Logging;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Repositories;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Service implementation for Admin operations
/// </summary>
public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IAdminRepository adminRepository, ILogger<AdminService> logger)
    {
        _adminRepository = adminRepository;
        _logger = logger;
    }

    public async Task<Admin?> AuthenticateAdminAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Authenticating admin with email {Email}", email);

            var admin = await _adminRepository.GetByEmailAsync(email, cancellationToken);

            if (admin == null || admin.Password != password)
            {
                _logger.LogWarning("Admin authentication failed for email {Email}", email);
                return null;
            }

            _logger.LogInformation("Admin authenticated successfully with email {Email}", email);
            return admin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating admin with email {Email}", email);
            throw;
        }
    }

    public async Task<Admin?> GetAdminByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving admin with ID {AdminId}", id);
            return await _adminRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin with ID {AdminId}", id);
            throw;
        }
    }
}

using AutoMapper;
using TourManagement.Application.DTOs;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

/// <summary>
/// Facade that provides DTO-based API for Tour operations
/// </summary>
public class TourFacade
{
    private readonly ITourService _tourService;
    private readonly IMapper _mapper;

    public TourFacade(ITourService tourService, IMapper mapper)
    {
        _tourService = tourService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TourDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tours = await _tourService.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TourDto>>(tours);
    }

    public async Task<TourDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tour = await _tourService.GetByIdAsync(id, cancellationToken);
        return tour == null ? null : _mapper.Map<TourDto>(tour);
    }

    public async Task<TourDto> CreateAsync(TourCreateDto dto, CancellationToken cancellationToken = default)
    {
        var tour = _mapper.Map<Tour>(dto);
        var created = await _tourService.CreateAsync(tour, cancellationToken);
        return _mapper.Map<TourDto>(created);
    }

    public async Task UpdateAsync(int id, TourUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var tour = await _tourService.GetByIdAsync(id, cancellationToken);
        if (tour != null)
        {
            _mapper.Map(dto, tour);
            await _tourService.UpdateAsync(tour, cancellationToken);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _tourService.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<TourDto>> GetActiveToursAsync(CancellationToken cancellationToken = default)
    {
        var tours = await _tourService.GetActiveToursAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TourDto>>(tours);
    }
}

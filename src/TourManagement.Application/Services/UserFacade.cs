using AutoMapper;
using TourManagement.Application.DTOs;
using TourManagement.Domain.Entities;
using TourManagement.Domain.Interfaces.Services;

namespace TourManagement.Application.Services;

public class UserFacade
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserFacade(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<UserDto?>(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByEmailAsync(email, cancellationToken);
        return _mapper.Map<UserDto?>(user);
    }

    public async Task<UserDto> CreateAsync(UserCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var user = _mapper.Map<User>(createDto);
        var createdUser = await _userService.CreateAsync(user, createDto.Password, cancellationToken);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task UpdateAsync(int id, UserUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var user = _mapper.Map<User>(updateDto);
        user.Id = id;
        await _userService.UpdateAsync(user, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await _userService.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<UserDto>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var users = await _userService.SearchAsync(searchTerm, cancellationToken);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        return await _userService.ValidateCredentialsAsync(email, password, cancellationToken);
    }
}

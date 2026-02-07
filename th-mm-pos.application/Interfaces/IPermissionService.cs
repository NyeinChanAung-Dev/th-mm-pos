using th_mm_pos.application.DTOs;

namespace th_mm_pos.application.Interfaces;

public interface IPermissionService
{
    Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto);
    Task<UserDto> UpdateUserPermissionsAsync(UserUpdateDto userUpdateDto);
    Task<bool> DeactivateUserAsync(int userId);
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<bool> CheckPermissionAsync(int userId, string permissionName);
}
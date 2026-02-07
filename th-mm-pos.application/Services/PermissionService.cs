using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.domain.Entities;
using th_mm_pos.domain.Interfaces;

namespace th_mm_pos.application.Services;

public class PermissionService(
    IUnitOfWork unitOfWork
) : IPermissionService
{
    public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password);

            // Create user
            var user = new User
            {
                Username = userCreateDto.Username,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Users.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            // Assign roles
            foreach (var roleId in userCreateDto.RoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                };
                // Note: This would need a UserRole repository or direct context access
                // For now, we'll save after creating the user
            }

            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<UserDto> UpdateUserPermissionsAsync(UserUpdateDto userUpdateDto)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            var user = await unitOfWork.Users.GetByIdAsync(userUpdateDto.UserId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Update IsActive if provided
            if (userUpdateDto.IsActive.HasValue)
            {
                user.IsActive = userUpdateDto.IsActive.Value;
            }

            // Update roles - would need to clear existing and add new ones
            // This requires access to UserRole repository or DbContext

            await unitOfWork.Users.UpdateAsync(user);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> DeactivateUserAsync(int userId)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            var user = await unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            await unitOfWork.Users.UpdateAsync(user);
            await unitOfWork.SaveChangesAsync();

            // TODO: Terminate active sessions

            await unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = await unitOfWork.Users.GetAllAsync();

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt
        });
    }

    public async Task<bool> CheckPermissionAsync(int userId, string permissionName)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        // This would need to query UserRoles -> RolePermissions -> Permissions
        // and check if any of the user's roles have the specified permission
        // For now, returning true as placeholder

        return true;
    }
}
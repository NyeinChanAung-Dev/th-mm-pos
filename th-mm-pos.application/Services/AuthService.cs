using th_mm_pos.application.DTOs;
using th_mm_pos.application.Interfaces;
using th_mm_pos.domain.Interfaces;

namespace th_mm_pos.application.Services;

public class AuthService(
    IUnitOfWork unitOfWork
) : IAuthService
{
    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        // Find user by username
        var users = await unitOfWork.Users.FindAsync(u => u.Username == username);
        var user = users.FirstOrDefault();

        if (user == null || !user.IsActive)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        // Verify password using BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await unitOfWork.Users.UpdateAsync(user);
            await unitOfWork.SaveChangesAsync();

            await unitOfWork.CommitTransactionAsync();

            // Get user details with roles and permissions
            var userDto = await GetCurrentUserAsync(user.Id);

            return new AuthResult
            {
                Success = true,
                User = userDto,
                Message = "Login successful"
            };
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task LogoutAsync(int userId)
    {
        // Cookie authentication logout is handled in the controller
        await Task.CompletedTask;
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        // Not needed for cookie authentication
        await Task.CompletedTask;
        return true;
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Get user roles and permissions
        var userRoles = await unitOfWork.Users.FindAsync(u => u.Id == userId);
        var userWithRoles = userRoles.FirstOrDefault();

        if (userWithRoles == null)
        {
            return null;
        }

        var roles = new List<string>();
        var permissions = new HashSet<string>();

        // This would need to be enhanced with proper eager loading
        // For now, returning basic user info
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
            Permissions = permissions.ToList()
        };
    }
}
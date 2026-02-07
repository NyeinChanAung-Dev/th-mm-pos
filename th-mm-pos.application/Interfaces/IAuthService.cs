using th_mm_pos.application.DTOs;

namespace th_mm_pos.application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password);
    Task LogoutAsync(int userId);
    Task<bool> ValidateSessionAsync(string sessionToken);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}
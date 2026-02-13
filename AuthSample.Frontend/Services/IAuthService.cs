using AuthSample.Frontend.Models;

namespace AuthSample.Frontend.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<bool> LoginAsync(LoginRequest request);
    Task<bool> RefreshTokenAsync();
    Task LogoutAsync();
    Task<UserInfoResponse?> GetCurrentUserAsync();
    Task<List<UserInfoResponse>> GetAllUsersAsync();
    Task<List<string>> GetFollowingAsync(Guid userId);
    Task<List<string>> GetFollowersAsync(Guid userId);
    Task<bool> FollowUserAsync(Guid userId);
    Task<bool> UnfollowUserAsync(Guid userId);
    Task<bool> IsAuthenticatedAsync();
}
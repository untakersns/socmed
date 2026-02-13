namespace AuthSample.Frontend.Models;

public class RegisterRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class UserInfoResponse
{
    public Guid Id { get; set; }
    public string? Login { get; set; }
    public int SubscriptionsCount { get; set; }
    public int SubscribersCount { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class ShortUserInfoResponse
{
    public string? Login { get; set; }
}
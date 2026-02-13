namespace socmed_front.Models
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string AccessToken { get; set; }= string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
    
    public record AuthResponse(string AccessToken, string RefreshToken);
    public record RefreshTokenRequest(string AccessToken, string RefreshToken);
    
    public class RefreshRequest
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
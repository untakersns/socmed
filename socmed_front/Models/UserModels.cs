using System.Text.Json.Serialization;

namespace socmed_front.Models
{
    public class UserDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }
    }

    public record UpdateProfileRequest(string UserName, string Email, string? Bio);
    public class UpdateUserProfileRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string? Bio { get; set; }
    }

}
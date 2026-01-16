using Microsoft.AspNetCore.Identity;

namespace socmed.Entity
{
    public class ApplicationUser:IdentityUser
    {
        public string? Bio { get; set; }
        public DateTime? BirthDate { get; set; } = DateTime.UtcNow;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;

        public virtual ICollection<UserFollower> ?Followers { get; set; } // Кто подписан на меня
        public virtual ICollection<UserFollower> ?Following { get; set; } // На кого подписан я
    }
}

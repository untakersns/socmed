namespace socmed.Entity
{
    public class UserFollower
    {
        public required string ObserverId { get; set; } // Тот, кто подписывается
        public required ApplicationUser Observer { get; set; }

        public required string TargetId { get; set; } // Тот, на кого подписываются
        public required ApplicationUser Target { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

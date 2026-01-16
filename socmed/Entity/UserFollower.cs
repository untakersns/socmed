namespace socmed.Entity
{
    public class UserFollower
    {
        public string ObserverId { get; set; } // Тот, кто подписывается
        public ApplicationUser Observer { get; set; }

        public string TargetId { get; set; } // Тот, на кого подписываются
        public ApplicationUser Target { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

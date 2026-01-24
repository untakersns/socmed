namespace socmed.Entity
{
    public class UserFollower
    {
        public string FollowerId { get; set; } = null!;
        public ApplicationUser Follower { get; set; } = null!;

        public string TargetId { get; set; } = null!;
        public ApplicationUser Target { get; set; } = null!;
    }
}

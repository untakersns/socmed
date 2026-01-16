using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using socmed.Entity;

namespace socmed.Data
{
    public class SMDbContext:IdentityDbContext<ApplicationUser>
    {
        public SMDbContext(DbContextOptions<SMDbContext> options) : base(options)  { }
        public DbSet<UserFollower> UserFollowers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserFollower>()
                .HasKey(k => new { k.ObserverId, k.TargetId });

            builder.Entity<UserFollower>()
                .HasOne(u => u.Observer)
                .WithMany(u => u.Following)
                .HasForeignKey(u => u.ObserverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserFollower>()
                .HasOne(u => u.Target)
                .WithMany(u => u.Followers)
                .HasForeignKey(u => u.TargetId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
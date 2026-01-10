using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace socmed.Data
{
    public class SMDbContext:IdentityDbContext<IdentityUser>
    {
        public SMDbContext(DbContextOptions<SMDbContext> options)
        : base(options) { }
    }
}

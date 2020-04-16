using JwtAuthentication.Infrastructure.Data.Entities;
using JwtAuthentication.Infrastructure.Data.EntityConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthentication.Infrastructure.Data
{
    public class JwtAuthenticationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
    {
        public JwtAuthenticationDbContext(DbContextOptions<JwtAuthenticationDbContext> options) : base(options)
        {

        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationUserConfiguration).Assembly);
        }
    }
}

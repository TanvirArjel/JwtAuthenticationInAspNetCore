using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JWTAuthenticationWithAspNetCoreAndAngular.Data.Entities;
using JWTAuthenticationWithAspNetCoreAndAngular.Data.EntityConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthenticationWithAspNetCoreAndAngular.Data
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

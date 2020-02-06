using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JWTAuthenticationWithAspNetCoreAndAngular.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JWTAuthenticationWithAspNetCoreAndAngular.Data.EntityConfigurations
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.HasIndex(ar => ar.Name).IsUnique(true);
            builder.HasIndex(ar => ar.NormalizedName).IsUnique(true);
        }
    }
}

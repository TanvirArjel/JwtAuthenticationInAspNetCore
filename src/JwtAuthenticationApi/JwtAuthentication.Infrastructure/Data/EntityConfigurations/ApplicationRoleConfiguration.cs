using JwtAuthentication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JwtAuthentication.Infrastructure.Data.EntityConfigurations
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

using JwtAuthentication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JwtAuthentication.Infrastructure.Data.EntityConfigurations
{
    public class EmailConfirmationCodeConfiguration : IEntityTypeConfiguration<EmailConfirmationCode>
    {
        public void Configure(EntityTypeBuilder<EmailConfirmationCode> builder)
        {
            builder.ToTable("EmailConfirmationCodes");
            builder.HasKey(evc => evc.Id);
            builder.Property(evc => evc.Email).HasMaxLength(50).IsRequired();
            builder.Property(evc => evc.Code).HasMaxLength(6).IsFixedLength().IsRequired();

            builder.Property(eu => eu.SentAtUtc).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
            builder.Property(eu => eu.UsedAtUtc).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
        }
    }
}

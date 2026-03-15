using CareerLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerLedger.Infrastructure.Data.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<CareerLedger.Domain.Entities.Application>
{
    public void Configure(EntityTypeBuilder<CareerLedger.Domain.Entities.Application> builder)
    {
        builder.ToTable("applications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(a => a.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.JobTitle)
            .HasColumnName("job_title")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.JobUrl)
            .HasColumnName("job_url")
            .HasMaxLength(2048);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();

        // Computed properties — not mapped to columns
        builder.Ignore(a => a.CurrentStatus);
        builder.Ignore(a => a.EventCount);
        builder.Ignore(a => a.LastUpdated);

        builder.HasIndex(a => a.AccountId)
            .HasDatabaseName("idx_applications_account_id");

        builder.HasIndex(a => a.CreatedAt)
            .IsDescending()
            .HasDatabaseName("idx_applications_created_at");

        builder.HasMany(a => a.Events)
            .WithOne(e => e.Application)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

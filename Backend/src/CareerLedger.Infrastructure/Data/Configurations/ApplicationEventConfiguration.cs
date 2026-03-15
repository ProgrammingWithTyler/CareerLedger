using CareerLedger.Domain.Entities;
using CareerLedger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerLedger.Infrastructure.Data.Configurations;

public class ApplicationEventConfiguration : IEntityTypeConfiguration<ApplicationEvent>
{
    public void Configure(EntityTypeBuilder<ApplicationEvent> builder)
    {
        builder.ToTable("application_events", t =>
        {
            t.HasCheckConstraint(
                "chk_event_type",
                "event_type IN ('Submitted', 'InReview', 'PhoneScreen', 'TechnicalInterview', " +
                "'OnsiteInterview', 'OfferReceived', 'OfferAccepted', 'OfferDeclined', " +
                "'Rejected', 'Withdrawn')"
            );
        });

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ApplicationId)
            .HasColumnName("application_id")
            .IsRequired();

        builder.Property(e => e.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(e => e.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasMaxLength(5000);

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();

        // Covering index for CurrentStatus derivation — targets <5ms query time
        // Covers: SELECT TOP 1 event_type FROM application_events
        //         WHERE application_id = @id ORDER BY occurred_at DESC, created_at DESC
        builder.HasIndex(e => new { e.ApplicationId, e.OccurredAt, e.CreatedAt })
            .IsDescending(false, true, true)
            .HasDatabaseName("idx_application_events_app_occurred");

        // Analytics index — account-level queries ordered by time
        builder.HasIndex(e => new { e.AccountId, e.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("idx_application_events_account_occurred");

        // Account FK — NoAction to prevent cascade conflict with Application → Events cascade
        builder.HasOne(e => e.Application)
            .WithMany(a => a.Events)
            .HasForeignKey(e => e.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

using CareerLedger.Domain.Enums;

namespace CareerLedger.Domain.Entities;

/// <summary>
/// Represents an immutable lifecycle event for a job application.
/// Events form an append-only audit log of application progress.
/// </summary>
public class ApplicationEvent
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; private set; }

    /// <summary>Foreign key to Application</summary>
    public Guid ApplicationId { get; private set; }

    /// <summary>Foreign key to Account (for data ownership)</summary>
    public Guid AccountId { get; private set; }

    /// <summary>Type of event (lifecycle state)</summary>
    public EventType EventType { get; private set; }

    /// <summary>When the event actually occurred (may be backfilled)</summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>Optional notes about this event (e.g., interview feedback, rejection reason)</summary>
    public string? Notes { get; private set; }

    /// <summary>When this event was logged in the system (UTC)</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Navigation property to parent Application</summary>
    public Application Application { get; private set; } = null!;

    // Private constructor for EF Core
    private ApplicationEvent() { }

    /// <summary>
    /// Factory method to create a new application event with validation.
    /// Events are IMMUTABLE after creation - no setters!
    /// </summary>
    /// <param name="applicationId">ID of the application this event belongs to</param>
    /// <param name="accountId">ID of the account that owns this application</param>
    /// <param name="eventType">Type of lifecycle event</param>
    /// <param name="occurredAt">When the event happened (UTC)</param>
    /// <param name="notes">Optional notes about this event</param>
    /// <returns>New ApplicationEvent instance</returns>
    /// <exception cref="ArgumentException">If required fields are invalid</exception>
    public static ApplicationEvent Create(
        Guid applicationId,
        Guid accountId,
        EventType eventType,
        DateTime occurredAt,
        string? notes = null)
    {
        if (applicationId == Guid.Empty)
            throw new ArgumentException("Application ID is required", nameof(applicationId));

        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID is required", nameof(accountId));

        if (occurredAt > DateTime.UtcNow.AddHours(1)) // Allow 1 hour buffer for clock skew
            throw new ArgumentException("Occurred date cannot be in the future", nameof(occurredAt));

        if (!string.IsNullOrWhiteSpace(notes) && notes.Length > 5000)
            throw new ArgumentException("Notes cannot exceed 5000 characters", nameof(notes));

        return new ApplicationEvent
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            AccountId = accountId,
            EventType = eventType,
            OccurredAt = occurredAt,
            Notes = notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}

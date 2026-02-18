using CareerLedger.Domain.Enums;

namespace CareerLedger.Domain.Entities;

/// <summary>
/// Represents a job application. Aggregate root that owns ApplicationEvents.
/// CurrentStatus is derived from the most recent event (not stored).
/// </summary>
public class Application
{
    /// <summary>Unique identifier</summary>
    public Guid Id { get; private set; }

    /// <summary>Foreign key to Account that owns this application</summary>
    public Guid AccountId { get; private set; }

    /// <summary>Company name (e.g., "TechCorp", "Startup XYZ")</summary>
    public string CompanyName { get; private set; } = string.Empty;

    /// <summary>Job title (e.g., "Senior Software Engineer")</summary>
    public string JobTitle { get; private set; } = string.Empty;

    /// <summary>Optional URL to job posting</summary>
    public string? JobUrl { get; private set; }

    /// <summary>When this application record was created (UTC)</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Navigation property to parent Account</summary>
    public Account Account { get; private set; } = null!;

    /// <summary>
    /// Collection of lifecycle events (audit trail).
    /// Events are immutable and append-only.
    /// </summary>
    public ICollection<ApplicationEvent> Events { get; private set; } = new List<ApplicationEvent>();

    /// <summary>
    /// Computed property: Current status derived from most recent event.
    /// Ordered by OccurredAt DESC, then CreatedAt DESC.
    /// Returns Submitted if no events exist (should never happen in practice).
    /// </summary>
    public EventType CurrentStatus
    {
        get
        {
            var latestEvent = Events
                .OrderByDescending(e => e.OccurredAt)
                .ThenByDescending(e => e.CreatedAt)
                .FirstOrDefault();

            return latestEvent?.EventType ?? EventType.Submitted;
        }
    }

    /// <summary>
    /// Computed property: Number of events (useful for UI display)
    /// </summary>
    public int EventCount => Events.Count;

    /// <summary>
    /// Computed property: Last time any event was added (for sorting by activity)
    /// </summary>
    public DateTime? LastUpdated => Events
        .OrderByDescending(e => e.CreatedAt)
        .FirstOrDefault()?.CreatedAt;

    // Private constructor for EF Core
    private Application() { }

    /// <summary>
    /// Factory method to create a new application with initial "Submitted" event.
    /// </summary>
    /// <param name="accountId">ID of the account creating this application</param>
    /// <param name="companyName">Company name (1-255 characters)</param>
    /// <param name="jobTitle">Job title (1-255 characters)</param>
    /// <param name="jobUrl">Optional URL to job posting (max 2048 characters)</param>
    /// <param name="submittedAt">When the application was submitted (defaults to now)</param>
    /// <param name="notes">Optional notes about the application</param>
    /// <returns>New Application instance with initial Submitted event</returns>
    /// <exception cref="ArgumentException">If required fields are invalid</exception>
    public static Application Create(
        Guid accountId,
        string companyName,
        string jobTitle,
        string? jobUrl = null,
        DateTime? submittedAt = null,
        string? notes = null)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID is required", nameof(accountId));

        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required", nameof(companyName));

        if (companyName.Length > 255)
            throw new ArgumentException("Company name cannot exceed 255 characters", nameof(companyName));

        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new ArgumentException("Job title is required", nameof(jobTitle));

        if (jobTitle.Length > 255)
            throw new ArgumentException("Job title cannot exceed 255 characters", nameof(jobTitle));

        if (!string.IsNullOrWhiteSpace(jobUrl) && jobUrl.Length > 2048)
            throw new ArgumentException("Job URL cannot exceed 2048 characters", nameof(jobUrl));

        var application = new Application
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CompanyName = companyName.Trim(),
            JobTitle = jobTitle.Trim(),
            JobUrl = jobUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // Create initial "Submitted" event
        var submittedEvent = ApplicationEvent.Create(
            application.Id,
            accountId,
            EventType.Submitted,
            submittedAt ?? DateTime.UtcNow,
            notes
        );

        application.Events.Add(submittedEvent);

        return application;
    }

    /// <summary>
    /// Update basic application information (not the lifecycle state).
    /// To change lifecycle state, add a new event via AddEvent.
    /// </summary>
    /// <param name="companyName">Updated company name</param>
    /// <param name="jobTitle">Updated job title</param>
    /// <param name="jobUrl">Updated job URL</param>
    public void UpdateBasicInfo(string companyName, string jobTitle, string? jobUrl)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name is required", nameof(companyName));

        if (companyName.Length > 255)
            throw new ArgumentException("Company name cannot exceed 255 characters", nameof(companyName));

        if (string.IsNullOrWhiteSpace(jobTitle))
            throw new ArgumentException("Job title is required", nameof(jobTitle));

        if (jobTitle.Length > 255)
            throw new ArgumentException("Job title cannot exceed 255 characters", nameof(jobTitle));

        if (!string.IsNullOrWhiteSpace(jobUrl) && jobUrl.Length > 2048)
            throw new ArgumentException("Job URL cannot exceed 2048 characters", nameof(jobUrl));

        CompanyName = companyName.Trim();
        JobTitle = jobTitle.Trim();
        JobUrl = jobUrl?.Trim();
    }

    /// <summary>
    /// Add a new lifecycle event to this application.
    /// Note: Transition validation should be done in ApplicationLifecycleService before calling this.
    /// </summary>
    /// <param name="event">The event to add</param>
    public void AddEvent(ApplicationEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        if (@event.ApplicationId != Id)
            throw new ArgumentException("Event does not belong to this application", nameof(@event));

        if (@event.AccountId != AccountId)
            throw new ArgumentException("Event does not belong to the same account", nameof(@event));

        Events.Add(@event);
    }
}

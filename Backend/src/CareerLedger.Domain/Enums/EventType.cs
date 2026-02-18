namespace CareerLedger.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a job application.
/// Transitions are validated by ApplicationLifecycleService.
/// </summary>
public enum EventType
{
    /// <summary>Application submitted to company</summary>
    Submitted,

    /// <summary>Company confirmed receipt and is reviewing</summary>
    InReview,

    /// <summary>Initial phone screening scheduled/completed</summary>
    PhoneScreen,

    /// <summary>Technical interview (coding, system design)</summary>
    TechnicalInterview,

    /// <summary>Full-day onsite or panel interview</summary>
    OnsiteInterview,

    /// <summary>Formal job offer received</summary>
    OfferReceived,

    /// <summary>Offer accepted (terminal state)</summary>
    OfferAccepted,

    /// <summary>Offer declined (terminal state)</summary>
    OfferDeclined,

    /// <summary>Application rejected by company (terminal state)</summary>
    Rejected,

    /// <summary>Application withdrawn by candidate (terminal state)</summary>
    Withdrawn
}

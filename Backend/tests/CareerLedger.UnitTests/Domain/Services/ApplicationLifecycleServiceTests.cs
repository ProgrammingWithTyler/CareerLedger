using CareerLedger.Domain.Enums;
using CareerLedger.Domain.Services;
using Xunit;

namespace CareerLedger.UnitTests.Domain.Services;

public class ApplicationLifecycleServiceTests
{
    private readonly ApplicationLifecycleService _service;

    public ApplicationLifecycleServiceTests()
    {
        _service = new ApplicationLifecycleService();
    }

    [Fact]
    public void IsValidTransition_FirstEventMustBeSubmitted_ReturnsTrue()
    {
        var result = _service.IsValidTransition(null, EventType.Submitted);
        Assert.True(result);
    }

    [Fact]
    public void IsValidTransition_FirstEventNotSubmitted_ReturnsFalse()
    {
        var result = _service.IsValidTransition(null, EventType.InReview);
        Assert.False(result);
    }

    [Fact]
    public void IsValidTransition_SubmittedToInReview_ReturnsTrue()
    {
        var result = _service.IsValidTransition(EventType.Submitted, EventType.InReview);
        Assert.True(result);
    }

    [Fact]
    public void IsValidTransition_OfferAcceptedToRejected_ReturnsFalse()
    {
        var result = _service.IsValidTransition(EventType.OfferAccepted, EventType.Rejected);
        Assert.False(result, "Cannot transition from terminal state");
    }

    [Fact]
    public void IsValidTransition_AllValidPaths_Verified()
    {
        var validTransitions = new[]
        {
            (EventType.Submitted, EventType.InReview),
            (EventType.Submitted, EventType.Rejected),
            (EventType.Submitted, EventType.Withdrawn),
            (EventType.InReview, EventType.PhoneScreen),
            (EventType.InReview, EventType.Rejected),
            (EventType.InReview, EventType.Withdrawn),
            (EventType.PhoneScreen, EventType.TechnicalInterview),
            (EventType.PhoneScreen, EventType.Rejected),
            (EventType.PhoneScreen, EventType.Withdrawn),
            (EventType.TechnicalInterview, EventType.OnsiteInterview),
            (EventType.TechnicalInterview, EventType.OfferReceived),
            (EventType.TechnicalInterview, EventType.Rejected),
            (EventType.TechnicalInterview, EventType.Withdrawn),
            (EventType.OnsiteInterview, EventType.OfferReceived),
            (EventType.OnsiteInterview, EventType.Rejected),
            (EventType.OnsiteInterview, EventType.Withdrawn),
            (EventType.OfferReceived, EventType.OfferAccepted),
            (EventType.OfferReceived, EventType.OfferDeclined),
            (EventType.OfferReceived, EventType.Withdrawn)
        };

        foreach (var (from, to) in validTransitions)
        {
            Assert.True(
                _service.IsValidTransition(from, to),
                $"Expected {from} -> {to} to be valid"
            );
        }
    }

    [Fact]
    public void IsValidTransition_AllInvalidPaths_Blocked()
    {
        var invalidTransitions = new[]
        {
            (EventType.InReview, EventType.Submitted),       // Backward
            (EventType.PhoneScreen, EventType.Submitted),    // Backward
            (EventType.OfferAccepted, EventType.PhoneScreen),// From terminal
            (EventType.Rejected, EventType.InReview),        // From terminal
            (EventType.Withdrawn, EventType.OfferReceived)   // From terminal
        };

        foreach (var (from, to) in invalidTransitions)
        {
            Assert.False(
                _service.IsValidTransition(from, to),
                $"Expected {from} -> {to} to be invalid"
            );
        }
    }

    [Fact]
    public void IsTerminal_TerminalStates_ReturnTrue()
    {
        Assert.True(_service.IsTerminal(EventType.OfferAccepted));
        Assert.True(_service.IsTerminal(EventType.OfferDeclined));
        Assert.True(_service.IsTerminal(EventType.Rejected));
        Assert.True(_service.IsTerminal(EventType.Withdrawn));
    }

    [Fact]
    public void IsTerminal_NonTerminalStates_ReturnFalse()
    {
        Assert.False(_service.IsTerminal(EventType.Submitted));
        Assert.False(_service.IsTerminal(EventType.InReview));
        Assert.False(_service.IsTerminal(EventType.PhoneScreen));
        Assert.False(_service.IsTerminal(EventType.TechnicalInterview));
        Assert.False(_service.IsTerminal(EventType.OnsiteInterview));
        Assert.False(_service.IsTerminal(EventType.OfferReceived));
    }

    [Fact]
    public void GetValidNextStates_NullCurrent_ReturnsSubmittedOnly()
    {
        var result = _service.GetValidNextStates(null);
        Assert.Single(result);
        Assert.Contains(EventType.Submitted, result);
    }

    [Fact]
    public void GetValidNextStates_TerminalState_ReturnsEmptyList()
    {
        var result = _service.GetValidNextStates(EventType.Rejected);
        Assert.Empty(result);
    }

    [Fact]
    public void GetValidNextStates_Submitted_ReturnsCorrectOptions()
    {
        var result = _service.GetValidNextStates(EventType.Submitted);
        Assert.Contains(EventType.InReview, result);
        Assert.Contains(EventType.Rejected, result);
        Assert.Contains(EventType.Withdrawn, result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetTransitionErrorMessage_NullCurrent_MentionsSubmitted()
    {
        var message = _service.GetTransitionErrorMessage(null, EventType.InReview);
        Assert.Contains("Submitted", message);
    }

    [Fact]
    public void GetTransitionErrorMessage_TerminalCurrent_MentionsTerminal()
    {
        var message = _service.GetTransitionErrorMessage(EventType.Rejected, EventType.InReview);
        Assert.Contains("terminal", message, StringComparison.OrdinalIgnoreCase);
    }
}

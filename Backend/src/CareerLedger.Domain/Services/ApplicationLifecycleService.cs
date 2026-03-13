using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareerLedger.Domain.Enums;

namespace CareerLedger.Domain.Services;

public class ApplicationLifecycleService : IApplicationLifecycleService
{

    private static readonly Dictionary<EventType, List<EventType>> ValidTransitions = new()
    {
        [EventType.Submitted] = new() {
            EventType.InReview,
            EventType.Rejected,
            EventType.Withdrawn
        },
        [EventType.InReview] = new() {
            EventType.PhoneScreen,
            EventType.Rejected,
            EventType.Withdrawn
        },
        [EventType.PhoneScreen] = new() {
            EventType.TechnicalInterview,
            EventType.Rejected,
            EventType.Withdrawn
        },
        [EventType.TechnicalInterview] = new() {
            EventType.OnsiteInterview,
            EventType.OfferReceived, // Can skip onsite
            EventType.Rejected,
            EventType.Withdrawn
        },
        [EventType.OnsiteInterview] = new() {
            EventType.OfferReceived,
            EventType.Rejected,
            EventType.Withdrawn
        },
        [EventType.OfferReceived] = new() {
            EventType.OfferAccepted,
            EventType.OfferDeclined,
            EventType.Withdrawn // Offer expired
        }
        // Terminal states have no valid transitions
    };

    public string GetTransitionErrorMessage(EventType? current, EventType proposed)
    {
        if (current == null)
            return $"First event must be 'Submitted', not '{proposed}'";

        if (IsTerminal(current.Value))
            return $"Cannot transition from terminal state '{current}'";

        return $"Invalid transition from '{current}' to '{proposed}'";
    }

    public List<EventType> GetValidNextStates(EventType? current)
    {
        if (current == null)
            return new List<EventType> { EventType.Submitted };

        if (IsTerminal(current.Value))
            return new List<EventType>();

        return ValidTransitions.ContainsKey(current.Value)
            ? ValidTransitions[current.Value]
            : new List<EventType>();
    }

    public bool IsTerminal(EventType eventType)
    {
        return eventType == EventType.OfferAccepted
            || eventType == EventType.OfferDeclined
            || eventType == EventType.Rejected
            || eventType == EventType.Withdrawn;
    }

    public bool IsValidTransition(EventType? current, EventType proposed)
    {
        // First event is always valid (null -> Submitted)
        if (current == null)
            return proposed == EventType.Submitted;

        // Terminal states cannot transition
        if (IsTerminal(current.Value))
            return false;

        // Check if transition is in valid list
        return ValidTransitions.ContainsKey(current.Value)
            && ValidTransitions[current.Value].Contains(proposed);
    }
}

using CareerLedger.Domain.Enums;

namespace CareerLedger.Domain.Services;

public interface IApplicationLifecycleService
{
    bool IsValidTransition(EventType? current, EventType proposed);
    bool IsTerminal(EventType eventType);
    string GetTransitionErrorMessage(EventType? current, EventType proposed);
    List<EventType> GetValidNextStates(EventType? current);
}


namespace SharedKernel;

/// <summary>
/// Interface for saving event history
/// Allows tracking of all events for debugging/auditing
/// </summary>
public interface IEventHistoryService
{
    Task SaveEventAsync<T>(T eventData, string eventType, string source, string orderId, string status);
}


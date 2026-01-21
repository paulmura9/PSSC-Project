namespace SharedKernel.Messaging;

/// <summary>
/// Interface for event handlers
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Event types this handler can process
    /// </summary>
    string[] EventTypes { get; }

    /// <summary>
    /// Handle an event from JSON
    /// </summary>
    Task<EventProcessingResult> HandleAsync(string eventJson, CancellationToken cancellationToken = default);
}


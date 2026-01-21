using System.Text.Json;

namespace SharedKernel.Messaging;

/// <summary>
/// Abstract base class for event handlers
/// </summary>
/// <typeparam name="TEvent">The type of event this handler processes</typeparam>
public abstract class AbstractEventHandler<TEvent> : IEventHandler where TEvent : class
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Event types this handler can process
    /// </summary>
    public abstract string[] EventTypes { get; }

    /// <summary>
    /// Handle raw JSON event - deserializes and calls typed handler
    /// </summary>
    public async Task<EventProcessingResult> HandleAsync(string eventJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var @event = DeserializeEvent(eventJson);
            return await OnHandleAsync(@event, cancellationToken);
        }
        catch (JsonException ex)
        {
            return EventProcessingResult.Failed($"JSON deserialization error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return EventProcessingResult.Failed($"Error handling event: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Override this method to implement event handling logic
    /// </summary>
    protected abstract Task<EventProcessingResult> OnHandleAsync(TEvent @event, CancellationToken cancellationToken);

    /// <summary>
    /// Deserialize JSON to event type
    /// </summary>
    private TEvent DeserializeEvent(string json)
    {
        var input = JsonSerializer.Deserialize<TEvent>(json, JsonOptions);
        if (input is null)
        {
            throw new InvalidOperationException($"Deserializing event generated null value. {json}");
        }
        return input;
    }
}


namespace SharedKernel;

/// <summary>
/// Record for storing event history in CSV
/// </summary>
public record EventHistoryRecord
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string OrderId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTime ProcessedAt { get; init; }
}


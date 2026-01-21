namespace SharedKernel.Messaging;

/// <summary>
/// Result of event processing
/// </summary>
public sealed record EventProcessingResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private EventProcessingResult(bool success, string? errorMessage, Exception? exception)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static EventProcessingResult Succeeded() => new(true, null, null);
    
    public static EventProcessingResult Failed(string errorMessage) => new(false, errorMessage, null);
    
    public static EventProcessingResult Failed(Exception exception) => new(false, exception.Message, exception);
    
    public static EventProcessingResult Failed(string errorMessage, Exception exception) => new(false, errorMessage, exception);
}


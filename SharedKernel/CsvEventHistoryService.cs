using System.Globalization;
using System.Text.Json;

namespace SharedKernel;

/// <summary>
/// Interface for saving event history
/// </summary>
public interface IEventHistoryService
{
    Task SaveEventAsync<T>(T eventData, string eventType, string source, string orderId, string status);
}

/// <summary>
/// Saves event history to a CSV file
/// Thread-safe implementation
/// </summary>
public class CsvEventHistoryService : IEventHistoryService
{
    private readonly string _csvPath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static readonly string[] Headers = { "Id", "EventType", "Source", "OrderId", "Status", "Payload", "ProcessedAt" };

    public CsvEventHistoryService(string csvPath)
    {
        _csvPath = csvPath;
        EnsureFileExists();
    }

    private void EnsureFileExists()
    {
        if (!File.Exists(_csvPath))
        {
            // Create file with headers
            File.WriteAllText(_csvPath, string.Join(",", Headers) + Environment.NewLine);
        }
    }

    public async Task SaveEventAsync<T>(T eventData, string eventType, string source, string orderId, string status)
    {
        await _lock.WaitAsync();
        try
        {
            var payload = JsonSerializer.Serialize(eventData);
            // Escape payload for CSV (wrap in quotes, escape internal quotes)
            var escapedPayload = "\"" + payload.Replace("\"", "\"\"") + "\"";

            var record = new EventHistoryRecord
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                Source = source,
                OrderId = orderId,
                Status = status,
                Payload = payload,
                ProcessedAt = DateTime.UtcNow
            };

            var line = $"{record.Id},{record.EventType},{record.Source},{record.OrderId},{record.Status},{escapedPayload},{record.ProcessedAt:O}";
            
            await File.AppendAllTextAsync(_csvPath, line + Environment.NewLine);
        }
        finally
        {
            _lock.Release();
        }
    }
}


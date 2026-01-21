using System.Text;

namespace Invoicing.Events;

/// <summary>
/// Events that occur when an invoice is generated
/// </summary>
public interface IInvoiceGeneratedEvent { }

/// <summary>
/// Event indicating invoice was generated successfully
/// Values are calculated in RON. EUR is derived for presentation.
/// </summary>
public record InvoiceGeneratedEvent : IInvoiceGeneratedEvent
{
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public decimal SubTotal { get; init; }
    public decimal Tax { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime GeneratedAt { get; init; }
    
    // Currency support - DB stores RON, EUR is derived
    public string Currency { get; init; } = "RON";
    public decimal TotalInRon { get; init; }
    public decimal? TotalInEur { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("===== Invoice Generated =====");
        sb.AppendLine($"Invoice: {InvoiceNumber} ({InvoiceId})");
        sb.AppendLine($"Order ID: {OrderId}");
        sb.AppendLine($"Shipment ID: {ShipmentId}");
        sb.AppendLine($"User ID: {UserId}");
        sb.AppendLine($"SubTotal: {SubTotal:N2} RON");
        sb.AppendLine($"Tax (VAT): {Tax:N2} RON");
        sb.AppendLine($"Total: {TotalAmount:N2} RON");
        if (TotalInEur.HasValue)
            sb.AppendLine($"Total (EUR): {TotalInEur.Value:N2} EUR");
        sb.AppendLine($"Generated: {GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("=============================");
        return sb.ToString();
    }
}

/// <summary>
/// Event indicating invoice generation failed
/// </summary>
public record InvoiceGenerationFailedEvent : IInvoiceGeneratedEvent
{
    public Guid ShipmentId { get; init; }
    public Guid OrderId { get; init; }
    public IEnumerable<string> Reasons { get; init; } = Enumerable.Empty<string>();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("===== Invoice Failed =====");
        sb.AppendLine($"Shipment ID: {ShipmentId}");
        sb.AppendLine($"Order ID: {OrderId}");
        sb.AppendLine($"Reasons: {string.Join(", ", Reasons)}");
        sb.AppendLine("==========================");
        return sb.ToString();
    }
}


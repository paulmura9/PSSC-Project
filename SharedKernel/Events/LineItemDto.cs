namespace SharedKernel;

/// <summary>
/// Generic DTO for line items in events (Order, Shipment, Invoice)
/// Replaces OrderLineEventDto, ShipmentLineItem, InvoiceLineItem, ShipmentLineEventDto
/// Uses primitive types for JSON serialization across service boundaries
/// </summary>
public sealed record LineItemDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }

    /// <summary>
    /// Optional VAT amount (used by Invoicing)
    /// </summary>
    public decimal? VatAmount { get; init; }

    /// <summary>
    /// Optional VAT rate (used by Invoicing)
    /// </summary>
    public decimal? VatRate { get; init; }

    public LineItemDto() { }

    public LineItemDto(string name, string? description, string? category, int quantity, decimal unitPrice, decimal lineTotal)
    {
        Name = name;
        Description = description;
        Category = category;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }
}


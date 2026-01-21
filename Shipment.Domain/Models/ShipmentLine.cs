namespace Shipment.Domain.Models;

/// <summary>
/// Represents a line item in a shipment
/// </summary>
public sealed record ShipmentLine
{
    public ProductName Name { get; }
    public string Description { get; }
    public string Category { get; }
    public Quantity Quantity { get; }
    public Money UnitPrice { get; }
    public Money LineTotal { get; }

    public ShipmentLine(ProductName name, string description, string category, Quantity quantity, Money unitPrice, Money lineTotal)
    {
        Name = name;
        Description = description;
        Category = category;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }
}


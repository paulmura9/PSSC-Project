namespace Shipment.Domain.Models;

/// <summary>
/// Represents a line item in a shipment
/// </summary>
public sealed record ShipmentLine
{
    public ProductName Name { get; }
    public ProductDescription Description { get; }
    public ProductCategory Category { get; }
    public Quantity Quantity { get; }
    public Money UnitPrice { get; }
    public Money LineTotal { get; }

    public ShipmentLine(ProductName name, ProductDescription description, ProductCategory category, Quantity quantity, Money unitPrice, Money lineTotal)
    {
        Name = name;
        Description = description;
        Category = category;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }
}


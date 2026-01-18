namespace Shipment.Domain.Models;

/// <summary>
/// Represents a line item in a shipment with Value Objects
/// </summary>
public sealed record ShipmentLine
{
    public ProductName Name { get; }
    public string? Description { get; }
    public string? Category { get; }
    public Quantity Quantity { get; }
    public Money UnitPrice { get; }
    public Money LineTotal { get; }

    public ShipmentLine(
        ProductName name, 
        string? description,
        string? category,
        Quantity quantity, 
        Money unitPrice, 
        Money lineTotal)
    {
        Name = name;
        Description = description;
        Category = category;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }

    /// <summary>
    /// Creates a ShipmentLine from primitive values
    /// </summary>
    public static ShipmentLine Create(
        string name, 
        string? description,
        string? category,
        int quantity, 
        decimal unitPrice)
    {
        var productName = new ProductName(name);
        var qty = new Quantity(quantity);
        var price = new Money(unitPrice);
        var total = new Money(unitPrice * quantity);

        return new ShipmentLine(productName, description, category, qty, price, total);
    }

    /// <summary>
    /// Creates a ShipmentLine from primitive values with pre-calculated total
    /// </summary>
    public static ShipmentLine Create(
        string name,
        string? description,
        string? category,
        int quantity,
        decimal unitPrice,
        decimal lineTotal)
    {
        var productName = new ProductName(name);
        var qty = new Quantity(quantity);
        var price = new Money(unitPrice);
        var total = new Money(lineTotal);

        return new ShipmentLine(productName, description, category, qty, price, total);
    }
}


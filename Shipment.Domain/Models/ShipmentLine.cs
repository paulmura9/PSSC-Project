namespace Shipment.Domain.Models;

/// <summary>
/// Represents a line item in a shipment
/// </summary>
public sealed record ShipmentLine
{
    public string Name { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal { get; }

    public ShipmentLine(string name, int quantity, decimal unitPrice, decimal lineTotal)
    {
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }
}


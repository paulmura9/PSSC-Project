namespace Ordering.Domain.Models;

/// <summary>
/// Represents a validated order line with calculated line total
/// </summary>
public sealed record ValidatedOrderLine
{
    //toate value object, nu trimite baze brute (ca la cvv)
    public string Name { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal { get; }

    internal ValidatedOrderLine(string name, int quantity, decimal unitPrice, decimal lineTotal)
    {
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }

    public static ValidatedOrderLine Create(string name, int quantity, decimal unitPrice)
    {
        return new ValidatedOrderLine(name, quantity, unitPrice, quantity * unitPrice);
    }
}


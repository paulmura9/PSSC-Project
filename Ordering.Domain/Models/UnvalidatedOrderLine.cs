namespace Ordering.Domain.Models;

/// <summary>
/// Represents an unvalidated order line from user input
/// </summary>
public sealed record UnvalidatedOrderLine
{
    public string Name { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    public UnvalidatedOrderLine(string name, int quantity, decimal unitPrice)
    {
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}


namespace Invoicing.Models;

/// <summary>
/// Represents an invoice line item
/// </summary>
public sealed record InvoiceLine
{
    public string Name { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal { get; }

    public InvoiceLine(string name, int quantity, decimal unitPrice, decimal lineTotal)
    {
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }
}


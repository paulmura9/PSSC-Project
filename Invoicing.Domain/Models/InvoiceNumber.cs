namespace Invoicing.Models;

/// <summary>
/// Value Object representing an invoice number
/// </summary>
public sealed record InvoiceNumber
{
    public string Value { get; }

    public InvoiceNumber(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Generates a new invoice number
    /// </summary>
    public static InvoiceNumber Generate()
    {
        var number = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        return new InvoiceNumber(number);
    }

    public override string ToString() => Value;
}


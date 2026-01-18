namespace Invoicing.Models;

/// <summary>
/// Value Object representing VAT rate based on product category
/// Essential = 11%, Electronics/Other = 21%
/// </summary>
public sealed record VatRate
{
    public decimal Value { get; }
    
    /// <summary>
    /// VAT rate for Essential products (11%)
    /// </summary>
    public static readonly VatRate Essential = new(0.11m);
    
    /// <summary>
    /// VAT rate for Electronics and other products (21%)
    /// </summary>
    public static readonly VatRate Default = new(0.21m);

    private VatRate(decimal value)
    {
        if (value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(nameof(value), "VAT rate must be between 0 and 1");
        Value = value;
    }

    /// <summary>
    /// Gets the VAT rate for a given product category
    /// </summary>
    public static VatRate ForCategory(ProductCategory category) => category switch
    {
        ProductCategory.Essential => Essential,
        ProductCategory.Electronics => Default,
        ProductCategory.Other => Default,
        _ => Default
    };

    /// <summary>
    /// Calculates VAT amount for a given net amount
    /// </summary>
    public decimal CalculateVat(decimal netAmount) => Math.Round(netAmount * Value, 2);

    /// <summary>
    /// Returns the percentage value (e.g., 11 or 21)
    /// </summary>
    public int Percentage => (int)(Value * 100);

    public override string ToString() => $"{Percentage}%";
}


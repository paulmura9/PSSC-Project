namespace Invoicing.Models;

/// <summary>
/// Represents an invoice line item with VAT calculation
/// Data comes pre-validated from Ordering/Shipment
/// VAT rate is determined by product category:
/// - Essential: 11%
/// - Electronics/Other: 21%
/// 
/// Discount is distributed proportionally across lines, then VAT is applied on discounted amount.
/// </summary>
public sealed record InvoiceLine
{
    public ProductName Name { get; }
    public ProductDescription Description { get; }
    public ProductCategory Category { get; }
    public Quantity Quantity { get; }
    public Money UnitPrice { get; }
    
    /// <summary>
    /// Initial net amount before discount (Quantity * UnitPrice)
    /// </summary>
    public Money LineNetInitial { get; }
    
    /// <summary>
    /// Discount amount applied to this line (proportional share of total discount)
    /// </summary>
    public Money LineDiscount { get; }
    
    /// <summary>
    /// Net amount after discount (LineNetInitial - LineDiscount)
    /// </summary>
    public Money LineNetAfterDiscount { get; }
    
    /// <summary>
    /// VAT rate based on product category
    /// </summary>
    public VatRate VatRate { get; }
    
    /// <summary>
    /// VAT amount for this line (LineNetAfterDiscount * VatRate)
    /// </summary>
    public Money VatAmount { get; }
    
    /// <summary>
    /// Total with VAT (LineNetAfterDiscount + VatAmount)
    /// </summary>
    public Money LineTotalWithVat { get; }

    private InvoiceLine(
        ProductName name, 
        ProductDescription description, 
        ProductCategory category, 
        Quantity quantity, 
        Money unitPrice,
        Money lineNetInitial,
        Money lineDiscount,
        Money lineNetAfterDiscount,
        VatRate vatRate,
        Money vatAmount,
        Money lineTotalWithVat)
    {
        Name = name;
        Description = description;
        Category = category;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineNetInitial = lineNetInitial;
        LineDiscount = lineDiscount;
        LineNetAfterDiscount = lineNetAfterDiscount;
        VatRate = vatRate;
        VatAmount = vatAmount;
        LineTotalWithVat = lineTotalWithVat;
    }

    /// <summary>
    /// Creates an InvoiceLine with proportional discount applied
    /// </summary>
    /// <param name="name">Product name</param>
    /// <param name="description">Product description</param>
    /// <param name="category">Product category (determines VAT rate)</param>
    /// <param name="quantity">Quantity ordered</param>
    /// <param name="unitPrice">Unit price</param>
    /// <param name="lineShare">This line's share of the subtotal (0-1)</param>
    /// <param name="totalDiscount">Total discount amount to distribute</param>
    public static InvoiceLine CreateWithDiscount(
        string name,
        string description,
        string category,
        int quantity,
        decimal unitPrice,
        decimal lineShare,
        decimal totalDiscount)
    {
        var parsedCategory = ProductCategoryExtensions.ParseCategory(category);
        var vatRate = VatRate.ForCategory(parsedCategory);
        
        var lineNetInitial = quantity * unitPrice;
        var lineDiscount = Math.Round(totalDiscount * lineShare, 2);
        var lineNetAfterDiscount = Math.Max(0, lineNetInitial - lineDiscount);
        var vatAmount = vatRate.CalculateVat(lineNetAfterDiscount);
        var lineTotalWithVat = lineNetAfterDiscount + vatAmount;

        return new InvoiceLine(
            new ProductName(name),
            new ProductDescription(description),
            parsedCategory,
            new Quantity(quantity),
            new Money(unitPrice),
            new Money(lineNetInitial),
            new Money(lineDiscount),
            new Money(lineNetAfterDiscount),
            vatRate,
            new Money(vatAmount),
            new Money(lineTotalWithVat));
    }

    /// <summary>
    /// Creates an InvoiceLine without discount (backwards compatibility)
    /// </summary>
    public static InvoiceLine Create(
        string name, 
        string description, 
        string category, 
        int quantity, 
        decimal unitPrice)
    {
        return CreateWithDiscount(name, description, category, quantity, unitPrice, 0, 0);
    }
    
    /// <summary>
    /// Creates an InvoiceLine from primitive values with lineTotal (backwards compatibility)
    /// </summary>
    public static InvoiceLine Create(
        string name, 
        string description, 
        string category, 
        int quantity, 
        decimal unitPrice,
        decimal lineTotal)
    {
        // Ignore lineTotal, recalculate with VAT
        return Create(name, description, category, quantity, unitPrice);
    }
}


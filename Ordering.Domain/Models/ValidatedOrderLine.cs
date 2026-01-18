namespace Ordering.Domain.Models;

/// <summary>
/// Represents a validated order line with calculated line total
/// Uses Value Objects for type safety
/// </summary>
public sealed record ValidatedOrderLine
{
    public ProductName Name { get; }
    public ProductDescription Description { get; }
    public ProductCategory Category { get; }
    public Quantity Quantity { get; }
    public Price UnitPrice { get; }
    public Price LineTotal { get; }

    internal ValidatedOrderLine(ProductName name, ProductDescription description, ProductCategory category, Quantity quantity, Price unitPrice, Price lineTotal)
    {
        Name = name;
        Description = description;
        Category = category;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = lineTotal;
    }

    /// <summary>
    /// Creates a ValidatedOrderLine from an UnvalidatedOrderLine
    /// </summary>
    public static ValidatedOrderLine CreateFrom(UnvalidatedOrderLine line)
    {
        var lineTotal = line.UnitPrice.Multiply(line.Quantity);
        return new ValidatedOrderLine(
            line.Name,
            line.Description,
            line.Category,
            line.Quantity,
            line.UnitPrice,
            lineTotal
        );
    }

    /// <summary>
    /// Factory method to create from raw values
    /// </summary>
    public static ValidatedOrderLine Create(string name, string description, string category, int quantity, decimal unitPrice)
    {
        var nameVo = ProductName.Create(name);
        var descVo = ProductDescription.Create(description);
        var catVo = ProductCategory.Create(category);
        var qtyVo = Quantity.Create(quantity);
        var priceVo = Price.Create(unitPrice);
        var lineTotal = priceVo.Multiply(qtyVo);
        
        return new ValidatedOrderLine(nameVo, descVo, catVo, qtyVo, priceVo, lineTotal);
    }
}


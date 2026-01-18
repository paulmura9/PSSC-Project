namespace Ordering.Domain.Models;

/// <summary>
/// Represents an unvalidated order line from user input
/// Uses Value Objects for type safety and validation
/// </summary>
public sealed record UnvalidatedOrderLine
{
    public ProductName Name { get; }
    public ProductDescription Description { get; }
    public ProductCategory Category { get; }
    public Quantity Quantity { get; }
    public Price UnitPrice { get; }

    public UnvalidatedOrderLine(ProductName name, ProductDescription description, ProductCategory category, Quantity quantity, Price unitPrice)
    {
        Name = name;
        Description = description;
        Category = category;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Factory method to create from raw values (validates and creates Value Objects)
    /// </summary>
    public static UnvalidatedOrderLine Create(string name, string description, string category, int quantity, decimal unitPrice)
    {
        return new UnvalidatedOrderLine(
            ProductName.Create(name),
            ProductDescription.Create(description),
            ProductCategory.Create(category),
            Quantity.Create(quantity),
            Price.Create(unitPrice)
        );
    }
}


namespace Invoicing.Models;

/// <summary>
/// Product category enum used to determine VAT rate
/// </summary>
public enum ProductCategory
{
    /// <summary>
    /// Essential products - VAT 11%
    /// </summary>
    Essential,
    
    /// <summary>
    /// Electronics products - VAT 21%
    /// </summary>
    Electronics,
    
    /// <summary>
    /// Other products - VAT 21% (default)
    /// </summary>
    Other
}

/// <summary>
/// Extension methods for ProductCategory
/// </summary>
public static class ProductCategoryExtensions
{
    /// <summary>
    /// Parses a string to ProductCategory (case-insensitive)
    /// </summary>
    public static ProductCategory ParseCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return ProductCategory.Other;

        return category.Trim().ToLowerInvariant() switch
        {
            "essential" => ProductCategory.Essential,
            "electronics" => ProductCategory.Electronics,
            _ => ProductCategory.Other
        };
    }
}


namespace Invoicing.Models;

/// <summary>
/// Currency converter for invoice presentation
/// Database stores RON only. EUR is derived for display.
/// 
/// Fixed conversion rate: 1 RON = 0.20 EUR
/// </summary>
public static class CurrencyConverter
{
    /// <summary>
    /// Fixed conversion rate: 1 RON = 0.20 EUR
    /// </summary>
    public const decimal RonToEurRate = 0.20m;

    /// <summary>
    /// Converts RON amount to EUR
    /// </summary>
    /// <param name="ronAmount">Amount in RON</param>
    /// <returns>Equivalent amount in EUR</returns>
    public static decimal ConvertRonToEur(decimal ronAmount)
    {
        return Math.Round(ronAmount * RonToEurRate, 2);
    }
}


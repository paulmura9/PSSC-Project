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

    /// <summary>
    /// Converts EUR amount to RON (for reference only)
    /// </summary>
    /// <param name="eurAmount">Amount in EUR</param>
    /// <returns>Equivalent amount in RON</returns>
    public static decimal ConvertEurToRon(decimal eurAmount)
    {
        return Math.Round(eurAmount / RonToEurRate, 2);
    }

    /// <summary>
    /// Gets the display amount based on requested currency
    /// </summary>
    /// <param name="ronAmount">Original amount in RON</param>
    /// <param name="currency">Requested display currency</param>
    /// <returns>Amount in requested currency</returns>
    public static decimal GetDisplayAmount(decimal ronAmount, Currency currency)
    {
        return currency.IsEur ? ConvertRonToEur(ronAmount) : ronAmount;
    }

    /// <summary>
    /// Gets the currency symbol
    /// </summary>
    public static string GetSymbol(Currency currency)
    {
        return currency.IsEur ? "€" : "RON";
    }
}


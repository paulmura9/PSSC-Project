using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that persists a priced order to the database
/// </summary>
public class PersistOrderOperation : OrderOperation<IPersistence>
{
    protected override async Task<IOrder> OnPricedAsync(PricedOrder order, IPersistence persistence, CancellationToken cancellationToken)
    {
        // Mask card number - show only last 4 digits
        var cardNumberMasked = MaskCardNumber(order.CardNumber);

        var persistableLines = order.Lines
            .Select(line => new PersistableOrderLine(
                line.Name,
                line.Quantity,
                line.UnitPrice,
                line.LineTotal))
            .ToList();

        var persistableOrder = new PersistableOrder(
            order.UserId,
            order.DeliveryAddress,
            order.PostalCode,
            order.Phone,
            cardNumberMasked,
            order.TotalPrice,
            persistableLines);

        var orderId = await persistence.SaveOrderAsync(persistableOrder, cancellationToken);

        var persistedOrder = new PersistedOrder(
            orderId,
            order.Lines,
            order.UserId,
            order.DeliveryAddress,
            order.PostalCode,
            order.Phone,
            order.TotalPrice,
            DateTime.UtcNow);

        return persistedOrder;
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return string.Empty;
        }

        // Remove any spaces or dashes
        var cleanedNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (cleanedNumber.Length <= 4)
        {
            return cleanedNumber;
        }

        // Show only last 4 digits
        var maskedLength = cleanedNumber.Length - 4;
        return new string('*', maskedLength) + cleanedNumber.Substring(maskedLength);
    }
}


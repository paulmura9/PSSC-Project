using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that validates an unvalidated order
/// </summary>
public class ValidateOrderOperation : OrderOperation
{
    protected override Task<IOrder> OnUnvalidatedAsync(UnvalidatedOrder order, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Validate delivery information
        if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
        {
            errors.Add("Delivery address is required");
        }

        if (string.IsNullOrWhiteSpace(order.PostalCode))
        {
            errors.Add("Postal code is required");
        }

        if (string.IsNullOrWhiteSpace(order.Phone))
        {
            errors.Add("Phone number is required");
        }

        // Validate payment information
        if (string.IsNullOrWhiteSpace(order.CardNumber))
        {
            errors.Add("Card number is required");
        }

        if (string.IsNullOrWhiteSpace(order.Cvv))
        {
            errors.Add("CVV is required");
        }
        else if (!Cvv.TryParse(order.Cvv, out _))
        {
            errors.Add("CVV must be exactly 3 digits");
        }

        if (string.IsNullOrWhiteSpace(order.Expiry))
        {
            errors.Add("Card expiry is required");
        }

        // Validate user
        if (order.UserId == Guid.Empty)
        {
            errors.Add("User ID is required");
        }

        // Validate lines
        if (order.Lines.Count == 0)
        {
            errors.Add("At least one order line is required");
        }
        else
        {
            for (int i = 0; i < order.Lines.Count; i++)
            {
                var line = order.Lines.ElementAt(i);
                
                if (string.IsNullOrWhiteSpace(line.Name))
                {
                    errors.Add($"Product name is required for line {i + 1}");
                }

                if (line.Quantity <= 0)
                {
                    errors.Add($"Quantity must be greater than 0 for line {i + 1}");
                }

                if (line.UnitPrice <= 0)
                {
                    errors.Add($"Unit price must be greater than 0 for line {i + 1}");
                }
            }
        }

        if (errors.Count > 0)
        {
            var invalidOrder = new InvalidOrder(order.Lines, errors);
            return Task.FromResult<IOrder>(invalidOrder);
        }

        // Create validated order with calculated line totals
        var validatedLines = order.Lines
            .Select(line => ValidatedOrderLine.Create(line.Name, line.Quantity, line.UnitPrice))
            .ToList();

        var validatedOrder = new ValidatedOrder(
            validatedLines,
            order.UserId,
            order.DeliveryAddress,
            order.PostalCode,
            order.Phone,
            order.CardNumber,
            order.Expiry);

        return Task.FromResult<IOrder>(validatedOrder);
    }
}


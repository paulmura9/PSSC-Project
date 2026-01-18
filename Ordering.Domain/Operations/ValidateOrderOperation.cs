using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that validates an unvalidated order
/// Validates pickup method, payment method, and address requirements
/// </summary>
public class ValidateOrderOperation : OrderOperation
{
    protected override Task<IOrder> OnUnvalidatedAsync(UnvalidatedOrder order, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Validate and create PickupMethod VO
        PickupMethod? pickupMethod = null;
        if (string.IsNullOrWhiteSpace(order.PickupMethodInput))
        {
            errors.Add("Pickup method is required");
        }
        else if (!PickupMethod.TryParse(order.PickupMethodInput, out pickupMethod))
        {
            errors.Add($"Invalid pickup method: {order.PickupMethodInput}. Allowed values: HomeDelivery, EasyBoxPickup, PostOfficePickup");
        }

        // Validate address based on pickup method
        if (pickupMethod != null)
        {
            if (pickupMethod.RequiresAddress)
            {
                // HomeDelivery requires address
                if (string.IsNullOrWhiteSpace(order.Street))
                    errors.Add("Street is required for HomeDelivery");
                if (string.IsNullOrWhiteSpace(order.City))
                    errors.Add("City is required for HomeDelivery");
                if (string.IsNullOrWhiteSpace(order.PostalCode))
                    errors.Add("Postal code is required for HomeDelivery");
                
                // HomeDelivery must NOT have PickupPointId
                if (!string.IsNullOrWhiteSpace(order.PickupPointIdInput))
                    errors.Add("PickupPointId must be empty for HomeDelivery");
            }
            else if (pickupMethod.RequiresPickupPointId)
            {
                // EasyBoxPickup or PostOfficePickup requires PickupPointId
                if (string.IsNullOrWhiteSpace(order.PickupPointIdInput))
                    errors.Add($"PickupPointId is required for {pickupMethod.Value}");
            }
        }

        // Validate and create PickupPointId VO (if provided)
        PickupPointId? pickupPointId = null;
        if (!string.IsNullOrWhiteSpace(order.PickupPointIdInput))
        {
            if (!PickupPointId.TryParse(order.PickupPointIdInput, out pickupPointId))
            {
                errors.Add($"Invalid pickup point ID: {order.PickupPointIdInput}");
            }
        }

        // Validate and create PaymentMethod VO
        PaymentMethod? paymentMethod = null;
        if (string.IsNullOrWhiteSpace(order.PaymentMethodInput))
        {
            errors.Add("Payment method is required");
        }
        else if (!PaymentMethod.TryParse(order.PaymentMethodInput, out paymentMethod))
        {
            errors.Add($"Invalid payment method: {order.PaymentMethodInput}. Allowed values: CashOnDelivery, CardOnDelivery, CardOnline");
        }

        // Phone is always required
        if (string.IsNullOrWhiteSpace(order.Phone))
        {
            errors.Add("Phone number is required");
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
                
                if (string.IsNullOrWhiteSpace(line.Name.Value))
                {
                    errors.Add($"Product name is required for line {i + 1}");
                }

                if (line.Quantity.Value <= 0)
                {
                    errors.Add($"Quantity must be greater than 0 for line {i + 1}");
                }

                if (line.UnitPrice.Value <= 0)
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
            .Select(ValidatedOrderLine.CreateFrom)
            .ToList();

        var validatedOrder = new ValidatedOrder(
            validatedLines,
            order.UserId,
            order.Street,
            order.City,
            order.PostalCode,
            order.Phone,
            order.Email,
            order.DeliveryNotes,
            order.PremiumSubscription,
            pickupMethod!,
            pickupPointId,
            paymentMethod!);

        return Task.FromResult<IOrder>(validatedOrder);
    }
}


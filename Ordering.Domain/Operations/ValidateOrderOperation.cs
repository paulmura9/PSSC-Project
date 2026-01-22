using Ordering.Domain.Models;
using static Ordering.Domain.Models.Order;

namespace Ordering.Domain.Operations;

/// <summary>
/// Operation that validates an unvalidated order
/// Validates pickup method, payment method, and address requirements
/// SYNC - pure transformation (no I/O)
/// </summary>
public class ValidateOrderOperation : OrderOperation
{
    protected override IOrder OnUnvalidated(UnvalidatedOrder order)
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
                // HomeDelivery requires address - validate using VOs
                if (string.IsNullOrWhiteSpace(order.Street))
                {
                    errors.Add("Street is required for HomeDelivery");
                }
                else
                {
                    try { Street.Create(order.Street); }
                    catch (ArgumentException ex) { errors.Add($"Invalid street: {ex.Message}"); }
                }
                
                if (string.IsNullOrWhiteSpace(order.City))
                {
                    errors.Add("City is required for HomeDelivery");
                }
                else
                {
                    try { City.Create(order.City); }
                    catch (ArgumentException ex) { errors.Add($"Invalid city: {ex.Message}"); }
                }
                
                if (string.IsNullOrWhiteSpace(order.PostalCode))
                {
                    errors.Add("Postal code is required for HomeDelivery");
                }
                else
                {
                    try { PostalCode.Create(order.PostalCode); }
                    catch (ArgumentException ex) { errors.Add($"Invalid postal code: {ex.Message}"); }
                }
                
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

        // Phone is always required - validate using PhoneNumber VO
        if (string.IsNullOrWhiteSpace(order.Phone))
        {
            errors.Add("Phone number is required");
        }
        else
        {
            try
            {
                PhoneNumber.Create(order.Phone);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Invalid phone number: {ex.Message}");
            }
        }

        // Validate email format if provided
        if (!string.IsNullOrWhiteSpace(order.Email))
        {
            try
            {
                EmailAddress.Create(order.Email);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Invalid email: {ex.Message}");
            }
        }

        // Validate delivery notes if provided
        if (!string.IsNullOrWhiteSpace(order.DeliveryNotes))
        {
            try
            {
                DeliveryNotes.Create(order.DeliveryNotes);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Invalid delivery notes: {ex.Message}");
            }
        }

        // Validate user using CustomerId VO
        try
        {
            CustomerId.Create(order.UserId);
        }
        catch (ArgumentException ex)
        {
            errors.Add($"Invalid user ID: {ex.Message}");
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

        // Invalid order
        if (errors.Count > 0)
        {
            return new InvalidOrder(order.Lines, errors);
        }

        // Create validated order with calculated line totals and Value Objects
        var validatedLines = order.Lines
            .Select(ValidatedOrderLine.CreateFrom)
            .ToList();

        // Create VOs for validated data
        var customerId = CustomerId.Create(order.UserId);
        var street = !string.IsNullOrWhiteSpace(order.Street) ? Street.Create(order.Street) : null;
        var city = !string.IsNullOrWhiteSpace(order.City) ? City.Create(order.City) : null;
        var postalCode = !string.IsNullOrWhiteSpace(order.PostalCode) ? PostalCode.Create(order.PostalCode) : null;
        var phone = PhoneNumber.Create(order.Phone);
        var email = !string.IsNullOrWhiteSpace(order.Email) ? EmailAddress.Create(order.Email) : null;
        var deliveryNotes = !string.IsNullOrWhiteSpace(order.DeliveryNotes) ? DeliveryNotes.Create(order.DeliveryNotes) : null;

        return new ValidatedOrder(
            validatedLines,
            customerId,
            street,
            city,
            postalCode,
            phone,
            email,
            deliveryNotes,
            order.PremiumSubscription,
            pickupMethod!,
            pickupPointId,
            paymentMethod!);
    }
}

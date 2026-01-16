using static Shipment.Domain.Models.Shipment;

namespace Shipment.Domain.Operations;

/// <summary>
/// Validates a shipment - transforms UnprocessedShipment to ValidatedShipment
/// </summary>
public class ValidateShipmentOperation : ShipmentOperation
{
    protected override Task<IShipment> OnUnprocessedAsync(UnprocessedShipment shipment, object state, CancellationToken cancellationToken)
    {
        var validationErrors = new List<string>();

        if (shipment.OrderId == Guid.Empty)
            validationErrors.Add("OrderId is required");

        if (shipment.UserId == Guid.Empty)
            validationErrors.Add("UserId is required");

        if (shipment.TotalPrice <= 0)
            validationErrors.Add("TotalPrice must be greater than zero");

        if (!shipment.Lines.Any())
            validationErrors.Add("At least one line item is required");

        if (validationErrors.Any())
        {
            return Task.FromResult<IShipment>(new InvalidShipment(shipment.OrderId, validationErrors));
        }

        return Task.FromResult<IShipment>(new ValidatedShipment(
            shipment.OrderId,
            shipment.UserId,
            shipment.TotalPrice,
            shipment.Lines,
            shipment.OrderPlacedAt));
    }
}


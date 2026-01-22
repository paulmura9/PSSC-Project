namespace Shipment.Domain.Exceptions;

/// <summary>
/// Exception thrown when shipment is in an invalid/unexpected state
/// </summary>
public class InvalidShipmentStateException : Exception
{
    public InvalidShipmentStateException(string stateName)
        : base($"Invalid shipment state: {stateName}")
    {
    }

    public InvalidShipmentStateException(string stateName, Exception innerException)
        : base($"Invalid shipment state: {stateName}", innerException)
    {
    }
}

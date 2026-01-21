namespace SharedKernel;

/// <summary>
/// Subscription names for Service Bus topics
/// Each subscription listens to its respective topic
/// </summary>
public static class SubscriptionNames
{
    /// <summary>Subscription on 'orders' topic - consumed by Shipment</summary>
    public const string OrderProcessor = "order-processor";
    
    /// <summary>Subscription on 'shipments' topic - consumed by Invoicing</summary>
    public const string ShipmentProcessor = "shipment-processor";
    
    /// <summary>Subscription on 'invoices' topic</summary>
    public const string InvoiceProcessor = "invoice-processor";
}


using Azure.Messaging.ServiceBus;

namespace Shipment.Domain;

/// <summary>
/// Factory for creating Service Bus clients for different topics
/// </summary>
public class ServiceBusClientFactory : IAsyncDisposable
{
    private readonly ServiceBusClient _ordersClient;
    private readonly ServiceBusClient _shipmentsClient;

    public ServiceBusClientFactory(string ordersConnectionString, string shipmentsConnectionString)
    {
        _ordersClient = new ServiceBusClient(ordersConnectionString);
        _shipmentsClient = new ServiceBusClient(shipmentsConnectionString);
    }

    /// <summary>
    /// Client for consuming from orders topic
    /// </summary>
    public ServiceBusClient OrdersClient => _ordersClient;

    /// <summary>
    /// Client for publishing to shipments topic
    /// </summary>
    public ServiceBusClient ShipmentsClient => _shipmentsClient;

    public async ValueTask DisposeAsync()
    {
        await _ordersClient.DisposeAsync();
        await _shipmentsClient.DisposeAsync();
    }
}


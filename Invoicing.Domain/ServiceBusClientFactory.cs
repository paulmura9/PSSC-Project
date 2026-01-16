using Azure.Messaging.ServiceBus;

namespace Invoicing;

/// <summary>
/// Factory for creating Service Bus clients for different topics
/// </summary>
public class ServiceBusClientFactory : IAsyncDisposable
{
    private readonly ServiceBusClient _shipmentsClient;
    private readonly ServiceBusClient _invoicesClient;

    public ServiceBusClientFactory(string shipmentsConnectionString, string invoicesConnectionString)
    {
        _shipmentsClient = new ServiceBusClient(shipmentsConnectionString);
        _invoicesClient = new ServiceBusClient(invoicesConnectionString);
    }

    /// <summary>
    /// Client for consuming from shipments topic
    /// </summary>
    public ServiceBusClient ShipmentsClient => _shipmentsClient;

    /// <summary>
    /// Client for publishing to invoices topic
    /// </summary>
    public ServiceBusClient InvoicesClient => _invoicesClient;

    public async ValueTask DisposeAsync()
    {
        await _shipmentsClient.DisposeAsync();
        await _invoicesClient.DisposeAsync();
    }
}


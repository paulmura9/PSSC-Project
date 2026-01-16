using Microsoft.AspNetCore.Mvc;
using Ordering.Api.DTOs;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Workflows;
using SharedKernel;
using static Ordering.Domain.Models.Order;
using static Ordering.Domain.Events.OrderPlacedEvent;

namespace Ordering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderWorkflow _placeOrderWorkflow;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        PlaceOrderWorkflow placeOrderWorkflow,
        IEventBus eventBus,
        ILogger<OrdersController> logger)
    {
        _placeOrderWorkflow = placeOrderWorkflow;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <summary>
    /// Places a new order
    /// </summary>
    [HttpPost("place")]
    [ProducesResponseType(typeof(PlaceOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaceOrderErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received place order request for user {UserId}", request.UserId);

        // Map input DTO to domain model
        var unvalidatedLines = request.Products
            .Select(p => new UnvalidatedOrderLine(p.Name, p.Quantity, p.UnitPrice))
            .ToList()
            .AsReadOnly();

        var unvalidatedOrder = new Order.UnvalidatedOrder(
            unvalidatedLines,
            request.UserId,
            request.DeliveryAddress,
            request.PostalCode,
            request.Phone,
            request.CardNumber,
            request.Cvv,
            request.Expiry);

        var command = new PlaceOrderCommand(unvalidatedOrder);

        // Execute workflow
        IOrderPlacedEvent workflowResult = await _placeOrderWorkflow.ExecuteAsync(command, cancellationToken);

        // Handle result and publish event if succeeded
        IActionResult response = workflowResult switch
        {
            OrderPlaceSucceededEvent @event => await PublishEventAsync(@event, cancellationToken),
            OrderPlaceFailedEvent @event => BadRequest(new PlaceOrderErrorResponse
            {
                Errors = @event.Reasons.ToList()
            }),
            _ => StatusCode(500, new PlaceOrderErrorResponse
            {
                Errors = new List<string> { "An unexpected error occurred" }
            })
        };

        return response;
    }

    private async Task<IActionResult> PublishEventAsync(OrderPlaceSucceededEvent @event, CancellationToken cancellationToken)
    {
        await _eventBus.PublishAsync(TopicNames.Orders, @event, cancellationToken);
        
        _logger.LogInformation("Order {OrderId} placed successfully and event published", @event.OrderId);

        return Ok(new PlaceOrderResponse
        {
            OrderId = @event.OrderId,
            TotalPrice = @event.TotalPrice,
            OccurredAt = @event.OccurredAt,
            Lines = @event.Lines.Select(l => new OrderLineResponse
            {
                Name = l.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                LineTotal = l.LineTotal
            }).ToList()
        });
    }
}


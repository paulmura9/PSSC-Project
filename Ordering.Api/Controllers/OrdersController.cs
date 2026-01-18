using Microsoft.AspNetCore.Mvc;
using Ordering.Api.DTOs;
using Ordering.Domain.Events;
using Ordering.Domain.Models;
using Ordering.Domain.Operations;
using Ordering.Domain.Workflows;
using static Ordering.Domain.Models.Order;

namespace Ordering.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly PlaceOrderWorkflow _placeOrderWorkflow;
    private readonly CancelOrderWorkflow _cancelOrderWorkflow;
    private readonly ReturnOrderWorkflow _returnOrderWorkflow;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        PlaceOrderWorkflow placeOrderWorkflow,
        CancelOrderWorkflow cancelOrderWorkflow,
        ReturnOrderWorkflow returnOrderWorkflow,
        IProductRepository productRepository,
        ILogger<OrdersController> logger)
    {
        _placeOrderWorkflow = placeOrderWorkflow;
        _cancelOrderWorkflow = cancelOrderWorkflow;
        _returnOrderWorkflow = returnOrderWorkflow;
        _productRepository = productRepository;
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

        // Step 1: Validate products exist and have enough stock
        var validatedLines = new List<(ProductLineInput Input, ProductInfo Product)>();
        
        foreach (var line in request.Products)
        {
            var product = await _productRepository.GetProductByNameAsync(line.Name, cancellationToken);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductName}", line.Name);
                return BadRequest(new PlaceOrderErrorResponse
                {
                    Errors = new List<string> { $"Product '{line.Name}' not found in catalog" }
                });
            }
            
            if (product.StockQuantity < line.Quantity)
            {
                _logger.LogWarning("Insufficient stock for {ProductName}. Requested: {Requested}, Available: {Available}", 
                    line.Name, line.Quantity, product.StockQuantity);
                return BadRequest(new PlaceOrderErrorResponse
                {
                    Errors = new List<string> { $"Insufficient stock for '{line.Name}'. Available: {product.StockQuantity}, Requested: {line.Quantity}" }
                });
            }
            
            validatedLines.Add((line, product));
        }

        // Step 2: Reserve stock for all products
        foreach (var (line, product) in validatedLines)
        {
            var reserved = await _productRepository.ReserveStockAsync(product.Name, line.Quantity, cancellationToken);
            if (!reserved)
            {
                _logger.LogError("Failed to reserve stock for {ProductName}", product.Name);
                return BadRequest(new PlaceOrderErrorResponse
                {
                    Errors = new List<string> { $"Failed to reserve stock for '{product.Name}'" }
                });
            }
            _logger.LogInformation("Reserved {Quantity} units of {ProductName}", line.Quantity, product.Name);
        }

        // Step 3: Create order with product info from catalog (use catalog price)
        var unvalidatedLines = validatedLines
            .Select(v => UnvalidatedOrderLine.Create(
                v.Product.Name, 
                v.Product.Description, 
                v.Product.Category, 
                v.Input.Quantity, 
                v.Product.Price))  // Use price from catalog
            .ToList()
            .AsReadOnly();

        var unvalidatedOrder = new Order.UnvalidatedOrder(
            unvalidatedLines,
            request.UserId,
            request.Street,
            request.City,
            request.PostalCode,
            request.Phone,
            request.Email,
            request.DeliveryNotes,
            request.PremiumSubscription,
            request.PickupMethod,
            request.PickupPointId,
            request.PaymentMethod);

        var command = new PlaceOrderCommand(unvalidatedOrder, request.VoucherCode);

        // Execute workflow (validates, prices, persists, and publishes)
        IOrderPlacedEvent workflowResult = await _placeOrderWorkflow.ExecuteAsync(command, cancellationToken);

        // Handle result
        return workflowResult switch
        {
            OrderPlacedEvent @event => HandleSuccess(@event),
            OrderPlaceFailedEvent @event => BadRequest(new PlaceOrderErrorResponse
            {
                Errors = @event.Reasons.ToList()
            }),
            _ => StatusCode(500, new PlaceOrderErrorResponse
            {
                Errors = new List<string> { "An unexpected error occurred" }
            })
        };
    }

    private IActionResult HandleSuccess(OrderPlacedEvent @event)
    {

        _logger.LogInformation("Order {OrderId} placed successfully and event published", @event.OrderId);

        return Ok(new PlaceOrderResponse
        {
            OrderId = @event.OrderId,
            PremiumSubscription = @event.PremiumSubscription,
            Subtotal = @event.Subtotal,
            DiscountAmount = @event.DiscountAmount,
            Total = @event.Total,
            VoucherCode = @event.VoucherCode,
            OccurredAt = @event.OccurredAt,
            Lines = @event.Lines.Select(l => new OrderLineResponse
            {
                Name = l.Name.Value,
                Description = l.Description.Value,
                Category = l.Category.Value,
                Quantity = l.Quantity.Value,
                UnitPrice = l.UnitPrice.Value,
                LineTotal = l.LineTotal.Value
            }).ToList()
        });
    }

    /// <summary>
    /// Cancels an existing order
    /// </summary>
    /// <param name="id">Order ID to cancel</param>
    /// <param name="request">Cancellation reason</param>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(CancelOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaceOrderErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received cancel order request for OrderId: {OrderId}, Reason: {Reason}", id, request.Reason);

        var command = new CancelOrderCommand(id, request.Reason);
        var result = await _cancelOrderWorkflow.ExecuteAsync(command, cancellationToken);

        return result switch
        {
            OrderCancelledSucceededEvent success => Ok(new CancelOrderResponse
            {
                OrderId = success.OrderId,
                Status = "Cancelled",
                Reason = success.Reason,
                CancelledAt = success.CancelledAt
            }),
            OrderCancellationFailedEvent failure => BadRequest(new PlaceOrderErrorResponse
            {
                Errors = new List<string> { failure.FailureReason }
            }),
            _ => StatusCode(500, new PlaceOrderErrorResponse
            {
                Errors = new List<string> { "An unexpected error occurred" }
            })
        };
    }

    /// <summary>
    /// Returns an existing order (after delivery)
    /// </summary>
    /// <param name="id">Order ID to return</param>
    /// <param name="request">Return reason</param>
    [HttpPost("{id:guid}/return")]
    [ProducesResponseType(typeof(ReturnOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PlaceOrderErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnOrder(Guid id, [FromBody] ReturnOrderRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received return order request for OrderId: {OrderId}, Reason: {Reason}", id, request.Reason);

        var command = new ReturnOrderCommand(id, request.Reason);
        var result = await _returnOrderWorkflow.ExecuteAsync(command, cancellationToken);

        return result switch
        {
            OrderReturnSucceededEvent success => Ok(new ReturnOrderResponse
            {
                OrderId = success.OrderId,
                Status = "Returned",
                Reason = success.ReturnReason,
                ReturnedAt = success.ReturnedAt
            }),
            OrderReturnFailedEvent failure => BadRequest(new PlaceOrderErrorResponse
            {
                Errors = new List<string> { failure.FailureReason }
            }),
            _ => StatusCode(500, new PlaceOrderErrorResponse
            {
                Errors = new List<string> { "An unexpected error occurred" }
            })
        };
    }
}


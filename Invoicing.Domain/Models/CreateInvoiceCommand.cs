namespace Invoicing.Models;

/// <summary>
/// Command to create an invoice from shipment event
/// Contains discount amount for proportional distribution across lines
/// Currency defaults to RON. EUR is derived for presentation only.
/// </summary>
public record CreateInvoiceCommand
{
    public Guid ShipmentId { get; }
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public string TrackingNumber { get; }
    public bool PremiumSubscription { get; }
    public decimal Subtotal { get; }
    public decimal DiscountAmount { get; }
    public decimal TotalAfterDiscount { get; }
    public decimal ShippingCost { get; }
    public decimal TotalWithShipping { get; }
    public IReadOnlyCollection<InvoiceLineInput> Lines { get; }
    public DateTime ShipmentCreatedAt { get; }
    public Currency? DisplayCurrency { get; }
    
    /// <summary>
    /// Payment status: "Authorized" for CardOnline, "Pending" otherwise
    /// </summary>
    public string PaymentStatus { get; }

    public CreateInvoiceCommand(
        Guid shipmentId,
        Guid orderId,
        Guid userId,
        string trackingNumber,
        bool premiumSubscription,
        decimal subtotal,
        decimal discountAmount,
        decimal totalAfterDiscount,
        decimal shippingCost,
        decimal totalWithShipping,
        IReadOnlyCollection<InvoiceLineInput> lines,
        DateTime shipmentCreatedAt,
        string paymentStatus = "Pending",
        Currency? displayCurrency = null)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        UserId = userId;
        TrackingNumber = trackingNumber;
        PremiumSubscription = premiumSubscription;
        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        TotalAfterDiscount = totalAfterDiscount;
        ShippingCost = shippingCost;
        TotalWithShipping = totalWithShipping;
        Lines = lines;
        ShipmentCreatedAt = shipmentCreatedAt;
        PaymentStatus = paymentStatus;
        DisplayCurrency = displayCurrency;
    }
}

/// <summary>
/// Input DTO for invoice line (before VAT calculation)
/// </summary>
public record InvoiceLineInput(
    string Name,
    string Description,
    string Category,
    int Quantity,
    decimal UnitPrice);

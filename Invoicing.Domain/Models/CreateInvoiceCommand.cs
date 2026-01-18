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


//la models inca 2   (6-7 modele/tipuro)
// la workflow sa treca din mai multe stari in alta (specifice shipping)
// tabela products validez daca e acolo, mai e produsul
// o clasa / fiser
//tabelele din baza de date sa fie accesibil pe contex (gen limitari pe connection string)  
//repositroy in infrastructure




//doar un workflow pe contex?
//cate operations ex
//la events?
//structura:  domain + infrastructure la fiecare? (la consola)


//in baza de date ce se salveaza?

//comanda e unvalidated(tip) si se face logica de validare
//clase care iti transforma aplicatie dintr-un tip in alt tip in kernel eventual
//3 operatii per context

//deci 
//validate order opration




//return order
//validae address

//order

//eventual un folder messeges

//EVENIMETE
////orderplaces
//shipnesend
//invoicinggenerated


namespace Invoicing.Models;

/// <summary>
/// Command to create an invoice from shipment event
/// </summary>
public record CreateInvoiceCommand(
    Guid ShipmentId,
    Guid OrderId,
    Guid UserId,
    string TrackingNumber,
    decimal TotalPrice,
    IReadOnlyCollection<InvoiceLine> Lines,
    DateTime ShipmentCreatedAt);


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


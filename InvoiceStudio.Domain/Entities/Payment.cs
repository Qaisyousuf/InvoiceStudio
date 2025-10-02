using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public DateTime PaymentDate { get; private set; }

    public PaymentMethod Method { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }

    private Payment() { } // EF Core

    public Payment(Guid invoiceId, decimal amount, string currency, DateTime paymentDate, PaymentMethod method)
    {
        InvoiceId = invoiceId;
        Amount = amount;
        Currency = currency;
        PaymentDate = paymentDate;
        Method = method;
    }

    public void UpdateDetails(decimal amount, DateTime paymentDate, PaymentMethod method, string? reference, string? notes)
    {
        Amount = amount;
        PaymentDate = paymentDate;
        Method = method;
        Reference = reference;
        Notes = notes;
        MarkAsUpdated();
    }
}

public enum PaymentMethod
{
    BankTransfer = 0,
    CreditCard = 1,
    DebitCard = 2,
    Cash = 3,
    PayPal = 4,
    Stripe = 5,
    Check = 6,
    Other = 99
}
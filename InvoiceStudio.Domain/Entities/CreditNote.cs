using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class CreditNote : BaseEntity
{
    public string CreditNoteNumber { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }

    // Original Invoice
    public Guid InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    // Client
    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    // Amounts
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EUR";

    // Reason
    public string Reason { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    // Status
    public CreditNoteStatus Status { get; private set; } = CreditNoteStatus.Draft;

    private CreditNote() { } // EF Core

    public CreditNote(string creditNoteNumber, Guid invoiceId, Guid clientId, decimal amount, string currency, string reason)
    {
        CreditNoteNumber = creditNoteNumber;
        InvoiceId = invoiceId;
        ClientId = clientId;
        Amount = amount;
        Currency = currency;
        Reason = reason;
        IssueDate = DateTime.UtcNow;
    }

    public void Issue()
    {
        if (Status != CreditNoteStatus.Draft)
            throw new InvalidOperationException("Only draft credit notes can be issued");

        Status = CreditNoteStatus.Issued;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        Status = CreditNoteStatus.Cancelled;
        MarkAsUpdated();
    }
}

public enum CreditNoteStatus
{
    Draft = 0,
    Issued = 1,
    Cancelled = 2
}
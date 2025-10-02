using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }
    public DateTime DueDate { get; private set; }

    // Client
    public Guid ClientId { get; private set; }
    public Client Client { get; private set; } = null!;

    // Status
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    // Currency
    public string Currency { get; private set; } = "EUR";

    // Totals
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // Payment
    public decimal PaidAmount { get; private set; }
    public DateTime? PaidDate { get; private set; }

    // Lines
    public List<InvoiceLine> Lines { get; private set; } = new();

    // Notes
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }

    private Invoice() { } // EF Core

    public Invoice(string invoiceNumber, Guid clientId, DateTime issueDate, DateTime dueDate)
    {
        InvoiceNumber = invoiceNumber;
        ClientId = clientId;
        IssueDate = issueDate;
        DueDate = dueDate;
    }

    public void AddLine(InvoiceLine line)
    {
        Lines.Add(line);
        RecalculateTotals();
        MarkAsUpdated();
    }

    public void RemoveLine(InvoiceLine line)
    {
        Lines.Remove(line);
        RecalculateTotals();
        MarkAsUpdated();
    }

    public void RecalculateTotals()
    {
        SubTotal = Lines.Sum(l => l.SubTotal);
        TaxAmount = Lines.Sum(l => l.TaxAmount);
        DiscountAmount = Lines.Sum(l => l.DiscountAmount);
        TotalAmount = SubTotal + TaxAmount - DiscountAmount;
    }

    public void Approve()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be approved");

        Status = InvoiceStatus.Approved;
        MarkAsUpdated();
    }

    public void Issue()
    {
        if (Status != InvoiceStatus.Approved)
            throw new InvalidOperationException("Only approved invoices can be issued");

        Status = InvoiceStatus.Issued;
        MarkAsUpdated();
    }

    public void MarkAsSent()
    {
        if (Status != InvoiceStatus.Issued)
            throw new InvalidOperationException("Only issued invoices can be sent");

        Status = InvoiceStatus.Sent;
        MarkAsUpdated();
    }

    public void MarkAsPaid(decimal amount, DateTime paidDate)
    {
        PaidAmount += amount;

        if (PaidAmount >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
            PaidDate = paidDate;
        }

        MarkAsUpdated();
    }

    public void Cancel()
    {
        Status = InvoiceStatus.Cancelled;
        MarkAsUpdated();
    }
}

public enum InvoiceStatus
{
    Draft = 0,
    Approved = 1,
    Issued = 2,
    Sent = 3,
    Paid = 4,
    Overdue = 5,
    Cancelled = 6
}
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

    // Company
    public Guid CompanyId { get; private set; }
    public Company Company { get; private set; } = null!;

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

    // Legal & Terms
    public string? LegalMentions { get; private set; }
    public string PaymentTerms { get; private set; } = "Paiement à réception";

    // Lines
    public List<InvoiceLine> Lines { get; private set; } = new();

    // Notes
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }

    // Private constructor for EF Core
    private Invoice() { }

    // Public constructor
    public Invoice(string invoiceNumber, Guid clientId, Guid companyId, DateTime issueDate, DateTime dueDate, string currency = "EUR")
    {
        InvoiceNumber = invoiceNumber;
        ClientId = clientId;
        CompanyId = companyId;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency;
    }

    // Line Management Methods
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

    // Legal Mentions Methods
    public void SetFrenchLegalMentions(string siret, string apeCode, bool isTvaExempt)
    {
        var mention = $@"Mentions légales:
- SIRET: {siret}
- APE: {apeCode}";

        if (isTvaExempt)
        {
            mention += "\n- TVA non applicable, art. 293 B du CGI";
            mention += "\n- Dispensé d'immatriculation au RCS et au RM";
        }

        mention += $@"

Conditions de règlement:
- Paiement: {PaymentTerms}";

        LegalMentions = mention;
        MarkAsUpdated();
    }

    public void SetDanishLegalMentions(string cvrNumber, bool isVatExempt)
    {
        var mention = $@"Juridiske oplysninger:
- CVR: {cvrNumber}";

        if (!isVatExempt)
        {
            mention += $"\n- Moms: DK{cvrNumber}";
        }
        else
        {
            mention += "\n- Momsfri";
        }

        mention += $@"

Betalingsbetingelser:
- Betaling: {PaymentTerms}";

        LegalMentions = mention;
        MarkAsUpdated();
    }

    // Payment Terms Method
    public void SetPaymentTerms(string paymentTerms)
    {
        PaymentTerms = paymentTerms;
        MarkAsUpdated();
    }

    // Status Management Methods
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
        PaidDate = paidDate;

        if (PaidAmount >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
        }

        MarkAsUpdated();
    }

    public void MarkAsOverdue()
    {
        if (Status == InvoiceStatus.Sent && DateTime.UtcNow > DueDate)
        {
            Status = InvoiceStatus.Overdue;
            MarkAsUpdated();
        }
    }

    public void Cancel()
    {
        Status = InvoiceStatus.Cancelled;
        MarkAsUpdated();
    }

    // Update Methods
    public void UpdateNotes(string? notes, string? terms)
    {
        Notes = notes;
        Terms = terms;
        MarkAsUpdated();
    }

    public void UpdateDates(DateTime issueDate, DateTime dueDate)
    {
        IssueDate = issueDate;
        DueDate = dueDate;
        MarkAsUpdated();
    }

    // Validation Methods
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(InvoiceNumber)
            && ClientId != Guid.Empty
            && CompanyId != Guid.Empty
            && IssueDate <= DueDate
            && Lines.Any()
            && TotalAmount >= 0;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(InvoiceNumber))
            errors.Add("Invoice number is required");

        if (ClientId == Guid.Empty)
            errors.Add("Client is required");

        if (CompanyId == Guid.Empty)
            errors.Add("Company is required");

        if (IssueDate > DueDate)
            errors.Add("Due date must be after issue date");

        if (!Lines.Any())
            errors.Add("At least one line item is required");

        if (TotalAmount < 0)
            errors.Add("Total amount cannot be negative");

        return errors;
    }

    // Business Logic Methods
    public bool CanBeEdited()
    {
        return Status == InvoiceStatus.Draft;
    }

    public bool CanBeDeleted()
    {
        return Status == InvoiceStatus.Draft || Status == InvoiceStatus.Cancelled;
    }

    public bool IsOverdue()
    {
        return Status == InvoiceStatus.Sent && DateTime.UtcNow > DueDate;
    }

    public decimal GetOutstandingBalance()
    {
        return TotalAmount - PaidAmount;
    }

    public bool IsFullyPaid()
    {
        return PaidAmount >= TotalAmount;
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
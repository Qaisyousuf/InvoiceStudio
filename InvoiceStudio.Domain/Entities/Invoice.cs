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

    // French Legal Requirements
    public string? LegalMentions { get; private set; }
    public string PaymentTerms { get; private set; } = "Paiement à réception";
    public decimal LatePenaltyRate { get; private set; } = 10.0m;  // 10% (BCE rate + 10 points)
    public decimal FixedRecoveryFee { get; private set; } = 40m;    // €40 legal minimum

    // Early payment discount (Escompte)
    public decimal? EarlyPaymentDiscountRate { get; private set; }
    public int? EarlyPaymentDiscountDays { get; private set; }

    // For EU B2B - Autoliquidation TVA
    public bool IsReverseTax { get; private set; } = false;
    public string? ReverseTaxMention { get; private set; }

    // Lines
    public List<InvoiceLine> Lines { get; private set; } = new();

    // Notes
    public string? Notes { get; private set; }
    public string? Terms { get; private set; }

    private Invoice() { } // EF Core

    public Invoice(string invoiceNumber, Guid clientId, DateTime issueDate, DateTime dueDate, string currency = "EUR")
    {
        InvoiceNumber = invoiceNumber;
        ClientId = clientId;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency;
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
- Paiement: {PaymentTerms}
- Escompte: Néant
- Pénalités de retard: {LatePenaltyRate}% (taux BCE + 10 points)
- Indemnité forfaitaire pour frais de recouvrement: {FixedRecoveryFee}€";

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
- Betaling: {PaymentTerms}
- Morarente: {LatePenaltyRate}%";

        LegalMentions = mention;
        MarkAsUpdated();
    }

    public void SetPaymentTerms(string paymentTerms, decimal latePenaltyRate = 10.0m, decimal fixedRecoveryFee = 40m)
    {
        PaymentTerms = paymentTerms;
        LatePenaltyRate = latePenaltyRate;
        FixedRecoveryFee = fixedRecoveryFee;
        MarkAsUpdated();
    }

    public void SetEarlyPaymentDiscount(decimal discountRate, int discountDays)
    {
        EarlyPaymentDiscountRate = discountRate;
        EarlyPaymentDiscountDays = discountDays;
        MarkAsUpdated();
    }

    public void SetReverseTax(bool isReverseTax, string? mention = null)
    {
        IsReverseTax = isReverseTax;
        ReverseTaxMention = mention ?? "Autoliquidation de la TVA";
        MarkAsUpdated();
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
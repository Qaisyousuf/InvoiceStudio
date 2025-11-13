using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class InvoiceLine : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;

    // Product reference (optional - can be free-text line)
    public Guid? ProductId { get; private set; }
    public Product? Product { get; private set; }

    // Line details
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = "pcs";
    public decimal UnitPrice { get; private set; }

    // Tax
    public decimal TaxRate { get; private set; }

    // Discount
    public decimal DiscountPercent { get; private set; }

    // Calculated amounts
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal Total { get; private set; }

    // Sort order
    public int LineOrder { get; private set; }

    private InvoiceLine() { } // EF Core

    public InvoiceLine(
        Guid invoiceId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate,
        Guid? productId = null)
    {
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        ProductId = productId;

        Calculate();
    }

    public void UpdateQuantity(decimal quantity)
    {
        Quantity = quantity;
        Calculate();
        MarkAsUpdated();
    }

    public void UpdatePrice(decimal unitPrice)
    {
        UnitPrice = unitPrice;
        Calculate();
        MarkAsUpdated();
    }

    public void UpdateDiscount(decimal discountPercent)
    {
        DiscountPercent = discountPercent;
        Calculate();
        MarkAsUpdated();
    }

    public void UpdateTaxRate(decimal taxRate)
    {
        TaxRate = taxRate;
        Calculate();
        MarkAsUpdated();
    }

    public void SetLineOrder(int order)
    {
        LineOrder = order;
        MarkAsUpdated();
    }

    private void Calculate()
    {
        SubTotal = Quantity * UnitPrice;
        DiscountAmount = SubTotal * (DiscountPercent / 100);
        var subtotalAfterDiscount = SubTotal - DiscountAmount;
        TaxAmount = subtotalAfterDiscount * (TaxRate / 100);
        Total = subtotalAfterDiscount + TaxAmount;
    }

    public void UpdateDetails(string description, decimal quantity, decimal unitPrice, decimal taxRate, Guid? productId = null)
    {
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        ProductId = productId;

        Calculate(); // Recalculate all amounts
        MarkAsUpdated(); // Mark entity as updated
    }
}
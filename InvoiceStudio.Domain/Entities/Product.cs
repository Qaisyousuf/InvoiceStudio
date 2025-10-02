using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Sku { get; private set; }

    // Pricing
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = "EUR";

    // Tax
    public decimal TaxRate { get; private set; } = 21.0m;

    // Type
    public ProductType Type { get; private set; } = ProductType.Service;

    // Unit
    public string Unit { get; private set; } = "pcs"; // pieces, hours, days, kg, etc.

    // Status
    public bool IsActive { get; private set; } = true;

    private Product() { } // EF Core

    public Product(string name, decimal unitPrice, string currency = "EUR")
    {
        Name = name;
        UnitPrice = unitPrice;
        Currency = currency;
    }

    public void UpdateDetails(string name, string? description, string? sku)
    {
        Name = name;
        Description = description;
        Sku = sku;
        MarkAsUpdated();
    }

    public void UpdatePricing(decimal unitPrice, string currency, decimal taxRate)
    {
        UnitPrice = unitPrice;
        Currency = currency;
        TaxRate = taxRate;
        MarkAsUpdated();
    }

    public void SetType(ProductType type)
    {
        Type = type;
        MarkAsUpdated();
    }

    public void SetUnit(string unit)
    {
        Unit = unit;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}

public enum ProductType
{
    Service = 0,
    Product = 1,
    Subscription = 2
}
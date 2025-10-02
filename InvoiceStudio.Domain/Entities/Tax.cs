using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Tax : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Rate { get; private set; }
    public TaxType Type { get; private set; }

    public string Country { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Tax() { } // EF Core

    public Tax(string name, decimal rate, TaxType type, string country)
    {
        Name = name;
        Rate = rate;
        Type = type;
        Country = country;
    }

    public void UpdateDetails(string name, string? description, decimal rate)
    {
        Name = name;
        Description = description;
        Rate = rate;
        MarkAsUpdated();
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        MarkAsUpdated();
    }

    public void RemoveAsDefault()
    {
        IsDefault = false;
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

public enum TaxType
{
    VAT = 0,
    GST = 1,
    SalesTax = 2,
    None = 99
}
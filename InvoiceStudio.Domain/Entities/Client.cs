using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? TaxId { get; private set; }
    public string? VatNumber { get; private set; }

    // Address
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }

    // Contact
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }

    // Business Settings
    public string PreferredCurrency { get; private set; } = "EUR";
    public int PaymentTermDays { get; private set; } = 30;
    public decimal? DefaultDiscountPercent { get; private set; }

    // Status
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private Client() { } // EF Core

    public Client(string name, string? email = null)
    {
        Name = name;
        Email = email;
    }

    public void UpdateDetails(string name, string? legalName, string? taxId, string? vatNumber)
    {
        Name = name;
        LegalName = legalName;
        TaxId = taxId;
        VatNumber = vatNumber;
        MarkAsUpdated();
    }

    public void UpdateAddress(string? street, string? city, string? postalCode, string? country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
        MarkAsUpdated();
    }

    public void UpdateContact(string? email, string? phone, string? website)
    {
        Email = email;
        Phone = phone;
        Website = website;
        MarkAsUpdated();
    }

    public void UpdateBusinessSettings(string currency, int paymentTermDays, decimal? discountPercent = null)
    {
        PreferredCurrency = currency;
        PaymentTermDays = paymentTermDays;
        DefaultDiscountPercent = discountPercent;
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
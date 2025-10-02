using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? TaxId { get; private set; }
    public string? VatNumber { get; private set; }
    public string? RegistrationNumber { get; private set; }

    // Address
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }

    // Contact
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }

    // Banking
    public string? BankName { get; private set; }
    public string? Iban { get; private set; }
    public string? Swift { get; private set; }

    // Branding
    public string? LogoPath { get; private set; }
    public string? PrimaryColor { get; private set; }

    // Default Settings
    public string DefaultCurrency { get; private set; } = "EUR";
    public decimal DefaultTaxRate { get; private set; } = 21.0m;
    public string InvoicePrefix { get; private set; } = "INV";
    public string QuotePrefix { get; private set; } = "QUO";

    private Company() { } // EF Core

    public Company(string name)
    {
        Name = name;
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

    public void UpdateBanking(string? bankName, string? iban, string? swift)
    {
        BankName = bankName;
        Iban = iban;
        Swift = swift;
        MarkAsUpdated();
    }
}
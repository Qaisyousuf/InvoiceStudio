using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }

    // Country
    public string? Country { get; private set; }
    public ClientType Type { get; private set; } = ClientType.Individual;

    // Universal Tax IDs
    public string? TaxId { get; private set; }
    public string? VatNumber { get; private set; }

    // French-specific (B2B)
    public string? Siret { get; private set; }
    public string? IntraCommunityVatFr { get; private set; }

    // Danish-specific (B2B)
    public string? CvrNumber { get; private set; }
    public string? DanishVatNumber { get; private set; }

    // Address
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CountryName { get; private set; }

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
    public string? LogoPath { get; private set; }  // ← ADD THIS LINE
    private Client() { } // EF Core

    public Client(string name, string? email = null, ClientType type = ClientType.Individual)
    {
        Name = name;
        Email = email;
        Type = type;
    }

    public void UpdateDetails(string name, string? legalName, string? taxId, string? vatNumber)
    {
        Name = name;
        LegalName = legalName;
        TaxId = taxId;
        VatNumber = vatNumber;
        MarkAsUpdated();
    }

    public void UpdateFrenchBusinessInfo(string siret, string? intraCommunityVat = null)
    {
        Siret = siret;
        IntraCommunityVatFr = intraCommunityVat;
        Type = ClientType.Company;
        Country = "FR";
        MarkAsUpdated();
    }

    public void UpdateDanishBusinessInfo(string cvrNumber, string? danishVatNumber = null)
    {
        CvrNumber = cvrNumber;
        DanishVatNumber = danishVatNumber ?? $"DK{cvrNumber}";
        Type = ClientType.Company;
        Country = "DK";
        MarkAsUpdated();
    }

    public void UpdateAddress(string? street, string? city, string? postalCode, string? country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        CountryName = country;
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

    public void SetType(ClientType type)
    {
        Type = type;
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
    public void UpdateLogo(string? logoPath)
    {
        LogoPath = logoPath;
        MarkAsUpdated();
    }
}

public enum ClientType
{
    Individual = 0,     // Particulier (B2C)
    Company = 1,        // Entreprise (B2B)
    PublicSector = 2    // Secteur public
}


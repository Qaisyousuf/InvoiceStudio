using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }

    // Country Settings
    public string Country { get; private set; } = "FR"; // FR, DK, etc.
    public string LegalForm { get; private set; } = "Auto-Entrepreneur";

    // Universal Business Registration
    public string? BusinessRegistrationNumber { get; private set; }
    public string? TaxId { get; private set; }
    public string? VatNumber { get; private set; }
    public bool IsVatExempt { get; private set; } = false;

    // French-specific (nullable)
    public string? Siret { get; private set; }
    public string? Siren { get; private set; }
    public string? ApeCode { get; private set; }
    public string? RcsNumber { get; private set; }
    public string? FrenchVatExemptionMention { get; private set; }

    // Danish-specific (nullable)
    public string? CvrNumber { get; private set; }
    public string? DanishVatNumber { get; private set; }
    public string? SENumber { get; private set; }

    // EU
    public string? IntraCommunityVat { get; private set; }

    // Address
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CountryName { get; private set; }

    // Contact
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }

    // Banking
    public string? BankName { get; private set; }
    public string? Iban { get; private set; }
    public string? Swift { get; private set; }

    // Insurance
    public string? InsuranceCompany { get; private set; }
    public string? InsurancePolicyNumber { get; private set; }

    // Branding
    public string? LogoPath { get; private set; }
    public string? PrimaryColor { get; private set; }

    // Default Settings
    public string DefaultCurrency { get; private set; } = "EUR";
    public decimal DefaultTaxRate { get; private set; } = 20.0m;
    public string InvoicePrefix { get; private set; } = "INV";
    public string QuotePrefix { get; private set; } = "QUO";

    private Company() { } // EF Core

    public Company(string name, string country = "FR")
    {
        Name = name;
        Country = country;

        // Set defaults based on country
        if (country == "DK")
        {
            DefaultCurrency = "DKK";
            DefaultTaxRate = 25.0m;
            LegalForm = "Enkeltmandsvirksomhed";
        }
        else if (country == "FR")
        {
            DefaultCurrency = "EUR";
            DefaultTaxRate = 20.0m;
            LegalForm = "Auto-Entrepreneur";
            FrenchVatExemptionMention = "TVA non applicable, art. 293 B du CGI";
        }
    }

    // French Registration
    public void UpdateFrenchRegistration(string siret, string? apeCode, bool isVatExempt)
    {
        if (Country != "FR")
            throw new InvalidOperationException("This company is not in France");

        Siret = siret;
        Siren = siret.Length >= 9 ? siret.Substring(0, 9) : string.Empty;
        ApeCode = apeCode;
        IsVatExempt = isVatExempt;
        BusinessRegistrationNumber = siret;

        if (isVatExempt)
        {
            FrenchVatExemptionMention = "TVA non applicable, art. 293 B du CGI";
        }

        MarkAsUpdated();
    }

    // Danish Registration
    public void UpdateDanishRegistration(string cvrNumber, string? seNumber, bool isVatExempt)
    {
        if (Country != "DK")
            throw new InvalidOperationException("This company is not in Denmark");

        CvrNumber = cvrNumber;
        SENumber = seNumber;
        IsVatExempt = isVatExempt;
        BusinessRegistrationNumber = cvrNumber;

        if (!isVatExempt && !string.IsNullOrEmpty(cvrNumber))
        {
            DanishVatNumber = $"DK{cvrNumber}";
        }

        MarkAsUpdated();
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

    public void UpdateBanking(string? bankName, string? iban, string? swift)
    {
        BankName = bankName;
        Iban = iban;
        Swift = swift;
        MarkAsUpdated();
    }

    public void UpdateInsurance(string? insuranceCompany, string? policyNumber)
    {
        InsuranceCompany = insuranceCompany;
        InsurancePolicyNumber = policyNumber;
        MarkAsUpdated();
    }

    public void SetCountry(string countryCode)
    {
        Country = countryCode;

        // Update defaults
        if (countryCode == "DK")
        {
            DefaultCurrency = "DKK";
            DefaultTaxRate = 25.0m;
        }
        else if (countryCode == "FR")
        {
            DefaultCurrency = "EUR";
            DefaultTaxRate = 20.0m;
        }

        MarkAsUpdated();
    }
}
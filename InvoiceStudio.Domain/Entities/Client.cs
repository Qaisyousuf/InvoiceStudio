using InvoiceStudio.Domain.Common;

namespace InvoiceStudio.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }

    // Country & Location
    public string? Country { get; private set; }
    public ClientType Type { get; private set; } = ClientType.Individual;

    // Universal Tax IDs
    public string? TaxId { get; private set; }

    // French-specific (B2B)
    public string? Siret { get; private set; }
    public string? Siren { get; private set; } // SIREN (siège) - 9 digits

    // Danish-specific (B2B)
    public string? CvrNumber { get; private set; }
    public string? DanishVatNumber { get; private set; }

    // Primary Address
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CountryName { get; private set; }

    // Primary Contact (backward compatibility)
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Website { get; private set; }

    // Business Settings
    public string PreferredCurrency { get; private set; } = "EUR";
    public int PaymentTermDays { get; private set; } = 30;
    public decimal? DefaultDiscountPercent { get; private set; }

    // Client Management
    public string? Category { get; private set; } // "VIP", "Regular", "Prospect", "Partner"
    public ClientPriority Priority { get; private set; } = ClientPriority.Normal;

    // Client Statistics & Metadata
    public DateTime? FirstInvoiceDate { get; private set; }
    public DateTime? LastInvoiceDate { get; private set; }
    public int TotalInvoices { get; private set; } = 0;
    public decimal TotalRevenue { get; private set; } = 0;
    public decimal OverdueAmount { get; private set; } = 0;
    public double AveragePaymentDays { get; private set; } = 0;

    // Status & Branding
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }
    public string? LogoPath { get; private set; }
    public string? InternalReference { get; private set; } // Internal client code

    // Navigation Properties
    public ICollection<Contact> Contacts { get; private set; } = new List<Contact>();
    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();
    public ICollection<ClientAddress> Addresses { get; private set; } = new List<ClientAddress>();

    private Client() { } // EF Core

    public Client(string name, string? email = null, ClientType type = ClientType.Individual)
    {
        Name = name;
        Email = email;
        Type = type;
    }

    #region Update Methods

    public void UpdateDetails(string name, string? legalName, string? taxId, string? siren)
    {
        Name = name;
        LegalName = legalName;
        TaxId = taxId;
        Siren = siren;
        MarkAsUpdated();
    }

    public void UpdateFrenchBusinessInfo(string siret, string? siren = null)
    {
        Siret = siret;
        Siren = siren;
        Type = ClientType.Company;
        Country = "FR";
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

    public void UpdateDanishBusinessInfo(string cvrNumber, string? danishVatNumber = null)
    {
        CvrNumber = cvrNumber;
        DanishVatNumber = danishVatNumber ?? $"DK{cvrNumber}";
        Type = ClientType.Company;
        Country = "DK";
        MarkAsUpdated();
    }

    public void UpdatePrimaryAddress(string? street, string? city, string? postalCode, string? country)
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

    public void UpdateClientManagement(string? category, ClientPriority priority)
    {
        Category = category;
        Priority = priority;
        MarkAsUpdated();
    }

    public void UpdateStatistics(DateTime? firstInvoiceDate, DateTime? lastInvoiceDate,
        int totalInvoices, decimal totalRevenue, decimal overdueAmount, double averagePaymentDays)
    {
        FirstInvoiceDate = firstInvoiceDate;
        LastInvoiceDate = lastInvoiceDate;
        TotalInvoices = totalInvoices;
        TotalRevenue = totalRevenue;
        OverdueAmount = overdueAmount;
        AveragePaymentDays = averagePaymentDays;
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

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        MarkAsUpdated();
    }

    public void UpdateInternalReference(string? internalReference)
    {
        InternalReference = internalReference;
        MarkAsUpdated();
    }

    #endregion

    #region Contact Management

    public void AddContact(Contact contact)
    {
        if (contact == null) throw new ArgumentNullException(nameof(contact));

        // Ensure only one primary contact
        if (contact.IsPrimary)
        {
            foreach (var existingContact in Contacts.Where(c => c.IsPrimary))
            {
                existingContact.SetAsPrimary(false);
            }
        }

        Contacts.Add(contact);
        MarkAsUpdated();
    }

    public void RemoveContact(Guid contactId)
    {
        var contact = Contacts.FirstOrDefault(c => c.Id == contactId);
        if (contact != null)
        {
            Contacts.Remove(contact);
            MarkAsUpdated();
        }
    }

    public Contact? GetPrimaryContact()
    {
        return Contacts.FirstOrDefault(c => c.IsPrimary);
    }

    #endregion

    #region Business Logic

    public bool IsLatePaymentRisk()
    {
        return AveragePaymentDays > PaymentTermDays + 15 ||
               OverdueAmount > 0;
    }

    public string GetDisplayAddress()
    {
        return $"{Street}, {City} {PostalCode}, {CountryName}".Trim(' ', ',');
    }

    #endregion
}

#region Supporting Entities

public class Contact : BaseEntity
{
    public Guid ClientId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Department { get; private set; }
    public ContactType ContactType { get; private set; } = ContactType.General;
    public bool IsPrimary { get; private set; } = false;
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    // Navigation
    public Client Client { get; private set; } = null!;

    private Contact() { } // EF Core

    public Contact(Guid clientId, string firstName, string lastName, string? email = null, ContactType type = ContactType.General)
    {
        ClientId = clientId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        ContactType = type;
    }

    public void UpdateDetails(string firstName, string lastName, string? email, string? phone, string? jobTitle, string? department)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        JobTitle = jobTitle;
        Department = department;
        MarkAsUpdated();
    }

    public void SetAsPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        MarkAsUpdated();
    }

    public void SetContactType(ContactType type)
    {
        ContactType = type;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        MarkAsUpdated();
    }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}".Trim();
    }
}

public class ClientAddress : BaseEntity
{
    public Guid ClientId { get; private set; }
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? PostalCode { get; private set; }
    public string? CountryName { get; private set; }
    public AddressType AddressType { get; private set; }
    public bool IsDefault { get; private set; } = false;
    public string? Label { get; private set; } // "Head Office", "Warehouse", etc.

    // Navigation
    public Client Client { get; private set; } = null!;

    private ClientAddress() { } // EF Core

    public ClientAddress(Guid clientId, AddressType type, string? street = null, string? city = null,
        string? postalCode = null, string? country = null, string? label = null)
    {
        ClientId = clientId;
        AddressType = type;
        Street = street;
        City = city;
        PostalCode = postalCode;
        CountryName = country;
        Label = label;
    }

    public void UpdateAddress(string? street, string? city, string? postalCode, string? country, string? label = null)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        CountryName = country;
        Label = label;
        MarkAsUpdated();
    }

    public void SetAsDefault(bool isDefault)
    {
        IsDefault = isDefault;
        MarkAsUpdated();
    }

    public string GetFormattedAddress()
    {
        return $"{Street}, {City} {PostalCode}, {CountryName}".Trim(' ', ',');
    }
}

#endregion

#region Enums

public enum ClientType
{
    Individual = 0,     // Particulier (B2C)
    Company = 1,        // Entreprise (B2B)
    PublicSector = 2    // Secteur public
}

public enum ClientPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    VIP = 3
}

public enum ContactType
{
    General = 0,
    Billing = 1,        // Accounts Payable
    Technical = 2,      // Project Manager
    DecisionMaker = 3,  // CEO, Director
    Support = 4,        // Customer Support
    Legal = 5          // Legal Department
}

public enum AddressType
{
    Primary = 0,
    Billing = 1,
    Shipping = 2,
    Office = 3,
    Warehouse = 4
}

#endregion
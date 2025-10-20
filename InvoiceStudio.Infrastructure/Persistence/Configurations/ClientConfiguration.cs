using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceStudio.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.Id);

        // Basic Information
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LegalName)
            .HasMaxLength(200);

        // Country & Type
        builder.Property(c => c.Country)
            .HasMaxLength(100); // Changed from 2 to 100

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<int>();

        // Universal Tax IDs
        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        // French-specific Business Registration
        builder.Property(c => c.Siret)
            .HasMaxLength(14)
            .HasComment("French SIRET number (14 digits)");

        builder.Property(c => c.Siren)
            .HasMaxLength(9)
            .HasComment("French SIREN number (9 digits)");

        // Danish-specific Business Registration
        builder.Property(c => c.CvrNumber)
            .HasMaxLength(8)
            .HasComment("Danish CVR number (8 digits)");

        builder.Property(c => c.DanishVatNumber)
            .HasMaxLength(12)
            .HasComment("Danish VAT number");

        // Primary Address
        builder.Property(c => c.Street)
            .HasMaxLength(200);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.PostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.CountryName)
            .HasMaxLength(100);

        // Primary Contact Information
        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Website)
            .HasMaxLength(200);

        // Business Settings
        builder.Property(c => c.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("EUR");

        builder.Property(c => c.PaymentTermDays)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(c => c.DefaultDiscountPercent)
            .HasPrecision(5, 2);

        // Client Management
        builder.Property(c => c.Category)
            .HasMaxLength(50)
            .HasComment("Client category (VIP, Regular, Prospect, etc.)");

        builder.Property(c => c.Priority)
            .IsRequired()
            .HasConversion<int>();

        // Client Statistics & Metadata
        builder.Property(c => c.FirstInvoiceDate)
            .HasComment("Date of first invoice for this client");

        builder.Property(c => c.LastInvoiceDate)
            .HasComment("Date of most recent invoice");

        builder.Property(c => c.TotalInvoices)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.TotalRevenue)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.OverdueAmount)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.AveragePaymentDays)
            .IsRequired()
            .HasDefaultValue(0);

        // Status & Branding
        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000)
            .HasComment("Internal notes about the client");

        builder.Property(c => c.LogoPath)
            .HasMaxLength(500)
            .HasComment("Path to client logo file");

        builder.Property(c => c.InternalReference)
            .HasMaxLength(50)
            .HasComment("Internal client reference code");

        // Navigation Properties
        builder.HasMany(c => c.Contacts)
            .WithOne(contact => contact.Client)
            .HasForeignKey(contact => contact.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Invoices)
            .WithOne(invoice => invoice.Client)
            .HasForeignKey(invoice => invoice.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Addresses)
            .WithOne(address => address.Client)
            .HasForeignKey(address => address.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance Indexes
        ConfigureIndexes(builder);
    }

    private static void ConfigureIndexes(EntityTypeBuilder<Client> builder)
    {
        // Basic search indexes
        builder.HasIndex(c => c.Name)
            .HasDatabaseName("IX_Clients_Name");

        builder.HasIndex(c => c.Email)
            .HasDatabaseName("IX_Clients_Email");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Clients_IsActive");

        // Business registration indexes  
        builder.HasIndex(c => c.Siret)
            .HasDatabaseName("IX_Clients_Siret")
            .IsUnique()
            .HasFilter("[Siret] IS NOT NULL");

        builder.HasIndex(c => c.Siren)
            .HasDatabaseName("IX_Clients_Siren")
            .HasFilter("[Siren] IS NOT NULL");

        builder.HasIndex(c => c.CvrNumber)
            .HasDatabaseName("IX_Clients_CvrNumber")
            .IsUnique()
            .HasFilter("[CvrNumber] IS NOT NULL");

        // Country and type indexes
        builder.HasIndex(c => c.Country)
            .HasDatabaseName("IX_Clients_Country");

        builder.HasIndex(c => c.Type)
            .HasDatabaseName("IX_Clients_Type");

        builder.HasIndex(c => c.Category)
            .HasDatabaseName("IX_Clients_Category");

        builder.HasIndex(c => c.Priority)
            .HasDatabaseName("IX_Clients_Priority");

        // Composite indexes for common queries
        builder.HasIndex(c => new { c.IsActive, c.Type })
            .HasDatabaseName("IX_Clients_Active_Type");

        builder.HasIndex(c => new { c.Country, c.IsActive })
            .HasDatabaseName("IX_Clients_Country_Active");

        builder.HasIndex(c => new { c.Category, c.IsActive })
            .HasDatabaseName("IX_Clients_Category_Active");

        // Performance indexes for statistics
        builder.HasIndex(c => c.TotalRevenue)
            .HasDatabaseName("IX_Clients_TotalRevenue");

        builder.HasIndex(c => c.OverdueAmount)
            .HasDatabaseName("IX_Clients_OverdueAmount")
            .HasFilter("[OverdueAmount] > 0");

        builder.HasIndex(c => c.LastInvoiceDate)
            .HasDatabaseName("IX_Clients_LastInvoiceDate");
    }
}
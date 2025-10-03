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

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LegalName)
            .HasMaxLength(200);

        // Country
        builder.Property(c => c.Country)
            .HasMaxLength(2);

        // Tax IDs
        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.VatNumber)
            .HasMaxLength(50);

        // French-specific
        builder.Property(c => c.Siret)
            .HasMaxLength(14);

        builder.Property(c => c.IntraCommunityVatFr)
            .HasMaxLength(50);

        // Danish-specific
        builder.Property(c => c.CvrNumber)
            .HasMaxLength(8);

        builder.Property(c => c.DanishVatNumber)
            .HasMaxLength(12);

        // Contact
        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Website)
            .HasMaxLength(200);

        // Address
        builder.Property(c => c.Street)
            .HasMaxLength(200);

        builder.Property(c => c.City)
            .HasMaxLength(100);

        builder.Property(c => c.PostalCode)
            .HasMaxLength(20);

        builder.Property(c => c.CountryName)
            .HasMaxLength(100);

        // Business Settings
        builder.Property(c => c.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.PaymentTermDays)
            .IsRequired();

        builder.Property(c => c.DefaultDiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);
        builder.Property(c => c.LogoPath)
    .HasMaxLength(500);

        // Indexes for Performance
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Email);
        builder.HasIndex(c => c.Siret);
        builder.HasIndex(c => c.CvrNumber);
        builder.HasIndex(c => c.Country);
        builder.HasIndex(c => c.IsActive);
    }
}
using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceStudio.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.LegalName)
            .HasMaxLength(200);

        // Country Settings
        builder.Property(c => c.Country)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(c => c.LegalForm)
            .IsRequired()
            .HasMaxLength(100);

        // Universal Business Registration
        builder.Property(c => c.BusinessRegistrationNumber)
            .HasMaxLength(50);

        builder.Property(c => c.TaxId)
            .HasMaxLength(50);

        builder.Property(c => c.VatNumber)
            .HasMaxLength(50);

        // French-specific
        builder.Property(c => c.Siret)
            .HasMaxLength(14);

        builder.Property(c => c.Siren)
            .HasMaxLength(9);

        builder.Property(c => c.ApeCode)
            .HasMaxLength(10);

        builder.Property(c => c.RcsNumber)
            .HasMaxLength(50);

        builder.Property(c => c.FrenchVatExemptionMention)
            .HasMaxLength(200);

        // Danish-specific
        builder.Property(c => c.CvrNumber)
            .HasMaxLength(8);

        builder.Property(c => c.DanishVatNumber)
            .HasMaxLength(12);

        builder.Property(c => c.SENumber)
            .HasMaxLength(20);

        builder.Property(c => c.IntraCommunityVat)
            .HasMaxLength(50);

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

        // Banking
        builder.Property(c => c.Iban)
            .HasMaxLength(50);

        builder.Property(c => c.Swift)
            .HasMaxLength(20);

        // Insurance
        builder.Property(c => c.InsuranceCompany)
            .HasMaxLength(200);

        builder.Property(c => c.InsurancePolicyNumber)
            .HasMaxLength(100);

        // Currency & Tax
        builder.Property(c => c.DefaultCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.DefaultTaxRate)
            .HasPrecision(5, 2);

        builder.Property(c => c.InvoicePrefix)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.QuotePrefix)
            .IsRequired()
            .HasMaxLength(10);

        // Indexes for Performance
        builder.HasIndex(c => c.Siret);
        builder.HasIndex(c => c.CvrNumber);
        builder.HasIndex(c => c.Country);
    }
}
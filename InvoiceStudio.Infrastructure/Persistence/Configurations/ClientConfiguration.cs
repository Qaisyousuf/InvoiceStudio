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

        builder.Property(c => c.Email)
            .HasMaxLength(100);

        builder.Property(c => c.PreferredCurrency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.PaymentTermDays)
            .IsRequired();

        builder.Property(c => c.DefaultDiscountPercent)
            .HasPrecision(5, 2);
    }
}
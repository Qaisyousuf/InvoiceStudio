using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceStudio.Infrastructure.Persistence.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Quantity)
            .HasPrecision(18, 2);

        builder.Property(l => l.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(l => l.TaxRate)
            .HasPrecision(5, 2);

        builder.Property(l => l.DiscountPercent)
            .HasPrecision(5, 2);

        builder.Property(l => l.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(l => l.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(l => l.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(l => l.Total)
            .HasPrecision(18, 2);

        builder.Property(l => l.Unit)
            .IsRequired()
            .HasMaxLength(20);

        // Indexes for Performance
        builder.HasIndex(l => l.InvoiceId);
        builder.HasIndex(l => l.ProductId);
        builder.HasIndex(l => l.LineOrder);

        // Relationships
        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
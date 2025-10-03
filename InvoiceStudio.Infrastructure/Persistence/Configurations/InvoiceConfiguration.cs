using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceStudio.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(i => i.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(i => i.PaidAmount)
            .HasPrecision(18, 2);

        // French/Danish Legal Fields
        builder.Property(i => i.LegalMentions)
            .HasMaxLength(2000);

        builder.Property(i => i.PaymentTerms)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.LatePenaltyRate)
            .HasPrecision(5, 2);

        builder.Property(i => i.FixedRecoveryFee)
            .HasPrecision(18, 2);

        builder.Property(i => i.EarlyPaymentDiscountRate)
            .HasPrecision(5, 2);

        builder.Property(i => i.ReverseTaxMention)
            .HasMaxLength(200);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.Property(i => i.Terms)
            .HasMaxLength(2000);

        // Indexes for Performance
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.ClientId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.IssueDate);
        builder.HasIndex(i => i.DueDate);

        // Relationships
        builder.HasOne(i => i.Client)
            .WithMany()
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Lines)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
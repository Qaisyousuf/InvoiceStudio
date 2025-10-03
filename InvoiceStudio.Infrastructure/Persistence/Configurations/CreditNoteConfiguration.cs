using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceStudio.Infrastructure.Persistence.Configurations;

public class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("CreditNotes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreditNoteNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(c => c.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Notes)
            .HasMaxLength(1000);

        // Indexes for Performance
        builder.HasIndex(c => c.CreditNoteNumber).IsUnique();
        builder.HasIndex(c => c.InvoiceId);
        builder.HasIndex(c => c.ClientId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.IssueDate);

        // Relationships
        builder.HasOne(c => c.Invoice)
            .WithMany()
            .HasForeignKey(c => c.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
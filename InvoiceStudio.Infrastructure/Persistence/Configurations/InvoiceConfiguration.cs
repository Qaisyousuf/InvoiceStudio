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

        // Basic Properties
        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.IssueDate)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .IsRequired();

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        // Decimal Properties with Precision
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

        // Optional DateTime
        builder.Property(i => i.PaidDate)
            .IsRequired(false);

        // Text Properties
        builder.Property(i => i.LegalMentions)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(i => i.PaymentTerms)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Notes)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(i => i.Terms)
            .HasMaxLength(2000)
            .IsRequired(false);

        // Foreign Key Properties - EXPLICITLY CONFIGURE THESE
        builder.Property(i => i.ClientId)
            .IsRequired();

        builder.Property(i => i.CompanyId)
            .IsRequired();

        // Indexes for Performance
        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.HasIndex(i => i.ClientId);
        builder.HasIndex(i => i.CompanyId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.IssueDate);
        builder.HasIndex(i => i.DueDate);

        builder.HasIndex(i => new { i.Status, i.DueDate })
            .HasDatabaseName("IX_Invoice_Status_DueDate");

        // Relationships - FIX THE CONFIGURATION HERE
        builder.HasOne(i => i.Client)
            .WithMany() // Specify the collection property name if Client has one
            .HasForeignKey(i => i.ClientId)
            .HasConstraintName("FK_Invoices_Clients_ClientId") // Explicit constraint name
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Company)
            .WithMany() // Specify the collection property name if Company has one
            .HasForeignKey(i => i.CompanyId)
            .HasConstraintName("FK_Invoices_Companies_CompanyId") // Explicit constraint name
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Lines)
            .WithOne(l => l.Invoice)
            .HasForeignKey(l => l.InvoiceId)
            .HasConstraintName("FK_InvoiceLines_Invoices_InvoiceId") // Explicit constraint name
            .OnDelete(DeleteBehavior.Cascade);
    }
}
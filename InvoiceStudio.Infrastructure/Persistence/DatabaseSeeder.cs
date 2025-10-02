using InvoiceStudio.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceStudio.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly InvoiceDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(InvoiceDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if data already exists
            if (await _context.Companies.AnyAsync())
            {
                _logger.LogInformation("Database already seeded");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            await SeedCompaniesAsync();
            await _context.SaveChangesAsync();

            await SeedTaxesAsync();
            await _context.SaveChangesAsync();

            await SeedClientsAsync();
            await _context.SaveChangesAsync();

            await SeedProductsAsync();
            await _context.SaveChangesAsync();

            await SeedInvoicesAsync();
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    private async Task SeedCompaniesAsync()
    {
        var company = new Company("InvoiceStudio Ltd");
        company.UpdateDetails(
            "InvoiceStudio Ltd",
            "InvoiceStudio Limited",
            "123456789",
            "FR12345678901"
        );
        company.UpdateAddress(
            "123 Business Street",
            "Paris",
            "75001",
            "France"
        );
        company.UpdateContact(
            "contact@invoicestudio.com",
            "+33 1 23 45 67 89",
            "www.invoicestudio.com"
        );
        company.UpdateBanking(
            "BNP Paribas",
            "FR7630006000011234567890189",
            "BNPAFRPPXXX"
        );

        _context.Companies.Add(company);
        await Task.CompletedTask;
    }

    private async Task SeedTaxesAsync()
    {
        var taxes = new[]
        {
            new Tax("VAT Standard", 20.0m, TaxType.VAT, "FR"),
            new Tax("VAT Reduced", 10.0m, TaxType.VAT, "FR"),
            new Tax("VAT Super Reduced", 5.5m, TaxType.VAT, "FR"),
            new Tax("No Tax", 0.0m, TaxType.None, "FR")
        };

        _context.Taxes.AddRange(taxes);
        await Task.CompletedTask;
    }

    private async Task SeedClientsAsync()
    {
        var clients = new[]
        {
            new Client("Acme Corporation", "contact@acme.com"),
            new Client("TechStart SAS", "hello@techstart.fr"),
            new Client("Global Solutions Inc", "info@globalsolutions.com")
        };

        clients[0].UpdateAddress("456 Commerce Ave", "Paris", "75002", "France");
        clients[0].UpdateBusinessSettings("EUR", 30);

        clients[1].UpdateAddress("789 Innovation Blvd", "Lyon", "69001", "France");
        clients[1].UpdateBusinessSettings("EUR", 15);

        clients[2].UpdateAddress("321 Enterprise St", "Marseille", "13001", "France");
        clients[2].UpdateBusinessSettings("EUR", 45);

        _context.Clients.AddRange(clients);
        await Task.CompletedTask;
    }

    private async Task SeedProductsAsync()
    {
        var products = new[]
        {
            new Product("Web Development", 85.00m, "EUR"),
            new Product("Consulting Services", 120.00m, "EUR"),
            new Product("UI/UX Design", 95.00m, "EUR"),
            new Product("Project Management", 100.00m, "EUR"),
            new Product("Technical Support", 60.00m, "EUR")
        };

        products[0].UpdateDetails("Web Development", "Full-stack web development services", "WEB-001");
        products[0].SetUnit("hours");

        products[1].UpdateDetails("Consulting Services", "IT consulting and advisory services", "CONS-001");
        products[1].SetUnit("hours");

        products[2].UpdateDetails("UI/UX Design", "User interface and experience design", "DESIGN-001");
        products[2].SetUnit("hours");

        products[3].UpdateDetails("Project Management", "Project planning and coordination", "PM-001");
        products[3].SetUnit("hours");

        products[4].UpdateDetails("Technical Support", "Technical assistance and support", "SUPPORT-001");
        products[4].SetUnit("hours");

        _context.Products.AddRange(products);
        await Task.CompletedTask;
    }

    private async Task SeedInvoicesAsync()
    {
        // We'll get the first client to create sample invoices
        var client = await _context.Clients.FirstAsync();
        var products = await _context.Products.ToListAsync();

        var invoice = new Invoice(
            "INV-2025-001",
            client.Id,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20)
        );

        // Add some lines
        invoice.AddLine(new InvoiceLine(
            invoice.Id,
            products[0].Name,
            40,
            products[0].UnitPrice,
            20.0m,
            products[0].Id
        ));

        invoice.AddLine(new InvoiceLine(
            invoice.Id,
            products[1].Name,
            10,
            products[1].UnitPrice,
            20.0m,
            products[1].Id
        ));

        invoice.Approve();
        invoice.Issue();

        _context.Invoices.Add(invoice);
        await Task.CompletedTask;
    }
}
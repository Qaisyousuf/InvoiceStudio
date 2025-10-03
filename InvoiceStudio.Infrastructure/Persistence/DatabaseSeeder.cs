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
        // French Company
        var frenchCompany = new Company("InvoiceStudio France", "FR");
        frenchCompany.UpdateFrenchRegistration("12345678901234", "6201Z", true);
        frenchCompany.UpdateDetails(
            "InvoiceStudio France",
            "InvoiceStudio SARL",
            "FR12345678901",
            null
        );
        frenchCompany.UpdateAddress(
            "123 Rue de la République",
            "Paris",
            "75001",
            "France"
        );
        frenchCompany.UpdateContact(
            "contact@invoicestudio.fr",
            "+33 1 23 45 67 89",
            "www.invoicestudio.fr"
        );
        frenchCompany.UpdateBanking(
            "BNP Paribas",
            "FR7630006000011234567890189",
            "BNPAFRPPXXX"
        );

        _context.Companies.Add(frenchCompany);
        await Task.CompletedTask;
    }

    private async Task SeedTaxesAsync()
    {
        var taxes = new[]
        {
            // French VAT rates
            new Tax("TVA Standard France", 20.0m, TaxType.VAT, "FR"),
            new Tax("TVA Intermédiaire France", 10.0m, TaxType.VAT, "FR"),
            new Tax("TVA Réduite France", 5.5m, TaxType.VAT, "FR"),
            new Tax("Pas de TVA France", 0.0m, TaxType.None, "FR"),
            
            // Danish VAT rates
            new Tax("Moms Standard Denmark", 25.0m, TaxType.VAT, "DK"),
            new Tax("No VAT Denmark", 0.0m, TaxType.None, "DK")
        };

        _context.Taxes.AddRange(taxes);
        await Task.CompletedTask;
    }

    private async Task SeedClientsAsync()
    {
        // French B2B Client
        var frenchClient = new Client("Acme Corporation France", "contact@acme.fr", ClientType.Company);
        frenchClient.UpdateFrenchBusinessInfo("98765432109876", "FR98765432109");
        frenchClient.UpdateAddress("456 Avenue des Champs-Élysées", "Paris", "75008", "France");
        frenchClient.UpdateBusinessSettings("EUR", 30);

        // French B2C Client
        var frenchIndividual = new Client("Jean Dupont", "jean.dupont@email.fr", ClientType.Individual);
        frenchIndividual.UpdateAddress("789 Rue du Commerce", "Lyon", "69001", "France");
        frenchIndividual.UpdateBusinessSettings("EUR", 15);

        // Danish B2B Client
        var danishClient = new Client("Copenhagen Tech ApS", "info@cphtech.dk", ClientType.Company);
        danishClient.UpdateDanishBusinessInfo("12345678", "DK12345678");
        danishClient.UpdateAddress("Nørrebrogade 123", "Copenhagen", "2200", "Denmark");
        danishClient.UpdateBusinessSettings("DKK", 30);

        var clients = new[] { frenchClient, frenchIndividual, danishClient };
        _context.Clients.AddRange(clients);
        await Task.CompletedTask;
    }

    private async Task SeedProductsAsync()
    {
        var products = new[]
        {
            new Product("Développement Web", 85.00m, "EUR"),
            new Product("Conseil IT", 120.00m, "EUR"),
            new Product("Design UI/UX", 95.00m, "EUR"),
            new Product("Gestion de Projet", 100.00m, "EUR"),
            new Product("Support Technique", 60.00m, "EUR")
        };

        products[0].UpdateDetails("Développement Web", "Services de développement full-stack", "WEB-001");
        products[0].SetUnit("heures");
        products[0].UpdatePricing(85.00m, "EUR", 20.0m);

        products[1].UpdateDetails("Conseil IT", "Conseil et accompagnement IT", "CONS-001");
        products[1].SetUnit("heures");
        products[1].UpdatePricing(120.00m, "EUR", 20.0m);

        products[2].UpdateDetails("Design UI/UX", "Conception d'interfaces utilisateur", "DESIGN-001");
        products[2].SetUnit("heures");
        products[2].UpdatePricing(95.00m, "EUR", 20.0m);

        products[3].UpdateDetails("Gestion de Projet", "Planification et coordination de projets", "PM-001");
        products[3].SetUnit("heures");
        products[3].UpdatePricing(100.00m, "EUR", 20.0m);

        products[4].UpdateDetails("Support Technique", "Assistance technique", "SUPPORT-001");
        products[4].SetUnit("heures");
        products[4].UpdatePricing(60.00m, "EUR", 20.0m);

        _context.Products.AddRange(products);
        await Task.CompletedTask;
    }

    private async Task SeedInvoicesAsync()
    {
        var client = await _context.Clients.FirstAsync();
        var products = await _context.Products.ToListAsync();
        var company = await _context.Companies.FirstAsync();

        var invoice = new Invoice(
            "INV-2025-001",
            client.Id,
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(20),
            "EUR"
        );

        // Set French legal mentions
        invoice.SetFrenchLegalMentions(company.Siret!, company.ApeCode!, company.IsVatExempt);
        invoice.SetPaymentTerms("Net 30 jours", 10.0m, 40.0m);

        // Add invoice lines
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
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Infrastructure.Persistence;
using InvoiceStudio.Infrastructure.Persistence.Repositories;
using InvoiceStudio.Presentation.Wpf.Services;
using InvoiceStudio.Presentation.Wpf.ViewModels;
using InvoiceStudio.Presentation.Wpf.Views.Clients;
using InvoiceStudio.Presentation.Wpf.Views.Company;
using InvoiceStudio.Presentation.Wpf.Views.Invoices;
using InvoiceStudio.Presentation.Wpf.Views.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;

namespace InvoiceStudio.Presentation.Wpf;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public App()
    {
        _host = CreateHostBuilder().Build();
    }

    public static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory)
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .UseSerilog((context, loggerConfig) =>
            {
                loggerConfig
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File("logs/invoicestudio-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);
            });
    }

    private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
    {
        // Database
        services.AddDbContext<InvoiceDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging(false);
            options.EnableServiceProviderCaching(true);
        });

        // Repositories
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ITaxRepository, TaxRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        // Services
        services.AddScoped<IBankStatementOcrService, BankStatementOcrService>();
        services.AddScoped<IBankingInfoParser, BankingInfoParser>();

        // ViewModels - Register as Transient for proper dialog creation
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ClientDialogViewModel>();
        services.AddTransient<ClientsListViewModel>();
        services.AddTransient<ProductDialogViewModel>();
        services.AddTransient<ProductsListViewModel>();
        services.AddTransient<InvoicesListViewModel>();
        services.AddTransient<InvoiceDialogViewModel>();
        services.AddTransient<InvoiceDetailViewModel>();
        services.AddTransient<EditInvoiceViewModel>();
        services.AddTransient<CompanySettingsViewModel>();

        // Views - Register as Transient
        services.AddTransient<MainWindow>();
        services.AddTransient<ClientsListView>();
        services.AddTransient<ProductsListView>();
        services.AddTransient<InvoicesListView>();
        services.AddTransient<CompanySettingsView>();

        // Add Serilog logger
        services.AddSingleton(Log.Logger);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            Log.Information("Starting InvoiceStudio application...");

            await _host.StartAsync();

            // Make service provider globally available
            ServiceProvider = _host.Services;

            // Ensure database exists and is up to date
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
                Log.Information("Ensuring database exists...");
                await context.Database.EnsureCreatedAsync();
                Log.Information("Database ready");
            }

            // Create and show main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            Log.Information("Application started successfully");
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            MessageBox.Show($"Startup Error: {ex.Message}\n\nDetails: {ex.InnerException?.Message}",
                "Application Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            Log.Information("Shutting down application...");
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            Log.Information("Application shutdown complete");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during application shutdown");
        }
        finally
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
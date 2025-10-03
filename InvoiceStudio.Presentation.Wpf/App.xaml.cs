using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Infrastructure.Persistence;
using InvoiceStudio.Infrastructure.Persistence.Repositories;
using InvoiceStudio.Presentation.Wpf.ViewModels;
using InvoiceStudio.Presentation.Wpf.Views.Clients;
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
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/invoicestudio-.txt",
                    rollingInterval: RollingInterval.Day);
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
        });
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ITaxRepository, TaxRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        // MainWindow

        services.AddTransient<InvoicesListViewModel>();
        services.AddTransient<InvoicesListView>();
        services.AddTransient<ClientsListViewModel>();
        services.AddTransient<ClientsListView>();
        services.AddTransient<ProductsListViewModel>();
        services.AddTransient<ProductsListView>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Seed the database
        using (var scope = _host.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();
        }

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }
        base.OnExit(e);
    }
}
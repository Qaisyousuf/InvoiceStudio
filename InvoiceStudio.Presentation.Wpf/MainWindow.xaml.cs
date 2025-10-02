using System.Windows;
using Serilog;

namespace InvoiceStudio.Presentation.Wpf;

public partial class MainWindow : Window
{
    private readonly ILogger _logger;

    public MainWindow(ILogger logger)
    {
        InitializeComponent();

        _logger = logger;
        _logger.Information("MainWindow initialized");

        Title = "InvoiceStudio - AI-Powered Invoice Generator";
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Dashboard";
        _logger.Information("Navigated to Dashboard");
        // TODO: Load dashboard view
    }

    private void InvoicesButton_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Invoices";
        _logger.Information("Navigated to Invoices");
        // TODO: Load invoices view
    }

    private void ClientsButton_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Clients";
        _logger.Information("Navigated to Clients");
        // TODO: Load clients view
    }

    private void ProductsButton_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Products";
        _logger.Information("Navigated to Products");
        // TODO: Load products view
    }

    private void ReportsButton_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Reports";
        _logger.Information("Navigated to Reports");
        // TODO: Load reports view
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        PageTitle.Text = "Settings";
        _logger.Information("Navigated to Settings");
        // TODO: Load settings view
    }
}
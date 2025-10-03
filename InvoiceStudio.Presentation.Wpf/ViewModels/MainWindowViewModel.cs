using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using InvoiceStudio.Presentation.Wpf.Views.Clients;
using InvoiceStudio.Presentation.Wpf.Views.Invoices;
using InvoiceStudio.Presentation.Wpf.Views.Products;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    private UserControl? _currentView;
    public UserControl? CurrentView
    {
        get => _currentView;
        set
        {
            _currentView = value;
            OnPropertyChanged(nameof(CurrentView));
        }
    }

    public MainWindowViewModel(ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        Title = "Dashboard";

        // Load Invoices view by default
        NavigateToInvoices();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        Title = "Dashboard";
        CurrentView = CreatePlaceholder("Dashboard - Coming Soon");
        _logger.Information("Navigated to Dashboard");
    }

    [RelayCommand]
    private void NavigateToInvoices()
    {
        Title = "Invoices";
        CurrentView = _serviceProvider.GetRequiredService<InvoicesListView>();
        _logger.Information("Navigated to Invoices");
    }

    [RelayCommand]
    private async void NavigateToClients()
    {
        Title = "Clients";
        var view = _serviceProvider.GetRequiredService<ClientsListView>();
        CurrentView = view;
        _logger.Information("Navigated to Clients");

        // Load data after view is set
        if (view.DataContext is ClientsListViewModel vm)
        {
            await vm.LoadClientsCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async void NavigateToProducts()
    {
        Title = "Products";
        var view = _serviceProvider.GetRequiredService<ProductsListView>();
        CurrentView = view;
        _logger.Information("Navigated to Products");
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        Title = "Reports";
        CurrentView = CreatePlaceholder("Reports - Coming Soon");
        _logger.Information("Navigated to Reports");
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        Title = "Settings";
        CurrentView = CreatePlaceholder("Settings - Coming Soon");
        _logger.Information("Navigated to Settings");
    }

    private UserControl CreatePlaceholder(string text)
    {
        return new UserControl
        {
            Content = new TextBlock
            {
                Text = text,
                FontSize = 24,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            }
        };
    }
}
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using InvoiceStudio.Presentation.Wpf.Views.Clients;
using InvoiceStudio.Presentation.Wpf.Views.Company;
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

        // Start with Dashboard instead of Invoices
        NavigateToDashboard();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        Title = "Dashboard";
        CurrentView = CreatePlaceholder("Dashboard - Coming Soon");
        _logger.Information("Navigated to Dashboard");
    }

    [RelayCommand]
    private async void NavigateToInvoices()
    {
        Title = "Invoices";
        var view = _serviceProvider.GetRequiredService<InvoicesListView>();
        CurrentView = view;
        _logger.Information("Navigated to Invoices");

        // Load data after view is set
        if (view.DataContext is InvoicesListViewModel vm)
        {
            await vm.LoadInvoicesCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async void NavigateToClients()
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error(ex, "Error navigating to Clients");
            CurrentView = CreatePlaceholder("Error loading Clients");
        }
    }

    [RelayCommand]
    private async void NavigateToProducts()
    {
        try
        {
            Title = "Products";
            var view = _serviceProvider.GetRequiredService<ProductsListView>();
            CurrentView = view;
            _logger.Information("Navigated to Products");

            // Load data after view is set (FIXED: This was missing!)
            if (view.DataContext is ProductsListViewModel vm)
            {
                await vm.LoadProductsCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error navigating to Products");
            CurrentView = CreatePlaceholder("Error loading Products");
        }
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        Title = "Reports";
        CurrentView = CreatePlaceholder("Reports - Coming Soon");
        _logger.Information("Navigated to Reports");
    }
    [RelayCommand]
    private async void NavigateToCompany()
    {
        try
        {
            Title = "Company";
            var view = _serviceProvider.GetRequiredService<CompanySettingsView>();
            CurrentView = view;
            _logger.Information("Navigated to Company");

            // Load data after view is set
            if (view.DataContext is CompanySettingsViewModel vm)
            {
                await vm.LoadCompanyCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error navigating to Company");
            CurrentView = CreatePlaceholder("Error loading Company");
        }
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
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.White
            }
        };
    }
}
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;

    [ObservableProperty]
    private int _totalInvoices;

    [ObservableProperty]
    private int _paidInvoices;

    [ObservableProperty]
    private int _overdueInvoices;

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private int _totalClients;

    [ObservableProperty]
    private int _totalProducts;

    public ObservableCollection<Invoice> RecentInvoices { get; } = new();

    public DashboardViewModel(
        IInvoiceRepository invoiceRepository,
        IClientRepository clientRepository,
        IProductRepository productRepository,
        ILogger logger)
    {
        _invoiceRepository = invoiceRepository;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
        _logger = logger;
        Title = "Dashboard";
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        try
        {
            IsBusy = true;
            _logger.Information("Loading dashboard data...");

            // Load all invoices
            var allInvoices = await _invoiceRepository.GetAllAsync();

            // Calculate statistics
            TotalInvoices = allInvoices.Count;
            PaidInvoices = allInvoices.Count(i => i.Status == InvoiceStatus.Paid);
            OverdueInvoices = allInvoices.Count(i => i.Status == InvoiceStatus.Overdue);
            TotalRevenue = allInvoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount);

            // Load recent invoices (last 5)
            var recentInvoices = allInvoices
                .OrderByDescending(i => i.IssueDate)
                .Take(5)
                .ToList();

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                RecentInvoices.Clear();
                foreach (var invoice in recentInvoices)
                {
                    RecentInvoices.Add(invoice);
                }
            });

            // Load client and product counts
            var clients = await _clientRepository.GetAllAsync();
            TotalClients = clients.Count;

            var products = await _productRepository.GetAllAsync();
            TotalProducts = products.Count;

            _logger.Information("Dashboard data loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading dashboard data");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
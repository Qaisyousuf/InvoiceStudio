using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Infrastructure.Persistence.Repositories;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using InvoiceStudio.Presentation.Wpf.Views.Clients;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.ObjectModel;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ClientsListViewModel : ViewModelBase
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _isLoading;

    public ObservableCollection<Client> Clients { get; } = new();

    // Dashboard Statistics
    [ObservableProperty]
    private int _totalClients;

    [ObservableProperty]
    private int _activeClients;

    [ObservableProperty]
    private int _companiesCount;

    [ObservableProperty]
    private int _individualsCount;

    private readonly IInvoiceRepository _invoiceRepository;

    public ClientsListViewModel(IClientRepository clientRepository, ILogger logger, IServiceProvider serviceProvider, IInvoiceRepository invoiceRepository)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _invoiceRepository = invoiceRepository; // Add this line
        Title = "Clients";
    }

    [RelayCommand]
    private async Task CreateClientAsync()
    {
        try
        {
            _logger.Information("Create client clicked");

            // Get the ClientDialogViewModel from DI
            var dialogViewModel = _serviceProvider.GetRequiredService<ClientDialogViewModel>();

            // Create the dialog window
            var dialogView = new ClientDialogView(dialogViewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Show dialog
            var result = dialogView.ShowDialog();

            if (result == true)
            {
                _logger.Information("Client dialog completed successfully");
                // Refresh clients list after successful creation
                await LoadClientsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening create client dialog");
        }
    }

    [RelayCommand]
    private async Task EditClientAsync(Client client)
    {
        if (client == null) return;

        try
        {
            _logger.Information("Edit client {ClientName}", client.Name);

            // Get the ClientDialogViewModel from DI
            var dialogViewModel = _serviceProvider.GetRequiredService<ClientDialogViewModel>();

            // Load the existing client data into the dialog
            dialogViewModel.LoadClient(client);

            // Create the dialog window
            var dialogView = new ClientDialogView(dialogViewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Show dialog
            var result = dialogView.ShowDialog();

            if (result == true)
            {
                _logger.Information("Client edit completed successfully");
                // Refresh clients list after successful edit
                await LoadClientsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening edit client dialog");
        }
    }
    [RelayCommand]
    private async Task DeleteClientAsync(Client client)
    {
        if (client == null) return;

        try
        {
            _logger.Information("Delete client {ClientName} requested", client.Name);

            // Check if client has associated invoices (using AsNoTracking to avoid conflicts)
            var invoiceCount = await _invoiceRepository.GetByClientIdAsync(client.Id);

            string confirmationMessage;
            if (invoiceCount.Any())
            {
                confirmationMessage = $"Client '{client.Name}' has {invoiceCount.Count} associated invoice(s).\n\n" +
                                     "Cannot delete client with existing invoices.\n\n" +
                                     "Please delete all invoices first, or deactivate the client instead.";

                System.Windows.MessageBox.Show(
                    confirmationMessage,
                    "Cannot Delete Client",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return;
            }

            // Show confirmation dialog for clients with no invoices
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the client '{client.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning,
                System.Windows.MessageBoxResult.No);

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                _logger.Information("Client deletion cancelled by user");
                return;
            }

            IsBusy = true;
            _logger.Information("Deleting client {ClientName}", client.Name);

            // Delete the client (only if no invoices exist)
            await _clientRepository.DeleteAsync(client);
            await _clientRepository.SaveChangesAsync();

            _logger.Information("Client {ClientName} deleted successfully", client.Name);

            // Refresh the clients list
            await LoadClientsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting client {ClientName}", client.Name);

            // Show user-friendly error message
            System.Windows.MessageBox.Show(
                $"Failed to delete client '{client.Name}'.\n\nThe client may have associated data that prevents deletion.\n\nTry deactivating the client instead.",
                "Delete Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }



    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        if (_isLoading) return;

        try
        {
            _isLoading = true;
            IsBusy = true;
            _logger.Information("Loading clients...");

            // Add timeout using CancellationToken
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var clients = await _clientRepository.GetAllAsync(cts.Token);

            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }

            _logger.Information("Loaded {Count} clients", clients.Count);

            // Update dashboard statistics
            UpdateDashboardStats();
        }
        catch (OperationCanceledException)
        {
            _logger.Error("Query timed out after 10 seconds");
            Clients.Clear();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load clients");
            Clients.Clear();
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    private void UpdateDashboardStats()
    {
        TotalClients = Clients.Count;
        ActiveClients = Clients.Count(c => c.IsActive);
        CompaniesCount = Clients.Count(c => c.Type == ClientType.Company);
        IndividualsCount = Clients.Count(c => c.Type == ClientType.Individual);
    }
}
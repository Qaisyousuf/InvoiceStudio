using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using InvoiceStudio.Presentation.Wpf.Views.Clients;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;
using WpfApplication = System.Windows.Application; // Add this alias to avoid namespace collision

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ClientsListViewModel : ViewModelBase
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IInvoiceRepository _invoiceRepository;
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

    // Search and Filter Properties
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showActiveOnly = true;

    [ObservableProperty]
    private ClientType? _selectedTypeFilter;

    [ObservableProperty]
    private string? _selectedCategoryFilter;

    // Collections for UI
    public ObservableCollection<string> Categories { get; } = new();
    public IEnumerable<ClientType> ClientTypes => Enum.GetValues<ClientType>();

    public ClientsListViewModel(
        IClientRepository clientRepository,
        ILogger logger,
        IServiceProvider serviceProvider,
        IInvoiceRepository invoiceRepository)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));

        Title = "Clients";

        // Initialize with loading clients
        _ = Task.Run(async () => await LoadClientsAsync());
    }

    [RelayCommand]
    private async Task CreateClientAsync()
    {
        try
        {
            _logger.Information("Creating new client - button clicked");

            // Validate service provider
            if (_serviceProvider == null)
            {
                _logger.Error("Service provider is null");
                ShowErrorMessage("Configuration Error", "Service provider not available. Please restart the application.");
                return;
            }

            // Get the ClientDialogViewModel from DI with detailed error handling
            ClientDialogViewModel? dialogViewModel;
            try
            {
                dialogViewModel = _serviceProvider.GetRequiredService<ClientDialogViewModel>();
                _logger.Information("ClientDialogViewModel resolved successfully");
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex, "Failed to resolve ClientDialogViewModel from DI container");
                ShowErrorMessage("Configuration Error",
                    "Client dialog service not registered. Please check dependency injection configuration.");
                return;
            }

            if (dialogViewModel == null)
            {
                _logger.Error("ClientDialogViewModel resolved to null");
                ShowErrorMessage("Configuration Error", "Failed to create client dialog.");
                return;
            }

            // Create the dialog window
            ClientDialogView? dialogView;
            try
            {
                dialogView = new ClientDialogView(dialogViewModel);
                _logger.Information("ClientDialogView created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create ClientDialogView");
                ShowErrorMessage("Dialog Error", $"Failed to create client dialog: {ex.Message}");
                return;
            }

            // Set owner window - FIXED: Use WpfApplication alias instead of Application
            try
            {
                if (WpfApplication.Current?.MainWindow != null)
                {
                    dialogView.Owner = WpfApplication.Current.MainWindow;
                    _logger.Information("Dialog owner set to MainWindow");
                }
                else
                {
                    _logger.Warning("MainWindow is null, dialog will not have owner");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to set dialog owner, continuing anyway");
            }

            // Show dialog
            _logger.Information("Showing client dialog");
            var result = dialogView.ShowDialog();
            _logger.Information("Dialog closed with result: {Result}", result);

            if (result == true)
            {
                _logger.Information("Client dialog completed successfully - refreshing client list");
                await LoadClientsAsync();
                ShowSuccessMessage("Success", "Client created successfully!");
            }
            else
            {
                _logger.Information("Client dialog was cancelled or closed without saving");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error in CreateClientAsync");
            ShowErrorMessage("Error", $"An unexpected error occurred: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EditClientAsync(Client? client)
    {
        if (client == null)
        {
            _logger.Warning("EditClientAsync called with null client");
            return;
        }

        try
        {
            _logger.Information("Editing client: {ClientId} - {ClientName}", client.Id, client.Name);

            // Get fresh client data from database to avoid stale data issues
            var freshClient = await _clientRepository.GetByIdAsync(client.Id);
            if (freshClient == null)
            {
                ShowErrorMessage("Error", "Client not found. It may have been deleted by another user.");
                await LoadClientsAsync();
                return;
            }

            // Get the ClientDialogViewModel from DI
            var dialogViewModel = _serviceProvider.GetRequiredService<ClientDialogViewModel>();

            // Load the existing client data into the dialog
            dialogViewModel.LoadClient(freshClient);

            // Create and show the dialog window
            var dialogView = new ClientDialogView(dialogViewModel);

            // FIXED: Use WpfApplication alias instead of Application
            if (WpfApplication.Current?.MainWindow != null)
            {
                dialogView.Owner = WpfApplication.Current.MainWindow;
            }

            var result = dialogView.ShowDialog();

            if (result == true)
            {
                _logger.Information("Client edit completed successfully - refreshing client list");
                await LoadClientsAsync();
                ShowSuccessMessage("Success", "Client updated successfully!");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error editing client {ClientName}", client.Name);
            ShowErrorMessage("Error", $"Failed to edit client: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteClientAsync(Client? client)
    {
        if (client == null)
        {
            _logger.Warning("DeleteClientAsync called with null client");
            return;
        }

        try
        {
            _logger.Information("Delete client {ClientName} requested", client.Name);

            // Check if client has associated invoices
            var invoices = await _invoiceRepository.GetByClientIdAsync(client.Id);

            if (invoices.Any())
            {
                var message = $"Client '{client.Name}' has {invoices.Count} associated invoice(s).\n\n" +
                             "Cannot delete client with existing invoices.\n\n" +
                             "Please delete all invoices first, or deactivate the client instead.";

                ShowWarningMessage("Cannot Delete Client", message);
                return;
            }

            // Show confirmation dialog
            var result = MessageBox.Show(
                $"Are you sure you want to delete the client '{client.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
            {
                _logger.Information("Client deletion cancelled by user");
                return;
            }

            IsBusy = true;
            _logger.Information("Deleting client {ClientName}", client.Name);

            await _clientRepository.DeleteAsync(client);
            await _clientRepository.SaveChangesAsync();

            _logger.Information("Client {ClientName} deleted successfully", client.Name);
            await LoadClientsAsync();
            ShowSuccessMessage("Success", "Client deleted successfully!");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting client {ClientName}", client.Name);
            ShowErrorMessage("Delete Error",
                $"Failed to delete client '{client.Name}'.\n\nThe client may have associated data that prevents deletion.\n\nTry deactivating the client instead.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleClientStatusAsync(Client? client)
    {
        if (client == null) return;

        try
        {
            _logger.Information("Toggling status for client {ClientName}", client.Name);

            if (client.IsActive)
            {
                client.Deactivate();
            }
            else
            {
                client.Activate();
            }

            await _clientRepository.UpdateAsync(client);
            await _clientRepository.SaveChangesAsync();

            _logger.Information("Client {ClientName} status changed to {Status}", client.Name, client.IsActive ? "Active" : "Inactive");
            await LoadClientsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error toggling client status");
            ShowErrorMessage("Error", "Failed to update client status.");
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

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var clients = await _clientRepository.GetAllAsync(cts.Token);

            // Apply filters
            var filteredClients = ApplyFilters(clients);

            // Update UI on main thread - FIXED: Use WpfApplication alias
            WpfApplication.Current.Dispatcher.Invoke(() =>
            {
                Clients.Clear();
                foreach (var client in filteredClients)
                {
                    Clients.Add(client);
                }

                // Update categories for filter dropdown
                UpdateCategories(clients);
                UpdateDashboardStats();
            });

            _logger.Information("Loaded {Count} clients ({FilteredCount} after filters)",
                clients.Count, filteredClients.Count());
        }
        catch (OperationCanceledException)
        {
            _logger.Error("Loading clients timed out after 30 seconds");
            ShowErrorMessage("Timeout", "Loading clients timed out. Please check your database connection.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load clients");
            ShowErrorMessage("Error", "Failed to load clients. Please try again.");
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadClientsAsync();
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = true;
        SelectedTypeFilter = null;
        SelectedCategoryFilter = null;
        _ = Task.Run(async () => await LoadClientsAsync());
    }

    private IEnumerable<Client> ApplyFilters(IEnumerable<Client> clients)
    {
        var filtered = clients.AsEnumerable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(c =>
                c.Name.ToLowerInvariant().Contains(searchLower) ||
                (c.Email?.ToLowerInvariant().Contains(searchLower) == true) ||
                (c.Phone?.Contains(SearchText) == true) ||
                (c.Category?.ToLowerInvariant().Contains(searchLower) == true));
        }

        // Active status filter
        if (ShowActiveOnly)
        {
            filtered = filtered.Where(c => c.IsActive);
        }

        // Type filter
        if (SelectedTypeFilter.HasValue)
        {
            filtered = filtered.Where(c => c.Type == SelectedTypeFilter.Value);
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(SelectedCategoryFilter))
        {
            filtered = filtered.Where(c => c.Category == SelectedCategoryFilter);
        }

        return filtered.OrderBy(c => c.Name);
    }

    private void UpdateCategories(IEnumerable<Client> clients)
    {
        var categories = clients
            .Where(c => !string.IsNullOrWhiteSpace(c.Category))
            .Select(c => c.Category!)
            .Distinct()
            .OrderBy(c => c);

        Categories.Clear();
        foreach (var category in categories)
        {
            Categories.Add(category);
        }
    }

    private void UpdateDashboardStats()
    {
        var allClients = Clients.ToList(); // Work with current filtered list

        TotalClients = allClients.Count;
        ActiveClients = allClients.Count(c => c.IsActive);
        CompaniesCount = allClients.Count(c => c.Type == ClientType.Company);
        IndividualsCount = allClients.Count(c => c.Type == ClientType.Individual);
    }

    #region Helper Methods

    private static void ShowSuccessMessage(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static void ShowWarningMessage(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static void ShowErrorMessage(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    #endregion
}
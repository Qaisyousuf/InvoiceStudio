using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;
using System.Collections.ObjectModel;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ClientsListViewModel : ViewModelBase
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger _logger;
    private bool _isLoading;

    public ObservableCollection<Client> Clients { get; } = new();

    public ClientsListViewModel(IClientRepository clientRepository, ILogger logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        Title = "Clients";
    }

    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        // Prevent multiple simultaneous loads
        if (_isLoading)
        {
            _logger.Information("Load already in progress, skipping");
            return;
        }

        try
        {
            _isLoading = true;
            IsBusy = true;
            _logger.Information("Starting to load clients...");

            var clients = await Task.Run(async () =>
                await _clientRepository.GetAllAsync());

            _logger.Information("Retrieved {Count} clients from database", clients.Count);

            // Ensure we're on UI thread for ObservableCollection updates
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Clients.Clear();
                foreach (var client in clients)
                {
                    Clients.Add(client);
                }
                _logger.Information("Successfully loaded {Count} clients to UI", clients.Count);
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading clients");
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    [RelayCommand]
    private void CreateClient()
    {
        _logger.Information("Create client clicked");
        // TODO: Open create client dialog
    }

    [RelayCommand]
    private void EditClient(Client client)
    {
        if (client == null) return;

        _logger.Information("Edit client {ClientName}", client.Name);
        // TODO: Open edit client dialog
    }

    [RelayCommand]
    private async Task DeleteClientAsync(Client client)
    {
        if (client == null) return;

        _logger.Information("Delete client {ClientName}", client.Name);
        // TODO: Add confirmation dialog, then delete
    }
}
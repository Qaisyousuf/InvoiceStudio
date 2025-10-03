using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using Serilog;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ProductsListViewModel : ViewModelBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;
    private bool _isLoading;

    public ObservableCollection<Product> Products { get; } = new();

    public ProductsListViewModel(IProductRepository productRepository, ILogger logger)
    {
        _productRepository = productRepository;
        _logger = logger;
        Title = "Products";
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (_isLoading)
        {
            _logger.Information("Load already in progress, skipping");
            return;
        }

        try
        {
            _isLoading = true;
            IsBusy = true;
            _logger.Information("Starting to load products...");

            var products = await Task.Run(async () =>
                await _productRepository.GetAllAsync());

            _logger.Information("Retrieved {Count} products from database", products.Count);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
                _logger.Information("Successfully loaded {Count} products to UI", products.Count);
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading products");
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
    }

    [RelayCommand]
    private void CreateProduct()
    {
        _logger.Information("Create product clicked");
        // TODO: Open create product dialog
    }

    [RelayCommand]
    private void EditProduct(Product product)
    {
        if (product == null) return;

        _logger.Information("Edit product {ProductName}", product.Name);
        // TODO: Open edit product dialog
    }

    [RelayCommand]
    private async Task DeleteProductAsync(Product product)
    {
        if (product == null) return;

        _logger.Information("Delete product {ProductName}", product.Name);
        // TODO: Add confirmation dialog, then delete
    }
}
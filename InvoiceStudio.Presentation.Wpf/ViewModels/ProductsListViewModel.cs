using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InvoiceStudio.Application.Abstractions;
using InvoiceStudio.Domain.Entities;
using InvoiceStudio.Presentation.Wpf.ViewModels.Base;
using InvoiceStudio.Presentation.Wpf.Views.Products;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.ObjectModel;

namespace InvoiceStudio.Presentation.Wpf.ViewModels;

public partial class ProductsListViewModel : ViewModelBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;
    private bool _isLoading;

    public ObservableCollection<Product> Products { get; } = new();

    private readonly IServiceProvider _serviceProvider;
 

    public ProductsListViewModel(IProductRepository productRepository, ILogger logger, IServiceProvider serviceProvider)
    {
        _productRepository = productRepository;
        _logger = logger;
        _serviceProvider = serviceProvider;
        Title = "Products";
    }



    [RelayCommand]
    private async Task CreateProductAsync()
    {
        try
        {
            _logger.Information("Create product clicked");

            // Get the ProductDialogViewModel from DI
            var dialogViewModel = _serviceProvider.GetRequiredService<ProductDialogViewModel>();

            // Create the dialog window
            var dialogView = new ProductDialogView(dialogViewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Show dialog
            var result = dialogView.ShowDialog();

            if (result == true)
            {
                _logger.Information("Product dialog completed successfully");
                // Refresh products list after successful creation
                await LoadProductsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening create product dialog");
        }
    }
    [RelayCommand]
    private async Task EditProductAsync(Product product)
    {
        if (product == null) return;

        try
        {
            _logger.Information("Edit product {ProductName}", product.Name);

            // Get the ProductDialogViewModel from DI
            var dialogViewModel = _serviceProvider.GetRequiredService<ProductDialogViewModel>();

            // Load the existing product data into the dialog
            dialogViewModel.LoadProduct(product);

            // Create the dialog window
            var dialogView = new ProductDialogView(dialogViewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            // Show dialog
            var result = dialogView.ShowDialog();

            if (result == true)
            {
                _logger.Information("Product edit completed successfully");
                // Refresh products list after successful edit
                await LoadProductsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error opening edit product dialog");
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync(Product product)
    {
        if (product == null) return;

        try
        {
            _logger.Information("Delete product {ProductName} requested", product.Name);

            // Show confirmation dialog
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the product '{product.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning,
                System.Windows.MessageBoxResult.No);

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                _logger.Information("Product deletion cancelled by user");
                return;
            }

            IsBusy = true;
            _logger.Information("Deleting product {ProductName}", product.Name);

            // Delete from repository
            await _productRepository.DeleteAsync(product);
            await _productRepository.SaveChangesAsync();

            _logger.Information("Product {ProductName} deleted successfully", product.Name);

            // Refresh the products list
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting product {ProductName}", product.Name);

            // Show error message to user
            System.Windows.MessageBox.Show(
                $"Failed to delete product '{product.Name}'.\n\nError: {ex.Message}",
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
    private async Task TestConnectionAsync()
    {
        try
        {
            _logger.Information("Testing database connection...");

            var canConnect = await _productRepository.TestConnectionAsync();
            _logger.Information("Can connect to database: {CanConnect}", canConnect);

            if (canConnect)
            {
                var count = await _productRepository.GetCountAsync();
                _logger.Information("Products count: {Count}", count);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Connection test failed");
        }
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        if (_isLoading) return;

        try
        {
            _isLoading = true;
            IsBusy = true;
            _logger.Information("Loading products...");

            // Add timeout using CancellationToken
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var products = await _productRepository.GetAllAsync(cts.Token);

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            _logger.Information("Loaded {Count} products", products.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.Error("Query timed out after 10 seconds");
            Products.Clear();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load products");
            Products.Clear();
        }
        finally
        {
            IsBusy = false;
            _isLoading = false;
        }
        // At the end of LoadProductsAsync method, after adding products to the collection
        UpdateDashboardStats();
    }

    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private int _activeProducts;

    [ObservableProperty]
    private decimal _totalValue;

    [ObservableProperty]
    private int _lowStockProducts; // You can set this to 0 for now

    private void UpdateDashboardStats()
    {
        TotalProducts = Products.Count;
        ActiveProducts = Products.Count(p => p.IsActive);
        TotalValue = Products.Where(p => p.IsActive).Sum(p => p.UnitPrice);
        LowStockProducts = 0; // Placeholder for future inventory feature
    }
}
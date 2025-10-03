using InvoiceStudio.Presentation.Wpf.ViewModels;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.Views.Products;

public partial class ProductsListView : UserControl
{
    public ProductsListView(ProductsListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Auto-load when view loads
        Loaded += (s, e) => _ = viewModel.LoadProductsCommand.ExecuteAsync(null);
    }
}
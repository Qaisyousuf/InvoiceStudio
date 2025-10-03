using System.Windows;
using InvoiceStudio.Presentation.Wpf.ViewModels;

namespace InvoiceStudio.Presentation.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
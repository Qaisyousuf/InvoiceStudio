using InvoiceStudio.Presentation.Wpf.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace InvoiceStudio.Presentation.Wpf.Views.Invoices;

public partial class InvoiceDialogView : Window
{
    private InvoiceDialogViewModel ViewModel => (InvoiceDialogViewModel)DataContext;

    public InvoiceDialogView(InvoiceDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Subscribe to validation changes to update UI
        viewModel.PropertyChanged += ViewModel_PropertyChanged;

        // Set window properties
        this.ShowInTaskbar = false;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InvoiceDialogViewModel.HasValidationErrors))
        {
            UpdateValidationUI();
        }
        else if (e.PropertyName == nameof(InvoiceDialogViewModel.IsBusy))
        {
            UpdateBusyState();
        }
    }

    private void UpdateValidationUI()
    {
        // You can add visual feedback for validation errors here
        // For example, show/hide error messages, change button states, etc.
    }

    private void UpdateBusyState()
    {
        if (ViewModel.IsBusy)
        {
            this.Cursor = System.Windows.Input.Cursors.Wait;
            this.IsEnabled = false;
        }
        else
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            this.IsEnabled = true;
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is InvoiceDialogViewModel vm)
            {
                // Disable the button to prevent double-clicks
                if (sender is Button saveButton)
                {
                    saveButton.IsEnabled = false;
                    saveButton.Content = "Creating...";
                }

                bool success = await vm.SaveAsync();
                if (success)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // Re-enable the button if save failed
                    if (sender is Button failedButton)
                    {
                        failedButton.IsEnabled = true;
                        failedButton.Content = ViewModel.IsEditMode ? "Update Invoice" : "Create Invoice";
                    }

                    // Show validation message if there are errors
                    if (vm.HasValidationErrors)
                    {
                        MessageBox.Show(vm.ValidationMessage, "Validation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Re-enable button on exception
            if (sender is Button errorButton)
            {
                errorButton.IsEnabled = true;
                errorButton.Content = ViewModel.IsEditMode ? "Update Invoice" : "Create Invoice";
            }

            MessageBox.Show($"An error occurred while saving the invoice: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // Execute the cancel command if available
        if (ViewModel.CancelDialogCommand?.CanExecute(null) == true)
        {
            ViewModel.CancelDialogCommand.Execute(null);
        }

        DialogResult = false;
        Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Focus on the client selection ComboBox when the window loads
        if (ViewModel.AvailableClients.Count > 0)
        {
            // Find the client ComboBox and focus on it
            var clientComboBox = this.FindName("ClientSelectionComboBox") as ComboBox;
            clientComboBox?.Focus();
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // Unsubscribe from events to prevent memory leaks
        if (DataContext is InvoiceDialogViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }

    // Helper method to show validation errors in a more user-friendly way
    private void ShowValidationError(string message)
    {
        MessageBox.Show(message, "Please check your input",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // Helper method to show success message
    private void ShowSuccessMessage(string message)
    {
        MessageBox.Show(message, "Success",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
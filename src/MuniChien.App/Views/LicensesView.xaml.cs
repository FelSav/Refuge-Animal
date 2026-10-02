using MuniChien.App.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace MuniChien.App.Views;

public partial class LicensesView : UserControl
{
    public LicensesView()
    {
        InitializeComponent();
    }


    private void LicensesDataGrid_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        if (DataContext is not LicensesViewModel viewModel)
        {
            return;
        }


        if (viewModel.SelectedLicense is null)
        {
            return;
        }


        if (viewModel.OpenSelectedLicenseCommand.CanExecute(null))
        {
            viewModel.OpenSelectedLicenseCommand.Execute(null);
        }
    }
}
using MuniChien.App.ViewModels;
using System.Windows;

namespace MuniChien.App.Views;

public partial class LicenseTariffsDialog : Window
{
    public LicenseTariffsDialog()
    {
        InitializeComponent();
        DataContext = new LicenseTariffsViewModel();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}

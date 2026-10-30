using MuniChien.App.ViewModels;
using System.Windows;

namespace MuniChien.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    // Rafraîchit les alertes après fermeture d'un dialogue d'achat/renouvellement.
    private void MainWindow_Activated(object sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.RefreshNotifications();
    }
}

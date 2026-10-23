using MuniChien.App.Services;
using System.Windows;

namespace MuniChien.App.Views;

public partial class SelectDogClientDialog : Window
{
    public OwnerDirectoryItem? SelectedClient { get; private set; }

    public SelectDogClientDialog(IReadOnlyList<OwnerDirectoryItem> clients)
    {
        InitializeComponent();
        ClientPicker.ItemsSource = clients.OrderBy(x => x.FullName).ThenBy(x => x.FileNumber).ToList();
        ClientPicker.SelectedItem = null;
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        if (ClientPicker.SelectedItem is not OwnerDirectoryItem client)
        {
            MessageBox.Show(this, "Choisissez un client avant de continuer.",
                "Client requis", MessageBoxButton.OK, MessageBoxImage.Warning);
            ClientPicker.Focus();
            return;
        }

        SelectedClient = client;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

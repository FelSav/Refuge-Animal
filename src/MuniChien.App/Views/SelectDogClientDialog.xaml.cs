using MuniChien.App.Services;
using System.Windows;

namespace MuniChien.App.Views;

public partial class SelectDogClientDialog : Window
{
    public OwnerDirectoryItem? SelectedClient { get; private set; }

    public SelectDogClientDialog(IReadOnlyList<OwnerDirectoryItem> clients)
    {
        InitializeComponent();
        ClientComboBox.ItemsSource = clients.OrderBy(x => x.FullName).ThenBy(x => x.FileNumber).ToList();
        ClientComboBox.SelectedIndex = -1;
    }

    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        if (ClientComboBox.SelectedItem is not OwnerDirectoryItem client)
        {
            MessageBox.Show(this, "Choisissez un client avant de continuer.",
                "Client requis", MessageBoxButton.OK, MessageBoxImage.Warning);
            ClientComboBox.Focus();
            return;
        }

        SelectedClient = client;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

using MuniChien.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MuniChien.App.Views;

public partial class OwnersView : UserControl
{
    public OwnersView()
    {
        InitializeComponent();
    }


    private void AddOwner_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OwnersViewModel viewModel) return;

        var dialog = new AddOwnerDialog();
        if (Window.GetWindow(this) is Window ownerWindow)
            dialog.Owner = ownerWindow;

        if (dialog.ShowDialog() != true || dialog.CreatedOwner is null) return;

        try
        {
            var created = viewModel.AddOwner(dialog.CreatedOwner);
            MessageBox.Show(
                $"Le dossier de {created.FullName} (#{created.FileNumber}) a été créé en démonstration.",
                "Propriétaire ajouté", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Impossible d'ajouter ce propriétaire au répertoire local. Vérifiez les champs et réessayez.",
                "Création impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }


    private void OpenLoyalty_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not OwnersViewModel viewModel || viewModel.OwnerId <= 0) return;

        var dialog = new LoyaltyCardsDialog(
            viewModel.OwnerId,
            $"{viewModel.FirstName} {viewModel.LastName}".Trim());

        if (Window.GetWindow(this) is Window ownerWindow)
            dialog.Owner = ownerWindow;

        dialog.ShowDialog();
        viewModel.RefreshLoyalty();
    }

    private void AddDog_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is not OwnersViewModel viewModel)
        {
            return;
        }


        string ownerName =
            $"{viewModel.FirstName} {viewModel.LastName}".Trim();


        AddDogDialog dialog =
            new(ownerName, viewModel.NextDogId, viewModel.OwnerId);


        if (Window.GetWindow(this) is Window ownerWindow)
        {
            dialog.Owner = ownerWindow;
        }


        bool? result =
            dialog.ShowDialog();


        if (result == true &&
            dialog.CreatedDog is not null)
        {
            viewModel.AddDog(
                dialog.CreatedDog);
        }
    }
}
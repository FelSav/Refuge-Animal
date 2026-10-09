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
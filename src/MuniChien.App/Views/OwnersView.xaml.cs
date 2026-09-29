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


        int nextDogId =
            viewModel.Dogs.Count == 0
                ? 1
                : viewModel.Dogs.Max(dog => dog.DogId) + 1;


        AddDogDialog dialog =
            new(ownerName, nextDogId);


        if (Window.GetWindow(this) is Window ownerWindow)
        {
            dialog.Owner = ownerWindow;
        }


        bool? result =
            dialog.ShowDialog();


        if (result == true &&
            dialog.CreatedDog is not null)
        {
            viewModel.Dogs.Add(
                dialog.CreatedDog);
        }
    }
}
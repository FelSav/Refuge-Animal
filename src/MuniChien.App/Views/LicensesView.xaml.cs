using MuniChien.App.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MuniChien.App.Views;

public partial class LicensesView : UserControl
{
    public LicensesView()
    {
        InitializeComponent();
    }


    // ==================================================
    // ENTRÉE = RECHERCHER
    // ==================================================

    private void SearchPanel_PreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }


        if (DataContext is not LicensesViewModel viewModel)
        {
            return;
        }


        e.Handled =
            true;


        if (viewModel.SearchCommand.CanExecute(null))
        {
            viewModel.SearchCommand.Execute(null);
        }
    }


    // ==================================================
    // DOUBLE-CLIC
    // ==================================================

    private void LicensesDataGrid_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        DependencyObject? source =
            e.OriginalSource as DependencyObject;


        // Un double-clic sur le bouton Voir ne doit
        // pas déclencher deux navigations.

        if (FindVisualParent<Button>(
                source) is not null)
        {
            return;
        }


        DataGridRow? row =
            FindVisualParent<DataGridRow>(
                source);


        if (row?.Item is not LicenseListItemViewModel license)
        {
            return;
        }


        e.Handled =
            true;


        OpenLicenseSafely(
            license);
    }


    // ==================================================
    // BOUTON VOIR
    // ==================================================

    private void ViewLicense_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }


        if (button.DataContext is not LicenseListItemViewModel license)
        {
            return;
        }


        e.Handled =
            true;


        OpenLicenseSafely(
            license);
    }


    // ==================================================
    // NAVIGATION SÉCURISÉE
    //
    // On attend la fin de l'événement actuel avant
    // de remplacer la vue contenant le DataGrid.
    // ==================================================

    private void OpenLicenseSafely(
        LicenseListItemViewModel license)
    {
        Dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(
                () =>
                {
                    if (DataContext is not LicensesViewModel viewModel)
                    {
                        return;
                    }


                    viewModel.SelectedLicense =
                        license;


                    if (viewModel.OpenSelectedLicenseCommand.CanExecute(null))
                    {
                        viewModel.OpenSelectedLicenseCommand.Execute(null);
                    }
                }));
    }


    // ==================================================
    // RECHERCHE D'UN PARENT VISUEL
    // ==================================================

    private static T?
        FindVisualParent<T>(
            DependencyObject? child)
        where T : DependencyObject
    {
        DependencyObject? current =
            child;


        while (current is not null)
        {
            if (current is T target)
            {
                return target;
            }


            current =
                VisualTreeHelper.GetParent(
                    current);
        }


        return null;
    }
}
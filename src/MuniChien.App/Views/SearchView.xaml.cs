using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MuniChien.App.ViewModels;

namespace MuniChien.App.Views;

public partial class SearchView : UserControl
{
    public SearchView() => InitializeComponent();

    private void DogResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!IsDataRow(e.OriginalSource as DependencyObject)) return;
        if (DataContext is SearchViewModel vm && vm.HasSelectedDog)
            vm.OpenDogCommand.Execute(null);
    }

    private void OwnerResultsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!IsDataRow(e.OriginalSource as DependencyObject)) return;
        if (DataContext is SearchViewModel vm && vm.HasSelectedOwner)
            vm.OpenOwnerCommand.Execute(null);
    }

    // Double-cliquer sur l'entête d'une colonne ne doit pas ouvrir la fiche.
    private static bool IsDataRow(DependencyObject? element)
    {
        while (element is not null)
        {
            if (element is DataGridRow) return true;
            if (element is DataGridColumnHeader) return false;
            // Les éléments de texte ne sont pas toujours des Visual.
            if (element is Visual)
                element = VisualTreeHelper.GetParent(element);
            else if (element is FrameworkContentElement content)
                element = content.Parent;
            else
                return false;
        }
        return false;
    }
}

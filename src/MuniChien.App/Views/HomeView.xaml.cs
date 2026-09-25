using MuniChien.App.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MuniChien.App.Views;

public partial class HomeView : UserControl
{
    private Point _dragStartPoint;
    private DashboardCardViewModel? _draggedCard;

    public HomeView()
    {
        InitializeComponent();
    }

    private void DashboardCard_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);

        if (sender is FrameworkElement element)
        {
            _draggedCard = element.DataContext as DashboardCardViewModel;
        }
    }

    private void DashboardCard_PreviewMouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed ||
            _draggedCard is null)
        {
            return;
        }

        Point currentPosition = e.GetPosition(null);

        Vector difference = _dragStartPoint - currentPosition;

        bool movedEnough =
            Math.Abs(difference.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(difference.Y) > SystemParameters.MinimumVerticalDragDistance;

        if (!movedEnough)
        {
            return;
        }

        DragDrop.DoDragDrop(
            (DependencyObject)sender,
            _draggedCard,
            DragDropEffects.Move);

        _draggedCard = null;
    }

    private void DashboardCard_DragOver(
        object sender,
        DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(DashboardCardViewModel)))
        {
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void DashboardCard_Drop(
        object sender,
        DragEventArgs e)
    {
        if (sender is not FrameworkElement targetElement)
        {
            return;
        }

        DashboardCardViewModel? targetCard =
            targetElement.DataContext as DashboardCardViewModel;

        DashboardCardViewModel? draggedCard =
            e.Data.GetData(typeof(DashboardCardViewModel))
            as DashboardCardViewModel;

        if (targetCard is null ||
            draggedCard is null ||
            targetCard == draggedCard)
        {
            return;
        }

        if (DataContext is not HomeViewModel viewModel)
        {
            return;
        }

        int oldIndex = viewModel.DashboardCards.IndexOf(draggedCard);
        int newIndex = viewModel.DashboardCards.IndexOf(targetCard);

        if (oldIndex < 0 || newIndex < 0)
        {
            return;
        }

        viewModel.DashboardCards.Move(oldIndex, newIndex);
        viewModel.SaveDashboardOrder();
    }
}
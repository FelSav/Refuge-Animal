using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MuniChien.App.ViewModels;

namespace MuniChien.App.Views;

public partial class HomeView : UserControl
{
    private Point _dragStartPoint;
    private Point _dragOffset;

    private DashboardCardViewModel? _draggedCard;
    private Border? _draggedBorder;

    private DragPreviewAdorner? _dragPreview;
    private AdornerLayer? _adornerLayer;

    private bool _layoutChangedDuringDrag;


    public HomeView()
    {
        InitializeComponent();
    }


    // ==================================================
    // DÉBUT DU DRAG
    // ==================================================

    private void DashboardCard_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        _dragStartPoint =
            e.GetPosition(RootGrid);

        if (sender is not Border border ||
            border.DataContext is not DashboardCardViewModel card)
        {
            return;
        }

        _draggedBorder = border;
        _draggedCard = card;

        // Permet à la carte visuelle de rester exactement
        // sous l'endroit où l'utilisateur l'a saisie.
        _dragOffset =
            e.GetPosition(border);
    }


    // ==================================================
    // DÉCLENCHEMENT DU DRAG
    // ==================================================

    private void DashboardCard_PreviewMouseMove(
        object sender,
        MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed ||
            _draggedCard is null ||
            _draggedBorder is null)
        {
            return;
        }

        Point currentPosition =
            e.GetPosition(RootGrid);

        Vector difference =
            currentPosition - _dragStartPoint;

        bool movedEnough =
            Math.Abs(difference.X) >
            SystemParameters.MinimumHorizontalDragDistance
            ||
            Math.Abs(difference.Y) >
            SystemParameters.MinimumVerticalDragDistance;

        if (!movedEnough)
        {
            return;
        }

        _layoutChangedDuringDrag = false;

        ShowDragPreview(_draggedBorder);

        // La vraie carte reste dans le layout,
        // mais devient discrète pendant le déplacement.
        _draggedBorder.Opacity = 0.05;

        try
        {
            DragDrop.DoDragDrop(
                _draggedBorder,
                _draggedCard,
                DragDropEffects.Move);
        }
        finally
        {
            _draggedBorder.Opacity = 1;

            HideDragPreview();

            if (_layoutChangedDuringDrag &&
                DataContext is HomeViewModel viewModel)
            {
                viewModel.SaveDashboardLayout();
            }

            _draggedCard = null;
            _draggedBorder = null;

            _layoutChangedDuringDrag = false;
        }
    }


    // ==================================================
    // LA CARTE VISUELLE SUIT LA SOURIS
    // ==================================================

    private void DashboardRoot_DragOver(
        object sender,
        DragEventArgs e)
    {
        if (_draggedCard is null ||
            !e.Data.GetDataPresent(
                typeof(DashboardCardViewModel)))
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        Point mousePosition =
            e.GetPosition(RootGrid);

        _dragPreview?.UpdatePosition(
            mousePosition.X - _dragOffset.X,
            mousePosition.Y - _dragOffset.Y);

        e.Effects =
            DragDropEffects.Move;

        e.Handled = true;
    }


    // ==================================================
    // RÉORDONNANCEMENT EN DIRECT
    // ==================================================

    private void DashboardCard_DragEnter(
        object sender,
        DragEventArgs e)
    {
        if (_draggedCard is null)
        {
            return;
        }

        if (sender is not Border targetBorder ||
            targetBorder.DataContext is not DashboardCardViewModel targetCard)
        {
            return;
        }

        if (targetCard == _draggedCard)
        {
            return;
        }

        if (DataContext is not HomeViewModel viewModel)
        {
            return;
        }

        int oldIndex =
            viewModel.DashboardCards.IndexOf(
                _draggedCard);

        int newIndex =
            viewModel.DashboardCards.IndexOf(
                targetCard);

        if (oldIndex < 0 ||
            newIndex < 0 ||
            oldIndex == newIndex)
        {
            return;
        }

        // C'est cette ligne qui fait "pousser"
        // les autres cartes pendant le déplacement.
        viewModel.DashboardCards.Move(
            oldIndex,
            newIndex);

        _layoutChangedDuringDrag = true;

        e.Effects =
            DragDropEffects.Move;
    }


    // ==================================================
    // FIN DU DRAG
    // ==================================================

    private void DashboardRoot_Drop(
        object sender,
        DragEventArgs e)
    {
        if (_draggedCard is null)
        {
            return;
        }

        e.Effects =
            DragDropEffects.Move;

        e.Handled = true;
    }


    // ==================================================
    // APERÇU VISUEL DE LA CARTE
    // ==================================================

    private void ShowDragPreview(
        Border sourceBorder)
    {
        _adornerLayer =
            AdornerLayer.GetAdornerLayer(
                RootGrid);

        if (_adornerLayer is null)
        {
            return;
        }

        _dragPreview =
            new DragPreviewAdorner(
                RootGrid,
                sourceBorder);

        _adornerLayer.Add(
            _dragPreview);

        Point mousePosition =
            Mouse.GetPosition(
                RootGrid);

        _dragPreview.UpdatePosition(
            mousePosition.X - _dragOffset.X,
            mousePosition.Y - _dragOffset.Y);
    }


    private void HideDragPreview()
    {
        if (_dragPreview is not null &&
            _adornerLayer is not null)
        {
            _adornerLayer.Remove(
                _dragPreview);
        }

        _dragPreview = null;
        _adornerLayer = null;
    }


    // ==================================================
    // ADORNER
    // Copie visuelle de la carte qui suit la souris
    // ==================================================

    private sealed class DragPreviewAdorner : Adorner
    {
        private readonly Border _preview;
        private readonly TranslateTransform _translation = new();

        public DragPreviewAdorner(
            UIElement adornedElement,
            FrameworkElement source)
            : base(adornedElement)
        {
            IsHitTestVisible = false;

            DashboardCardViewModel? card =
                source.DataContext as DashboardCardViewModel;

            TextBlock title =
                new()
                {
                    Text = card?.Title ?? string.Empty,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(101, 112, 134))
                };

            TextBlock value =
                new()
                {
                    Text = card?.Value ?? string.Empty,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 24,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 6, 0, 0),
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(36, 36, 36))
                };

            StackPanel content =
                new()
                {
                    VerticalAlignment = VerticalAlignment.Center
                };

            content.Children.Add(title);
            content.Children.Add(value);

            _preview =
                new Border
                {
                    Width = source.ActualWidth,
                    Height = source.ActualHeight,

                    Background = Brushes.White,

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(232, 117, 36)),

                    BorderThickness =
                        new Thickness(2),

                    CornerRadius =
                        new CornerRadius(8),

                    Padding =
                        new Thickness(16),

                    Opacity = 0.96,

                    Child = content,

                    RenderTransform = _translation
                };

            AddVisualChild(_preview);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index));
            }

            return _preview;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _preview.Measure(
                new Size(
                    _preview.Width,
                    _preview.Height));

            return constraint;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _preview.Arrange(
                new Rect(
                    0,
                    0,
                    _preview.Width,
                    _preview.Height));

            return finalSize;
        }

        public void UpdatePosition(
            double left,
            double top)
        {
            _translation.X = left;
            _translation.Y = top;
        }
    }
}
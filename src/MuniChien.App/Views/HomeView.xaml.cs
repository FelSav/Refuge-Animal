using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MuniChien.App.ViewModels;
using System.Windows.Media.Animation;

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
            viewModel.DashboardCards.IndexOf(_draggedCard);

        int newIndex =
            viewModel.DashboardCards.IndexOf(targetCard);

        if (oldIndex < 0 ||
            newIndex < 0 ||
            oldIndex == newIndex)
        {
            return;
        }

        // On mémorise la position visuelle actuelle
        // de toutes les cartes AVANT le déplacement.
        Dictionary<DashboardCardViewModel, Point> oldPositions =
            CaptureDashboardCardPositions();

        // Changement réel de l'ordre.
        viewModel.DashboardCards.Move(
            oldIndex,
            newIndex);

        // Anime les autres cartes vers leur nouvelle position.
        AnimateDashboardReorder(oldPositions);

        _layoutChangedDuringDrag = true;

        e.Effects = DragDropEffects.Move;
        e.Handled = true;
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

    private Dictionary<DashboardCardViewModel, Point>
    CaptureDashboardCardPositions()
    {
        Dictionary<DashboardCardViewModel, Point> positions = [];

        if (DataContext is not HomeViewModel viewModel)
        {
            return positions;
        }

        foreach (DashboardCardViewModel card in viewModel.DashboardCards)
        {
            FrameworkElement? container =
                DashboardItemsControl
                    .ItemContainerGenerator
                    .ContainerFromItem(card)
                as FrameworkElement;

            if (container is null)
            {
                continue;
            }

            try
            {
                Point position =
                    container
                        .TransformToAncestor(RootGrid)
                        .Transform(new Point(0, 0));

                positions[card] = position;
            }
            catch (InvalidOperationException)
            {
                // Le conteneur peut momentanément être
                // détaché pendant une mise à jour du layout.
            }
        }

        return positions;
    }

    private void AnimateDashboardReorder(
    Dictionary<DashboardCardViewModel, Point> oldPositions)
    {
        if (DataContext is not HomeViewModel viewModel)
        {
            return;
        }

        // On enlève les anciennes animations afin de repartir
        // de positions de layout propres.
        foreach (DashboardCardViewModel card in viewModel.DashboardCards)
        {
            FrameworkElement? container =
                DashboardItemsControl
                    .ItemContainerGenerator
                    .ContainerFromItem(card)
                as FrameworkElement;

            if (container is null)
            {
                continue;
            }

            container.RenderTransform =
                Transform.Identity;
        }

        // Force WPF à calculer immédiatement
        // les nouvelles positions du UniformGrid.
        DashboardItemsControl.UpdateLayout();

        Duration duration =
            new(TimeSpan.FromMilliseconds(160));

        CubicEase easing =
            new()
            {
                EasingMode = EasingMode.EaseOut
            };

        foreach (DashboardCardViewModel card in viewModel.DashboardCards)
        {
            // La carte tenue par la souris possède déjà
            // son propre aperçu flottant.
            if (card == _draggedCard)
            {
                continue;
            }

            if (!oldPositions.TryGetValue(
                    card,
                    out Point oldPosition))
            {
                continue;
            }

            FrameworkElement? container =
                DashboardItemsControl
                    .ItemContainerGenerator
                    .ContainerFromItem(card)
                as FrameworkElement;

            if (container is null)
            {
                continue;
            }

            Point newPosition;

            try
            {
                newPosition =
                    container
                        .TransformToAncestor(RootGrid)
                        .Transform(new Point(0, 0));
            }
            catch (InvalidOperationException)
            {
                continue;
            }

            double offsetX =
                oldPosition.X - newPosition.X;

            double offsetY =
                oldPosition.Y - newPosition.Y;

            if (Math.Abs(offsetX) < 0.5 &&
                Math.Abs(offsetY) < 0.5)
            {
                continue;
            }

            TranslateTransform transform =
                new();

            container.RenderTransform =
                transform;

            DoubleAnimation xAnimation =
                new()
                {
                    From = offsetX,
                    To = 0,
                    Duration = duration,
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.Stop
                };

            DoubleAnimation yAnimation =
                new()
                {
                    From = offsetY,
                    To = 0,
                    Duration = duration,
                    EasingFunction = easing,
                    FillBehavior = FillBehavior.Stop
                };

            transform.BeginAnimation(
                TranslateTransform.XProperty,
                xAnimation);

            transform.BeginAnimation(
                TranslateTransform.YProperty,
                yAnimation);
        }
    }

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
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MuniChien.App.Views.Controls;

/// <summary>
/// Redirige la molette d'un tableau vers le défilement vertical de la page.
/// Le tableau garde ses colonnes et sa sélection, sans petit scroll vertical interne.
/// </summary>
public static class DirectoryScrollBehavior
{
    public static readonly DependencyProperty ForwardMouseWheelProperty =
        DependencyProperty.RegisterAttached(
            "ForwardMouseWheel",
            typeof(bool),
            typeof(DirectoryScrollBehavior),
            new PropertyMetadata(false, OnForwardMouseWheelChanged));

    public static void SetForwardMouseWheel(DependencyObject element, bool value)
        => element.SetValue(ForwardMouseWheelProperty, value);

    public static bool GetForwardMouseWheel(DependencyObject element)
        => (bool)element.GetValue(ForwardMouseWheelProperty);

    private static void OnForwardMouseWheelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        grid.PreviewMouseWheel -= ForwardWheelToPage;
        if (e.NewValue is true)
            grid.PreviewMouseWheel += ForwardWheelToPage;
    }

    private static void ForwardWheelToPage(object sender, MouseWheelEventArgs e)
    {
        if (sender is not DependencyObject element)
            return;

        // Seul le ScrollViewer *parent* du tableau doit défiler.
        DependencyObject? current = VisualTreeHelper.GetParent(element);
        while (current is not null && current is not ScrollViewer)
            current = VisualTreeHelper.GetParent(current);

        if (current is not ScrollViewer page || page.ScrollableHeight <= 0)
            return;

        // 20 pixels par ligne Windows; gère aussi les petites variations des pavés tactiles.
        int lines = SystemParameters.WheelScrollLines;
        double step = lines < 0 ? page.ViewportHeight : Math.Max(lines, 1) * 20.0;
        page.ScrollToVerticalOffset(page.VerticalOffset - (e.Delta / 120.0) * step);
        e.Handled = true;
    }
}

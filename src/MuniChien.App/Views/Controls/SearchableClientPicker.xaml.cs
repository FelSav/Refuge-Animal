using MuniChien.App.Services;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MuniChien.App.Views.Controls;

/// <summary>
/// Sélecteur réutilisable : recherche par nom, téléphone ou dossier.
/// Affiche au maximum 12 résultats; ne crée jamais 14 000 éléments visuels.
/// En mode API, ItemsSource pourra être remplacé par les résultats d'une recherche serveur.
/// </summary>
public partial class SearchableClientPicker : UserControl
{
    private bool _updatingText;
    private const int MaximumResults = 12;
    private readonly DispatcherTimer _typingDelay = new() { Interval = TimeSpan.FromMilliseconds(170) };

    public SearchableClientPicker()
    {
        InitializeComponent();
        _typingDelay.Tick += (_, _) => { _typingDelay.Stop(); UpdateMatches(); };
        Loaded += (_, _) => DisplayCurrentSelection();
        Unloaded += (_, _) => CloseResults();
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(SearchableClientPicker),
        new PropertyMetadata(null, (control, _) => ((SearchableClientPicker)control).CloseResults()));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(SearchableClientPicker),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (control, _) => ((SearchableClientPicker)control).DisplayCurrentSelection()));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private sealed record Match(object Item, string Name, string Details, string Phone, string CellPhone, string FileNumber);

    private void CloseResults()
    {
        _typingDelay.Stop();
        if (ResultsPopup is not null) ResultsPopup.IsOpen = false;
    }

    private void DisplayCurrentSelection()
    {
        if (SearchInput is null || HintText is null) return;
        _updatingText = true;
        SearchInput.Text = SelectedItem switch
        {
            OwnerDirectoryItem owner => owner.FullName,
            PaymentOwnerOption payment => payment.OwnerName,
            _ => string.Empty
        };
        SearchInput.CaretIndex = SearchInput.Text.Length;
        HintText.Visibility = string.IsNullOrEmpty(SearchInput.Text) ? Visibility.Visible : Visibility.Collapsed;
        _updatingText = false;
        CloseResults();
    }

    private static Match? ConvertItem(object item)
    {
        string name, phone, cell, file, municipality;
        switch (item)
        {
            case OwnerDirectoryItem owner:
                name = owner.FullName; phone = owner.Phone; cell = owner.CellPhone;
                file = owner.FileNumber; municipality = owner.Municipality;
                break;
            case PaymentOwnerOption payment:
                name = payment.OwnerName; phone = payment.Phone; cell = payment.CellPhone;
                file = payment.FileNumber; municipality = payment.Municipality;
                break;
            default:
                return null;
        }
        var phones = string.Join(" / ", new[] { phone, cell }.Where(x => !string.IsNullOrWhiteSpace(x)));
        var details = string.Join("  ·  ", new[] { $"#{file}", phones, municipality }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
        return new Match(item, name, details, phone, cell, file);
    }

    private void UpdateMatches()
    {
        if (SearchInput is null || ResultsList is null) return;
        var query = SearchInput.Text.Trim();
        if (query.Length < 2)
        {
            ResultsPopup.IsOpen = false;
            return;
        }
        var matches = (ItemsSource?.Cast<object>() ?? [])
            .Select(ConvertItem)
            .OfType<Match>()
            .Where(item => LocalClientSearchRules.Matches(query, item.Name,
                item.Phone, item.CellPhone, item.FileNumber))
            .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(MaximumResults + 1)
            .ToList();
        var more = matches.Count > MaximumResults;
        ResultsList.ItemsSource = matches.Take(MaximumResults).ToList();
        NoResults.Visibility = matches.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ResultsHint.Text = more ? "Premiers résultats — précisez votre recherche" :
            "Choisissez un dossier dans les résultats";
        ResultsPopup.IsOpen = true;
    }

    private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        HintText.Visibility = string.IsNullOrWhiteSpace(SearchInput.Text) ? Visibility.Visible : Visibility.Collapsed;
        if (!_updatingText)
        {
            _typingDelay.Stop();
            if (SearchInput.Text.Trim().Length < 2) CloseResults();
            else _typingDelay.Start();
        }
    }

    private void SearchInput_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        SearchInput.SelectAll();
    }

    private void SearchInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CloseResults();
            DisplayCurrentSelection();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && SearchInput.Text.Trim().Length >= 2)
        {
            _typingDelay.Stop();
            UpdateMatches();
            if (ResultsList.Items.Count > 0)
                SelectMatch((Match)ResultsList.Items[0]);
            e.Handled = true;
        }
    }

    private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultsList.SelectedItem is Match selected) SelectMatch(selected);
    }

    private void SelectMatch(Match match)
    {
        SelectedItem = match.Item;
        ResultsPopup.IsOpen = false;
        DisplayCurrentSelection();
    }

    private void ResultsPopup_Closed(object? sender, EventArgs e)
    {
        // Une recherche non sélectionnée ne remplace jamais le client actif.
        if (!SearchInput.IsKeyboardFocusWithin) DisplayCurrentSelection();
    }
}

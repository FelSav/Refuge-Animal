using MuniChien.App.Navigation;
using MuniChien.App.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.IO;

namespace MuniChien.App.ViewModels;

/// <summary>Gestion administrative des tarifs, exclusivement en démonstration locale.</summary>
public sealed class LicenseTariffsViewModel : ViewModelBase
{
    private readonly LocalLicenseTariffService _service = new();
    private readonly CultureInfo _currencyCulture = CultureInfo.GetCultureInfo("fr-CA");

    private string _filterMunicipality = "Toutes les municipalités";
    private string _filterYear = DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
    private LicenseTariffEntry? _selectedTariff;
    private Guid? _editingId;
    private string _municipality = MunicipalityCatalog.All[0];
    private string _year = DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
    private string _label = string.Empty;
    private string _amount = string.Empty;
    private string _statusMessage = "Aucun tarif n'est prédéfini. Entrez uniquement les montants confirmés par le refuge.";

    public LicenseTariffsViewModel()
    {
        MunicipalityOptions = ["Toutes les municipalités", .. MunicipalityCatalog.All];
        Municipalities = MunicipalityCatalog.All;
        SaveCommand = new RelayCommand(Save);
        NewCommand = new RelayCommand(New);
        DeleteCommand = new RelayCommand(Delete);
        Refresh();
    }

    public IReadOnlyList<string> MunicipalityOptions { get; }
    public IReadOnlyList<string> Municipalities { get; }
    public ObservableCollection<LicenseTariffEntry> VisibleTariffs { get; } = [];
    public ICommand SaveCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand DeleteCommand { get; }

    public string FilterMunicipality
    {
        get => _filterMunicipality;
        set
        {
            if (_filterMunicipality == value) return;
            _filterMunicipality = value ?? "Toutes les municipalités";
            OnPropertyChanged();
            Refresh();
        }
    }

    public string FilterYear
    {
        get => _filterYear;
        set
        {
            if (_filterYear == value) return;
            _filterYear = value ?? "";
            OnPropertyChanged();
            Refresh();
        }
    }

    public LicenseTariffEntry? SelectedTariff
    {
        get => _selectedTariff;
        set
        {
            if (_selectedTariff?.Id == value?.Id) return;
            _selectedTariff = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanDelete));
            if (value is null) return;

            _editingId = value.Id;
            Municipality = value.Municipality;
            Year = value.Year.ToString(CultureInfo.InvariantCulture);
            Label = value.Label;
            Amount = value.Amount.ToString("0.00", _currencyCulture);
            OnPropertyChanged(nameof(EditorTitle));
            StatusMessage = "Tarif sélectionné. Modifiez les champs, puis enregistrez.";
        }
    }

    public string Municipality
    {
        get => _municipality;
        set { if (_municipality == value) return; _municipality = value ?? ""; OnPropertyChanged(); }
    }

    public string Year
    {
        get => _year;
        set { if (_year == value) return; _year = value ?? ""; OnPropertyChanged(); }
    }

    public string Label
    {
        get => _label;
        set { if (_label == value) return; _label = value ?? ""; OnPropertyChanged(); }
    }

    public string Amount
    {
        get => _amount;
        set { if (_amount == value) return; _amount = value ?? ""; OnPropertyChanged(); }
    }

    public string EditorTitle => _editingId.HasValue ? "Modifier un tarif" : "Ajouter un tarif";
    public bool CanDelete => _selectedTariff is not null;
    public string ResultsDisplay => $"{VisibleTariffs.Count} tarif(s) configuré(s)";

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    private void Refresh()
    {
        IEnumerable<LicenseTariffEntry> matches = _service.GetAll();
        if (_filterMunicipality != "Toutes les municipalités")
            matches = matches.Where(x => x.Municipality == _filterMunicipality);

        if (!string.IsNullOrWhiteSpace(_filterYear))
        {
            if (int.TryParse(_filterYear.Trim(), NumberStyles.None,
                CultureInfo.InvariantCulture, out int year))
                matches = matches.Where(x => x.Year == year);
            else
                matches = [];
        }

        VisibleTariffs.Clear();
        foreach (var item in matches)
            VisibleTariffs.Add(item);
        OnPropertyChanged(nameof(ResultsDisplay));
    }

    private void New()
    {
        SelectedTariff = null;
        _editingId = null;
        Municipality = MunicipalityCatalog.All.Contains(_filterMunicipality, StringComparer.Ordinal)
            ? _filterMunicipality : MunicipalityCatalog.All[0];
        Year = int.TryParse(_filterYear, NumberStyles.None, CultureInfo.InvariantCulture, out int selectedYear)
            && selectedYear >= 1900 && selectedYear <= 2200
            ? selectedYear.ToString(CultureInfo.InvariantCulture)
            : DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
        Label = "";
        Amount = "";
        OnPropertyChanged(nameof(EditorTitle));
        StatusMessage = "Nouveau tarif : choisissez la municipalité, l'année, le libellé et le montant.";
    }

    private void Save()
    {
        if (!MunicipalityCatalog.All.Contains(Municipality, StringComparer.Ordinal))
        {
            ShowValidation("Sélectionnez une municipalité du catalogue.");
            return;
        }

        if (!int.TryParse(Year.Trim(), NumberStyles.None, CultureInfo.InvariantCulture,
                out int year) || year < 1900 || year > 2200)
        {
            ShowValidation("Entrez une année valide entre 1900 et 2200.");
            return;
        }

        string label = Label.Trim();
        if (label.Length < 1 || label.Length > 80)
        {
            ShowValidation("Le libellé doit contenir de 1 à 80 caractères.");
            return;
        }

        string numericAmount = Amount.Trim().Replace(',', '.');
        if (!decimal.TryParse(numericAmount,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out decimal amount)
            || amount < 0 || decimal.Round(amount, 2) != amount)
        {
            ShowValidation("Entrez un montant positif ou nul, avec au plus deux décimales (ex. 25,50).");
            return;
        }

        try
        {
            var entry = new LicenseTariffEntry
            {
                Id = _editingId ?? Guid.NewGuid(),
                Municipality = Municipality,
                Year = year,
                Label = label,
                Amount = amount,
                UpdatedAt = DateTimeOffset.Now
            };
            _service.Save(entry);
            // Afficher l'entrée enregistrée même si les filtres précédents l'excluaient.
            _filterMunicipality = entry.Municipality;
            _filterYear = entry.Year.ToString(CultureInfo.InvariantCulture);
            OnPropertyChanged(nameof(FilterMunicipality));
            OnPropertyChanged(nameof(FilterYear));
            New();
            Refresh();
            StatusMessage = $"Tarif « {entry.Label} » enregistré localement pour {entry.Municipality} ({entry.Year}).";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            ShowValidation($"Impossible d'enregistrer le tarif : {ex.Message}");
        }
    }

    private void Delete()
    {
        LicenseTariffEntry? selected = SelectedTariff;
        if (selected is null) return;

        if (MessageBox.Show(
                $"Supprimer définitivement le tarif « {selected.Label} » ({selected.Municipality}, {selected.Year}) ?",
                "Confirmer la suppression du tarif", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            _service.Delete(selected.Id);
            New();
            Refresh();
            StatusMessage = "Tarif supprimé du stockage local de démonstration.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowValidation($"Impossible de supprimer le tarif : {ex.Message}");
        }
    }

    private void ShowValidation(string message)
    {
        StatusMessage = message;
        MessageBox.Show(message, "Gestion des tarifs", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

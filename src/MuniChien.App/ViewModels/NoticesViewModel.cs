using MuniChien.App.Navigation;
using MuniChien.App.Services;
using MuniChien.App.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public sealed class NoticesViewModel : ViewModelBase
{
    public const string JanuaryCampaign = "Premier avis (janvier)";
    public const string MarchCampaign = "Rappel de paiement (mars)";

    private readonly INoticeService _noticeService;
    private string _selectedCampaign = JanuaryCampaign;
    private int _selectedYear = DateTime.Today.Year;
    private string _selectedMunicipality = "Toutes les municipalités";
    private string _ownerSearch = string.Empty;
    private string _operationMessage = string.Empty;

    public NoticesViewModel(INoticeService? noticeService = null)
    {
        _noticeService = noticeService ?? new LocalNoticeService();
        YearOptions = new[] { DateTime.Today.Year - 1, DateTime.Today.Year, DateTime.Today.Year + 1 };
        MunicipalityOptions = new[] { "Toutes les municipalités" }
            .Concat(MunicipalityCatalog.All).ToArray();

        ShowJanuaryCommand = new RelayCommand(() => SelectedCampaign = JanuaryCampaign);
        ShowMarchCommand = new RelayCommand(() => SelectedCampaign = MarchCampaign);
        RefreshCommand = new RelayCommand(RefreshCandidates);
        SelectAllCommand = new RelayCommand(() => SelectCandidates(true));
        DeselectAllCommand = new RelayCommand(() => SelectCandidates(false));
        PreviewCommand = new RelayCommand(OpenPreview);
        ConfigureTemplateCommand = new RelayCommand(OpenTemplateSettings);
        SimulateSentCommand = new RelayCommand(SimulateSent);

        RefreshCandidates();
    }

    public IReadOnlyList<int> YearOptions { get; }
    public IReadOnlyList<string> MunicipalityOptions { get; }
    public ObservableCollection<NoticeCandidateViewModel> Candidates { get; } = new();

    public string SelectedCampaign
    {
        get => _selectedCampaign;
        set
        {
            if (_selectedCampaign == value) return;
            _selectedCampaign = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsJanuarySelected));
            OnPropertyChanged(nameof(IsMarchSelected));
            RefreshCandidates();
        }
    }

    public bool IsJanuarySelected => SelectedCampaign == JanuaryCampaign;
    public bool IsMarchSelected => SelectedCampaign == MarchCampaign;

    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (_selectedYear == value) return;
            _selectedYear = value;
            OnPropertyChanged();
            RefreshCandidates();
        }
    }

    public string SelectedMunicipality
    {
        get => _selectedMunicipality;
        set
        {
            if (_selectedMunicipality == value) return;
            _selectedMunicipality = value;
            OnPropertyChanged();
            RefreshCandidates();
        }
    }

    public string OwnerSearch
    {
        get => _ownerSearch;
        set
        {
            if (_ownerSearch == value) return;
            _ownerSearch = value;
            OnPropertyChanged();
        }
    }

    public string OperationMessage
    {
        get => _operationMessage;
        private set
        {
            if (_operationMessage == value) return;
            _operationMessage = value;
            OnPropertyChanged();
        }
    }

    public int CandidateCount => Candidates.Count;
    public bool HasCandidates => CandidateCount > 0;
    public int SelectedCount => Candidates.Count(c => c.IsSelected);
    public int SentCount => Candidates.Count(c => c.WasSent);
    public string TotalBalanceDisplay => $"{Candidates.Sum(c => c.Balance):0.00} $";

    public ICommand ShowJanuaryCommand { get; }
    public ICommand ShowMarchCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand DeselectAllCommand { get; }
    public ICommand PreviewCommand { get; }
    public ICommand ConfigureTemplateCommand { get; }
    public ICommand SimulateSentCommand { get; }

    private void RefreshCandidates()
    {
        foreach (var previous in Candidates)
            previous.PropertyChanged -= CandidatePropertyChanged;
        Candidates.Clear();

        var source = _noticeService.GetCandidates(SelectedYear, SelectedCampaign);
        foreach (var data in source)
        {
            if (SelectedMunicipality != "Toutes les municipalités"
                && !string.Equals(data.Municipality, SelectedMunicipality, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrWhiteSpace(OwnerSearch)
                && !data.OwnerName.Contains(OwnerSearch.Trim(), StringComparison.OrdinalIgnoreCase)
                && !data.FileNumber.Contains(OwnerSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            var lastSent = _noticeService.GetLastSentDate(data.OwnerId, SelectedYear, SelectedCampaign);
            var item = new NoticeCandidateViewModel
            {
                OwnerId = data.OwnerId,
                FileNumber = data.FileNumber,
                OwnerName = data.OwnerName,
                Municipality = data.Municipality,
                DogsDescription = data.DogsDescription,
                Balance = data.Balance,
                AmountInvoiced = data.AmountInvoiced,
                PaymentsReceived = data.PaymentsReceived,
                LateFees = data.LateFees,
                Address = data.Address,
                Email = data.Email,
                Telephone = data.Telephone,
                CellPhone = data.CellPhone,
                Dogs = data.Dogs,
                WasSent = lastSent.HasValue,
                LastSentAt = lastSent,
                IsSelected = !lastSent.HasValue
            };
            item.PropertyChanged += CandidatePropertyChanged;
            Candidates.Add(item);
        }

        OperationMessage = string.Empty;
        RefreshStatistics();
    }

    private void CandidatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NoticeCandidateViewModel.IsSelected))
            OnPropertyChanged(nameof(SelectedCount));
    }

    private void SelectCandidates(bool isSelected)
    {
        foreach (var candidate in Candidates)
            candidate.IsSelected = isSelected;
    }

    private List<NoticeCandidateViewModel> GetSelection() =>
        Candidates.Where(candidate => candidate.IsSelected).ToList();

    private void OpenTemplateSettings()
    {
        try
        {
            var dialog = new NoticeTemplateSettingsDialog();
            if (Application.Current?.MainWindow is Window window)
                dialog.Owner = window;
            if (dialog.ShowDialog() == true)
                OperationMessage = "Modèle d'avis enregistré localement. Les trois paramètres sont communs à toutes les municipalités.";
        }
        catch (Exception ex)
        {
            OperationMessage = $"Impossible d'ouvrir la configuration : {ex.Message}";
        }
    }

    private void OpenPreview()
    {
        var selection = GetSelection();
        if (selection.Count == 0)
        {
            OperationMessage = "Sélectionne au moins un dossier avant de préparer un avis.";
            return;
        }

        try
        {
            var dialog = new NoticePreviewDialog(selection, SelectedCampaign, SelectedYear);
            if (Application.Current?.MainWindow is Window window)
                dialog.Owner = window;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            OperationMessage = $"Impossible de générer l'aperçu : {ex.Message}";
        }
    }

    private void SimulateSent()
    {
        var selection = GetSelection();
        if (selection.Count == 0)
        {
            OperationMessage = "Sélectionne au moins un dossier.";
            return;
        }

        var newNotices = selection.Where(candidate => !candidate.WasSent).ToList();
        if (newNotices.Count == 0)
        {
            OperationMessage = "Tous les avis sélectionnés ont déjà un envoi simulé pour cette campagne.";
            return;
        }

        var answer = MessageBox.Show(
            $"Marquer {newNotices.Count} avis comme envoyés dans la démonstration ?\n\n" +
            "Cela ne crée aucun envoi postal, ne modifie aucune donnée réelle et ne survivra pas au redémarrage.",
            "Simulation d'envoi — MuniChien", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes) return;

        foreach (var candidate in newNotices)
            _noticeService.RecordSent(candidate.OwnerId, SelectedYear, SelectedCampaign, DateTime.Now);

        RefreshCandidates();
        OperationMessage = $"{newNotices.Count} dossier(s) marqué(s) comme envoyé(s) en mode démonstration.";
    }

    private void RefreshStatistics()
    {
        OnPropertyChanged(nameof(CandidateCount));
        OnPropertyChanged(nameof(HasCandidates));
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(SentCount));
        OnPropertyChanged(nameof(TotalBalanceDisplay));
    }
}

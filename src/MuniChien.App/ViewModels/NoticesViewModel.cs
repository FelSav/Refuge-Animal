using MuniChien.App.Navigation;
using MuniChien.App.Printing;
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
        YearOptions = Enumerable.Range(DateTime.Today.Year - 4, 6).Reverse().ToArray();
        MunicipalityOptions = new[] { "Toutes les municipalités" }
            .Concat(MunicipalityCatalog.All).ToArray();

        ShowJanuaryCommand = new RelayCommand(() => SelectedCampaign = JanuaryCampaign);
        ShowMarchCommand = new RelayCommand(() => SelectedCampaign = MarchCampaign);
        RefreshCommand = new RelayCommand(RefreshCandidates);
        SelectAllCommand = new RelayCommand(() => SelectCandidates(true));
        DeselectAllCommand = new RelayCommand(() => SelectCandidates(false));
        PreviewCommand = new RelayCommand(OpenPreview);
        PreviewEnvelopesCommand = new RelayCommand(() => OpenMailingPreview(true));
        PreviewLabelsCommand = new RelayCommand(() => OpenMailingPreview(false));
        ConfigureTemplateCommand = new RelayCommand(OpenTemplateSettings);
        SimulateSentCommand = new RelayCommand(SimulateSent);
        ClearHistoryCommand = new RelayCommand(ClearDemoHistory);

        RefreshCandidates();
    }

    public IReadOnlyList<int> YearOptions { get; }
    public IReadOnlyList<string> MunicipalityOptions { get; }
    public ObservableCollection<NoticeCandidateViewModel> Candidates { get; } = new();
    public ObservableCollection<NoticeHistoryItemViewModel> History { get; } = new();
    public int HistoryCount => History.Count;
    public bool HasHistory => HistoryCount > 0;
    public int JanuaryHistoryCount => History.Count(h => h.Campaign == JanuaryCampaign);
    public int MarchHistoryCount => History.Count(h => h.Campaign == MarchCampaign);

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
    public ICommand PreviewEnvelopesCommand { get; }
    public ICommand PreviewLabelsCommand { get; }
    public ICommand ConfigureTemplateCommand { get; }
    public ICommand SimulateSentCommand { get; }
    public ICommand ClearHistoryCommand { get; }

    private void RefreshCandidates()
    {
        try
        {
            LoadCandidatesAndHistory();
        }
        catch (Exception ex)
        {
            OperationMessage = $"Impossible de charger les avis ou l'historique : {ex.Message}";
        }
    }

    private void LoadCandidatesAndHistory()
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

        RefreshHistory();
        OperationMessage = string.Empty;
        RefreshStatistics();
    }

    private void RefreshHistory()
    {
        History.Clear();
        var records = _noticeService.GetSentHistory()
            .Where(entry => entry.Year == SelectedYear);

        if (SelectedMunicipality != "Toutes les municipalités")
            records = records.Where(entry => string.Equals(
                entry.Municipality, SelectedMunicipality, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(OwnerSearch))
        {
            string search = OwnerSearch.Trim();
            records = records.Where(entry =>
                entry.OwnerName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || entry.FileNumber.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var record in records.OrderByDescending(entry => entry.SentAt))
            History.Add(new NoticeHistoryItemViewModel(record));

        OnPropertyChanged(nameof(HistoryCount));
        OnPropertyChanged(nameof(HasHistory));
        OnPropertyChanged(nameof(JanuaryHistoryCount));
        OnPropertyChanged(nameof(MarchHistoryCount));
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

    private void OpenMailingPreview(bool envelopes)
    {
        var selection = GetSelection();
        if (selection.Count == 0)
        {
            OperationMessage = "Sélectionne au moins un dossier avant de préparer un support postal.";
            return;
        }

        // Une adresse incomplète n'entre jamais dans le document imprimable.
        // Les autres dossiers peuvent quand même être prévisualisés, avec
        // confirmation explicite du nombre de dossiers exclus.
        var valid = new List<NoticeCandidateViewModel>();
        var invalid = new List<string>();
        foreach (var recipient in selection)
        {
            if (NoticeMailingDocumentBuilder.TryValidateRecipient(recipient, out string reason))
                valid.Add(recipient);
            else
                invalid.Add($"#{recipient.FileNumber} — {recipient.OwnerName} : {reason}");
        }

        if (valid.Count == 0)
        {
            OperationMessage = $"Aucune adresse postale complète parmi les {selection.Count} dossiers sélectionnés. " +
                "Vérifie la rue, la municipalité, la province et le code postal.";
            MessageBox.Show(string.Join("\n", invalid.Take(12)), "Adresses à compléter",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (invalid.Count > 0)
        {
            string details = string.Join("\n", invalid.Take(10));
            if (invalid.Count > 10)
                details += $"\n... et {invalid.Count - 10} autre(s).";
            MessageBoxResult answer = MessageBox.Show(
                $"{invalid.Count} dossier(s) exclus de l'impression postale car leur adresse est incomplète :\n\n" +
                details + "\n\n" +
                $"Continuer avec les {valid.Count} dossier(s) possédant une adresse complète ?",
                "Adresses postales à vérifier", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes)
                return;
        }

        try
        {
            var dialog = new NoticeMailingPreviewDialog(valid, envelopes);
            if (Application.Current?.MainWindow is Window window)
                dialog.Owner = window;
            dialog.ShowDialog();
            OperationMessage = invalid.Count == 0
                ? $"Support postal de démonstration préparé pour {valid.Count} dossier(s)."
                : $"Support postal préparé pour {valid.Count} dossier(s); {invalid.Count} exclu(s) (adresse incomplète).";
        }
        catch (Exception ex)
        {
            OperationMessage = $"Impossible de créer le support postal : {ex.Message}";
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
            $"Confirmer {newNotices.Count} envoi(s) SIMULÉ(S) pour {SelectedCampaign}, année {SelectedYear} ?\n\n" +
            "Il n'y a AUCUN envoi postal réel. L'historique de démonstration sera sauvegardé sur cet ordinateur.\n" +
            "La prévisualisation et l'impression ne marquent jamais un avis comme envoyé.",
            "Confirmation d'envois simulés — MuniChien", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes) return;

        int saved = 0;
        try
        {
            foreach (var candidate in newNotices)
            {
                if (_noticeService.WasSent(candidate.OwnerId, SelectedYear, SelectedCampaign))
                    continue;

                _noticeService.RecordSent(candidate.OwnerId, SelectedYear, SelectedCampaign, DateTime.Now);
                saved++;
            }
        }
        catch (Exception ex)
        {
            RefreshCandidates();
            OperationMessage = $"Enregistrement interrompu après {saved} dossier(s) : {ex.Message}";
            return;
        }

        RefreshCandidates();
        OperationMessage = $"{saved} envoi(s) SIMULÉ(S) enregistré(s) dans l'historique local. Aucun courrier n'a été envoyé.";
    }

    private void ClearDemoHistory()
    {
        try
        {
            if (_noticeService.GetSentHistory().Count == 0)
            {
                OperationMessage = "L'historique de démonstration est déjà vide.";
                return;
            }
        }
        catch (Exception ex)
        {
            OperationMessage = $"Impossible de lire l'historique : {ex.Message}";
            return;
        }

        var confirmation = MessageBox.Show(
            "Supprimer TOUT l'historique d'envois SIMULÉS enregistré sur cet ordinateur ?\n\n" +
            "Cette action ne touche pas aux dossiers propriétaires, paiements, licences ou documents imprimés.\n" +
            "Elle ne concerne pas les futurs avis officiels de l'API.",
            "Réinitialiser uniquement la démonstration", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
            return;

        try
        {
            _noticeService.ClearDemoHistory();
            RefreshCandidates();
            OperationMessage = "Historique de démonstration effacé. Aucun dossier réel n'a été modifié.";
        }
        catch (Exception ex)
        {
            OperationMessage = $"Impossible de réinitialiser l'historique : {ex.Message}";
        }
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

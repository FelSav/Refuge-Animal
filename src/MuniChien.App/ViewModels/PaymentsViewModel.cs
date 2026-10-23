using MuniChien.App.Navigation;
using MuniChien.App.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace MuniChien.App.ViewModels;

public class PaymentsViewModel : ViewModelBase
{
    // ==================================================
    // PROPRIÉTAIRE
    // ==================================================

    private PaymentOwnerOption?
        _selectedOwner;

    private readonly IPaymentService
        _paymentService;


    // ==================================================
    // NOUVEAU PAIEMENT
    // ==================================================

    private DateTime?
        _newPaymentDate =
        DateTime.Today;


    private string _amountInput =
        string.Empty;


    private bool _isMunicipalityPayment;

    private bool _isCheque;


    // ==================================================
    // RÉSUMÉ
    // ==================================================

    private decimal _amountDue;

    private decimal _lateFees;

    private decimal _totalPayments;

    private decimal _balance;


    // ==================================================
    // VALIDATION
    // ==================================================

    private string _validationMessage =
        string.Empty;


    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public PaymentsViewModel(
        IPaymentService? paymentService = null)
    {
        _paymentService =
            paymentService ?? new LocalPaymentService();


        RegisterPaymentCommand =
            new RelayCommand(
                RegisterPayment);

        CancelPaymentCommand =
            new RelayCommand(
                ResetPaymentForm);


        ReloadCommand = new RelayCommand(ReloadAccounts);
        ReloadAccounts();
    }


    // ==================================================
    // OPTIONS PROPRIÉTAIRES
    // ==================================================

    public DataLoadState LoadState { get; } = new();
    public ICommand ReloadCommand { get; }

    public ObservableCollection<PaymentOwnerOption>
        OwnerOptions
    { get; } = [];


    public PaymentOwnerOption?
        SelectedOwner
    {
        get =>
            _selectedOwner;

        set
        {
            if (_selectedOwner == value)
            {
                return;
            }


            _selectedOwner =
                value;


            OnPropertyChanged();

            OnPropertyChanged(
                nameof(OwnerName));

            OnPropertyChanged(
                nameof(FileNumber));

            OnPropertyChanged(
                nameof(Municipality));

            OnPropertyChanged(
                nameof(HasSelectedOwner));


            ValidationMessage =
                string.Empty;


            LoadSelectedAccountSafely();
        }
    }


    public bool HasSelectedOwner =>
        SelectedOwner is not null;


    public string OwnerName =>
        SelectedOwner?.OwnerName
        ?? "Aucun client sélectionné";


    public string FileNumber =>
        SelectedOwner is null
            ? "—"
            : $"#{SelectedOwner.FileNumber}";


    public string Municipality =>
        SelectedOwner?.Municipality
        ?? "—";


    // ==================================================
    // RÉSUMÉ DU COMPTE
    // ==================================================

    public string AmountDueDisplay =>
        FormatMoney(
            _amountDue);


    public string LateFeesDisplay =>
        FormatMoney(
            _lateFees);


    public string TotalPaymentsDisplay =>
        FormatMoney(
            _totalPayments);


    public string BalanceDisplay =>
        FormatMoney(
            _balance);


    public bool HasOutstandingBalance =>
        _balance > 0;


    // ==================================================
    // HISTORIQUE
    // ==================================================

    public ObservableCollection<PaymentListItemViewModel>
        PaymentHistory
    { get; } =
        [];


    public bool HasPaymentHistory =>
        PaymentHistory.Count > 0;


    // ==================================================
    // NOUVEAU PAIEMENT
    // ==================================================

    public DateTime? NewPaymentDate
    {
        get =>
            _newPaymentDate;

        set
        {
            if (_newPaymentDate == value)
            {
                return;
            }

            _newPaymentDate =
                value;

            OnPropertyChanged();
        }
    }


    public string AmountInput
    {
        get =>
            _amountInput;

        set
        {
            if (_amountInput == value)
            {
                return;
            }

            _amountInput =
                value;

            OnPropertyChanged();
        }
    }


    public bool IsMunicipalityPayment
    {
        get =>
            _isMunicipalityPayment;

        set
        {
            if (_isMunicipalityPayment == value)
            {
                return;
            }

            _isMunicipalityPayment =
                value;

            OnPropertyChanged();
        }
    }


    public bool IsCheque
    {
        get =>
            _isCheque;

        set
        {
            if (_isCheque == value)
            {
                return;
            }

            _isCheque =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // VALIDATION
    // ==================================================

    public string ValidationMessage
    {
        get =>
            _validationMessage;

        private set
        {
            if (_validationMessage == value)
            {
                return;
            }

            _validationMessage =
                value;

            OnPropertyChanged();
        }
    }


    // ==================================================
    // COMMANDES
    // ==================================================

    public ICommand RegisterPaymentCommand { get; }

    public ICommand CancelPaymentCommand { get; }


    // ==================================================
    // ENREGISTRER
    // ==================================================

    private void RegisterPayment()
    {
        ValidationMessage =
            string.Empty;


        if (SelectedOwner is null)
        {
            ValidationMessage =
                "Veuillez sélectionner un client.";

            return;
        }


        if (!NewPaymentDate.HasValue)
        {
            ValidationMessage =
                "Veuillez sélectionner une date de paiement.";

            return;
        }


        if (NewPaymentDate.Value.Date > DateTime.Today ||
            NewPaymentDate.Value.Date < new DateTime(2000, 1, 1))
        {
            ValidationMessage = "La date du paiement doit être comprise entre le 1er janvier 2000 et aujourd'hui.";
            return;
        }

        if (!TryParseAmount(AmountInput, out decimal amount))
        {
            ValidationMessage = "Entrez un montant valide, par exemple 25,50 $.";
            return;
        }
        try
        {
            LocalMoneyRules.ValidatePositiveAmount(amount);
        }
        catch (ArgumentException ex)
        {
            ValidationMessage = ex.Message;
            return;
        }

        try
        {
            _paymentService.AddPayment(
                SelectedOwner.OwnerId,
                NewPaymentDate.Value,
                amount,
                IsMunicipalityPayment,
                IsCheque);
            LoadSelectedAccountSafely();
            if (!LoadState.HasError) ResetPaymentForm();
        }
        catch (ArgumentException ex)
        {
            ValidationMessage = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            ValidationMessage = ex.Message;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Saisie paiement : {ex.GetType().Name}");
            ValidationMessage = "Le paiement n'a pas pu être confirmé. Vérifiez le dossier avant de réessayer.";
            LoadState.Fail("Impossible de confirmer l'état du compte. Actualisez avant une nouvelle saisie.");
        }
    }


    // ==================================================
    // ANNULER / RESET
    // ==================================================

    private void ResetPaymentForm()
    {
        NewPaymentDate =
            DateTime.Today;

        AmountInput =
            string.Empty;

        IsMunicipalityPayment =
            false;

        IsCheque =
            false;

        ValidationMessage =
            string.Empty;
    }


    // ==================================================
    // RAFRAÎCHIR PROPRIÉTAIRE
    // ==================================================

    private void ReloadAccounts()
    {
        int targetId = SelectedOwner?.OwnerId ?? 1;
        LoadState.Begin();
        try
        {
            var available = _paymentService.GetOwnerOptions();
            OwnerOptions.Clear();
            foreach (var owner in available) OwnerOptions.Add(owner);
            _selectedOwner = null;
            SelectedOwner = OwnerOptions.FirstOrDefault(o => o.OwnerId == targetId)
                ?? OwnerOptions.FirstOrDefault();
            if (SelectedOwner is null) RefreshCurrentOwner();
            if (!LoadState.HasError)
                LoadState.Complete(SelectedOwner is not null, "Aucun compte de paiement local disponible.");
        }
        catch (Exception ex) { ShowPaymentLoadError(ex); }
    }

    private void LoadSelectedAccountSafely()
    {
        LoadState.Begin();
        try
        {
            RefreshCurrentOwner();
            LoadState.Complete(SelectedOwner is not null, "Aucun compte sélectionné.");
        }
        catch (Exception ex) { ShowPaymentLoadError(ex); }
    }

    private void ShowPaymentLoadError(Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Lecture des paiements : {ex.GetType().Name}");
        OwnerOptions.Clear();
        _selectedOwner = null;
        OnPropertyChanged(nameof(SelectedOwner));
        OnPropertyChanged(nameof(HasSelectedOwner));
        OnPropertyChanged(nameof(OwnerName));
        OnPropertyChanged(nameof(FileNumber));
        OnPropertyChanged(nameof(Municipality));
        PaymentHistory.Clear();
        _amountDue = _lateFees = _totalPayments = _balance = 0m;
        RefreshSummaryBindings();
        OnPropertyChanged(nameof(HasPaymentHistory));
        LoadState.Fail("Impossible de charger les comptes ou leurs paiements. Veuillez réessayer.");
    }

    private void RefreshCurrentOwner()
    {
        PaymentHistory.Clear();


        if (SelectedOwner is null)
        {
            _amountDue =
                0m;

            _lateFees =
                0m;

            _totalPayments =
                0m;

            _balance =
                0m;


            RefreshSummaryBindings();
            OnPropertyChanged(nameof(HasPaymentHistory));

            return;
        }


        foreach (
            PaymentListItemViewModel payment
            in _paymentService
                .GetPaymentsForOwner(
                    SelectedOwner.OwnerId))
        {
            PaymentHistory.Add(
                payment);
        }


        PaymentAccountSnapshot snapshot =
            _paymentService
                .GetAccountSnapshot(
                    SelectedOwner.OwnerId);


        _amountDue =
            snapshot.AmountDue;

        _lateFees =
            snapshot.LateFees;

        _totalPayments =
            snapshot.TotalPayments;

        _balance =
            snapshot.Balance;


        RefreshSummaryBindings();


        OnPropertyChanged(
            nameof(HasPaymentHistory));
    }


    // ==================================================
    // RAFRAÎCHIR RÉSUMÉ
    // ==================================================

    private void RefreshSummaryBindings()
    {
        OnPropertyChanged(
            nameof(AmountDueDisplay));

        OnPropertyChanged(
            nameof(LateFeesDisplay));

        OnPropertyChanged(
            nameof(TotalPaymentsDisplay));

        OnPropertyChanged(
            nameof(BalanceDisplay));

        OnPropertyChanged(
            nameof(HasOutstandingBalance));
    }


    // ==================================================
    // MONTANT
    // ==================================================

    private static bool TryParseAmount(
        string input,
        out decimal amount)
    {
        string normalized =
            input
                .Trim()
                .Replace(
                    ',',
                    '.');


        return decimal.TryParse(
            normalized,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out amount);
    }


    // ==================================================
    // AFFICHAGE MONÉTAIRE
    // ==================================================

    private static string FormatMoney(
        decimal value)
    {
        return
            $"{value:0.00} $";
    }
}
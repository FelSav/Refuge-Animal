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

    public PaymentsViewModel()
    {
        OwnerOptions =
            LocalPaymentStore
                .GetOwnerOptions();


        RegisterPaymentCommand =
            new RelayCommand(
                RegisterPayment);

        CancelPaymentCommand =
            new RelayCommand(
                ResetPaymentForm);


        SelectedOwner =
            OwnerOptions
                .FirstOrDefault(
                    owner =>
                        owner.OwnerId == 1)
            ?? OwnerOptions
                .FirstOrDefault();
    }


    // ==================================================
    // OPTIONS PROPRIÉTAIRES
    // ==================================================

    public IReadOnlyList<PaymentOwnerOption>
        OwnerOptions
    { get; }


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


            RefreshCurrentOwner();
        }
    }


    public bool HasSelectedOwner =>
        SelectedOwner is not null;


    public string OwnerName =>
        SelectedOwner?.OwnerName
        ?? "Aucun propriétaire sélectionné";


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
                "Veuillez sélectionner un propriétaire.";

            return;
        }


        if (!NewPaymentDate.HasValue)
        {
            ValidationMessage =
                "Veuillez sélectionner une date de paiement.";

            return;
        }


        if (!TryParseAmount(
                AmountInput,
                out decimal amount) ||
            amount <= 0)
        {
            ValidationMessage =
                "Le montant doit être un nombre supérieur à 0.";

            return;
        }


        LocalPaymentStore.AddPayment(
            SelectedOwner.OwnerId,
            NewPaymentDate.Value,
            amount,
            IsMunicipalityPayment,
            IsCheque);


        RefreshCurrentOwner();


        ResetPaymentForm();
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

            return;
        }


        foreach (
            PaymentListItemViewModel payment
            in LocalPaymentStore
                .GetPaymentsForOwner(
                    SelectedOwner.OwnerId))
        {
            PaymentHistory.Add(
                payment);
        }


        PaymentAccountSnapshot snapshot =
            LocalPaymentStore
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
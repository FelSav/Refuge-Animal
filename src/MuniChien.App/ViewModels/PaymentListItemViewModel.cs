namespace MuniChien.App.ViewModels;

public class PaymentListItemViewModel
{
    public int OwnerId { get; init; }

    public string ReceiptNumber { get; init; } =
        string.Empty;

    public DateTime PaymentDate { get; init; }

    public decimal Amount { get; init; }

    public bool IsMunicipalityPayment { get; init; }

    public bool IsCheque { get; init; }

    public string LicenseNumber { get; init; } =
        string.Empty;


    // ==================================================
    // AFFICHAGE
    // ==================================================

    public string PaymentDateDisplay =>
        PaymentDate.ToString(
            "yyyy-MM-dd");


    public string AmountDisplay =>
        $"{Amount:0.00} $";


    public string MunicipalityPaymentDisplay =>
        IsMunicipalityPayment
            ? "Oui"
            : "Non";


    public string ChequeDisplay =>
        IsCheque
            ? "Oui"
            : "Non";
}
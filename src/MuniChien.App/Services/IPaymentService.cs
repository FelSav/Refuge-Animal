using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public interface IPaymentService
{
    IReadOnlyList<PaymentOwnerOption> GetOwnerOptions();

    IReadOnlyList<PaymentListItemViewModel> GetPaymentsForOwner(
        int ownerId);

    PaymentAccountSnapshot GetAccountSnapshot(
        int ownerId);

    PaymentListItemViewModel AddPayment(
        int ownerId,
        DateTime paymentDate,
        decimal amount,
        bool isMunicipalityPayment,
        bool isCheque,
        string licenseNumber = "");
}
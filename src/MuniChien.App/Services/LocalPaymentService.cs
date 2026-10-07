using MuniChien.App.ViewModels;

namespace MuniChien.App.Services;

public class LocalPaymentService : IPaymentService
{
    public IReadOnlyList<PaymentOwnerOption> GetOwnerOptions()
    {
        return LocalPaymentStore.GetOwnerOptions();
    }


    public IReadOnlyList<PaymentListItemViewModel> GetPaymentsForOwner(
        int ownerId)
    {
        return LocalPaymentStore.GetPaymentsForOwner(
            ownerId);
    }


    public PaymentAccountSnapshot GetAccountSnapshot(
        int ownerId)
    {
        return LocalPaymentStore.GetAccountSnapshot(
            ownerId);
    }


    public PaymentListItemViewModel AddPayment(
        int ownerId,
        DateTime paymentDate,
        decimal amount,
        bool isMunicipalityPayment,
        bool isCheque,
        string licenseNumber = "")
    {
        return LocalPaymentStore.AddPayment(
            ownerId,
            paymentDate,
            amount,
            isMunicipalityPayment,
            isCheque,
            licenseNumber);
    }
}
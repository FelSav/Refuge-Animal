using MuniChien.App.Views;
using System.Windows;

namespace MuniChien.App.Services;

public static class LicenseRenewalDialogService
{
    public static bool Show(
        string dogName,
        string ownerName,
        string licenseNumber,
        string currentStatus,
        DateTime currentExpirationDate,
        DateTime renewalDate,
        DateTime newExpirationDate,
        int coverageYear,
        out bool extendToNextYear,
        out decimal amountToBill)
    {
        RenewLicenseDialog dialog =
            new RenewLicenseDialog(
                dogName,
                ownerName,
                licenseNumber,
                currentStatus,
                currentExpirationDate,
                renewalDate,
                newExpirationDate,
                coverageYear);


        if (Application.Current?.MainWindow is Window mainWindow &&
            mainWindow.IsVisible)
        {
            dialog.Owner =
                mainWindow;
        }


        bool confirmed = dialog.ShowDialog() == true;
        extendToNextYear = confirmed && dialog.ExtendToNextYear;
        amountToBill = confirmed ? dialog.AmountToBill : 0m;
        return confirmed;
    }
}
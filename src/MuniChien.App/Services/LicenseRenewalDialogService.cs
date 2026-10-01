using MuniChien.App.Views;
using System.Windows;

namespace MuniChien.App.Services;

public static class LicenseRenewalDialogService
{
    public static bool Show(
        string dogName,
        string ownerName,
        string licenseNumber,
        DateTime currentExpirationDate,
        DateTime renewalDate,
        DateTime newExpirationDate)
    {
        RenewLicenseDialog dialog =
            new RenewLicenseDialog(
                dogName,
                ownerName,
                licenseNumber,
                currentExpirationDate,
                renewalDate,
                newExpirationDate);

        if (Application.Current?.MainWindow is Window mainWindow &&
            mainWindow.IsVisible)
        {
            dialog.Owner = mainWindow;
        }

        return dialog.ShowDialog() == true;
    }
}
using System.Windows;

namespace MuniChien.App.Views;

public partial class RenewLicenseDialog : Window
{
    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public RenewLicenseDialog(
        string dogName,
        string ownerName,
        string licenseNumber,
        DateTime currentExpirationDate,
        DateTime renewalDate,
        DateTime newExpirationDate)
    {
        InitializeComponent();


        DogName =
            dogName;

        OwnerName =
            ownerName;

        LicenseNumber =
            licenseNumber;

        CurrentExpirationDate =
            FormatDate(
                currentExpirationDate);

        RenewalDate =
            FormatDate(
                renewalDate);

        NewExpirationDate =
            FormatDate(
                newExpirationDate);


        DataContext =
            this;
    }


    // ==================================================
    // DONNÉES AFFICHÉES
    // ==================================================

    public string DogName { get; }

    public string OwnerName { get; }

    public string LicenseNumber { get; }

    public string CurrentExpirationDate { get; }

    public string RenewalDate { get; }

    public string NewExpirationDate { get; }


    // ==================================================
    // CONFIRMER
    // ==================================================

    private void Confirm_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult =
            true;

        Close();
    }


    // ==================================================
    // ANNULER
    // ==================================================

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult =
            false;

        Close();
    }


    // ==================================================
    // FORMAT
    // ==================================================

    private static string FormatDate(
        DateTime date)
    {
        return date.ToString(
            "yyyy-MM-dd");
    }
}
using System.Windows;

namespace MuniChien.App.Views;

public partial class RenewLicenseDialog : Window
{
    public RenewLicenseDialog(
        string dogName,
        string ownerName,
        string licenseNumber,
        string currentStatus,
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

        CurrentStatus =
            currentStatus;

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


    public string DogName { get; }

    public string OwnerName { get; }

    public string LicenseNumber { get; }

    public string CurrentStatus { get; }

    public string CurrentExpirationDate { get; }

    public string RenewalDate { get; }

    public string NewExpirationDate { get; }


    private void Confirm_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult =
            true;

        Close();
    }


    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult =
            false;

        Close();
    }


    private static string FormatDate(
        DateTime date)
    {
        return date.ToString(
            "yyyy-MM-dd");
    }
}
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

    public string NewExpirationDate { get; private set; }

    public bool ExtendToNextYear =>
        ExceptionalExtensionCheckBox.IsChecked == true;


    private void ExceptionalExtensionCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        int renewalYear = DateTime.ParseExact(
            RenewalDate,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture).Year;

        int expirationYear = renewalYear + (ExtendToNextYear ? 1 : 0);
        NewExpirationDate = FormatDate(new DateTime(expirationYear, 12, 31));
        ExpirationDateTextBlock.Text = NewExpirationDate;
    }
    private void Confirm_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ExtendToNextYear)
        {
            MessageBoxResult permissionConfirmation = MessageBox.Show(
                "Confirmez-vous que le refuge a expressément autorisé " +
                "la prolongation exceptionnelle jusqu'au 31 décembre de l'année suivante ?",
                "Prolongation exceptionnelle",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (permissionConfirmation != MessageBoxResult.Yes)
            {
                return;
            }
        }

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
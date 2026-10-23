using System.Globalization;
using MuniChien.App.Services;
using System.Windows;

namespace MuniChien.App.Views;

public partial class RenewLicenseDialog : Window
{
    public RenewLicenseDialog(
        string dogName, string ownerName, string licenseNumber,
        string currentStatus, DateTime currentExpirationDate,
        DateTime renewalDate, DateTime newExpirationDate, int coverageYear)
    {
        InitializeComponent();
        DogName = dogName;
        OwnerName = ownerName;
        LicenseNumber = licenseNumber;
        CurrentStatus = currentStatus;
        CurrentExpirationDate = FormatDate(currentExpirationDate);
        RenewalDate = FormatDate(renewalDate);
        CoverageYear = coverageYear;
        NewExpirationDate = FormatDate(newExpirationDate);
        DataContext = this;
        ExpirationDateTextBlock.Text = NewExpirationDate;
    }

    public string DogName { get; }
    public string OwnerName { get; }
    public string LicenseNumber { get; }
    public string CurrentStatus { get; }
    public string CurrentExpirationDate { get; }
    public string RenewalDate { get; }
    public int CoverageYear { get; }
    public string NewExpirationDate { get; private set; }
    public decimal AmountToBill { get; private set; }
    public bool ExtendToNextYear => ExceptionalExtensionCheckBox.IsChecked == true;

    private void ExceptionalExtensionCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        // Le tarif saisi couvre le renouvellement une seule fois.
        // La prolongation autorisée ne provoque aucun deuxième débit.
        NewExpirationDate = FormatDate(new DateTime(
            CoverageYear + (ExtendToNextYear ? 1 : 0), 12, 31));
        ExpirationDateTextBlock.Text = NewExpirationDate;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        string input = AmountTextBox.Text.Trim();
        bool valid = decimal.TryParse(input, NumberStyles.Number,
                         CultureInfo.GetCultureInfo("fr-CA"), out decimal amount)
                  || decimal.TryParse(input, NumberStyles.Number,
                         CultureInfo.InvariantCulture, out amount);
        if (!valid || amount <= 0m || amount > 100000m || !LocalMoneyRules.HasAtMostTwoFractionalDigits(amount))
        {
            MessageBox.Show("Saisissez un montant positif avec au plus deux décimales.",
                "Montant invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (ExtendToNextYear && MessageBox.Show(
                "Le refuge a-t-il expressément autorisé la prolongation exceptionnelle " +
                "jusqu'au 31 décembre de l'année suivante ?",
                "Autorisation exceptionnelle", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        if (MessageBox.Show(
            $"Renouveler la licence {LicenseNumber} pour {CoverageYear} " +
            $"(expiration : {NewExpirationDate}) ?\n\n" +
            $"Ajouter {amount:N2} $ au compte du propriétaire ?\n\n" +
            "Ce montant est saisi manuellement. Aucun tarif officiel n'est appliqué.",
            "Confirmer renouvellement et facturation", MessageBoxButton.YesNo,
            MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        AmountToBill = amount;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private static string FormatDate(DateTime date) => date.ToString("yyyy-MM-dd");
}

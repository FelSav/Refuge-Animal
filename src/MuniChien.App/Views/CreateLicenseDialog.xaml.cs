using MuniChien.App.ViewModels;
using System.Globalization;
using System.Windows;

namespace MuniChien.App.Views;

public partial class CreateLicenseDialog : Window
{
    public int DogId { get; private set; }
    public DateTime IssueDate { get; private set; }
    public decimal Amount { get; private set; }

    public CreateLicenseDialog(IReadOnlyList<OwnerDogViewModel> dogs)
    {
        InitializeComponent();
        DogCombo.ItemsSource = dogs.Select(d => new DogOption
        {
            DogId = d.DogId,
            Display = $"{d.DogName} — {d.OwnerName} (chien #{d.DogId})"
        }).ToList();
        DogCombo.SelectedIndex = 0;
        IssueDatePicker.SelectedDate = DateTime.Today;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DogCombo.SelectedValue is not int dogId || !IssueDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Choisissez un chien et une date.", "Champs requis",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DateTime date = IssueDatePicker.SelectedDate.Value.Date;
        if (date > DateTime.Today || date.Year < 2000)
        {
            MessageBox.Show("Sélectionnez une date d'émission valide (aujourd'hui ou avant).",
                "Date invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        string text = AmountTextBox.Text.Trim();
        bool valid = decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("fr-CA"), out decimal amount)
            || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        if (!valid || amount <= 0m || amount > 100000m || decimal.Round(amount, 2) != amount)
        {
            MessageBox.Show("Entrez un montant positif avec au plus deux décimales.",
                "Montant invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (MessageBox.Show(
            $"Créer cette licence et ajouter {amount:N2} $ au compte du propriétaire ?\n\n" +
            "Cette facturation est une simulation locale, sans tarif officiel automatique.",
            "Confirmer la facturation", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;
        DogId = dogId;
        IssueDate = date;
        Amount = amount;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private sealed class DogOption
    {
        public int DogId { get; init; }
        public string Display { get; init; } = string.Empty;
    }
}

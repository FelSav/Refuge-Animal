using MuniChien.App.Services;
using System.Windows;
using System.Windows.Controls;

namespace MuniChien.App.Views;

public partial class AddOwnerDialog : Window
{
    public AddOwnerDialog()
    {
        InitializeComponent();
        MunicipalityBox.ItemsSource = MunicipalityCatalog.All;
        FirstNameBox.Focus();
    }

    public OwnerDirectoryItem? CreatedOwner { get; private set; }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string firstName = FirstNameBox.Text.Trim();
        string lastName = LastNameBox.Text.Trim();
        if (firstName.Length == 0)
        {
            ShowError("Le prénom du propriétaire est obligatoire.", FirstNameBox);
            return;
        }
        if (lastName.Length == 0)
        {
            ShowError("Le nom du propriétaire est obligatoire.", LastNameBox);
            return;
        }

        if (MunicipalityBox.SelectedItem is not string municipality ||
            !MunicipalityCatalog.All.Contains(municipality))
        {
            ShowError("Veuillez sélectionner une municipalité dans la liste.", MunicipalityBox);
            return;
        }

        string email = EmailBox.Text.Trim();
        if (email.Length > 0 && (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@')))
        {
            ShowError("Veuillez saisir un courriel valide ou laisser ce champ vide.", EmailBox);
            return;
        }

        CreatedOwner = new OwnerDirectoryItem
        {
            // La création dans le service attribue les identifiants de démonstration.
            OwnerId = 0,
            FileNumber = string.Empty,
            FirstName = firstName,
            LastName = lastName,
            Municipality = municipality,
            Status = (StatusBox.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Inactif"
                ? "Inactif" : "Actif",
            Phone = PhoneBox.Text.Trim(),
            CellPhone = CellPhoneBox.Text.Trim(),
            Email = email,
            Address = AddressBox.Text.Trim(),
            Comments = CommentsBox.Text.Trim()
        };

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void ShowError(string message, Control field)
    {
        MessageBox.Show(message, "Information manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
        field.Focus();
    }
}

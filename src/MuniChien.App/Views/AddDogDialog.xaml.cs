using MuniChien.App.ViewModels;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace MuniChien.App.Views;

public partial class AddDogDialog : Window
{
    private readonly int _dogId;
    private readonly string _ownerName;


    private readonly List<string> _breedOptions =
    [
        "Berger allemand",
        "Berger australien",
        "Caniche",
        "Chihuahua",
        "Croisé",
        "Golden Retriever",
        "Husky",
        "Labrador",
        "Shih Tzu"
    ];


    private readonly List<string> _colorOptions =
    [
        "Beige",
        "Blanc",
        "Bleu merle",
        "Brun",
        "Doré",
        "Gris",
        "Noir",
        "Roux"
    ];


    public AddDogDialog(
        string ownerName,
        int dogId)
    {
        InitializeComponent();

        _ownerName = ownerName;
        _dogId = dogId;

        OwnerNameText.Text = ownerName;

        BreedComboBox.ItemsSource =
            _breedOptions;

        ColorComboBox.ItemsSource =
            _colorOptions;
    }


    public OwnerDogViewModel? CreatedDog { get; private set; }


    // ==================================================
    // AJOUT
    // ==================================================

    private void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        // Nom obligatoire
        string dogName =
            DogNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(dogName))
        {
            ShowValidationError(
                "Le nom du chien est obligatoire.",
                DogNameTextBox);

            return;
        }


        // Race contrôlée
        if (!TryGetCanonicalValue(
                BreedComboBox.Text,
                _breedOptions,
                out string breed))
        {
            MessageBox.Show(
                "Veuillez sélectionner une race existante dans la liste.",
                "Race invalide",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            BreedComboBox.Focus();

            return;
        }


        // Couleur contrôlée
        if (!TryGetCanonicalValue(
                ColorComboBox.Text,
                _colorOptions,
                out string color))
        {
            MessageBox.Show(
                "Veuillez sélectionner une couleur existante dans la liste.",
                "Couleur invalide",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            ColorComboBox.Focus();

            return;
        }


        // Âge
        if (!int.TryParse(
                AgeValueTextBox.Text.Trim(),
                out int ageValue) ||
            ageValue < 0)
        {
            ShowValidationError(
                "L'âge doit être un nombre valide.",
                AgeValueTextBox);

            return;
        }


        string ageUnit =
            GetComboBoxValue(
                AgeUnitComboBox);

        int ageMonths =
            ageUnit == "ans"
                ? ageValue * 12
                : ageValue;


        // Poids
        if (!TryParseWeight(
                WeightTextBox.Text,
                out double weightKg) ||
            weightKg <= 0)
        {
            ShowValidationError(
                "Le poids doit être un nombre valide supérieur à 0.",
                WeightTextBox);

            return;
        }


        // Création
        CreatedDog = new OwnerDogViewModel
        {
            DogId = _dogId,

            DogName = dogName,

            OwnerName = _ownerName,

            Breed = breed,

            AgeMonths = ageMonths,

            WeightKg = weightKg,

            Color = color,

            Sex =
                GetComboBoxValue(
                    SexComboBox),

            Status =
                GetComboBoxValue(
                    StatusComboBox),

            Sterilized =
                GetComboBoxValue(
                    SterilizedComboBox),

            LicenseNumber =
                LicenseTextBox.Text.Trim()
        };


        DialogResult = true;

        Close();
    }


    // ==================================================
    // ANNULER
    // ==================================================

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;

        Close();
    }


    // ==================================================
    // NORMALISATION DES CHOIX
    // ==================================================

    private static bool TryGetCanonicalValue(
        string input,
        IEnumerable<string> options,
        out string canonicalValue)
    {
        string normalizedInput =
            input.Trim();


        string? match =
            options.FirstOrDefault(
                option =>
                    option.Equals(
                        normalizedInput,
                        StringComparison.OrdinalIgnoreCase));


        if (match is null)
        {
            canonicalValue =
                string.Empty;

            return false;
        }


        canonicalValue =
            match;

        return true;
    }


    // ==================================================
    // POIDS
    // ==================================================

    private static bool TryParseWeight(
        string input,
        out double weight)
    {
        string normalized =
            input
                .Trim()
                .Replace(',', '.');


        return double.TryParse(
            normalized,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out weight);
    }


    // ==================================================
    // COMBOBOX
    // ==================================================

    private static string GetComboBoxValue(
        ComboBox comboBox)
    {
        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            return selectedItem.Content?.ToString()
                ?? string.Empty;
        }

        return comboBox.Text.Trim();
    }


    // ==================================================
    // ERREUR
    // ==================================================

    private static void ShowValidationError(
        string message,
        Control control)
    {
        MessageBox.Show(
            message,
            "Information invalide",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        control.Focus();
    }
}
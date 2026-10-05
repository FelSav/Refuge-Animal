using System.Windows;

namespace MuniChien.App.Views;

public partial class AnnualCloseDialog : Window
{
    // ==================================================
    // CONSTRUCTEUR
    // ==================================================

    public AnnualCloseDialog(
        int currentYear,
        int nextYear,
        string lastBackup)
    {
        InitializeComponent();


        CurrentYear =
            currentYear.ToString();

        NextYear =
            nextYear.ToString();

        LastBackup =
            lastBackup;


        DataContext =
            this;
    }


    // ==================================================
    // DONNÉES
    // ==================================================

    public string CurrentYear { get; }

    public string NextYear { get; }

    public string LastBackup { get; }


    // ==================================================
    // CONFIRMATION
    // ==================================================

    private void ConfirmationCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        if (ConfirmButton is null)
        {
            return;
        }


        ConfirmButton.IsEnabled =
            ConfirmationCheckBox
                .IsChecked == true;
    }


    // ==================================================
    // CONFIRMER
    // ==================================================

    private void Confirm_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ConfirmationCheckBox
                .IsChecked != true)
        {
            return;
        }


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
}
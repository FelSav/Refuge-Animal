using MuniChien.App.Services;
using MuniChien.App.Printing;
using System;
using System.Windows;

namespace MuniChien.App.Views;

public partial class NoticeTemplateSettingsDialog : Window
{
    public NoticeTemplateSettingsDialog()
    {
        InitializeComponent();
        var settings = NoticeTemplateSettingsStore.Load();
        RegulationBox.Text = settings.RegulationText;
        DeadlinePicker.SelectedDate = settings.PaymentDeadline;
        InstructionsBox.Text = settings.PaymentInstructionsText;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string regulation = RegulationBox.Text.Trim();
        string instructions = InstructionsBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(regulation) || string.IsNullOrWhiteSpace(instructions))
        {
            ValidationText.Text = "Les deux textes sont obligatoires.";
            return;
        }

        try
        {
            // Valide également que les paragraphes entreront dans la page imprimée.
            NoticePrintDocumentBuilder.ValidateTextLengths(regulation, instructions);
            NoticeTemplateSettingsStore.Save(new NoticeTemplateSettings
            {
                RegulationText = regulation,
                PaymentDeadline = DeadlinePicker.SelectedDate,
                PaymentInstructionsText = instructions
            });
            DialogResult = true;
        }
        catch (Exception ex)
        {
            ValidationText.Text = $"Impossible d'enregistrer le modèle : {ex.Message}";
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}

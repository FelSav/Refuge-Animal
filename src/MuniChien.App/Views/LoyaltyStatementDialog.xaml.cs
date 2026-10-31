using MuniChien.App.Printing;
using MuniChien.App.Services;
using System;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MuniChien.App.Views;

public partial class LoyaltyStatementDialog : Window
{
    private readonly LoyaltyStatementSnapshot _statement;
    private bool _ready;

    public LoyaltyStatementDialog(LoyaltyStatementSnapshot statement)
    {
        InitializeComponent();
        _statement = statement;
        SubtitleText.Text = $"{statement.Client.FullName} · {statement.ProgramName} · Cycle {statement.CycleNumber}";
        StatusText.Text = statement.IsArchived ? "Cadeau remis" : statement.IsProvisional ? "Provisoire" : "Récompense disponible";
        var count = LoyaltyStatementDocumentBuilder.AttachmentCount(statement);
        AttachmentHintText.Text = count == 0
            ? "Aucune image de facture associée aux achats de ce cycle."
            : $"{count} justificatif(s) associé(s). L'ajout d'images peut créer plusieurs pages.";
        IncludeImagesBox.IsEnabled = count > 0;
        _ready = true;
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        try
        {
            PreviewReader.Document = LoyaltyStatementDocumentBuilder.Build(_statement,
                IncludeImagesBox.IsChecked == true);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Impossible de préparer le relevé : {ex.Message}",
                "Aperçu indisponible", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Attachments_Changed(object sender, RoutedEventArgs e)
    {
        if (_ready) RefreshPreview();
    }

    private void Print_Click(object sender, RoutedEventArgs e) => Print(false);
    private void Pdf_Click(object sender, RoutedEventArgs e) => Print(true);

    private void Print(bool pdf)
    {
        try
        {
            var dialog = new PrintDialog();
            if (pdf)
            {
                PrintQueue? pdfPrinter = null;
                using (var server = new LocalPrintServer())
                    pdfPrinter = server.GetPrintQueues()
                        .FirstOrDefault(q => q.Name.Equals("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase));
                if (pdfPrinter is null)
                {
                    MessageBox.Show(this,
                        "L'imprimante Windows « Microsoft Print to PDF » est introuvable. " +
                        "Activez-la dans les fonctionnalités facultatives de Windows, ou utilisez « Imprimer » " +
                        "et choisissez une autre imprimante PDF.",
                        "Export PDF indisponible", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                dialog.PrintQueue = pdfPrinter;
            }
            if (dialog.ShowDialog() != true) return;
            if (dialog.PrintableAreaWidth <= 0 || dialog.PrintableAreaHeight <= 0)
                throw new InvalidOperationException("Zone imprimable non disponible.");
            // Nouveau document : un FlowDocument ne peut appartenir à deux lecteurs.
            var doc = LoyaltyStatementDocumentBuilder.Build(_statement,
                IncludeImagesBox.IsChecked == true, dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
            dialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator,
                $"Relevé fidélité — {_statement.ProgramName} — {_statement.Client.FullName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"L'impression a échoué : {ex.Message}",
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}

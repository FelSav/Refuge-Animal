using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MuniChien.App.Services;

/// <summary>
/// Impression ponctuelle des commentaires consignés dans les fiches.
/// Les données imprimées proviennent du service de répertoire et non du formulaire en cours d'édition.
/// L'API remplacera le stockage, mais pas le document imprimable.
/// </summary>
public static class CommentPrintService
{
    public static void Print(
        string title,
        IReadOnlyList<(string Label, string Value)> details,
        string? comments)
    {
        try
        {
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() != true) return;

            double width = dialog.PrintableAreaWidth;
            double height = dialog.PrintableAreaHeight;
            if (width <= 0 || height <= 0)
            {
                MessageBox.Show("Le format de papier de l'imprimante est invalide.",
                    "Impression des commentaires", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var document = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                Foreground = Brushes.Black,
                PageWidth = width,
                PageHeight = height,
                PagePadding = new Thickness(42),
                ColumnWidth = Math.Max(100, width - 84),
                ColumnGap = 0
            };

            document.Blocks.Add(new Paragraph(new Run("MuniChien — Le Refuge Animal"))
            {
                FontSize = 12,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 0, 0, 18)
            });

            document.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 18)
            });

            foreach (var detail in details)
            {
                var row = new Paragraph { Margin = new Thickness(0, 0, 0, 5) };
                row.Inlines.Add(new Bold(new Run(detail.Label + " : ")));
                row.Inlines.Add(new Run(string.IsNullOrWhiteSpace(detail.Value) ? "—" : detail.Value));
                document.Blocks.Add(row);
            }

            document.Blocks.Add(new Paragraph(new Run("Commentaires"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 28, 0, 12)
            });

            string content = string.IsNullOrWhiteSpace(comments)
                ? "Aucun commentaire enregistré."
                : comments;

            // Préserve les retours à la ligne des commentaires.
            var note = new Paragraph { Margin = new Thickness(0, 0, 0, 22) };
            string[] lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0) note.Inlines.Add(new LineBreak());
                note.Inlines.Add(new Run(lines[i]));
            }
            document.Blocks.Add(note);

            document.Blocks.Add(new Paragraph(new Run($"Imprimé le {DateTime.Now:yyyy-MM-dd} à {DateTime.Now:HH:mm} — Document interne"))
            {
                FontSize = 10,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 28, 0, 0)
            });

            IDocumentPaginatorSource source = document;
            dialog.PrintDocument(source.DocumentPaginator, title);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Impossible d'imprimer les commentaires.\n\n{ex.Message}",
                "Impression des commentaires", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

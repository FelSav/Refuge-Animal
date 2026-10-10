using MuniChien.App.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MuniChien.App.Services;

/// <summary>
/// Impression WPF des tableaux de rapport (imprimante ou Microsoft Print to PDF).
/// Le FlowDocument assure la pagination lorsque plusieurs pages sont nécessaires.
/// </summary>
public static class ReportPrintService
{
    public static void Print(
        Window owner,
        string title,
        string description,
        string municipality,
        string period,
        string generatedAt,
        string summary,
        IReadOnlyList<string> headers,
        IReadOnlyList<ReportPreviewRowViewModel> rows)
    {
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            double width = dialog.PrintableAreaWidth;
            double height = dialog.PrintableAreaHeight;
            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException("L'imprimante n'a pas fourni de zone imprimable valide.");
            }

            var document = new FlowDocument
            {
                PageWidth = width,
                PageHeight = height,
                PagePadding = new Thickness(28),
                ColumnWidth = double.PositiveInfinity,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 10,
                Foreground = Brushes.Black
            };

            document.Blocks.Add(new Paragraph(new Run("MuniChien — Le Refuge Animal"))
            {
                FontSize = 10,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 0, 0, 8)
            });
            document.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 19,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 7)
            });
            document.Blocks.Add(new Paragraph(new Run(description))
            {
                FontSize = 9,
                Margin = new Thickness(0, 0, 0, 10)
            });
            document.Blocks.Add(new Paragraph(new Run(
                $"Municipalité : {municipality}    |    Période : {period}    |    Généré le : {generatedAt}"))
            {
                FontSize = 9,
                Margin = new Thickness(0, 0, 0, 8)
            });

            if (!string.IsNullOrWhiteSpace(summary))
            {
                document.Blocks.Add(new Paragraph(new Run(summary))
                {
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 12)
                });
            }

            // Ne pas décaler les colonnes si certains entêtes sont vides.
            var indexes = Enumerable.Range(0, Math.Min(6, headers.Count))
                .Where(i => !string.IsNullOrWhiteSpace(headers[i]))
                .ToList();
            var table = new Table { CellSpacing = 0, FontSize = 8 };
            foreach (int _ in indexes)
            {
                table.Columns.Add(new TableColumn
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }
            var group = new TableRowGroup();
            table.RowGroups.Add(group);

            var header = new TableRow { Background = new SolidColorBrush(Color.FromRgb(243, 245, 248)) };
            foreach (int index in indexes)
            {
                header.Cells.Add(CreateCell(headers[index], bold: true));
            }
            group.Rows.Add(header);

            foreach (var row in rows)
            {
                var values = new[] { row.Value1, row.Value2, row.Value3, row.Value4, row.Value5, row.Value6 };
                var tableRow = new TableRow();
                foreach (int index in indexes)
                {
                    tableRow.Cells.Add(CreateCell(values[index]));
                }
                group.Rows.Add(tableRow);
            }

            if (rows.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("Aucune donnée pour les critères sélectionnés."))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                });
            }
            else
            {
                document.Blocks.Add(table);
            }

            document.Blocks.Add(new Paragraph(new Run(
                "Document interne — Données locales de démonstration. Ne constitue pas un rapport comptable officiel."))
            {
                FontSize = 8,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 18, 0, 0)
            });

            dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, title);
        }
        catch (Exception error)
        {
            MessageBox.Show(owner,
                $"L'impression du rapport a échoué : {error.Message}",
                "Erreur d'impression",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static TableCell CreateCell(string value, bool bold = false)
    {
        var paragraph = new Paragraph(new Run(value ?? string.Empty))
        {
            Margin = new Thickness(0),
            FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal
        };
        return new TableCell(paragraph)
        {
            Padding = new Thickness(3, 6, 3, 6),
            BorderBrush = new SolidColorBrush(Color.FromRgb(224, 228, 234)),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };
    }
}

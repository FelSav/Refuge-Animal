using MuniChien.App.Services;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MuniChien.App.Printing;

/// <summary>
/// Le même FlowDocument est construit pour l'aperçu, l'imprimante et Microsoft Print to PDF.
/// Les factures locales sont annexées uniquement sur demande (confidentialité).
/// </summary>
public static class LoyaltyStatementDocumentBuilder
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-CA");
    private static readonly Brush Ink = new SolidColorBrush(Color.FromRgb(36, 43, 56));
    private static readonly Brush Subtle = new SolidColorBrush(Color.FromRgb(99, 112, 132));
    private static readonly Brush Orange = new SolidColorBrush(Color.FromRgb(187, 85, 18));

    public static int AttachmentCount(LoyaltyStatementSnapshot statement) =>
        statement.Purchases.Count(p => p is not null && !string.IsNullOrWhiteSpace(p.AttachmentPath));

    public static FlowDocument Build(LoyaltyStatementSnapshot statement, bool includeImages,
        double pageWidth = 793.7, double pageHeight = 1122.5)
    {
        var doc = new FlowDocument
        {
            PageWidth = pageWidth, PageHeight = pageHeight,
            PagePadding = new Thickness(37, 30, 37, 40),
            ColumnWidth = double.PositiveInfinity,
            FontFamily = new FontFamily("Segoe UI"), FontSize = 10.5,
            Foreground = Ink,
            Background = Brushes.White
        };
        var blocks = doc.Blocks;
        blocks.Add(Text("REFUGE ANIMAL  ·  MuniChien", 10, Subtle, true, 5));
        blocks.Add(Text("Relevé détaillé de fidélité", 21, Ink, true, 4));
        blocks.Add(Text($"{statement.ProgramName}   •   Cycle n° {statement.CycleNumber}", 11, Orange, true, 13));
        blocks.Add(Text(statement.IsArchived ? "CYCLE TERMINÉ — CADEAU REMIS" : statement.IsProvisional
            ? "RELEVÉ PROVISOIRE — achats à compléter" : "12 ACHATS VÉRIFIÉS — cadeau à remettre", 10, Ink, true, 13));

        var owner = statement.Client;
        blocks.Add(Text("COORDONNÉES DU CLIENT", 10, Orange, true, 3));
        blocks.Add(Text($"{owner.FullName}   ·   Dossier n° {owner.FileNumber}", 12, Ink, true, 3));
        AddIfKnown(blocks, owner.Address);
        AddIfKnown(blocks, owner.Municipality);
        var phones = string.Join("   ·   ", new[] { owner.Phone, owner.CellPhone }
            .Where(p => !string.IsNullOrWhiteSpace(p)).Distinct());
        AddIfKnown(blocks, phones);
        AddIfKnown(blocks, owner.Email);
        blocks.Add(Text($"Cycle commencé : {statement.CycleStartedAt:dd/MM/yyyy}    |    Relevé préparé : {DateTime.Now:dd/MM/yyyy HH:mm}",
            9, Subtle, false, 13));

        blocks.Add(Text("HISTORIQUE DES 12 SACS", 11, Ink, true, 7));
        // Les largeurs fixes (en pixels WPF) empêchent les colonnes d'être écrasées
        // lors de l'export PDF. Les détails restent regroupés dans une grande colonne.
        var historyWidth = Math.Max(300, pageWidth - doc.PagePadding.Left - doc.PagePadding.Right - 14);
        blocks.Add(BuildHistoryTable(statement, historyWidth));
        blocks.Add(Text($"Achats documentés : {statement.PurchaseCount}/12" +
            (statement.LegacyCount > 0 ? $"  ·  Anciennes coches sans détail : {statement.LegacyCount}" : ""),
            9, Subtle, false, 13));

        blocks.Add(Text("13e SAC — RÉCOMPENSE", 11, Orange, true, 4));
        if (statement.Gift is { } gift)
        {
            var p = gift.Purchase;
            blocks.Add(Text($"Sac exact à remettre : {p.ProductName} — {p.Format}", 12, Ink, true, 3));
            blocks.Add(Text($"Référence : {p.ProductReference}   ·   Achat d'origine n° {gift.Index + 1}   ·   Prix payé : {p.PricePaid.ToString("C", French)}", 10, Ink, false, 3));
            blocks.Add(Text(statement.IsArchived
                ? $"Remise confirmée le {statement.RewardGivenAt:dd/MM/yyyy HH:mm}."
                : "Récompense disponible — remise à confirmer par un employé.",
                9.5, Subtle, false, 10));
        }
        else
        {
            blocks.Add(Text("Non déterminé : les 12 achats détaillés sont nécessaires pour choisir le sac exact.",
                10, Subtle, false, 10));
        }

        if (includeImages)
        {
            var attachments = statement.Purchases.Select((p, i) => new { Purchase = p, Number = i + 1 })
                .Where(x => !string.IsNullOrWhiteSpace(x.Purchase?.AttachmentPath)).ToList();
            if (attachments.Count > 0)
            {
                blocks.Add(Text("JUSTIFICATIFS JOINTS", 11, Ink, true, 7));
                foreach (var item in attachments)
                {
                    blocks.Add(Text($"Justificatif — sac n° {item.Number} / facture {SafeInvoice(item.Purchase!.InvoiceNumber)}", 10, Ink, true, 5));
                    var path = item.Purchase.AttachmentPath;
                    try
                    {
                        if (!File.Exists(path)) throw new FileNotFoundException();
                        var info = new FileInfo(path);
                        if (info.Length > 10 * 1024 * 1024) throw new IOException("Image supérieure à 10 Mo");
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(path, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.DecodePixelWidth = 1250;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        var image = new Image
                        {
                            Source = bitmap, Stretch = Stretch.Uniform,
                            MaxWidth = Math.Max(150, pageWidth - 110), MaxHeight = Math.Max(200, pageHeight - 160)
                        };
                        var section = new BlockUIContainer(image) { Margin = new Thickness(0, 0, 0, 14) };
                        blocks.Add(section);
                    }
                    catch (Exception)
                    {
                        blocks.Add(Text("Image indisponible sur ce poste — justificatif non inclus.", 9.5, Orange, false, 8));
                    }
                }
            }
        }
        else if (AttachmentCount(statement) > 0)
        {
            blocks.Add(Text($"{AttachmentCount(statement)} image(s) de facture associée(s), non incluses dans ce document.",
                8.5, Subtle, false, 8));
        }
        blocks.Add(Text("Document généré à partir des données locales de démonstration. Vérifier les achats et les justificatifs avant tout envoi.",
            8, Subtle, false, 0));
        return doc;
    }

    /// <summary>
    /// Historique compact : un achat par ligne, sur quatre colonnes lisibles.
    /// Les références et numéros de facture sont conservés, sous le nom du sac.
    /// </summary>
    private static Table BuildHistoryTable(LoyaltyStatementSnapshot statement, double availableWidth)
    {
        var table = new Table
        {
            CellSpacing = 0,
            FontSize = 10,
            Margin = new Thickness(0, 0, 0, 2)
        };
        // WPF FlowDocument : utiliser des largeurs absolues (Pixel), pas des poids Star.
        // Proportions : numéro 6 %, date 15 %, détails 65 %, prix 14 %.
        foreach (var ratio in new[] { 0.06, 0.15, 0.65, 0.14 })
            table.Columns.Add(new TableColumn { Width = new GridLength(availableWidth * ratio, GridUnitType.Pixel) });

        var group = new TableRowGroup();
        table.RowGroups.Add(group);
        var heading = new TableRow { Background = new SolidColorBrush(Color.FromRgb(242, 245, 249)) };
        heading.Cells.Add(HistoryCell("N°", true));
        heading.Cells.Add(HistoryCell("Date", true));
        heading.Cells.Add(HistoryCell("Sac / référence / facture", true));
        heading.Cells.Add(HistoryCell("Prix", true, TextAlignment.Right));
        group.Rows.Add(heading);

        for (var i = 0; i < LocalLoyaltyService.PurchaseCount; i++)
        {
            var purchase = statement.Purchases[i];
            var originalDate = statement.PurchaseDates[i];
            var isReward = statement.Gift?.Index == i;
            var row = new TableRow
            {
                Background = isReward
                    ? new SolidColorBrush(Color.FromRgb(255, 246, 235))
                    : i % 2 == 1
                        ? new SolidColorBrush(Color.FromRgb(250, 251, 253))
                        : Brushes.White
            };

            row.Cells.Add(HistoryCell((i + 1).ToString(CultureInfo.InvariantCulture), true));
            row.Cells.Add(HistoryCell(purchase?.PurchasedAt.ToString("dd/MM/yyyy")
                ?? originalDate?.ToString("dd/MM/yyyy") ?? "—"));
            row.Cells.Add(PurchaseDetailCell(purchase, originalDate.HasValue));
            row.Cells.Add(HistoryCell(purchase is null ? "—" : purchase.PricePaid.ToString("C", French),
                true, TextAlignment.Right));
            group.Rows.Add(row);
        }

        return table;
    }

    private static TableCell PurchaseDetailCell(LoyaltyPurchaseRecord? purchase, bool hasLegacyMark)
    {
        if (purchase is null)
            return HistoryCell(hasLegacyMark ? "Coche à compléter" : "Non enregistré");

        var firstLine = new Paragraph { Margin = new Thickness(0), FontSize = 10 };
        firstLine.Inlines.Add(new Run(purchase.ProductName) { FontWeight = FontWeights.SemiBold });
        if (!string.IsNullOrWhiteSpace(purchase.Format))
            firstLine.Inlines.Add(new Run("  ·  " + purchase.Format));

        var secondLine = new Paragraph
        {
            Margin = new Thickness(0), FontSize = 9,
            Foreground = Subtle
        };
        secondLine.Inlines.Add(new Run($"Réf. : {purchase.ProductReference}   ·   Facture : {SafeInvoice(purchase.InvoiceNumber)}"));

        var cell = new TableCell
        {
            Padding = new Thickness(5, 3.5, 5, 3.5),
            BorderBrush = new SolidColorBrush(Color.FromRgb(229, 233, 239)),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };
        cell.Blocks.Add(firstLine);
        cell.Blocks.Add(secondLine);
        return cell;
    }

    private static TableCell HistoryCell(string content, bool bold = false,
        TextAlignment alignment = TextAlignment.Left) => new(new Paragraph(new Run(content))
    {
        Margin = new Thickness(0),
        FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
        TextAlignment = alignment
    })
    {
        Padding = new Thickness(5, 4, 5, 4),
        BorderBrush = new SolidColorBrush(Color.FromRgb(229, 233, 239)),
        BorderThickness = new Thickness(0, 0, 0, 0.5)
    };

    private static string SafeInvoice(string? value) => string.IsNullOrWhiteSpace(value) ? "non précisée" : value;

    private static void AddIfKnown(BlockCollection blocks, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) blocks.Add(Text(value, 9.5, Ink, false, 2));
    }

    private static Paragraph Text(string content, double size, Brush brush, bool bold, double marginBottom) =>
        new(new Run(content))
        {
            FontSize = size, Foreground = brush,
            FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
            Margin = new Thickness(0, 0, 0, marginBottom)
        };

}

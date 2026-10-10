using MuniChien.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MuniChien.App.Printing;

/// <summary>
/// Supports postaux de DEMONSTRATION (enveloppes no 10 / étiquettes 3 x 10).
/// Une adresse incomplète n'est JAMAIS imprimée. Validation avant l'aperçu
/// ET au moment de construire les pages.
/// </summary>
public static class NoticeMailingDocumentBuilder
{
    private const double DipsPerInch = 96.0;
    private const double LetterWidth = 8.5 * DipsPerInch;
    private const double LetterHeight = 11.0 * DipsPerInch;
    private const double EnvelopeWidth = 9.5 * DipsPerInch;
    private const double EnvelopeHeight = 4.125 * DipsPerInch;

    private static readonly Brush Ink = Brushes.Black;
    private static readonly Brush Soft = Brushes.DimGray;
    private static readonly Brush TestRed = Brushes.Firebrick;

    private static readonly Regex PostalCode = new(
        @"[ABCEGHJ-NPRSTVXY]\d[ABCEGHJ-NPRSTV-Z][ -]?\d[ABCEGHJ-NPRSTV-Z]\d$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Ce contrôle est adapté aux champs d'adresse locaux temporaires.
    /// L'API fournira plus tard des champs distincts rue/ville/province/code postal.
    /// La validation n'est pas une vérification postale de l'adresse réelle.
    /// </summary>
    public static bool TryValidateRecipient(NoticeCandidateViewModel recipient, out string reason)
        => TryReadAddress(recipient, out _, out _, out reason);

    public static FixedDocument BuildEnvelopes(IReadOnlyList<NoticeCandidateViewModel> candidates)
    {
        var document = NewDocument(EnvelopeWidth, EnvelopeHeight);
        foreach (var candidate in candidates)
        {
            if (!TryReadAddress(candidate, out string street, out string city, out string reason))
                throw new InvalidOperationException(
                    $"Adresse incomplète pour {candidate.OwnerName} (dossier #{candidate.FileNumber}) : {reason}.");

            var page = NewPage(EnvelopeWidth, EnvelopeHeight);
            Write(page, "Le Refuge Animal", 43, 40, 300, 22, 15, bold: true);
            Write(page, "2650, MARCOTTE", 43, 62, 320, 18, 12);
            Write(page, "Roberval, QC  G8H 2M9", 43, 79, 330, 20, 12);

            const double addressX = 395;
            WriteFitted(page, candidate.OwnerName, addressX, 165, 465, 28, 19, 12, bold: true);
            WriteFitted(page, street, addressX, 194, 465, 24, 16, 10);
            WriteFitted(page, city, addressX, 219, 465, 24, 16, 10);
            Write(page, $"Dossier : #{candidate.FileNumber}", addressX, 250, 450, 16, 10,
                color: Soft);

            Write(page, "DÉMONSTRATION — NE PAS ENVOYER", 43, 358, 825, 24, 19,
                bold: true, alignment: TextAlignment.Center, color: TestRed);
            document.Pages.Add(new PageContent { Child = page });
        }
        return document;
    }

    public static FixedDocument BuildLabels(IReadOnlyList<NoticeCandidateViewModel> candidates)
    {
        var document = NewDocument(LetterWidth, LetterHeight);
        const double labelWidth = 2.625 * DipsPerInch;
        const double labelHeight = DipsPerInch;
        const double gapX = 0.125 * DipsPerInch;
        const double left = 0.1875 * DipsPerInch;
        const double top = 0.5 * DipsPerInch;

        for (int start = 0; start < candidates.Count; start += 30)
        {
            var page = NewPage(LetterWidth, LetterHeight);
            var batch = candidates.Skip(start).Take(30).ToList();
            for (int i = 0; i < batch.Count; i++)
            {
                var candidate = batch[i];
                if (!TryReadAddress(candidate, out string street, out string city, out string reason))
                    throw new InvalidOperationException(
                        $"Adresse incomplète pour {candidate.OwnerName} (dossier #{candidate.FileNumber}) : {reason}.");

                int row = i / 3;
                int col = i % 3;
                double x = left + col * (labelWidth + gapX);
                double y = top + row * labelHeight;

                // Repère temporaire à calibrer selon les planches réellement utilisées.
                var frame = new Rectangle
                {
                    Width = labelWidth - 1,
                    Height = labelHeight - 1,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                FixedPage.SetLeft(frame, x);
                FixedPage.SetTop(frame, y);
                page.Children.Add(frame);

                double innerX = x + 9;
                double innerWidth = labelWidth - 18;
                WriteFitted(page, candidate.OwnerName, innerX, y + 6, innerWidth, 19, 12, 8, bold: true);
                WriteFitted(page, street, innerX, y + 26, innerWidth, 16, 10.5, 8);
                WriteFitted(page, city, innerX, y + 42, innerWidth, 16, 10.5, 8);
                Write(page, "DÉMO — NE PAS ENVOYER", innerX, y + 60, innerWidth, 15,
                    9, bold: true, color: TestRed);
            }
            document.Pages.Add(new PageContent { Child = page });
        }
        return document;
    }

    private static bool TryReadAddress(
        NoticeCandidateViewModel candidate,
        out string street, out string cityLine, out string reason)
    {
        street = string.Empty;
        cityLine = string.Empty;
        reason = string.Empty;

        if (string.IsNullOrWhiteSpace(candidate.OwnerName) ||
            string.IsNullOrWhiteSpace(candidate.Address) || candidate.Address.Trim() == "—")
        {
            reason = "nom ou adresse non renseigné";
            return false;
        }

        // Exemples acceptés :
        // "123, boulevard Exemple, Roberval, QC, G8H 2M9"
        // "123 rue Principale, Roberval, QC G8H 2M9"
        var parts = candidate.Address.Split(',',
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            reason = "rue, municipalité ou province manquante";
            return false;
        }

        var match = PostalCode.Match(parts[^1].Trim());
        if (!match.Success)
        {
            reason = "code postal canadien manquant ou invalide";
            return false;
        }

        string beforePostalCode = parts[^1][..match.Index].Trim(' ', ',', '-', '—');
        string province;
        string municipality;
        int streetEndExclusive;
        if (beforePostalCode.Length > 0)
        {
            province = beforePostalCode;
            municipality = parts[^2];
            streetEndExclusive = parts.Length - 2;
        }
        else
        {
            if (parts.Length < 4)
            {
                reason = "province manquante";
                return false;
            }
            province = parts[^2];
            municipality = parts[^3];
            streetEndExclusive = parts.Length - 3;
        }

        if (!province.Equals("QC", StringComparison.OrdinalIgnoreCase) &&
            !province.Equals("Québec", StringComparison.OrdinalIgnoreCase))
        {
            reason = "province absente ou différente de QC";
            return false;
        }

        street = string.Join(", ", parts.Take(streetEndExclusive)).Trim();
        if (street.Length == 0 || municipality.Trim().Length == 0 || street == "—")
        {
            reason = "rue ou municipalité manquante";
            return false;
        }

        string postal = match.Value.ToUpperInvariant().Replace(" ", "").Replace("-", "");
        postal = postal[..3] + " " + postal[3..];
        cityLine = $"{municipality.Trim()}, QC  {postal}";
        return true;
    }

    private static FixedDocument NewDocument(double width, double height)
    {
        var document = new FixedDocument();
        document.DocumentPaginator.PageSize = new Size(width, height);
        return document;
    }

    private static FixedPage NewPage(double width, double height) => new()
    {
        Width = width,
        Height = height,
        Background = Brushes.White
    };

    private static void WriteFitted(FixedPage page, string text, double x, double y,
        double width, double height, double preferredFontSize, double minimumFontSize,
        bool bold = false)
    {
        double size = preferredFontSize;
        var weight = bold ? FontWeights.Bold : FontWeights.Normal;
        var typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal,
            weight, FontStretches.Normal);
        while (size >= minimumFontSize)
        {
            var layout = new FormattedText(text, CultureInfo.GetCultureInfo("fr-CA"),
                FlowDirection.LeftToRight, typeface, size, Ink, 1.0);
            if (layout.WidthIncludingTrailingWhitespace <= width - 1)
            {
                Write(page, text, x, y, width, height, size, bold);
                return;
            }
            size -= 0.5;
        }

        // Aucun texte tronqué en silence sur une étiquette ou une enveloppe.
        throw new InvalidOperationException(
            $"Le texte « {text} » dépasse la largeur prévue : vérifiez l'adresse avant l'impression.");
    }

    private static void Write(FixedPage page, string value, double x, double y, double width,
        double height, double fontSize, bool bold = false,
        TextAlignment alignment = TextAlignment.Left, Brush? color = null)
    {
        var block = new TextBlock
        {
            Text = value,
            Width = width,
            Height = height,
            FontFamily = new FontFamily("Arial"),
            FontSize = fontSize,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            Foreground = color ?? Ink,
            TextAlignment = alignment,
            TextWrapping = TextWrapping.NoWrap,
            TextTrimming = TextTrimming.None
        };
        FixedPage.SetLeft(block, x);
        FixedPage.SetTop(block, y);
        page.Children.Add(block);
    }
}

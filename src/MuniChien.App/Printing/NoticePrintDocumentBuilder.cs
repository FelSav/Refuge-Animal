using MuniChien.App.Services;
using MuniChien.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MuniChien.App.Printing;

/// <summary>
/// Reconstitution vectorielle du formulaire « Avis spécifique » (Access) à partir du PDF
/// fourni comme référence. AUCUN PDF contenant de vrais dossiers n'est embarqué.
/// Page lettre 8,5 x 11 pouces (816 x 1056 DIPs), dessin en coordonnées 612 x 792.
/// Prototype démo seulement : aucune donnée ni somme ne doit être envoyée aux citoyens.
/// </summary>
public static class NoticePrintDocumentBuilder
{
    private const double ReferenceWidth = 612;
    private const double ReferenceHeight = 792;
    private const double LetterWidth = 816;
    private const double LetterHeight = 1056;
    private const int DogsPerPage = 11;
    private static readonly Brush Ink = Brushes.Black;
    private static readonly Brush Muted = Brushes.DimGray;
    private static readonly Brush Rule = new SolidColorBrush(Color.FromRgb(80, 80, 80));
    private static readonly Brush Pale = new SolidColorBrush(Color.FromRgb(241, 241, 241));

    public static void ValidateTextLengths(string regulation, string paymentInstructions)
    {
        if (regulation.Length > 380)
            throw new ArgumentException("Le texte de réglementation doit contenir au maximum 380 caractères pour tenir sur une fiche.");
        if (paymentInstructions.Length > 290)
            throw new ArgumentException("Le texte des méthodes de paiement doit contenir au maximum 290 caractères.");
    }

    public static FixedDocument Build(
        IReadOnlyList<NoticeCandidateViewModel> candidates,
        string campaign,
        int year,
        NoticeTemplateSettings settings)
    {
        ValidateTextLengths(settings.RegulationText, settings.PaymentInstructionsText);

        var document = new FixedDocument();
        document.DocumentPaginator.PageSize = new Size(LetterWidth, LetterHeight);

        foreach (var candidate in candidates)
        {
            var groups = candidate.Dogs.Count == 0
                ? new List<List<NoticeDogData>> { new() }
                : candidate.Dogs.Chunk(DogsPerPage).Select(chunk => chunk.ToList()).ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                var page = BuildPage(candidate, groups[i], i + 1, groups.Count, campaign, year, settings);
                var content = new PageContent { Child = page };
                document.Pages.Add(content);
            }
        }

        return document;
    }

    private static FixedPage BuildPage(
        NoticeCandidateViewModel candidate,
        IReadOnlyList<NoticeDogData> dogs,
        int pageIndex,
        int pageCount,
        string campaign,
        int year,
        NoticeTemplateSettings settings)
    {
        var page = new FixedPage
        {
            Width = LetterWidth,
            Height = LetterHeight,
            Background = Brushes.White
        };

        var canvas = new Canvas
        {
            Width = ReferenceWidth,
            Height = ReferenceHeight,
            RenderTransform = new ScaleTransform(LetterWidth / ReferenceWidth, LetterHeight / ReferenceHeight)
        };
        FixedPage.SetLeft(canvas, 0);
        FixedPage.SetTop(canvas, 0);
        page.Children.Add(canvas);

        // En-tête : mêmes sections et emplacements que l'original.
        Write(canvas, "Le Refuge Animal", 53, 24, 220, 18, 11, bold: true);
        Write(canvas, "2650, MARCOTTE", 53, 41, 210, 11, 8);
        Write(canvas, "Roberval", 53, 52, 210, 11, 8);
        Write(canvas, "G8H 2M9", 53, 63, 210, 11, 8);
        Write(canvas, "Téléphone : (418) 275-3006", 53, 74, 210, 12, 8);

        Write(canvas, $"Recensement canin pour {candidate.Municipality}", 258, 24, 300, 20, 12, bold: true,
            alignment: TextAlignment.Right);
        Write(canvas, $"No. de dossier :     {candidate.FileNumber}", 335, 53, 223, 14, 10, bold: true,
            alignment: TextAlignment.Right);
        if (pageCount > 1)
            Write(canvas, $"Page {pageIndex}/{pageCount}", 470, 69, 88, 10, 7, alignment: TextAlignment.Right);

        Write(canvas, "Propriétaire du (des) chien(s) :", 53, 88, 225, 14, 9.5, bold: true);
        Line(canvas, 53, 101, 218, 101, 0.7);
        Write(canvas, candidate.OwnerName, 53, 104, 222, 18, 9, bold: true);
        Write(canvas, NoBlank(candidate.Address), 53, 121, 222, 31, 8, wrapping: true);
        Write(canvas, "Courriel : " + NoBlank(candidate.Email), 53, 155, 225, 12, 7.9);
        Write(canvas, "Date de naissance : —", 53, 168, 225, 12, 7.9);
        Write(canvas, $"Téléphone : {NoBlank(candidate.Telephone)}   Cell. : {NoBlank(candidate.CellPhone)}",
            53, 181, 230, 13, 7.4);

        Write(canvas, "Modifications :", 289, 87, 268, 14, 9, bold: true);
        DrawRect(canvas, 289, 101, 269, 93, false);
        foreach (double y in new[] { 117d, 132d, 148d, 164d, 179d })
            Line(canvas, 289, y, 558, y, 0.55);
        Line(canvas, 424, 132, 424, 148, 0.55);
        Line(canvas, 424, 179, 424, 194, 0.55);
        Write(canvas, "Prénom, nom", 290, 103, 267, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Write(canvas, "Adresse", 290, 119, 267, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Write(canvas, "Municipalité", 290, 135, 134, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Write(canvas, "Code postal", 425, 135, 132, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Write(canvas, "Courriel", 290, 150, 267, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Write(canvas, "Date de naissance", 290, 166, 267, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Write(canvas, "Téléphone", 290, 181, 134, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Write(canvas, "Cellulaire", 425, 181, 132, 12, 7.2, alignment: TextAlignment.Center, color: Muted);
        Line(canvas, 53, 197, 558, 197, 0.75);

        Write(canvas, candidate.Municipality.ToUpperInvariant(), 53, 200, 505, 15, 10.5,
            bold: true, alignment: TextAlignment.Center);
        Write(canvas, $"LICENCES DE CHIENS {year}", 53, 215, 505, 14, 9.6,
            bold: true, alignment: TextAlignment.Center);
        WriteFittedParagraph(canvas, settings.RegulationText, 53, 231, 505, 28, 8, 5.8);

        Write(canvas, "Liste de vos chiens dans nos dossiers :", 53, 262, 395, 13, 8.5, bold: true);
        Write(canvas, "Je n'ai plus", 513, 260, 45, 12, 7.0, bold: true, alignment: TextAlignment.Center);
        Write(canvas, "ce chien", 513, 269, 45, 10, 7.0, bold: true, alignment: TextAlignment.Center);

        Write(canvas, "Nom", 53, 275, 116, 11, 7.6, bold: true);
        Write(canvas, "Race", 173, 275, 106, 11, 7.6, bold: true);
        Write(canvas, "Couleur", 283, 275, 101, 11, 7.6, bold: true);
        Write(canvas, "Sexe", 392, 275, 36, 11, 7.6, bold: true);
        Write(canvas, "Stérilisé", 429, 275, 48, 11, 7.6, bold: true);
        Write(canvas, "# Licence", 480, 275, 48, 11, 7.6, bold: true);

        int visibleRows = Math.Max(1, dogs.Count);
        for (int i = 0; i < dogs.Count; i++)
        {
            var dog = dogs[i];
            double y = 289 + i * 11;
            Write(canvas, NoBlank(dog.Name), 53, y, 114, 10, 7.1);
            Write(canvas, NoBlank(dog.Breed), 173, y, 106, 10, 7.0);
            Write(canvas, NoBlank(dog.Color), 283, y, 104, 10, 7.0);
            Write(canvas, NoBlank(dog.Sex), 392, y, 32, 10, 7.1);
            Check(canvas, 440, y + 0.5, IsYes(dog.Sterilized));
            Write(canvas, NoBlank(dog.LicenseNumber), 480, y, 44, 10, 7.0);
            Check(canvas, 531, y + 0.5, false);
        }

        double balanceY = 289 + visibleRows * 11 + 1;
        Write(canvas, $"Montant facturé : {Money(candidate.AmountInvoiced)}", 53, balanceY, 198, 14, 8.0, bold: true);
        Write(canvas, $"Paiement reçu : {Money(candidate.PaymentsReceived)}", 263, balanceY, 171, 14, 8.2, bold: true);
        Write(canvas, $"Total à payer : {Money(candidate.Balance)}", 438, balanceY, 120, 14, 8.4,
            bold: true, alignment: TextAlignment.Right);

        // Les frais de retard expliquent pourquoi le total n'est pas simplement
        // le montant facturé moins les paiements reçus (ex. 75 + 10 - 25 = 60).
        double extraFeeHeight = candidate.LateFees > 0 ? 13 : 0;
        if (candidate.LateFees > 0)
            Write(canvas, $"Frais de retard : {Money(candidate.LateFees)}",
                53, balanceY + 13, 220, 12, 8.1, bold: true);
        Line(canvas, 53, balanceY + 15 + extraFeeHeight, 558, balanceY + 15 + extraFeeHeight, 0.75);

        double newY = balanceY + 19 + extraFeeHeight;
        Write(canvas, "Nouveau chien :", 53, newY, 101, 13, 8.4, bold: true);
        Write(canvas, "Race", 157, newY, 95, 13, 8.4, bold: true);
        Write(canvas, "Couleur", 260, newY, 93, 13, 8.4, bold: true);
        Write(canvas, "Sexe", 363, newY, 37, 13, 8.4, bold: true);
        Write(canvas, "Stérilisé", 406, newY, 49, 13, 8.4, bold: true);
        // Deux lignes entièrement au-dessus du tableau, sans chevaucher
        // la ligne de séparation du solde ni le cadre gris.
        Write(canvas, "Réservé à", 467, newY, 91, 10, 7.6,
            alignment: TextAlignment.Center);
        Write(canvas, "l'administration", 467, newY + 10, 91, 10, 7.6,
            alignment: TextAlignment.Center);

        // Hauteur suffisante pour le titre sur deux lignes.
        double tableY = newY + 23;
        foreach (var (x, width) in new[] { (53d, 99d), (157d, 99d), (260d, 98d), (363d, 40d) })
        {
            DrawRect(canvas, x, tableY, width, 44, false);
            Line(canvas, x, tableY + 14, x + width, tableY + 14, 0.55);
            Line(canvas, x, tableY + 29, x + width, tableY + 29, 0.55);
        }
        for (int i = 0; i < 3; i++)
        {
            double y = tableY + 2 + 14.5 * i;
            Check(canvas, 411, y, false);
            Write(canvas, "Oui", 423, y - 1, 32, 10, 7.4);
        }

        // La zone administration reprend bien les TROIS cases grisées
        // du formulaire Access. Dessiner les traits APRÈS le remplissage
        // évite qu'ils soient cachés par le fond gris.
        DrawRect(canvas, 467, tableY, 91, 44, true);
        Line(canvas, 467, tableY + 14, 558, tableY + 14, 0.55);
        Line(canvas, 467, tableY + 29, 558, tableY + 29, 0.55);

        // Le tarif est volontairement inconnu : aucun 25 $ fictif dans le modèle.
        double noteY = tableY + 49;
        Write(canvas,
            "Veuillez ajouter le tarif en vigueur pour chaque nouveau chien et déduire les chiens retirés, après vérification.",
            53, noteY, 505, 17, 7.5, wrapping: true);
        string deadline = settings.PaymentDeadline.HasValue
            ? settings.PaymentDeadline.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "À DÉFINIR";
        Write(canvas, $"Votre paiement devra nous parvenir avant le {deadline}.",
            53, noteY + 18, 505, 13, 8.2);

        double signY = noteY + 56;
        Line(canvas, 53, signY, 222, signY, 0.65);
        Line(canvas, 237, signY, 289, signY, 0.65);
        Line(canvas, 314, signY, 485, signY, 0.65);
        Line(canvas, 501, signY, 558, signY, 0.65);
        Write(canvas, "Signature propriétaire", 53, signY + 4, 173, 13, 8.5, bold: true);
        Write(canvas, "Date", 237, signY + 4, 50, 13, 8.5, bold: true);
        Write(canvas, "Signature du représentant", 314, signY + 4, 179, 13, 8.5, bold: true);
        Write(canvas, "Date", 501, signY + 4, 54, 13, 8.5, bold: true);

        double payTop = signY + 24;
        DrawRect(canvas, 53, payTop, 505, 111, false);
        FillRect(canvas, 53, payTop, 505, 14, Rule);
        Write(canvas, "Mode de paiement", 57, payTop + 1, 496, 11, 8, bold: true, color: Brushes.White);
        Line(canvas, 349, payTop + 14, 349, payTop + 111, 0.7);
        Write(canvas, "Par carte de crédit :", 61, payTop + 19, 136, 13, 8.2);
        Check(canvas, 211, payTop + 21, false);
        Write(canvas, "Visa", 225, payTop + 20, 53, 12, 8);
        Check(canvas, 278, payTop + 21, false);
        Write(canvas, "MasterCard", 293, payTop + 20, 57, 12, 8);
        Write(canvas, "Paiement sécurisé en ligne ou au téléphone :", 61, payTop + 45, 278, 15, 8);
        Write(canvas, "ne pas inscrire de numéro de carte ni de CVV sur cet avis.",
            61, payTop + 60, 276, 30, 7.4, wrapping: true);
        Write(canvas, "Autres modes de paiement :", 359, payTop + 19, 185, 13, 8.2, bold: true);
        Check(canvas, 367, payTop + 39, false);
        Write(canvas, "Chèque", 381, payTop + 39, 56, 11, 7.9);
        Check(canvas, 450, payTop + 39, false);
        Write(canvas, "Mandat poste", 464, payTop + 39, 85, 11, 7.7);
        Write(canvas, "À l'ordre de Refuge Animal", 365, payTop + 52, 183, 13, 7.3);
        Check(canvas, 367, payTop + 73, false);
        Write(canvas, "Argent", 381, payTop + 73, 52, 11, 8);
        Check(canvas, 450, payTop + 73, false);
        Write(canvas, "Non payé", 464, payTop + 73, 79, 11, 7.8);
        Write(canvas, "Montant du paiement :", 359, payTop + 93, 122, 13, 7.8, bold: true);
        DrawRect(canvas, 478, payTop + 91, 72, 15, false);

        WriteFittedParagraph(canvas, settings.PaymentInstructionsText,
            53, payTop + 119, 505, 40, 8.2, 5.8, bold: true);

        // Deux marquages permanents sur les impressions de test.
        Write(canvas, "DÉMONSTRATION — NE PAS ENVOYER", 53, 761, 505, 19, 13.5,
            bold: true, alignment: TextAlignment.Center, color: Brushes.Firebrick);
        Write(canvas, $"Aperçu {campaign} — Données fictives non validées par l'API",
            53, 746, 505, 12, 7.2, alignment: TextAlignment.Center, color: Brushes.Firebrick);
        var watermark = Write(canvas, "DÉMONSTRATION — NE PAS ENVOYER", 116, 352, 420, 44, 27,
            bold: true, alignment: TextAlignment.Center, color: Brushes.Firebrick);
        watermark.Opacity = 0.10;
        watermark.RenderTransform = new RotateTransform(-20, 210, 22);
        watermark.IsHitTestVisible = false;

        return page;
    }

    private static string NoBlank(string? input) => string.IsNullOrWhiteSpace(input) ? "—" : input;
    private static bool IsYes(string value) => value.Equals("Oui", StringComparison.OrdinalIgnoreCase);
    private static string Money(decimal value) => $"{value.ToString("0.00", CultureInfo.InvariantCulture)} $";

    private static TextBlock Write(
        Canvas canvas, string value, double x, double y, double width, double height,
        double fontSize, bool bold = false, TextAlignment alignment = TextAlignment.Left,
        Brush? color = null, bool wrapping = false)
    {
        var text = new TextBlock
        {
            Text = value,
            Width = width,
            Height = height,
            FontFamily = new FontFamily("Arial"),
            FontSize = fontSize,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            Foreground = color ?? Ink,
            TextAlignment = alignment,
            TextWrapping = wrapping ? TextWrapping.Wrap : TextWrapping.NoWrap,
            TextTrimming = wrapping ? TextTrimming.None : TextTrimming.CharacterEllipsis
        };
        Canvas.SetLeft(text, x);
        Canvas.SetTop(text, y);
        canvas.Children.Add(text);
        return text;
    }

    private static void WriteFittedParagraph(Canvas canvas, string text, double x, double y,
        double width, double height, double fontSize, double minimumSize, bool bold = false)
    {
        var block = Write(canvas, text, x, y, width, height, fontSize, bold, wrapping: true);
        for (double size = fontSize; size >= minimumSize; size -= 0.2)
        {
            block.FontSize = size;
            // Remove Height restriction while measuring the real number of wrapped lines.
            block.Height = double.NaN;
            block.Measure(new Size(width, double.PositiveInfinity));
            double requiredHeight = block.DesiredSize.Height;
            block.Height = height;
            if (requiredHeight <= height + 0.3)
                return;
        }
        canvas.Children.Remove(block);
        throw new InvalidOperationException(
            "Un des textes du modèle est trop long pour le format lettre. Réduisez-le dans « Configurer le modèle ». ");
    }

    private static void Check(Canvas canvas, double x, double y, bool selected)
    {
        DrawRect(canvas, x, y, 8, 8, false);
        if (selected) Write(canvas, "✓", x - 0.5, y - 3, 12, 13, 9.5, bold: true);
    }

    private static void DrawRect(Canvas canvas, double x, double y, double width, double height, bool shaded)
    {
        var rectangle = new Rectangle
        {
            Width = width,
            Height = height,
            Stroke = Rule,
            StrokeThickness = 0.65,
            Fill = shaded ? Pale : Brushes.Transparent
        };
        Canvas.SetLeft(rectangle, x);
        Canvas.SetTop(rectangle, y);
        canvas.Children.Add(rectangle);
    }

    private static void FillRect(Canvas canvas, double x, double y, double width, double height, Brush fill)
    {
        var rectangle = new Rectangle { Width = width, Height = height, Fill = fill };
        Canvas.SetLeft(rectangle, x);
        Canvas.SetTop(rectangle, y);
        canvas.Children.Add(rectangle);
    }

    private static void Line(Canvas canvas, double x1, double y1, double x2, double y2, double thickness)
    {
        canvas.Children.Add(new System.Windows.Shapes.Line
        {
            X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
            Stroke = Rule, StrokeThickness = thickness
        });
    }
}

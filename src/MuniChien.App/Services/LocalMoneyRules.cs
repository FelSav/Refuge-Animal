namespace MuniChien.App.Services;

// Garde-fou partagé : la valeur sauvegardée correspond toujours à des cents entiers.
// La limite maximale reste celle de la démonstration locale (non un tarif officiel).
internal static class LocalMoneyRules
{
    public const decimal MaximumAmount = 100000m;

    public static void ValidatePositiveAmount(decimal amount)
    {
        if (amount <= 0m || amount > MaximumAmount || !HasAtMostTwoFractionalDigits(amount))
            throw new ArgumentException("Le montant doit être positif, ne pas dépasser 100 000 $ et avoir au plus deux décimales.");
    }

    // Vérifie aussi les zéros inutiles saisis après la 2e décimale (ex.: 25,000).
    public static bool HasAtMostTwoFractionalDigits(decimal amount) =>
        ((decimal.GetBits(amount)[3] >> 16) & 0xFF) <= 2;

    public static void ValidatePaymentDate(DateTime date)
    {
        if (date.Date > DateTime.Today || date.Date < new DateTime(2000, 1, 1))
            throw new ArgumentException("La date du paiement doit être comprise entre le 1er janvier 2000 et aujourd'hui.");
    }
}

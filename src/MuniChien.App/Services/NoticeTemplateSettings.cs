using System;

namespace MuniChien.App.Services;

// Un seul modèle pour toutes les municipalités et les deux campagnes.
// Aucun montant/tarif n'est défini ici.
public sealed class NoticeTemplateSettings
{
    public string RegulationText { get; set; } =
        "CONFORMÉMENT À VOTRE RÉGLEMENTATION MUNICIPALE IL EST DE VOTRE DEVOIR DE VOUS " +
        "PROCURER UNE LICENCE POUR VOTRE CHIEN. SI VOUS AVEZ DES CHANGEMENTS À APPORTER " +
        "À VOTRE DOSSIER, VEUILLEZ NOUS EN AVISER LE PLUS RAPIDEMENT POSSIBLE.";

    // Volontairement vide tant que le refuge n'a pas choisi la date de campagne.
    // L'expiration des licences au 31 décembre est une autre règle.
    public DateTime? PaymentDeadline { get; set; }

    public string PaymentInstructionsText { get; set; } =
        "VOUS POUVEZ FAIRE VOTRE PAIEMENT EN LIGNE AU WWWREFUGEANIMAL.COM, " +
        "PAR LA POSTE (EN RETOURNANT CET AVIS), PAR TÉLÉPHONE OU AU REFUGE ANIMAL DE ROBERVAL";
}

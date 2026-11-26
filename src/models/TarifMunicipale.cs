using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class TarifMunicipale
    {
        public int tarification_id { get; set; }
        public int municipalite_id { get; set; }
        public int annee { get; set; }
        public decimal tarif_premier_chien  { get; set; }
        public decimal tarif_deuxieme_chien { get; set; }
        public decimal tarif_chenil { get; set; }
        public DateOnly date_Limite_paiement { get; set; }
        public decimal frais_retard { get; set; }
    }

}
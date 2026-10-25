using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Paiement
    {
        public int paiement_id { get; set; }
        public int proprietaire_id { get; set; }
        public int municipalite_id { get; set; }
        public DateOnly date_paiement { get; set; }
        public decimal montant { get; set; }
        public string mode_paiement { get; set; }
        public string precision { get; set; }
        public bool paye_municipalité { get; set; }
        public string commentaire { get; set; }




    }

}
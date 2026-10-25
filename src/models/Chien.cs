using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Chien
    {
        public int chien_id { get; set; }
        public int proprietaire_id { get; set; }
        public string nom { get; set; }
        public int race_id { get; set; }
        public int couleur_id { get; set; }
        public string sexe { get; set; }
        public DateTime date_naissance { get; set; }
        public double poids_kg { get; set; }
        
        public bool sterilise { get; set; }
        public string? photo_path { get; set; }
        public string? commentaire { get; set; }
        public bool actif { get; set; }
        public string? raison_inactivite { get; set; }
        public DateTime? date_inactivite { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }

}
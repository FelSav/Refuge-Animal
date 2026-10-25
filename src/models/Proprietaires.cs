using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Proprietaires
    {
        public int proprietaire_id { get; set; }
        public int numero_dossier { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public string numero_civique { get; set; }
        public string? appartement { get; set; }
        public int rue_id { get; set; }
        public string code_postal { get; set; }
        public string telephone { get; set; }
        public string courriel { get; set; }
        public DateTime date_naissance { get; set; }
        public bool est_chenil { get; set; }
        public string? commentaire { get; set; }
        public bool actif { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

    }

}

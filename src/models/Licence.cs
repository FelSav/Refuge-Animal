using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Licence
    {
        public int licence_id { get; set; }
        public int proprietaire_id { get; set; }
        public int municipalite_id { get; set; }
        public int numero{ get; set; }
        public int annee { get; set; }
        public DateOnly date_emission { get; set; }
        public DateOnly date_expiration { get; set; }
        public string nom_proprietaire { get; set; }
        public string prenom_proprietaire { get; set; }
        public string numero_civique { get; set; }
        public string? appartement { get; set; }
        public string rue { get; set; }
        public string nom_municipalite { get; set; }
        public string code_postal { get; set; }
        public string telephone { get; set; }
        public string courriel { get; set; }
    }

}
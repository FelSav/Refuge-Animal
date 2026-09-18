using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Proprietaires
    {
        public int Id { get; set; }
        public int Num_Dossier { get; set; }
        public string Nom { get; set; }
        public string Prenom { get;set; }
        public string Num_Civique  { get; set; }
        public string Appartement { get; set; }
        public int Rue_Id { get; set; }
        public int Municipalite_Id { get; set; }
        public string Code_Postal { get; set; }
        public string Tel { get; set; }
        public string courriel { get; set; }
        public DateOnly Date_Naissance { get;set; }
        public bool Est_Chenil { get;set; }
        public string Commentaire { get; set; }
        public bool actif { get; set; }
        public int version { get; set; }
        public DateOnly Creation { get; set; }

    }

}

 
namespace ApiMuniChien_V1_Proprietaire.models {
  public  class ResumeChiens
  {
      public int  chien_id { get; set; }
      public int  proprietaire_id  { get; set; }
      public string nom  { get; set; }
      public int race_id { get; set; }
      public int couleur_id { get; set; }
      public string sexe { get; set; }
      public DateTime date_naissance { get; set; }
      public int age { get; set; }
      public double poids_kg { get; set; }
      public bool sterilise { get; set; } 
      public string? photo_path { get; set; }
      public string? commentaire { get; set; }
      public bool actif { get; set; }
      public string? raison_inactivite { get; set; }
      public DateTime date_inactivite { get; set; }
      public DateTime created_at { get; set; }
      public DateTime updated_at { get; set; }

    // Faire un objet licence d' un pour contenir la licence en cours puis pour faire une liste de licences precedentes du chien
    public Licence? licenceEnCours { get; set; }
    public List<Licence>? licencesPrecedentes { get; set; }
  }

}
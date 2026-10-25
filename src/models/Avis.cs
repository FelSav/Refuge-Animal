using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Avis
    {
        public int avis_id { get; set; }
        public string code { get; set; }
        public string titre { get; set; }
        public string sous_titre { get; set; }

        public string text { get; set; }
        public string lieu { get; set; }

    }

}
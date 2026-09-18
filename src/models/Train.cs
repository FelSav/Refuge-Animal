using System.Diagnostics;

namespace ApiMuniChien_V1_Proprietaire.models
{
    public class Train
    {
        public int id { get; set; }
        public required string model { get; set; }
        public required string couleur { get; set; }
        public bool enstock { get; set; }
    }

}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiMuniChien_V1_Proprietaire.models;

[ApiController]
[Route("api/accueil")]
public class AccueilController : ControllerBase
{
    private readonly ChienContext _c_context;
    
    private readonly LicenceChienContext _lcc_context;

    private readonly PaiementContext _p_context;
    private readonly AvisContext _a_context;
    private readonly LicenceContext _lc_context;

    public AccueilController(ChienContext c_context,  LicenceChienContext lcc_context, PaiementContext p_context, AvisContext a_context, LicenceContext lc_context )
    {
        _c_context = c_context;
        _lcc_context = lcc_context;
        _p_context = p_context;
        _a_context = a_context;
        _lc_context = lc_context;
    }

    // GET: api/accueil
    [HttpGet]
    public async Task<ActionResult<ResumeAccueil>> GetAccueil()
    {
        Console.WriteLine(">>> Requête GET /api/accueil reçue");

        // 1. Nombre de chiens actifs
        var chiensActifs = await _c_context.Chiens
            .CountAsync(c => c.actif);
        Console.WriteLine($">>> Chiens actifs : {chiensActifs}");
        // 2. Nombre de chiens inactifs
        var chiensInactifs = await _c_context.Chiens
            .CountAsync(c => !c.actif);
        Console.WriteLine($">>> Chiens inactifs : {chiensInactifs}");
        // 3. Nombre de chiens sans licence


        var list_licencechiens_id = await _lcc_context.LicenceChiens.Select(lc => lc.chien_id).ToListAsync();
        var chiensSansLicence = 0;

        // recuperer l,id des chiens mais aussi si ils sont actifs ou inactifs pour ne pas compter les chiens inactifs dans le nombre de chiens sans licence
        var list_chiens = await _c_context.Chiens.Select(c => new { c.chien_id, c.actif }).ToListAsync();
        foreach (var chien in list_chiens)
        {
            if (chien.actif && !list_licencechiens_id.Contains(chien.chien_id))
            {
                chiensSansLicence++;
            }
        }

        // 4. Paiment du jour
        var nbPaiementJour = await _p_context.Paiements
        // Comparer la date de paiement avec la date actuelle (aujourd'hui)
        .CountAsync(p => p.date_paiement == DateOnly.FromDateTime(DateTime.Now));

        // 5. Paiements du mois
        var nbPaiementMois = await _p_context.Paiements
        // Comparer le mois et l'année de la date de paiement avec le mois et l'année actuels
        .CountAsync(p => p.date_paiement.Month == DateTime.Now.Month && p.date_paiement.Year == DateTime.Now.Year);

        // 6. Voir les avis a envoyé
        // voir le nombre d'avis juste compte le nombre d'avis dans la table avis
        var nbAvis = await _a_context.Avis.CountAsync();

        // 7. Nombre de liciences a renouveler
        // comparer avec la date du premier janvier de chaque année en cours pour voir si la licence est expirée ou non, si elle est expirée alors on l'ajoute au nombre de licences a renouveler
        var anneeEnCours = DateTime.Now.Year;
        var premierJourAnnee = new DateOnly(anneeEnCours, 1, 1);
        var nbLicencesARenouveler = await _lc_context.Licences.CountAsync(l => l.date_expiration > premierJourAnnee); 
        

        // Verification puis envoie 
        Console.WriteLine($">>> Chiens actifs : {chiensActifs}");
        Console.WriteLine($">>> Chiens inactifs : {chiensInactifs}");
        Console.WriteLine($">>> Chiens sans licence : {chiensSansLicence}");
        Console.WriteLine($">>> Paiements du jour : {nbPaiementJour}");
        Console.WriteLine($">>> Paiements du mois : {nbPaiementMois}");
        Console.WriteLine($">>> Nombre d'avis : {nbAvis}");
        Console.WriteLine($">>> Nombre de licences à renouveler : {nbLicencesARenouveler}");
        var resume = new ResumeAccueil
        {
            NombreChiensActifs = chiensActifs,
            NombreChiensInactifs = chiensInactifs,
            NombreChiensSansLicence = chiensSansLicence,
            NombrePaiementsJour = nbPaiementJour,
            NombrePaiementsMois = nbPaiementMois,
            NombreAvis = nbAvis,
            NombreLicencesARenouveler = nbLicencesARenouveler
        };

        return Ok(resume);
    }
}
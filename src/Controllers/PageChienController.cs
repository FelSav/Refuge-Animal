
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiMuniChien_V1_Proprietaire.models;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;

[ApiController]
[Route("api/chiens")]
public class PageChienController : ControllerBase
{
    private readonly Context _context;

    public PageChienController(Context context)
    {
        _context = context;
    }

    // GET: api/chiens
    [HttpGet]
    public async Task<ActionResult<List<ResumeChiens>>> GetChiens()
    {
        Console.WriteLine(">>> Requête GET /api/chiens reçue");
        // 1. recupere toute les information sur les chiens et envoyé la liste des chiens au front-end
        var Resumechiens = new List<ResumeChiens>();
        var chiens = await _context.Chiens.ToListAsync();
        Console.WriteLine($">>> Chiens récupérés : {chiens.Count}");

        // Refaire une nouvelle liste de chiens mais avec des données en plus comme l'age calculer sur la date de naissance, la licence du chien en cours et les precedentes licences que le chien a pu avoir 
        foreach (var chien in chiens)
        {
            Console.WriteLine($">>>Chiens : " + chien.nom + " " + chien.chien_id + " " + chien.proprietaire_id);
            var chienEnCours = new ResumeChiens
            {
                //chien_id = chien.chien_id,
                //proprietaire_id = chien.proprietaire_id,
                nom = chien.nom,
                sexe = chien.sexe,
                date_naissance = chien.date_naissance,
                poids_kg = chien.poids_kg,
                sterilise = chien.sterilise,
                photo_path = chien.photo_path,
                commentaire = chien.commentaire,
                actif = chien.actif,
                raison_inactivite = chien.raison_inactivite,
                date_inactivite = chien.date_inactivite ?? DateTime.MinValue, // si null on met une date par defaut
                created_at = chien.created_at,
                updated_at = chien.updated_at
            };
            // Ajouter La couleur du chien
            var couleur = await _context.Couleur
                .Where(co => co.couleur_id == chien.couleur_id)
                .Select(co=>co.nom)
                .ToListAsync();
            chienEnCours.couleur = couleur.FirstOrDefault();

            // ajouter la race du chien
            var race = await _context.Races
                .Where(ra => ra.race_id == chien.race_id)
                .Select(ra=>ra.nom)
                .ToListAsync();
            chienEnCours.race = race.FirstOrDefault();

            // calculer l'age du chien en fonction de la date de naissance
            var age = DateTime.Now.Year - chien.date_naissance.Year;
            if (DateTime.Now.DayOfYear < chien.date_naissance.DayOfYear)
                age--;
            // ajouter l'age du chien a la liste des chiens
            chienEnCours.age = age;


            // recuperer les licences lieé au chien
            var listLicenceId = await _context.LicenceChiens
                .Where(lc => lc.chien_id == chien.chien_id)
                .Select(lc => lc.licence_id)
                .ToListAsync();

            foreach(var Id in listLicenceId){
                Console.WriteLine(">>> liste id licence : "+ Id);
            }
            // Vu qu'on a une liste d'id de licence on va recupere la licence de l'annee en cours car elle correpond a la licence en cours

            foreach (var licenceId in listLicenceId)
            {
                var licence = await _context.Licences.FirstOrDefaultAsync(l => l.licence_id == licenceId && l.date_expiration >= DateOnly.FromDateTime(DateTime.Now));
                if (licence != null)
                {
                    chienEnCours.licenceEnCours = licence;
                    // on retire la licence en cours pour faire une liste de licences precedentes
                    listLicenceId.Remove(licenceId);
                    break;
                }
            }

            chienEnCours.licencesPrecedentes = new List<Licence>();
            foreach (var licenceId in listLicenceId)
            {
                var licence = await _context.Licences.FirstOrDefaultAsync(l => l.licence_id == licenceId);
                if (licence != null)
                {
                    chienEnCours.licencesPrecedentes.Add(licence);
                }
            }

            // Recuperer les infos sur le proprietaire du chien pour les ajouter a la licence en cours et aux licences precedentes
            if (chienEnCours.licenceEnCours != null)
            {
                var proprietaire = await _context.Proprietaires.FirstOrDefaultAsync(p => p.proprietaire_id == chienEnCours.licenceEnCours.proprietaire_id);
                if (proprietaire != null)
                {
                    chienEnCours.licenceEnCours.nom_proprietaire = proprietaire.nom;
                    chienEnCours.licenceEnCours.prenom_proprietaire = proprietaire.prenom;
                    chienEnCours.licenceEnCours.numero_civique = proprietaire.numero_civique;
                    chienEnCours.licenceEnCours.appartement = proprietaire.appartement;

                    chienEnCours.licenceEnCours.code_postal = proprietaire.code_postal;
                    chienEnCours.licenceEnCours.telephone = proprietaire.telephone;
                    chienEnCours.licenceEnCours.courriel = proprietaire.courriel;

                    // Aller chercher dans la table rue : l'id de la municipalite et le nom de la rue
                    chienEnCours.licenceEnCours.rue = await _context.Rues.Where(r => r.rue_id == proprietaire.rue_id).Select(r => r.nom).FirstOrDefaultAsync();
                    var municipaliteId = await _context.Rues.Where(r => r.rue_id == proprietaire.rue_id).Select(r => r.municipalite_id).FirstOrDefaultAsync();
                    chienEnCours.licenceEnCours.nom_municipalite = await _context.Municipalites.Where(m => m.municipalite_id == municipaliteId).Select(m => m.nom).FirstOrDefaultAsync();

                    // Ensuite les licences de la liste des precedentes 
                    chienEnCours.licencesPrecedentes.ForEach(async (licence) =>
                    {
                        // Recuperer le proprietaire de la licence qui est en train d'etre traité
                        var licenceAjouterPrecendente = new Licence();
                        if (licence.proprietaire_id != null && licence.proprietaire_id >= 0)
                        {
                            var proprietairePrecedent = await _context.Proprietaires.FirstOrDefaultAsync(p => p.proprietaire_id == licence.proprietaire_id);
                            licenceAjouterPrecendente.nom_proprietaire = proprietairePrecedent.nom;
                            licenceAjouterPrecendente.prenom_proprietaire = proprietairePrecedent.prenom;
                            licenceAjouterPrecendente.numero_civique = proprietairePrecedent.numero_civique;
                            licenceAjouterPrecendente.appartement = proprietairePrecedent.appartement;

                            licenceAjouterPrecendente.code_postal = proprietairePrecedent.code_postal;
                            licenceAjouterPrecendente.telephone = proprietairePrecedent.telephone;
                            licenceAjouterPrecendente.courriel = proprietairePrecedent.courriel;

                            // Aller chercher dans la table rue : l'id de la municipalite et le nom de la rue
                            licenceAjouterPrecendente.rue = await _context.Rues.Where(r => r.rue_id == proprietaire.rue_id).Select(r => r.nom).FirstOrDefaultAsync();
                            var municipaliteId = await _context.Rues.Where(r => r.rue_id == proprietaire.rue_id).Select(r => r.municipalite_id).FirstOrDefaultAsync();
                            licenceAjouterPrecendente.nom_municipalite = await _context.Municipalites.Where(m => m.municipalite_id == municipaliteId).Select(m => m.nom).FirstOrDefaultAsync();

                            chienEnCours.licencesPrecedentes.Add(licenceAjouterPrecendente);
                        }
                    });
                }


            }

            Resumechiens.Add(chienEnCours);
        }



        return Ok(Resumechiens);
    }

    // POST: api/chiens/[{categorie:"",valeur=""}]
    // Post: api/chiens/nom nom_proprietaire race couleur municipalité sexe date_naissance poids_kg sterilise actif date_inactivite created_at updated_at                                                            
    [HttpPost]
    public async Task<ActionResult<ResumeChiens>> RecevoirDonnees() 
    {
        // Creer un résumé pour y strocker les données recuperer et traiter
        var resumeChien = new ResumeChiens();
        // Ensuite avec les données récupere faire une recherche
        
        string? nom = Request.Form["nom"].ToString();
        string? nom_proprietaire = Request.Form["nom_proprietaire"].ToString();
        string? race = Request.Form["race"].ToString();
        string? couleur = Request.Form["couleur"].ToString();
        string? municipalite = Request.Form["municipalite"].ToString();
        string? sexe = Request.Form["sexe"].ToString();
        string? date_naissance = Request.Form["date_naissance"].ToString();
        string? poids_kg = Request.Form["poid_kg"].ToString();
        string? sterilise = Request.Form["sterilise"].ToString();
        string? actif = Request.Form["actif"].ToString();
        string? date_inactivite= Request.Form["date_inactivite"].ToString();
        string? created_at = Request.Form["created_at"].ToString();
        string? updated_at = Request.Form["updated_at"].ToString();


        // Message comme quoi il n'y a renvoy si rien n'est selectionner
        
        if(nom == null && race == null && couleur == null && municipalite == null &&  sexe == null && date_naissance == null && poids_kg == null && sterilise == null && actif == null && date_inactivite == null && created_at == null && updated_at == null)
        {
            return Ok("Erreur Chien : Aucun catégorie sélectionée.");
        }
        else{
            Console.WriteLine(">>>>>>>>>>>>>>>>>> UN TRUC RECU");
        }

        // Constuire une requete  
        //var  requete = _c_context;
        
       /* if (date_naissance != null)
        {
            DateTime dDateNaissance = DateTime.Parse(date_naissance);
            var requetetest = await _c_context.Chiens.Select(c=>c.date_naissance == dDateNaissance).ToListAsync();
            //requete = await _c_context.Chiens.Select(c=>c.date_naissance).ToListAsync();

            // Je dois creer une boucle dans laquel je vais regarder chaque date de naissance dans laquel je vais devoir
            // Pas besoin. C'etait une erreur de type avec la variable requete
        }

        /*var listLicenceId = await _lcc_context.LicenceChiens
                .Where(lc => lc.chien_id == chien.chien_id)
                .Select(lc => lc.licence_id)
                .ToListAsync();*/

        //version test constructeur de requete avec l'orm (pt)
        IQueryable<Chien> requete = _context.Chiens.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(nom))
            //requete = requete.Where(c=>c.nom.Contains(nom));
            requete = requete.Where(c => EF.Functions.ILike(c.nom, $"%{nom}%"));
        /*if(!string.IsNullOrWhiteSpace(nom_proprietaire))
        requete = requete.Where(c=>
        c.chien.Any())*/
        if(int.TryParse(race, out var raceId))
            requete = requete.Where(c => c.race_id == raceId);
        if(int.TryParse(couleur, out var couleurId))
            requete = requete.Where(c=>c.couleur_id == couleurId);
        // La requete dois sauter de la table chien a licencechien puis licence puis regarder la colone municipalite id
        if(int.TryParse(municipalite, out var municipaliteId)){
            requete = requete.Where(c =>
            c.LicenceChiens.Any(lc =>
            lc.Licence.municipalite_id == municipaliteId));
        }
        if(DateTime.TryParse(date_naissance, out var naissance)){
            var debut = naissance.Date;
            var fin = debut.AddDays(1);

            requete = requete.Where(c =>
                c.date_naissance >= debut &&
                c.date_naissance < fin);
        }

        if(double.TryParse(poids_kg,out var poids)){
            requete = requete.Where(c => c.poids_kg == poids);
        }
        if(!string.IsNullOrWhiteSpace(sexe))
            requete = requete.Where(c=>c.sexe == sexe);
        if(bool.TryParse(actif, out var estactif))
            requete = requete.Where(c=>c.actif == estactif);
        if(bool.TryParse(sterilise, out var estSterilise))
            requete = requete.Where(c=>c.sterilise == estSterilise);
        if(DateTime.TryParse(date_inactivite, out var inactivite)){
            var debut = inactivite.Date;
            var fin = debut.AddDays(1);

            requete = requete.Where(c =>
                c.date_inactivite.HasValue &&
                c.date_inactivite.Value >= debut &&
                c.date_inactivite.Value < fin);
        }
        if(DateTime.TryParse(created_at, out var date_creation)){
            var debut = date_creation.Date;
            var fin = debut.AddDays(1);

            requete = requete.Where(c=>
                c.created_at >= debut && 
                c.created_at < fin);
        }
        
        var resultats = await requete.ToListAsync();
        //resumeChien = resultats;
        // resutalt == list de chien
        Console.WriteLine("La requete envoyé a la bd : "+ requete.ToQueryString());
        //Console.WriteLine("Le resultat recu de la bd : "+ );
        if(resultats == null)
        {
            Console.WriteLine(">>>>>< Resultat est null");
        }else{
            Console.WriteLine("<<<<   Resultat n'est pas null   >>>>");
            foreach(var valeur in resultats)
            {
                Console.WriteLine(valeur);
            }
        }
        return Ok(resultats);
    }

   

}

// Task<ActionResult<ResumeChiens>> POSTChien() IActionResult

//nom race couleur municipalité sexe date_naissance poids_kg sterilise actif date_inactivite created_at updated_at
# Registre des décisions — MuniChien

Révision : 6 septembre 2026.

## Sources et statut

Source principale : `Projet_Refuge_Animal_Document_Comprehension.docx`, version du 5 septembre 2026 indiquée par la demande, contenu récupéré depuis la pièce jointe de la conversation « Refuge Animal ». Les références ci-dessous désignent ses sections et identifiants MUN. Le document précise qu’il n’est pas une conception technique définitive. Sa section de validation ne prouve pas à elle seule une approbation client déjà obtenue.

Source complémentaire : décisions et état de la fondation .NET/WPF rapportés dans cette conversation. Les propositions de l’ancien assistant ne sont pas automatiquement des exigences client. En cas d’écart, les besoins du document mis à jour priment; les mécanismes proposés dans ces cinq fichiers restent explicitement séparés des décisions acquises.

## Exigences officielles à respecter

| ID local | Décision / règle | Source |
| --- | --- | --- |
| D01 | Deux logiciels indépendants, deux bases séparées. Fidélité peut fonctionner sans dossier MuniChien. | §§ 1, 4.1 |
| D02 | Serveur central hébergeant API et bases; six postes clients; aucun accès direct aux bases depuis les postes. | §§ 4.2, 7 |
| D03 | Création/consultation simultanées de dossiers différents; empêcher deux modifications concurrentes du même dossier; identifiants sans doublons. | §§ 4.3, 7.3; MUN-014 |
| D04 | Aucun compte individuel employé au quotidien. Authentification administrateur pour fermeture, restauration, suppressions définitives concernées, paramètres sensibles et maintenance. | § 8.1 |
| D05 | Sauvegardes automatiques quotidiennes sur serveur, deux semaines glissantes; sauvegarde avant fermeture; restauration administrateur. Copie externe recommandée à l’avenir. | § 8.4 |
| D06 | MuniChien conserve son nom, concerne uniquement les chiens et fonctionne sans Access en production. | §§ 5.1, 5.3; MUN-001 |
| D07 | Propriétaire : dossier unique, numéro existant conservé si possible, données utiles et date de naissance existante conservées, inactifs consultables. | § 5.2; MUN-002–003 |
| D08 | Chien : naissance enregistrée, âge calculé, poids précis kg; inactivité datée automatiquement avec raison facultative; commentaires distincts et durables. | § 5.3; MUN-004–006 |
| D09 | Vraie structure Licence, renouvellement annuel, numéro réutilisable entre années, remplacement perdu/endommagé, historique cinq ans puis suppression selon règle retenue; fiche de renouvellement au contenu fonctionnel conservé. | § 5.4; MUN-007 |
| D10 | Tarifs municipaux configurables : premier/deuxième chien, chenil, échéance et retard; municipalité active/inactive; historique cinq ans. | § 5.5; MUN-008 |
| D11 | Paiements comptant/carte/chèque/autre avec précision obligatoire; paiement direct municipalité si utilisé; solde expliqué. Suppression annuelle possible seulement selon fonctionnement validé et archivage/sauvegarde. | § 5.6 |
| D12 | Recherche multi-filtres, âge calculé, seuil de poids variable, actifs par défaut/inactifs accessibles; total de chiens et non de propriétaires. | § 5.7; MUN-009–010 |
| D13 | Sélection d’adresse officielle proposée; ajout possible d’une rue absente; autocomplétion des races/couleurs et possibilité d’ajout. Internet autorisé pour les adresses. | §§ 5.3, 5.8, 7.4; MUN-011 |
| D14 | Rapports utiles, avis, filtres avant impression, export PDF. Éditeur des modèles d’avis non requis en première version. | § 5.9; MUN-012 |
| D15 | Fermeture administrateur : sauvegarde, résumé, contrôle dettes/paiements, choix de report si nécessaire, remises à zéro/recalculs appropriés; commentaires et historiques préservés; aucune inactivation automatique des chiens. | § 5.10; MUN-013 |
| D16 | Prototype frontend présenté au client avant développement complet et connexion de toute la logique à la base. | §§ 9.2, 13.1 |
| D17 | Migration Access sur copie, original non modifié pendant analyse/essais, conservation des références utiles, comparaison des totaux/échantillons; pas de reproduction aveugle des tables Tampon. | § 10.1 |
| D18 | MVP manuel sans dépendance Acomba ni fusion; ces évolutions arrivent seulement en fin de projet si possible. | §§ 11.1–11.3 |
| D19 | Protéger renseignements personnels et sauvegardes, limiter les accès, validations serveur et chiffrement fiable lorsque possible. | §§ 8.2–8.3 |

## Choix techniques actuels de l’équipe

| Choix | Statut et conséquence |
| --- | --- |
| C# et .NET 10 | Retenus dans la conversation pour la fondation. Choix de l’équipe, pas exigence du document officiel. |
| WPF | Client Windows actuellement retenu; fondation rapportée fonctionnelle. Compatible avec l’API centrale. |
| SQL Server | Moteur actuellement retenu par l’équipe; reste sur le serveur, jamais accessible directement depuis WPF. Édition et configuration à confirmer. |
| EF Core | Orientation technique prévue pour `MuniChien.Data`; installation et connexion non considérées comme déjà réalisées. Exécution côté serveur. |
| App, Core, Data, Infrastructure, Tests | Structure existante rapportée dans la conversation; à adapter à la nouvelle frontière API. |

La proposition ancienne de relier directement WPF à Data/SQL Server est **remplacée** par le passage obligatoire par l’API. Le choix C#/.NET/WPF/SQL Server peut être conservé sans conserver cette ancienne architecture.

## Propositions de conception, non encore décisions officielles

| Proposition | But / validation nécessaire |
| --- | --- |
| API ASP.NET Core et éventuel projet Contracts | Adapter la solution .NET; valider le découpage et les contrats. |
| Verrou serveur par dossier avec expiration + contrôle de version | Empêcher le deuxième éditeur et les écritures périmées; valider durée, reprise après panne et périmètre du dossier. |
| `rowversion`, transactions et clés serveur | Garantir l’intégrité avec SQL Server; détails de réalisation. |
| Versions de tarifs et écritures financières explicatives | Préserver les calculs déjà appliqués; vérifier la correspondance avec les règles Access. |
| Coordination des écritures pendant clôture, protection contre double exécution et bilan | Sécuriser une opération globale; mécanismes à concevoir. |
| Signalement des sauvegardes échouées et vérification préalable | Rendre l’exploitation fiable; modalités d’affichage à définir. |

## Questions encore ouvertes

| Question | État et impact |
| --- | --- |
| Même numéro de licence simultanément dans deux municipalités? | Explicitement à confirmer (§§ 5.4, 14). Ne pas figer l’unicité globale; conserver le contexte municipal dans la conception. |
| Paiements partiels autorisés? | Explicitement à confirmer (§§ 5.6, 14). Ne pas les déclarer acceptés ou interdits définitivement. |
| Fournisseur de validation d’adresses et indisponibilité du service? | Choix technique ouvert (§ 1); définir aussi la validation d’une rue absente sans contourner la sélection officielle. |
| Départ exact du délai de cinq ans et déclenchement des purges? | Durées officielles connues; détails opérationnels à préciser. Protéger les historiques encore utiles et les liens. |
| Règles exactes de report, archivage et recalcul annuel? | Analyser Access, montrer le résultat au client et valider avant réalisation définitive. |
| Inventaire complet des rapports et fiche de renouvellement? | Reprendre les modèles existants; la liste générale ne remplace pas leur vérification. |
| Données manquantes à la migration et validations de saisie? | Ne rien inventer; définir valeurs inconnues, normalisation et compléments nécessaires. |
| Authentification administrateur, hébergement, chiffrement, version/édition serveur? | Précisions techniques de l’équipe; aucune technologie exacte imposée par le document. |

Les questions Fidélité et Acomba restent dans le document global et ne sont pas développées ici. Elles ne bloquent pas le prototype MuniChien.

## Portée de cette livraison

Les cinq fichiers décrivent MuniChien et les seuls impacts architecturaux globaux nécessaires. Aucun logiciel, schéma détaillé ou fichier Fidélité n’est créé. Les tests, la migration et la validation client sont des travaux à réaliser, pas des résultats déjà obtenus.

Le chemin `/mnt/data` cité dans la demande provient de l’environnement de la conversation d’origine. Cette session Windows livre les cinq fichiers dans son dossier `outputs`, sous leurs noms demandés : [architecture.md](architecture.md), [database.md](database.md), [ecrans.md](ecrans.md), [mvp.md](mvp.md) et [decisions.md](decisions.md).

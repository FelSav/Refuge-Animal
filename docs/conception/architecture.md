# Architecture — MuniChien

Révision : 6 septembre 2026. Source fonctionnelle : `Projet_Refuge_Animal_Document_Comprehension.docx`, version du 5 septembre 2026, notamment §§ 1, 4, 7–10 et 13. Les choix techniques issus de la conversation « Refuge Animal » sont distincts des exigences officielles. Voir [decisions.md](decisions.md).

## Architecture exigée par le document

Deux logiciels indépendants disposent de deux bases séparées : MuniChien et Fidélité. Le serveur du Refuge Animal héberge l’API centrale et les deux bases. Les six postes utilisent des applications clientes et ne se connectent jamais directement aux bases.

```text
Six postes de travail
  Application MuniChien ──┐
  Application Fidélité ───┴── API centrale sur le serveur
                              ├── Domaine MuniChien → Base MuniChien
                              └── Domaine Fidélité  → Base Fidélité
```

L’API centralise les validations, les calculs métier, la sécurité et les accès concurrents. Chaque logiciel fonctionne sans l’autre. Aucun dossier, table partagée ou lien obligatoire entre leurs bases ne doit créer une dépendance fonctionnelle. Le découpage interne de l’API est une décision de conception, pas une exigence imposant deux services distincts.

Ce dossier ne conçoit pas le logiciel Fidélité au-delà de ces frontières globales. La fusion et Acomba sont des évolutions éventuelles de fin de projet, après stabilisation des deux logiciels. Les opérations essentielles de MuniChien restent réalisables manuellement.

## Choix techniques actuels de l’équipe

La conversation de projet retient C#, .NET 10, WPF et SQL Server. Ces choix restent cohérents avec un client Windows et une API serveur; ils ne proviennent pas du document officiel, qui reste neutre sur les technologies (§§ 1 et 7.1). La fondation WPF est rapportée comme compilée et fonctionnelle dans la conversation; le dépôt n’a pas été vérifié dans cette révision documentaire.

Adaptation proposée de la structure existante :

| Projet | Responsabilité et statut |
| --- | --- |
| `MuniChien.App` | Client WPF existant : écrans, navigation, appels API; aucun accès SQL ni DbContext. |
| `MuniChien.Core` | Modèle métier et règles indépendantes de l’interface. |
| `MuniChien.Data` | Accès SQL Server côté serveur; EF Core prévu par l’équipe, à configurer. |
| `MuniChien.Infrastructure` | Adaptateurs techniques, rapports, adresses et sauvegardes selon leur lieu d’exécution. |
| `MuniChien.Tests` | Vérifications métier et techniques. |
| `MuniChien.Api` | Ajout proposé : API ASP.NET Core, validations et orchestration serveur. |
| `MuniChien.Contracts` | Ajout proposé si utile : contrats d’échange sans dépendance SQL ou WPF. |

L’ancienne liaison directe `App → Data → SQL Server` doit être remplacée par `App → API → Data → SQL Server`. Les références de projets déjà proposées dans la conversation doivent être revues en conséquence. Aucune modification du code n’est effectuée par ces fichiers.

## Concurrence sur six postes

Exigences (§§ 4.3 et 7.3, MUN-014) : consultation et création simultanées de dossiers différents; identifiants générés sans doublons; deuxième modification concurrente du même dossier empêchée. Afficher le nom de l’employé détenant le verrou n’est pas nécessaire.

Mécanisme proposé, à valider : verrou de modification par dossier géré par l’API, avec durée limitée et renouvellement, complété par un jeton de version vérifié lors de chaque écriture. Un second poste peut consulter, mais ne peut pas modifier tant que le verrou est détenu. Un poste ayant perdu son verrou ou présentant une version périmée ne peut pas enregistrer. L’API refuse l’écriture sans écrasement et demande un rechargement explicite.

Le périmètre du dossier verrouillé doit inclure les opérations liées susceptibles de modifier le même solde ou les mêmes informations. Les opérations sur des dossiers différents restent indépendantes. Les transactions serveur garantissent qu’une opération composée réussit entièrement ou ne laisse pas de résultat partiel. Une fermeture annuelle exige une coordination globale des écritures MuniChien, sans bloquer arbitrairement Fidélité.

## Accès et protection

Les employés n’ont pas de comptes individuels au quotidien. Consultation, recherche, création, modification courante et rapports autorisés sont accessibles directement. Un bouton « Connexion administrateur » protège uniquement les opérations sensibles : fermeture annuelle, restauration, suppressions définitives autorisées, paramètres sensibles et maintenance (§ 8.1).

L’API contrôle réellement ces droits; masquer un bouton ne suffit pas. Les postes ne reçoivent pas les identifiants de base de données. Protéger les renseignements personnels et les sauvegardes, limiter les accès et chiffrer les données sensibles lorsque la technologie le permet de manière fiable (§ 8.2–8.3). Le mécanisme précis d’authentification, la durée de session et le chiffrement sont à définir, sans prétendre que ce document établit une conformité juridique complète.

## Exploitation et sauvegardes

- Sauvegarde automatique quotidienne sur le serveur, avec deux semaines glissantes de conservation.
- Sauvegarde obligatoire avant fermeture annuelle et opérations sensibles concernées.
- Restauration réservée à l’administrateur et vérifiée sur un environnement d’essai.
- Copie sur un autre support ou appareil : recommandation future du document, pas dépendance du MVP.
- Proposition d’exploitation : contrôler le succès des sauvegardes, signaler les échecs et ne pas lancer une fermeture si sa sauvegarde préalable a échoué.

Le réseau local et le serveur sont essentiels. Internet est autorisé pour la sélection d’adresses officielles; les opérations principales ne doivent pas dépendre d’un service cloud inutile. Aucun fonctionnement hors ligne avec synchronisation ultérieure n’est spécifié. En cas de perte de connexion, afficher un état clair et empêcher les enregistrements dont la réussite ne peut être confirmée.

## Ordre de réalisation

Valider le besoin, préparer le prototype frontend et faire valider les parcours avant connexion de toute la logique aux données. Finaliser ensuite le modèle, développer l’API et les fonctions, tester sur six postes, préparer la migration Access sur copie et obtenir la validation client. Voir [ecrans.md](ecrans.md), [database.md](database.md) et [mvp.md](mvp.md).

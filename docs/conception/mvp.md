# MVP du nouveau Muni-Chien

## Objectif

Ce document définit le périmètre de la première version fonctionnelle du nouveau Muni-Chien.

Le MVP doit permettre au Refuge Animal d'utiliser une version moderne et stable du logiciel pour les opérations essentielles liées :

- aux propriétaires ;
- aux chiens ;
- aux licences ;
- aux municipalités ;
- aux paiements ;
- aux recherches courantes.

Le but n'est pas de reproduire immédiatement toutes les fonctions de l'ancien logiciel Access.

La priorité est d'obtenir une base solide, testable et réellement utilisable avant d'ajouter les fonctions plus avancées.

---

# Principe général

Le développement du MVP suivra cette logique :

```text
1. Faire fonctionner l'application
2. Faire fonctionner la base de données
3. Gérer les propriétaires
4. Gérer les chiens
5. Gérer les licences
6. Gérer les municipalités et listes de référence
7. Gérer les paiements
8. Ajouter les fonctions secondaires
9. Préparer la migration réelle
```

Le MVP doit rester suffisamment petit pour être développé et testé progressivement.

---

# Objectifs du MVP

La première version devra permettre de :

- démarrer l'application sur Windows ;
- se connecter à la base SQL centralisée ;
- rechercher un propriétaire ;
- consulter un dossier ;
- créer et modifier un propriétaire ;
- ajouter et modifier un chien ;
- créer et consulter une licence ;
- consulter l'historique des licences ;
- gérer les municipalités ;
- gérer les rues ;
- gérer les races ;
- gérer les couleurs ;
- enregistrer un paiement ;
- consulter l'historique des paiements ;
- fonctionner correctement sur plusieurs postes.

---

# Hors MVP initial

Certaines fonctions importantes de l'ancien logiciel seront conservées dans le projet, mais ne sont pas nécessaires pour la toute première version utilisable.

Elles pourront être développées après que les opérations principales soient stables.

Fonctions reportées après le noyau du MVP :

- génération complète des avis ;
- enveloppes ;
- étiquettes ;
- rapports avancés ;
- statistiques avancées ;
- fermeture annuelle complète ;
- restauration depuis l'interface ;
- système complet de comptes utilisateurs ;
- export Excel avancé ;
- cartes de fidélité.

La sauvegarde de la base devra toutefois être prévue très tôt dans le projet, même si son interface complète arrive plus tard.

---

# Phase 0 — Préparation technique

## Objectif

Créer une fondation propre avant de développer les fonctions métier.

## À réaliser

- Créer la solution .NET
- Créer le projet WPF
- Mettre en place la structure du projet
- Ajouter Entity Framework Core
- Configurer SQL Server Express pour le développement
- Créer une base de données de développement
- Configurer la chaîne de connexion
- Créer les premières migrations Entity Framework
- Vérifier que l'application peut se connecter à SQL Server
- Préparer des données fictives pour les tests

## Critère de réussite

L'application démarre et peut lire / écrire une donnée fictive dans SQL Server.

---

# Phase 1 — Municipalités et données de référence

## Objectif

Créer les données nécessaires avant de pouvoir enregistrer correctement les propriétaires et les chiens.

## Municipalités

Fonctions minimales :

- afficher les municipalités ;
- ajouter une municipalité ;
- modifier une municipalité ;
- désactiver une municipalité.

Informations minimales :

- nom ;
- code postal ;
- date limite de paiement ;
- tarif du premier chien ;
- tarif du deuxième chien ;
- tarif chenil ;
- frais de retard ;
- statut actif.

## Rues

Fonctions minimales :

- afficher les rues ;
- filtrer par municipalité ;
- ajouter ;
- modifier ;
- désactiver.

## Races

Fonctions minimales :

- afficher ;
- ajouter ;
- modifier ;
- désactiver.

## Couleurs

Fonctions minimales :

- afficher ;
- ajouter ;
- modifier ;
- désactiver.

## Critère de réussite

Les données de référence peuvent être créées et utilisées dans les autres écrans.

---

# Phase 2 — Gestion des propriétaires

## Objectif

Permettre la création et la gestion complète d'un dossier propriétaire.

## Écran Recherche

Recherche minimale par :

- numéro de dossier ;
- nom ;
- prénom.

Recherche souhaitée dès que possible par :

- téléphone ;
- adresse ;
- courriel ;
- numéro de licence.

## Fiche propriétaire

Informations minimales :

- numéro de dossier ;
- prénom ;
- nom ;
- date de naissance ;
- numéro civique ;
- rue ;
- appartement ;
- municipalité ;
- code postal ;
- téléphone ;
- cellulaire ;
- courriel ;
- propriétaire de chenil ;
- actif / inactif ;
- commentaire.

## Actions minimales

- créer un propriétaire ;
- modifier un propriétaire ;
- désactiver un propriétaire ;
- ouvrir un propriétaire depuis la recherche.

## Validations

Le système devra au minimum empêcher :

- deux numéros de dossier identiques ;
- l'enregistrement d'un dossier sans les champs obligatoires ;
- une municipalité inexistante ;
- des formats clairement invalides lorsque pertinent.

## Critère de réussite

Un utilisateur peut créer, retrouver et modifier un propriétaire depuis l'application.

---

# Phase 3 — Gestion des chiens

## Objectif

Permettre d'associer un ou plusieurs chiens à un propriétaire.

## Informations minimales

- nom ;
- propriétaire ;
- race ;
- couleur ;
- sexe ;
- stérilisé ;
- actif / archivé.

## Actions minimales

- ajouter un chien ;
- modifier un chien ;
- archiver un chien ;
- afficher les chiens d'un propriétaire ;
- ouvrir un chien depuis la fiche propriétaire.

## Important

Chaque chien doit posséder son propre identifiant unique interne.

Il ne faut pas reproduire la limite de l'ancien Access où les chiens dépendent principalement du numéro de propriétaire sans identifiant robuste indépendant.

## Critère de réussite

Un propriétaire peut posséder plusieurs chiens et chaque chien est identifiable indépendamment.

---

# Phase 4 — Gestion des licences

## Objectif

Créer une véritable gestion des licences indépendante du chien.

## Informations minimales

- numéro de licence ;
- chien ;
- municipalité ;
- année ;
- date d'émission ;
- date d'expiration si nécessaire ;
- statut.

## Actions minimales

- créer une licence ;
- modifier une licence ;
- annuler / désactiver une licence ;
- afficher la licence active d'un chien ;
- afficher l'historique des licences ;
- rechercher un numéro de licence.

## Règle importante

Le numéro de licence ne doit plus être stocké directement dans la table `Chien`.

Chaque licence doit être une entrée distincte.

## À confirmer avant finalisation

- unicité du numéro de licence ;
- réutilisation possible d'un numéro ;
- fonctionnement exact d'une licence d'une année à l'autre ;
- date d'expiration réelle.

## Critère de réussite

Un chien peut posséder plusieurs licences historiques et une licence courante identifiable.

---

# Phase 5 — Paiements

## Objectif

Permettre d'enregistrer et de consulter les paiements d'un propriétaire.

## Résumé financier minimal

Afficher :

- montant dû ;
- frais de retard ;
- total payé ;
- solde restant.

## Paiement

Informations minimales :

- propriétaire ;
- date ;
- montant ;
- mode de paiement ;
- numéro de reçu ;
- statut du traitement journalier.

## Actions minimales

- enregistrer un paiement ;
- consulter les paiements ;
- afficher le solde recalculé ;
- empêcher un montant invalide.

## Modes de paiement

À confirmer avec le client.

Valeurs possibles :

- comptant ;
- débit ;
- crédit ;
- chèque ;
- autre.

## Critère de réussite

Un paiement enregistré apparaît dans l'historique et modifie correctement le solde présenté.

---

# Phase 6 — Paiements journaliers

## Objectif

Reproduire le besoin métier actuel lié au traitement des paiements quotidiens.

## MVP proposé

- afficher les paiements non traités ;
- afficher le total ;
- générer une liste ou un rapport simple ;
- marquer les paiements comme traités après confirmation.

## À confirmer

Le comportement exact de l'ancien champ `Z` doit être validé avant de considérer cette fonction comme définitive.

## Critère de réussite

Les paiements d'une journée peuvent être identifiés comme traités sans perdre leur historique.

---

# Phase 7 — Sécurité des données et concurrence

## Objectif

S'assurer que l'application reste fiable avec plusieurs postes.

## À mettre en place

- clés étrangères SQL ;
- transactions pour les opérations importantes ;
- validations côté application ;
- gestion des erreurs ;
- détection des conflits de modification lorsque nécessaire ;
- `RowVersion` ou mécanisme équivalent pour certaines tables ;
- journaux techniques pour les erreurs importantes.

## Cas à tester

- deux postes ouvrent le même propriétaire ;
- deux postes modifient le même dossier ;
- deux paiements sont enregistrés presque en même temps ;
- deux utilisateurs tentent d'utiliser le même numéro de licence.

## Critère de réussite

Le logiciel ne doit pas écraser silencieusement des modifications importantes.

---

# Phase 8 — Sauvegarde minimale

## Objectif

Ne jamais dépendre uniquement de la base active.

## MVP minimal

Avant l'utilisation réelle du logiciel :

- une méthode documentée de sauvegarde SQL doit exister ;
- les sauvegardes doivent être testées ;
- une restauration doit être testée dans un environnement de développement.

L'interface graphique complète de sauvegarde / restauration peut arriver plus tard.

## Critère de réussite

L'équipe sait créer une sauvegarde et prouver qu'elle peut être restaurée.

---

# Phase 9 — Tests avec données fictives

## Objectif

Tester les principaux parcours avant toute migration réelle.

## Scénarios minimaux

### Propriétaire

```text
Créer
→ rechercher
→ ouvrir
→ modifier
→ sauvegarder
```

### Chien

```text
Ouvrir propriétaire
→ ajouter chien
→ modifier chien
→ consulter chien
```

### Licence

```text
Ouvrir chien
→ créer licence
→ rechercher numéro
→ consulter historique
```

### Paiement

```text
Ouvrir propriétaire
→ enregistrer paiement
→ vérifier historique
→ vérifier solde
```

### Multi-postes

```text
Poste A modifie un dossier
+
Poste B utilise la même base
→ vérifier cohérence
```

---

# Phase 10 — Migration pilote depuis Access

## Objectif

Tester la migration avant de toucher aux données réelles du client.

## Procédure

1. Utiliser une copie de la base Access.
2. Exporter les données nécessaires.
3. Importer dans une base SQL de test.
4. Vérifier les quantités.
5. Vérifier les relations.
6. Vérifier plusieurs dossiers manuellement.
7. Vérifier les chiens.
8. Vérifier les licences.
9. Vérifier les paiements.
10. Corriger le processus de migration.

## Important

Aucune base réelle contenant des données personnelles ne doit être ajoutée au dépôt GitHub.

---

# Phase 11 — Validation interne

Avant présentation au client, l'équipe devra tester le MVP.

## Checklist minimale

- [ ] L'application démarre correctement
- [ ] La connexion SQL fonctionne
- [ ] La recherche fonctionne
- [ ] La création d'un propriétaire fonctionne
- [ ] La modification d'un propriétaire fonctionne
- [ ] L'ajout d'un chien fonctionne
- [ ] La modification d'un chien fonctionne
- [ ] La création d'une licence fonctionne
- [ ] La recherche de licence fonctionne
- [ ] L'historique des licences fonctionne
- [ ] L'enregistrement d'un paiement fonctionne
- [ ] Le calcul du solde fonctionne
- [ ] Les municipalités fonctionnent
- [ ] Les rues fonctionnent
- [ ] Les races fonctionnent
- [ ] Les couleurs fonctionnent
- [ ] Plusieurs postes peuvent utiliser la même base
- [ ] Une sauvegarde de test a été créée
- [ ] Une restauration de test a été réussie
- [ ] Aucune donnée réelle du client n'est présente sur GitHub

---

# MVP fonctionnel attendu

Une première version peut être considérée comme réellement utilisable lorsque le parcours suivant fonctionne :

```text
Ouverture de Muni-Chien
        |
        v
Recherche d'un propriétaire
        |
        v
Consultation du dossier
        |
        ├── Modifier les coordonnées
        |
        ├── Ajouter / modifier un chien
        |
        ├── Créer / consulter une licence
        |
        └── Enregistrer / consulter un paiement
```

Le tout doit fonctionner sur la base SQL centralisée utilisée par plusieurs postes.

---

# Fonctions après le MVP principal

Une fois le noyau stable, les fonctions suivantes pourront être ajoutées.

## Avis

- modèles d'avis ;
- génération ;
- aperçu ;
- historique ;
- impression ;
- PDF ;
- enveloppes ;
- étiquettes.

## Rapports

- propriétaires ;
- chiens ;
- licences ;
- paiements ;
- soldes ;
- revenus ;
- statistiques.

## Administration

- interface de sauvegarde ;
- interface de restauration ;
- fermeture annuelle ;
- journalisation ;
- comptes utilisateurs ;
- permissions.

## Autres modules

- cartes de fidélité ;
- fonctions supplémentaires demandées par le client.

---

# Ordre de développement recommandé

```text
1. Fondation .NET / WPF / SQL Server
2. Modèle de données + migrations EF Core
3. Municipalités / rues / races / couleurs
4. Recherche
5. Propriétaires
6. Chiens
7. Licences
8. Paiements
9. Paiements journaliers
10. Concurrence / validations / erreurs
11. Sauvegarde testée
12. Tests multi-postes
13. Migration pilote Access
14. Validation du MVP
15. Avis / rapports / administration avancée
```

---

# Définition de terminé

Une fonctionnalité n'est pas considérée comme terminée uniquement parce que son écran existe.

Pour être considérée comme terminée, elle doit :

- fonctionner avec la base SQL ;
- valider les données ;
- gérer les erreurs de base ;
- être testée avec des données fictives ;
- ne pas casser les autres fonctions ;
- être poussée sur GitHub ;
- être relue avant fusion dans `main`.

---

# Points à confirmer avec le client avant la version finale

- Champs obligatoires des propriétaires
- Champs obligatoires des chiens
- Fonctionnement exact des numéros de licence
- Durée de validité des licences
- Tarification exacte par municipalité
- Règle exacte du deuxième chien
- Règle exacte des chenils
- Fonctionnement des frais de retard
- Modes de paiement utilisés
- Signification exacte de l'option `Municipalité` dans Paiement
- Signification officielle du champ `Z`
- Rapports réellement indispensables
- Avis réellement utilisés
- Besoin de comptes utilisateurs
- Permissions nécessaires
- Emplacement du serveur
- Emplacement des sauvegardes
- Fréquence des sauvegardes
- Besoin futur d'accès à distance

---

# Décision actuelle

Le MVP se concentre d'abord sur le cœur opérationnel de Muni-Chien :

```text
Propriétaires
+
Chiens
+
Licences
+
Municipalités
+
Paiements
+
Recherche
```

Les fonctions administratives et de rapports seront ajoutées progressivement une fois ce noyau stable.

Cette approche permet de tester rapidement la nouvelle architecture tout en réduisant les risques avant la migration complète de l'ancien système.

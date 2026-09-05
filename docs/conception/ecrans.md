# Écrans du nouveau Muni-Chien

## Objectif

Ce document décrit les écrans prévus pour la nouvelle version de Muni-Chien.

Il s'appuie sur l'analyse de l'ancienne application Access, mais vise une interface plus claire, plus moderne et plus sécuritaire.

L'objectif est de conserver les fonctions importantes de l'ancien logiciel tout en améliorant :

- la navigation ;
- la lisibilité ;
- la séparation des responsabilités ;
- la validation des données ;
- la sécurité des opérations sensibles ;
- la rapidité d'utilisation au quotidien.

---

# Navigation générale

La navigation principale proposée est la suivante :

```text
Accueil
├── Recherche
│   └── Fiche propriétaire
│       ├── Chiens
│       ├── Licences
│       ├── Paiements
│       └── Avis
│
├── Paiements journaliers
├── Avis
├── Rapports
├── Paramètres
│   ├── Municipalités
│   ├── Rues
│   ├── Races
│   ├── Couleurs
│   └── Modèles d'avis
│
└── Administration
    ├── Sauvegarde
    ├── Restauration
    └── Fermeture annuelle
```

L'accès exact à certaines sections pourra être ajusté selon les besoins du client.

---

# Écran Accueil

## Rôle

L'écran `Accueil` est le point d'entrée principal de l'application.

Il doit permettre d'accéder rapidement aux fonctions les plus utilisées.

## Éléments proposés

- Nom de l'application
- Municipalité active
- Utilisateur connecté, si une gestion de comptes est ajoutée
- Bouton `Recherche`
- Bouton `Paiements journaliers`
- Bouton `Avis`
- Bouton `Rapports`
- Bouton `Paramètres`
- Bouton `Administration`
- Indication de l'état de connexion à la base de données

## Améliorations par rapport à l'ancien système

- Interface plus claire
- Boutons plus explicites
- Suppression des icônes ambiguës
- Affichage clair de la municipalité active
- Accès rapide aux opérations importantes

---

# Écran Recherche

## Rôle

Permet de retrouver rapidement un propriétaire ou un chien.

## Recherche proposée

Un champ de recherche unique pourrait permettre de rechercher par :

- numéro de dossier ;
- prénom ;
- nom ;
- adresse ;
- téléphone ;
- courriel ;
- nom du chien ;
- numéro de licence.

## Filtres

Filtres proposés :

- Municipalité
- Actifs seulement
- Tous
- Propriétaires
- Chiens
- Licence

## Résultats

Le tableau de résultats pourrait afficher :

- Numéro de dossier
- Prénom
- Nom
- Adresse
- Municipalité
- Nombre de chiens
- Solde
- Statut

## Actions

Depuis un résultat :

- Ouvrir la fiche propriétaire
- Accéder directement aux paiements
- Accéder directement au chien ou à la licence recherchée

---

# Écran Fiche propriétaire

## Rôle

La fiche propriétaire sera le cœur de l'application.

Elle permettra de consulter et modifier toutes les informations importantes liées à un propriétaire.

## Section identité

Champs proposés :

- Numéro de dossier
- Prénom
- Nom
- Date de naissance
- Statut actif / inactif
- Propriétaire d'un chenil

## Section coordonnées

- Numéro civique
- Rue
- Appartement
- Municipalité
- Code postal
- Téléphone
- Cellulaire
- Courriel

## Section informations complémentaires

- Commentaire
- Avis général
- Date de création
- Date de dernière modification

## Résumé financier

La fiche affichera :

- Montant facturé
- Frais de retard
- Total payé
- Solde actuel

Un bouton permettra d'ouvrir la section `Paiements`.

## Résumé des avis

Afficher :

- Dernier avis envoyé
- Date du dernier avis
- Bouton pour voir l'historique complet

## Section chiens

La fiche affichera les chiens associés au propriétaire.

Pour chaque chien :

- Nom
- Race
- Couleur
- Sexe
- Stérilisé
- Licence active
- Statut

## Actions principales

- Modifier le propriétaire
- Ajouter un chien
- Ouvrir un chien
- Enregistrer un paiement
- Créer un avis
- Désactiver le dossier
- Retour à la recherche

---

# Écran Chien

## Rôle

Permet de gérer les informations d'un chien indépendamment de la fiche générale du propriétaire.

## Informations proposées

- Nom
- Propriétaire
- Race
- Couleur
- Sexe
- Stérilisé
- Statut actif / archivé
- Date de création

## Licences

Une section dédiée affichera :

- Numéro de licence
- Année
- Municipalité
- Date d'émission
- Date d'expiration
- Statut
- Montant

## Actions

- Modifier le chien
- Ajouter une licence
- Renouveler une licence
- Consulter l'historique des licences
- Archiver le chien

---

# Écran Licences

## Rôle

Permet de gérer les licences de manière plus claire que dans l'ancien système.

Les licences ne seront plus simplement un champ du chien.

## Liste proposée

Colonnes :

- Numéro de licence
- Chien
- Propriétaire
- Municipalité
- Année
- Statut
- Date d'expiration

## Filtres

- Municipalité
- Année
- Active
- Expirée
- Annulée
- Numéro de licence

## Actions

- Créer une licence
- Renouveler une licence
- Annuler une licence
- Ouvrir le chien
- Ouvrir le propriétaire

---

# Écran Paiements

## Rôle

Permet de consulter la situation financière d'un propriétaire et d'enregistrer de nouveaux paiements.

## Résumé financier

Afficher clairement :

```text
Montants facturés
+ Frais
- Paiements
= Solde actuel
```

## Historique

Le tableau des paiements affichera :

- Numéro de reçu
- Date
- Montant
- Mode de paiement
- Statut journalier
- Note éventuelle

## Nouveau paiement

Champs proposés :

- Date
- Montant
- Mode de paiement
- Option liée à la municipalité, si ce comportement est confirmé
- Note

## Modes de paiement

À confirmer avec le client.

Exemples possibles :

- Comptant
- Débit
- Crédit
- Chèque
- Autre

## Actions

- Enregistrer
- Annuler
- Imprimer / générer un reçu
- Voir le détail du paiement

## Sécurité

Le système devra :

- empêcher les montants invalides ;
- demander confirmation pour certaines opérations ;
- conserver l'historique des paiements ;
- éviter la suppression accidentelle d'un paiement.

---

# Écran Paiements journaliers

## Rôle

Remplace la fonction actuelle de traitement des paiements journaliers.

## Contenu proposé

Afficher les paiements non encore traités dans le rapport journalier.

Colonnes :

- Numéro de reçu
- Date
- Propriétaire
- Montant
- Mode de paiement
- Municipalité

## Actions

- Générer un rapport
- Aperçu
- Imprimer
- Exporter en PDF
- Marquer les paiements comme traités

## Important

Le comportement exact du champ `Z` de l'ancien système devra être confirmé avant développement final.

---

# Écran Avis

## Rôle

Permet de générer des avis pour un ou plusieurs propriétaires.

## Étape 1 — Type de document

Choix possibles :

- Avis
- Enveloppe
- Étiquettes

## Étape 2 — Modèle

Pour un avis :

- Choix du modèle
- Aperçu du titre
- Aperçu du contenu

## Étape 3 — Destinataires

Options proposées :

- Un propriétaire
- Plusieurs propriétaires
- Tous les propriétaires ayant un solde
- Filtrer par municipalité
- Filtrer par rue
- Filtrer selon d'autres critères si nécessaire

## Étape 4 — Aperçu

Avant impression :

- Nombre de documents
- Destinataires
- Modèle choisi
- Date limite
- Montant dû si applicable

## Actions

- Aperçu
- Imprimer
- Exporter en PDF
- Annuler

## Historique

Chaque avis envoyé ou généré pourra être conservé dans l'historique du propriétaire.

---

# Écran Rapports

## Rôle

Regroupe les différentes listes et statistiques.

## Catégories proposées

### Propriétaires

- Liste des propriétaires
- Propriétaires actifs
- Propriétaires inactifs

### Chiens

- Liste des chiens
- Par race
- Par municipalité
- Par licence

### Finances

- Soldes impayés
- Frais de retard
- Paiements
- Revenus
- Revenus par municipalité
- Revenus par période

### Licences

- Licences actives
- Licences expirées
- Licences par année
- Licences par municipalité

## Filtres

- Municipalité
- Date de début
- Date de fin
- Année
- Statut

## Sorties

- Aperçu
- Impression
- Export PDF
- Export CSV / Excel si nécessaire

---

# Écran Paramètres

## Rôle

Regroupe les paramètres généraux et les listes de référence.

## Sections proposées

- Informations générales
- Municipalités
- Rues
- Races
- Couleurs
- Modèles d'avis
- Options d'impression
- Options liées aux licences

---

# Écran Municipalités

## Rôle

Permet de gérer les municipalités et leurs règles de tarification.

## Informations

- Nom
- Code postal
- Date limite
- Tarif premier chien
- Tarif deuxième chien
- Tarif chenil
- Frais de retard
- Statut actif / inactif

## Actions

- Ajouter
- Modifier
- Désactiver

La suppression définitive devrait être évitée si la municipalité est utilisée dans l'historique.

---

# Écran Rues

## Rôle

Permet de gérer les rues associées à chaque municipalité.

## Fonctions

- Recherche
- Ajout
- Modification
- Désactivation
- Filtre par municipalité

---

# Écran Races

## Rôle

Permet de gérer la liste des races.

## Fonctions

- Recherche
- Ajout
- Modification
- Désactivation

---

# Écran Couleurs

## Rôle

Permet de gérer la liste des couleurs.

## Fonctions

- Recherche
- Ajout
- Modification
- Désactivation

---

# Écran Modèles d'avis

## Rôle

Permet de créer et modifier les modèles d'avis.

## Informations

- Code
- Titre
- Sous-titre
- Texte
- Statut actif

## Amélioration proposée

Permettre l'utilisation de variables dans le texte.

Exemples :

```text
{Prenom}
{Nom}
{NumeroDossier}
{MontantDu}
{DateLimite}
{Municipalite}
```

## Actions

- Ajouter
- Modifier
- Aperçu
- Désactiver

---

# Écran Administration

## Rôle

Regroupe les fonctions sensibles.

Ces fonctions pourront être limitées aux utilisateurs autorisés si une gestion de comptes est mise en place.

Sections :

- Sauvegarde
- Restauration
- Fermeture annuelle

---

# Écran Sauvegarde

## Rôle

Permet de créer une sauvegarde de la base SQL.

## Informations proposées

- Date de dernière sauvegarde
- Emplacement
- État de la dernière opération

## Actions

- Créer une sauvegarde
- Ouvrir le dossier des sauvegardes
- Vérifier une sauvegarde

---

# Écran Restauration

## Rôle

Permet de restaurer une sauvegarde.

## Sécurité proposée

Avant restauration :

1. Sélection du fichier
2. Vérification de la sauvegarde
3. Création automatique d'une sauvegarde de sécurité de la base actuelle
4. Confirmation explicite
5. Restauration
6. Rapport de réussite ou d'erreur

---

# Écran Fermeture annuelle

## Rôle

Remplace la fonction `Remise à zéro` de l'ancien système.

## Objectif

Préparer la nouvelle année sans supprimer inutilement l'historique.

## Assistant proposé

L'opération pourrait être présentée sous forme d'étapes :

```text
1. Vérification du système
2. Vérification des paiements journaliers
3. Sauvegarde
4. Aperçu des opérations
5. Confirmation
6. Fermeture de l'année
7. Création de la nouvelle année
8. Rapport final
```

## Affichage avant confirmation

Le système devra afficher clairement :

- année actuelle ;
- nouvelle année ;
- nombre de propriétaires concernés ;
- nombre de chiens ;
- soldes reportés ;
- frais concernés ;
- licences concernées.

## Sécurité

- Transaction de base de données
- Sauvegarde obligatoire
- Confirmation renforcée
- Interdiction de lancer deux fermetures simultanément
- Journalisation de l'opération

---

# Messages et confirmations

La nouvelle application devra utiliser des messages clairs.

## Exemples

Avant suppression ou désactivation :

```text
Voulez-vous vraiment désactiver ce chien ?
```

Avant une restauration :

```text
Cette opération remplacera les données actuellement utilisées.

Une sauvegarde de sécurité sera créée avant la restauration.

Voulez-vous continuer ?
```

Avant la fermeture annuelle :

```text
Vous êtes sur le point de fermer l'année 2026 et de préparer l'année 2027.

Cette opération modifiera plusieurs données.

Une sauvegarde sera créée automatiquement avant de continuer.
```

---

# Gestion des erreurs

Les messages techniques ne devront pas être affichés directement aux utilisateurs.

Exemple à éviter :

```text
SqlException 547 FK_Proprietaire_Municipalite
```

Exemple souhaité :

```text
Impossible de supprimer cette municipalité puisqu'elle est encore utilisée par des dossiers existants.
```

Les détails techniques pourront être enregistrés dans un journal pour le diagnostic.

---

# Interface et ergonomie

## Principes

La nouvelle interface devra privilégier :

- une apparence simple et professionnelle ;
- des boutons clairement nommés ;
- des formulaires bien séparés ;
- un accès rapide aux fonctions quotidiennes ;
- une taille de texte lisible ;
- des tableaux redimensionnables ;
- une navigation cohérente ;
- des validations visibles.

## Couleurs

Les couleurs pourront être utilisées pour faciliter la compréhension.

Exemples :

- Vert : actif / payé / succès
- Orange : attention
- Rouge : impayé / erreur / opération dangereuse
- Gris : inactif / archivé

Les couleurs ne devront jamais être le seul moyen de transmettre une information.

---

# Recherche et rapidité d'utilisation

Puisque l'ancienne application est utilisée quotidiennement, la nouvelle version devra rester rapide.

La recherche devrait être accessible très rapidement après l'ouverture du logiciel.

Objectif proposé :

```text
Accueil
→ Recherche
→ Dossier
```

en seulement quelques actions.

---

# Écrans prioritaires pour le MVP

Les écrans essentiels pour une première version fonctionnelle sont :

1. Accueil
2. Recherche
3. Fiche propriétaire
4. Chien
5. Licences
6. Paiements
7. Municipalités
8. Rues
9. Races
10. Couleurs

Les écrans suivants pourront ensuite compléter le MVP ou arriver dans une deuxième étape :

- Avis
- Paiements journaliers
- Rapports avancés
- Modèles d'avis
- Sauvegarde
- Restauration
- Fermeture annuelle

La priorité exacte sera définie dans `mvp.md`.

---

# Points restant à confirmer avec le client

- Quels écrans sont utilisés le plus souvent ?
- La municipalité doit-elle être choisie à l'ouverture comme dans l'ancien logiciel ?
- Les employés doivent-ils voir plusieurs municipalités en même temps ?
- Faut-il conserver une recherche distincte par licence ou utiliser une recherche globale ?
- Quels modes de paiement doivent être proposés ?
- Quels rapports sont réellement utilisés ?
- Les impressions d'enveloppes et d'étiquettes doivent-elles absolument être conservées ?
- Des comptes utilisateurs sont-ils nécessaires ?
- Qui doit avoir accès aux fonctions d'administration ?
- Le client souhaite-t-il pouvoir exporter vers Excel ?
- Les reçus doivent-ils toujours être imprimables ?
- Quels champs doivent être obligatoires lors de la création d'un propriétaire ?
- Quels champs doivent être obligatoires lors de la création d'un chien ?

---

# Décision actuelle

La nouvelle interface conservera les grands concepts de l'ancien Muni-Chien, mais les fonctions seront mieux séparées.

La fiche propriétaire restera le centre de la gestion quotidienne.

La recherche, les paiements et la gestion des chiens devront rester accessibles rapidement.

Les opérations administratives sensibles seront séparées des opérations courantes.

# Modèle de données du nouveau Muni-Chien

## Objectif

Ce document décrit la structure proposée pour la nouvelle base de données de Muni-Chien.

Le modèle est basé sur l'analyse de l'ancienne application Microsoft Access, mais il corrige plusieurs limites de la structure actuelle.

L'objectif est de :

- conserver les données utiles existantes ;
- améliorer les relations entre les données ;
- permettre l'utilisation simultanée sur plusieurs postes ;
- conserver davantage d'historique ;
- simplifier les futures évolutions du logiciel ;
- faciliter la migration des données Access vers SQL Server.

---

## Principes généraux

La nouvelle base de données utilisera des identifiants uniques internes pour les principales entités.

Chaque table principale possédera donc une clé primaire indépendante.

Les relations entre tables seront représentées par des clés étrangères explicites.

Les tables temporaires utilisées dans l'ancien Access ne seront pas reproduites directement dans la nouvelle architecture.

---

# Vue générale du modèle

```text
Municipalite
    |
    ├── Proprietaire
    |      |
    |      ├── Chien
    |      |     |
    |      |     └── Licence
    |      |
    |      ├── Paiement
    |      |
    |      └── AvisEnvoye
    |
    └── Rue

Race ────── Chien
Couleur ─── Chien

AvisModele ─── AvisEnvoye
```

---

# Table Proprietaire

## Rôle

Contient les informations relatives aux propriétaires.

Dans l'ancien système, cette information se trouve principalement dans la table `Propriétaire`.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| ProprietaireId | int | Clé primaire |
| NumeroDossier | int | Ancien numéro de dossier / identifiant métier |
| Prenom | nvarchar | Prénom |
| Nom | nvarchar | Nom |
| DateNaissance | date nullable | Date de naissance |
| NumeroCivique | nvarchar | Numéro civique |
| Appartement | nvarchar nullable | Appartement |
| RueId | int nullable | Rue associée |
| MunicipaliteId | int | Municipalité du propriétaire |
| CodePostal | nvarchar | Code postal |
| Telephone | nvarchar nullable | Téléphone principal |
| Cellulaire | nvarchar nullable | Téléphone cellulaire |
| Courriel | nvarchar nullable | Adresse courriel |
| EstChenil | bit | Indique si le propriétaire possède un chenil |
| Actif | bit | Statut actif / inactif |
| Commentaire | nvarchar(max) nullable | Commentaire libre |
| AvisGeneral | nvarchar(max) nullable | Information générale liée aux avis |
| DateCreation | datetime2 | Date de création du dossier |
| DateModification | datetime2 | Dernière modification |

## Contraintes proposées

- `ProprietaireId` est la clé primaire.
- `NumeroDossier` doit être unique.
- `MunicipaliteId` est une clé étrangère vers `Municipalite`.
- `RueId` est une clé étrangère facultative vers `Rue`.

## Remarque

Le numéro de dossier historique doit être conservé pour faciliter la migration et permettre aux employés de retrouver les mêmes dossiers qu'avant.

---

# Table Chien

## Rôle

Contient les chiens appartenant à un propriétaire.

Dans l'ancien système, la table `Chien` ne possède pas de véritable identifiant unique indépendant.

La nouvelle version doit corriger cette limite.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| ChienId | int | Clé primaire |
| ProprietaireId | int | Propriétaire du chien |
| Nom | nvarchar | Nom du chien |
| RaceId | int nullable | Race |
| CouleurId | int nullable | Couleur |
| Sexe | nvarchar nullable | Sexe |
| EstSterilise | bit | Stérilisé ou non |
| Actif | bit | Chien actif / archivé |
| DateCreation | datetime2 | Date de création |
| DateModification | datetime2 | Dernière modification |

## Relations

- `ProprietaireId` → `Proprietaire`
- `RaceId` → `Race`
- `CouleurId` → `Couleur`

## Important

Le numéro de licence ne sera plus enregistré directement dans `Chien`.

Les licences seront conservées dans une table indépendante afin de garder un historique par année.

---

# Table Licence

## Rôle

Représente une licence attribuée à un chien.

Cette table n'existe pas dans l'ancien système.

Dans Access, le numéro de licence est stocké directement dans le champ `Chien.Licence`.

La création d'une vraie table `Licence` constitue donc une amélioration importante du nouveau modèle.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| LicenceId | int | Clé primaire |
| ChienId | int | Chien concerné |
| MunicipaliteId | int | Municipalité émettrice |
| NumeroLicence | nvarchar | Numéro de licence |
| Annee | int | Année de validité |
| DateEmission | date nullable | Date d'émission |
| DateExpiration | date nullable | Date d'expiration |
| Statut | nvarchar | Active, expirée, annulée, etc. |
| Montant | decimal(10,2) nullable | Montant associé à la licence |
| DateCreation | datetime2 | Date de création |

## Contraintes proposées

- `LicenceId` est la clé primaire.
- `ChienId` est une clé étrangère vers `Chien`.
- `MunicipaliteId` est une clé étrangère vers `Municipalite`.
- Une contrainte d'unicité devra empêcher l'utilisation accidentelle du même numéro de licence lorsqu'il doit être unique.

## Historique

La licence ne devrait normalement pas être supprimée lors du changement d'année.

Une nouvelle licence ou une nouvelle période de validité pourra être créée.

Cela permettra de connaître l'historique complet des licences d'un chien.

---

# Table Paiement

## Rôle

Contient les paiements effectués par les propriétaires.

L'ancien système possède déjà une table `Paiement`.

Le principe de conserver chaque paiement comme une opération distincte doit être conservé.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| PaiementId | int | Clé primaire |
| ProprietaireId | int | Propriétaire |
| DatePaiement | datetime2 | Date du paiement |
| Montant | decimal(10,2) | Montant payé |
| ModePaiement | nvarchar nullable | Comptant, débit, crédit, chèque, autre |
| PaiementMunicipalite | bit | Correspond à l'ancien indicateur municipalité, à confirmer |
| TraiteJournalier | bit | Indique si le paiement a été inclus dans la fermeture journalière |
| NumeroRecu | int nullable | Numéro de reçu |
| Note | nvarchar(max) nullable | Note optionnelle |
| DateCreation | datetime2 | Date d'enregistrement |

## Relations

- `ProprietaireId` → `Proprietaire`

## Important

Dans l'ancien système, le champ `Z` semble indiquer si un paiement a déjà été traité dans le processus de paiements journaliers.

Dans la nouvelle version, ce concept devra recevoir un nom plus explicite, par exemple `TraiteJournalier`.

Le comportement exact devra être confirmé avec le client avant développement final.

---

# Gestion du solde

## Ancien fonctionnement

Le solde restant est calculé à partir :

```text
Solde dû
+ Frais de retard
- Paiements effectués
```

## Proposition

Éviter autant que possible de stocker une valeur de solde qui pourrait devenir incohérente avec les opérations financières.

Le système devrait conserver :

- les montants facturés ;
- les frais ;
- les paiements ;
- les ajustements éventuels.

Le solde pourra ensuite être calculé à partir de ces opérations.

Pour le MVP, une structure plus simple pourra être utilisée si nécessaire, mais elle devra toujours conserver l'historique des paiements.

---

# Table TransactionFinanciere

## Statut

Table proposée pour une version plus robuste du système.

Elle peut être utilisée dès le MVP ou ajoutée dans une étape suivante selon la complexité retenue.

## Rôle

Permet de conserver l'historique complet de ce qui modifie le solde d'un propriétaire.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| TransactionId | int | Clé primaire |
| ProprietaireId | int | Propriétaire |
| Type | nvarchar | Licence, frais retard, ajustement, paiement, etc. |
| Montant | decimal(10,2) | Montant positif ou négatif |
| DateTransaction | datetime2 | Date |
| Description | nvarchar nullable | Description |
| Annee | int nullable | Année concernée |
| ReferenceId | int nullable | Référence vers une autre entité si nécessaire |

## Avantage

Cette approche permettrait de ne plus dépendre d'un champ `Solde dû` modifié manuellement par plusieurs requêtes.

---

# Table Municipalite

## Rôle

Contient les municipalités desservies par Muni-Chien et leurs paramètres de tarification.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| MunicipaliteId | int | Clé primaire |
| Nom | nvarchar | Nom |
| CodePostal | nvarchar nullable | Code postal |
| DateLimitePaiement | date nullable | Date limite |
| MontantPremierChien | decimal(10,2) | Tarif normal |
| MontantChenil | decimal(10,2) | Tarif chenil |
| FraisRetard | decimal(10,2) | Frais de retard |
| DeuxiemeChienTarifDifferent | bit | Active le tarif différent |
| MontantDeuxiemeChien | decimal(10,2) nullable | Tarif du deuxième chien |
| Active | bit | Municipalité active ou non |

## Important

Les paramètres de municipalité sont directement liés aux règles de calcul du système.

La suppression physique d'une municipalité déjà utilisée devrait être évitée.

On privilégiera un statut actif / inactif.

---

# Table Rue

## Rôle

Contient les rues reconnues pour chaque municipalité.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| RueId | int | Clé primaire |
| MunicipaliteId | int | Municipalité |
| Nom | nvarchar | Nom de la rue |
| Active | bit | Rue active ou archivée |

## Contraintes

Une même rue ne devrait pas être ajoutée plusieurs fois dans la même municipalité.

Relation :

`MunicipaliteId` → `Municipalite`

---

# Table Race

## Rôle

Liste de référence des races de chiens.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| RaceId | int | Clé primaire |
| Nom | nvarchar | Nom de la race |
| Active | bit | Active ou archivée |

## Remarque

Une race utilisée par un chien ne devrait pas être supprimée physiquement.

---

# Table Couleur

## Rôle

Liste de référence des couleurs de chiens.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| CouleurId | int | Clé primaire |
| Nom | nvarchar | Couleur |
| Active | bit | Active ou archivée |

---

# Table AvisModele

## Rôle

Contient les modèles d'avis configurables.

Correspond à la table / section `Avis` de l'ancien logiciel.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| AvisModeleId | int | Clé primaire |
| Code | nvarchar | Code de l'avis |
| Titre | nvarchar | Titre |
| SousTitre | nvarchar nullable | Sous-titre |
| Texte | nvarchar(max) | Contenu |
| Actif | bit | Modèle actif ou archivé |

---

# Table AvisEnvoye

## Rôle

Conserve l'historique des avis produits ou envoyés aux propriétaires.

L'ancien système conserve surtout le code et la date du dernier avis directement dans le propriétaire.

La nouvelle version devrait conserver tout l'historique.

## Champs proposés

| Champ | Type proposé | Description |
|---|---|---|
| AvisEnvoyeId | int | Clé primaire |
| ProprietaireId | int | Propriétaire |
| AvisModeleId | int nullable | Modèle utilisé |
| DateAvis | datetime2 | Date |
| MontantDu | decimal(10,2) nullable | Solde au moment de l'avis |
| DateLimitePaiement | date nullable | Date indiquée dans l'avis |
| TypeDocument | nvarchar nullable | Avis, enveloppe, étiquette, etc. |
| Note | nvarchar(max) nullable | Information supplémentaire |

## Relations

- `ProprietaireId` → `Proprietaire`
- `AvisModeleId` → `AvisModele`

---

# Table Utilisateur

## Statut

À confirmer avec le client.

## Rôle possible

Permettre d'identifier les personnes utilisant le logiciel et de limiter certaines fonctions sensibles.

## Champs possibles

| Champ | Type proposé | Description |
|---|---|---|
| UtilisateurId | int | Clé primaire |
| NomUtilisateur | nvarchar | Identifiant |
| NomAffichage | nvarchar | Nom affiché |
| MotDePasseHash | nvarchar nullable | Si authentification locale |
| Role | nvarchar | Utilisateur, administrateur, etc. |
| Actif | bit | Compte actif |

## Fonctions potentiellement protégées

- Configuration
- Restauration
- Fermeture annuelle
- Modification des tarifs
- Gestion des utilisateurs

---

# Historique et suppression des données

La nouvelle application devrait éviter la suppression physique des données importantes.

Pour plusieurs tables, un champ `Actif` ou un statut permettra plutôt d'archiver les entrées.

Cela s'applique notamment à :

- Propriétaires
- Chiens
- Municipalités
- Rues
- Races
- Couleurs
- Modèles d'avis

Les paiements, licences et avis envoyés devraient normalement être conservés comme historique.

---

# Tables Access qui ne seront pas reproduites directement

Certaines tables de l'ancien système servent uniquement de tables temporaires ou techniques.

Elles ne devraient pas devenir des tables permanentes dans le nouveau modèle.

## Tables temporaires

- `Tampon Adresse`
- `Tampon année`
- `Tampon solde`

Leur rôle sera remplacé par :

- des requêtes SQL ;
- des transactions ;
- des calculs temporaires dans l'application ;
- des vues si nécessaire.

## Table système

- `MSysCompactError`

Cette table appartient au fonctionnement interne de Microsoft Access et ne doit pas être migrée.

---

# Archives

L'ancien système possède une table `Archives` utilisée notamment pour conserver des totaux financiers historiques.

Dans la nouvelle version, il serait préférable de conserver directement les opérations financières détaillées plutôt que seulement des totaux annuels.

Les statistiques historiques pourront alors être recalculées à partir des données.

Une table d'archives séparée ne sera ajoutée que si un besoin métier réel est confirmé.

---

# Relations principales proposées

```text
Municipalite 1 ───── N Proprietaire

Municipalite 1 ───── N Rue

Proprietaire 1 ───── N Chien

Proprietaire 1 ───── N Paiement

Proprietaire 1 ───── N AvisEnvoye

Proprietaire 1 ───── N TransactionFinanciere

Chien 1 ──────────── N Licence

Race 1 ───────────── N Chien

Couleur 1 ────────── N Chien

Municipalite 1 ───── N Licence

AvisModele 1 ─────── N AvisEnvoye
```

---

# Contraintes importantes

La nouvelle base devra notamment empêcher :

- deux propriétaires avec le même numéro de dossier ;
- des paiements liés à un propriétaire inexistant ;
- des chiens liés à un propriétaire inexistant ;
- des licences liées à un chien inexistant ;
- des rues liées à une municipalité inexistante ;
- des montants financiers invalides ;
- des suppressions qui brisent l'historique.

Des contraintes SQL et des validations applicatives seront utilisées ensemble.

---

# Gestion de la concurrence

Puisque Muni-Chien sera utilisé sur environ 6 à 7 postes, deux utilisateurs pourraient modifier le même dossier en même temps.

La base SQL permet déjà de gérer plusieurs connexions simultanées, mais l'application devra aussi détecter les conflits de modification importants.

Entity Framework Core pourra utiliser un mécanisme de concurrence optimiste.

Une colonne de version, par exemple `RowVersion`, pourra être ajoutée aux tables qui sont souvent modifiées.

Exemples :

- `Proprietaire`
- `Chien`
- `Municipalite`

---

# Migration depuis Access

La migration devra se faire depuis une copie de la base réelle.

Aucune donnée réelle ne doit être placée sur GitHub.

## Ordre proposé

```text
1. Municipalités
2. Rues
3. Races
4. Couleurs
5. Propriétaires
6. Chiens
7. Licences existantes
8. Paiements
9. Modèles d'avis
10. Historique utile / archives
```

## Transformation importante des licences

Pour chaque chien ayant actuellement un numéro dans `Chien.Licence` :

```text
Ancien :
Chien.Licence = "12345"

Nouveau :
Chien
└── Licence
    ├── NumeroLicence = "12345"
    ├── Annee = année de migration ou année déterminée
    └── Statut = à déterminer
```

L'année et le statut exacts des licences historiques devront être déterminés selon les données disponibles.

---

# Données à ne jamais mettre sur GitHub

- Base Access réelle
- Sauvegardes réelles
- Export de propriétaires
- Noms et coordonnées réels
- Paiements réels
- Toute autre donnée personnelle du client

Pour le développement et les tests, utiliser uniquement des données fictives.

---

# Modèle minimal proposé pour le MVP

Les tables minimales nécessaires pour une première version fonctionnelle sont :

```text
Proprietaire
Chien
Licence
Paiement
Municipalite
Rue
Race
Couleur
AvisModele
AvisEnvoye
```

`TransactionFinanciere` et `Utilisateur` pourront être intégrées dès le départ ou ajoutées ensuite selon les décisions prises avec le client.

---

# Points restant à confirmer

- Les comptes utilisateurs sont-ils nécessaires ?
- Le numéro de licence doit-il être unique globalement ou seulement par municipalité / année ?
- Les numéros de licence sont-ils réutilisables d'une année à l'autre ?
- Le champ `Municipalité` dans un paiement signifie-t-il que la municipalité a payé ?
- Le champ `Z` représente-t-il officiellement un paiement déjà traité dans le rapport journalier ?
- Quels modes de paiement doivent être supportés ?
- Les soldes doivent-ils être conservés comme valeur ou entièrement recalculés ?
- Doit-on conserver l'historique complet des changements de tarifs ?
- Quelle quantité d'historique financier doit être migrée ?
- Existe-t-il des propriétaires pouvant être liés à plusieurs municipalités ?
- Faut-il conserver les anciens dossiers inactifs indéfiniment ?
- Comment gérer précisément les licences lors du changement d'année ?

---

# Décision actuelle

Le modèle cible sera relationnel et utilisera SQL Server Express.

Les principales améliorations structurelles par rapport à Access sont :

- ajout d'un identifiant unique pour chaque chien ;
- création d'une vraie entité `Licence` ;
- relations SQL explicites ;
- conservation de l'historique des licences ;
- conservation de l'historique des avis ;
- paiements conservés comme opérations séparées ;
- suppression des tables temporaires Access ;
- utilisation de clés étrangères ;
- préparation à l'utilisation simultanée sur plusieurs postes.

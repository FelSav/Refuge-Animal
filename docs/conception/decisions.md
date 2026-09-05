# Journal des décisions du projet Muni-Chien

## Objectif

Ce document conserve les principales décisions prises pendant l'analyse et la conception du nouveau Muni-Chien.

Il sert de référence commune pour l'équipe afin d'éviter de revenir constamment sur les mêmes choix et de garder une trace claire des orientations du projet.

Les décisions pourront évoluer si de nouvelles informations sont obtenues auprès du client.

---

# Décision 001 — Remplacement progressif de Microsoft Access

## Décision

Le nouveau Muni-Chien remplacera progressivement l'application actuelle développée dans Microsoft Access.

## Raisons

L'ancien système contient plusieurs limites :

- structure de données difficile à maintenir ;
- logique métier répartie entre requêtes, formulaires et VBA ;
- relations non toujours explicites ;
- utilisation de tables temporaires ;
- difficulté à faire évoluer proprement le logiciel ;
- besoin d'une meilleure utilisation multi-postes.

## Conséquence

La nouvelle application sera développée indépendamment de Microsoft Access.

Access sera utilisé comme source de migration et comme référence pour comprendre les règles métier existantes.

---

# Décision 002 — Application de bureau Windows

## Décision

Le nouveau Muni-Chien sera une application de bureau Windows.

## Technologie retenue

- C#
- .NET
- WPF

## Raisons

Le logiciel doit être utilisé localement au Refuge Animal sur plusieurs postes Windows.

Une application de bureau permet de rester indépendante d'Internet et de conserver une utilisation proche du logiciel actuel.

---

# Décision 003 — Utilisation de SQL Server Express

## Décision

La base principale du nouveau Muni-Chien utilisera SQL Server Express.

## Raisons

Le logiciel devra fonctionner sur environ 6 à 7 ordinateurs.

SQL Server Express permet :

- une base centralisée ;
- plusieurs connexions simultanées ;
- une meilleure gestion de la concurrence qu'un fichier Access ;
- l'utilisation d'Entity Framework Core ;
- des sauvegardes SQL ;
- une évolution future vers une édition plus complète de SQL Server si nécessaire.

## Conséquence

SQLite n'est pas retenu comme base principale du système multi-postes.

---

# Décision 004 — Base de données centralisée

## Décision

Les 6 à 7 postes utiliseront tous la même base de données centrale.

## Architecture

```text
Postes Muni-Chien
       |
       v
Réseau local
       |
       v
SQL Server Express
       |
       v
Base de données Muni-Chien
```

## Conséquence

Les données ne seront pas copiées séparément sur chaque poste.

Une modification effectuée sur un poste devra être visible depuis les autres postes.

---

# Décision 005 — Fonctionnement sans Internet

## Décision

Le fonctionnement normal de Muni-Chien ne devra pas dépendre d'une connexion Internet.

## Conséquence

Les opérations courantes devront fonctionner sur le réseau local :

- recherche ;
- propriétaires ;
- chiens ;
- licences ;
- paiements ;
- rapports ;
- configuration.

Une panne Internet ne doit pas empêcher l'utilisation normale du logiciel.

---

# Décision 006 — Entity Framework Core

## Décision

Entity Framework Core sera utilisé pour l'accès aux données entre l'application C# et SQL Server.

## Raisons

Cela permet :

- de représenter les tables avec des classes C# ;
- de gérer les relations ;
- de créer des migrations ;
- de simplifier l'accès aux données ;
- de garder le modèle SQL cohérent avec l'application.

---

# Décision 007 — Séparation des responsabilités

## Décision

Le nouveau projet devra séparer clairement :

- l'interface ;
- la logique métier ;
- l'accès aux données ;
- la base de données.

## Conséquence

Les règles de calcul ne devront pas être directement mélangées dans les écrans WPF.

Cette séparation facilitera :

- les tests ;
- la maintenance ;
- la collaboration ;
- les évolutions futures.

---

# Décision 008 — Identifiant unique pour chaque chien

## Décision

Chaque chien possédera un identifiant unique interne `ChienId`.

## Raisons

Dans l'ancien Access, les chiens sont principalement liés au propriétaire par `NoProp` et ne disposent pas d'un identifiant indépendant suffisamment robuste.

## Conséquence

Deux chiens appartenant au même propriétaire pourront être distingués sans ambiguïté.

---

# Décision 009 — Création d'une vraie entité Licence

## Décision

La nouvelle base possédera une table `Licence`.

## Ancien système

Dans Access, le numéro de licence est stocké directement dans :

```text
Chien.Licence
```

## Nouveau système

```text
Chien
└── Licence
    ├── NumeroLicence
    ├── Annee
    ├── Municipalite
    ├── DateEmission
    ├── DateExpiration
    └── Statut
```

## Raisons

Cela permet :

- de conserver l'historique ;
- de gérer les licences par année ;
- d'éviter d'écraser une ancienne licence ;
- de rechercher plus facilement les licences ;
- de mieux gérer les renouvellements.

---

# Décision 010 — Conservation de l'historique des licences

## Décision

Les anciennes licences ne devront normalement pas être supprimées lors d'un changement d'année.

## Conséquence

Un chien pourra posséder plusieurs licences historiques.

Le fonctionnement exact des numéros réutilisés d'une année à l'autre reste à confirmer avec le client.

---

# Décision 011 — Paiements conservés comme opérations distinctes

## Décision

Chaque paiement restera un enregistrement indépendant.

## Raisons

L'ancien système possède déjà une table `Paiement` distincte, ce qui permet de conserver l'historique.

Cette approche est préférable à simplement remplacer une valeur de solde.

## Conséquence

L'historique financier devra être conservé.

---

# Décision 012 — Revoir la gestion du solde

## Décision

La nouvelle version devra éviter autant que possible de dépendre d'un champ de solde modifié directement par plusieurs opérations.

## Ancien calcul observé

```text
Solde dû
+ Frais de retard
- Paiements
= Balance
```

## Orientation

Le système devrait conserver les opérations financières et calculer le solde à partir de celles-ci lorsque possible.

## Statut

La structure finale de cette partie reste à confirmer pendant le développement du modèle financier.

---

# Décision 013 — Historique des avis

## Décision

La nouvelle version devrait conserver un historique des avis envoyés.

## Ancien système

Le propriétaire conserve principalement :

- la date du dernier avis ;
- le code du dernier avis.

## Nouveau système

Une table `AvisEnvoye` permettra de conserver plusieurs avis pour un même propriétaire.

---

# Décision 014 — Conservation des municipalités historiques

## Décision

Une municipalité déjà utilisée ne devrait pas être supprimée physiquement.

## Orientation

Utiliser un statut actif / inactif.

## Raisons

Les anciennes licences, paiements et propriétaires doivent continuer à pouvoir référencer la municipalité historique.

---

# Décision 015 — Conservation des races, couleurs et rues utilisées

## Décision

Les éléments de référence déjà utilisés dans des données historiques ne devraient pas être supprimés physiquement.

## Orientation

Utiliser un statut actif / inactif lorsque nécessaire.

---

# Décision 016 — Suppression des tables temporaires Access

## Décision

Les tables temporaires de l'ancien système ne seront pas reproduites comme tables permanentes.

## Tables concernées

- `Tampon Adresse`
- `Tampon année`
- `Tampon solde`

## Remplacement

Leur rôle sera remplacé par :

- requêtes SQL ;
- transactions ;
- calculs temporaires ;
- logique applicative ;
- vues SQL si nécessaire.

---

# Décision 017 — Ne pas migrer les tables système Access

## Décision

Les tables techniques Microsoft Access ne seront pas migrées.

## Exemple

- `MSysCompactError`

---

# Décision 018 — Préserver les données utiles existantes

## Décision

La migration devra conserver autant que possible les données utiles de l'ancien système.

## Données prioritaires

- propriétaires ;
- chiens ;
- municipalités ;
- rues ;
- races ;
- couleurs ;
- licences actuelles ;
- paiements ;
- modèles d'avis ;
- historique financier utile.

## Important

La migration sera réalisée depuis une copie de la base réelle.

---

# Décision 019 — Aucune donnée réelle du client sur GitHub

## Décision

Aucune donnée réelle de Muni-Chien ne doit être ajoutée au dépôt GitHub.

## Interdit

- fichiers `.accdb` réels ;
- sauvegardes ;
- exports de propriétaires ;
- coordonnées de clients ;
- paiements réels ;
- captures contenant des données personnelles.

## Autorisé

- code source ;
- documentation ;
- scripts SQL ;
- données fictives ;
- tests.

---

# Décision 020 — Utilisation de données fictives pour le développement

## Décision

Les tests et démonstrations du projet utiliseront des données fictives.

## Raisons

Cela permet :

- de protéger les données personnelles ;
- de travailler sur GitHub sans risque ;
- de faciliter les tests automatisés ;
- de reproduire les problèmes sans utiliser de données client.

---

# Décision 021 — Gestion multi-postes dès la conception

## Décision

La concurrence entre plusieurs utilisateurs doit être prise en compte dès le développement.

## Cas concernés

- deux utilisateurs modifient le même propriétaire ;
- deux utilisateurs enregistrent des paiements ;
- deux utilisateurs travaillent sur les licences ;
- deux utilisateurs modifient une municipalité.

## Orientation

Utiliser une stratégie de concurrence optimiste, par exemple avec `RowVersion`, lorsque nécessaire.

---

# Décision 022 — La fiche propriétaire reste centrale

## Décision

La fiche propriétaire restera le principal écran de travail du nouveau Muni-Chien.

## Elle permettra notamment d'accéder à :

- informations du propriétaire ;
- chiens ;
- licences ;
- paiements ;
- avis ;
- commentaires.

## Raisons

Cette organisation conserve la rapidité de travail de l'ancien système tout en améliorant sa présentation.

---

# Décision 023 — Recherche plus globale

## Décision

La nouvelle recherche devrait idéalement permettre de retrouver un dossier à partir de plusieurs informations.

## Critères souhaités

- numéro de dossier ;
- nom ;
- prénom ;
- téléphone ;
- courriel ;
- adresse ;
- chien ;
- numéro de licence.

## Statut

Le périmètre exact de la première version est défini dans `mvp.md`.

---

# Décision 024 — Séparer les opérations administratives sensibles

## Décision

Les opérations sensibles seront séparées des fonctions quotidiennes.

## Fonctions concernées

- restauration ;
- fermeture annuelle ;
- configuration des tarifs ;
- sauvegardes ;
- gestion éventuelle des utilisateurs.

## Raisons

Cela réduit le risque d'une opération accidentelle.

---

# Décision 025 — Sauvegarde obligatoire avant opérations critiques

## Décision

Une sauvegarde devra être créée avant certaines opérations importantes.

## Cas principal

- fermeture annuelle ;
- restauration.

## Orientation

La sauvegarde devra être vérifiable et sa réussite confirmée avant de continuer.

---

# Décision 026 — Fermeture annuelle sécurisée

## Décision

La fonction actuelle `Remise à zéro` devra être remplacée par un processus plus sécuritaire.

## Le nouveau processus devra inclure :

- vérifications préalables ;
- sauvegarde ;
- aperçu des changements ;
- confirmation explicite ;
- transaction ;
- journalisation ;
- rapport final.

## Objectif

Éviter les suppressions inutiles et préserver davantage d'historique.

---

# Décision 027 — Priorité au MVP

## Décision

Le développement commencera par le cœur fonctionnel plutôt que par la reproduction complète de toutes les fonctions Access.

## Priorités

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

## Fonctions pouvant arriver ensuite

- avis complets ;
- rapports avancés ;
- enveloppes ;
- étiquettes ;
- administration avancée ;
- comptes utilisateurs ;
- cartes de fidélité.

---

# Décision 028 — Validation progressive

## Décision

Le projet sera développé et testé par étapes.

## Principe

Une fonctionnalité doit être fonctionnelle avec la base SQL avant d'être considérée comme terminée.

## Une fonctionnalité terminée doit :

- fonctionner ;
- valider ses données ;
- gérer les erreurs de base ;
- être testée avec des données fictives ;
- être poussée sur GitHub ;
- être relue avant fusion dans `main`.

---

# Décision 029 — Migration pilote avant migration finale

## Décision

Une migration de test devra être réalisée avant la migration finale des données du Refuge Animal.

## Étapes

```text
Copie Access
→ import SQL de test
→ vérification
→ correction
→ nouvel essai
→ validation
→ migration finale
```

## Raisons

La migration ne doit pas être improvisée directement sur la base de production.

---

# Décision 030 — Comptes utilisateurs non encore confirmés

## Décision

Le système de comptes utilisateurs n'est pas encore officiellement retenu pour le MVP.

## Intérêt potentiel

Il pourrait servir à :

- identifier qui effectue une modification ;
- limiter l'administration ;
- protéger la restauration ;
- protéger la fermeture annuelle ;
- gérer des rôles.

## Statut

À confirmer avec le client.

---

# Décision 031 — Cartes de fidélité hors noyau initial

## Décision

Le module de cartes de fidélité fait partie des objectifs futurs du projet, mais il ne doit pas bloquer la reconstruction du module Muni-Chien.

## Conséquence

La priorité actuelle est de remplacer correctement les fonctions de licences de chiens et de gestion associée.

Le module de fidélité pourra être conçu comme un module distinct lorsque le noyau principal sera suffisamment stable.

---

# Décision 032 — Documentation avant développement

## Décision

L'équipe documente d'abord :

- l'ancien système ;
- les règles métier ;
- l'architecture ;
- la base de données ;
- les écrans ;
- le MVP ;
- les décisions.

## Raisons

Cela permet de réduire les erreurs de conception avant de commencer le développement.

---

# Décisions encore ouvertes

Les sujets suivants doivent encore être confirmés ou décidés :

- machine qui hébergera SQL Server ;
- méthode exacte de déploiement sur les 6 à 7 postes ;
- comptes utilisateurs ;
- permissions ;
- modes de paiement ;
- signification exacte du champ `Municipalité` dans Paiement ;
- signification officielle du champ `Z` ;
- règles exactes de réutilisation des numéros de licence ;
- durée réelle d'une licence ;
- règles complètes de tarification ;
- besoins exacts en rapports ;
- besoins exacts en avis ;
- stratégie finale de sauvegarde ;
- emplacement des sauvegardes ;
- fréquence des sauvegardes ;
- accès à distance éventuel ;
- structure finale du modèle financier.

---

# Références

Les décisions détaillées sont complétées par les documents suivants :

```text
docs/analyse-access/
├── tables.md
├── relations.md
├── fonctionnalites.md
├── regles-metier.md
├── migration.md
└── interface.md

docs/conception/
├── architecture.md
├── database.md
├── ecrans.md
├── mvp.md
└── decisions.md
```

Ces documents doivent rester cohérents entre eux au fur et à mesure que le projet évolue.

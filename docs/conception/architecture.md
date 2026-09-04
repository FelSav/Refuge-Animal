# Architecture du nouveau Muni-Chien

## Objectif

Le nouveau Muni-Chien doit remplacer progressivement l'application actuelle développée dans Microsoft Access.

L'application doit être plus moderne, plus stable et plus simple à maintenir tout en conservant les fonctions importantes de l'ancien logiciel.

Le système doit fonctionner localement au Refuge Animal et être utilisable simultanément sur environ 6 à 7 ordinateurs.

---

## Type d'application

Le nouveau Muni-Chien sera une application de bureau Windows.

Technologies prévues :

- Langage : C#
- Framework : .NET
- Interface graphique : WPF
- Accès aux données : Entity Framework Core
- Base de données : SQL Server Express

---

## Architecture générale

Le système utilisera une architecture client-serveur sur le réseau local du Refuge Animal.

Les différents ordinateurs utiliseront tous la même base de données centrale.

```text
Ordinateur 1 ─┐
Ordinateur 2 ─┤
Ordinateur 3 ─┤
Ordinateur 4 ─┼── Réseau local ── Serveur SQL
Ordinateur 5 ─┤
Ordinateur 6 ─┤
Ordinateur 7 ─┘
```

Chaque poste aura l'application Muni-Chien installée localement.

Tous les postes travailleront sur la même base de données centrale.

---

## Base de données

La base de données sera centralisée afin que tous les utilisateurs puissent consulter et modifier les mêmes informations.

SQL Server Express est actuellement le choix privilégié.

Ce choix permet notamment :

- L'utilisation simultanée par plusieurs postes
- Une meilleure gestion des accès concurrents qu'une base Access
- Une base relationnelle structurée
- L'utilisation d'Entity Framework Core
- La possibilité d'effectuer des sauvegardes
- Une évolution future vers une version plus complète de SQL Server si nécessaire

SQLite n'est pas retenu pour la base principale puisque l'application doit être utilisée simultanément sur plusieurs ordinateurs.

---

## Hébergement de la base de données

La base SQL devra être installée sur un ordinateur ou un serveur accessible en permanence sur le réseau local du Refuge Animal.

Ce poste ne devra pas dépendre de l'ouverture de l'application Muni-Chien pour que la base soit accessible.

Les postes clients se connecteront à ce serveur à travers le réseau local.

Le choix exact de la machine qui hébergera SQL Server devra être confirmé avec le client.

---

## Fonctionnement hors ligne

Muni-Chien ne devra pas dépendre d'Internet pour fonctionner.

Une panne d'Internet ne devra pas empêcher :

- La recherche d'un propriétaire
- La consultation des chiens
- La gestion des licences
- L'enregistrement des paiements
- La production des rapports
- La consultation des données

Une panne du réseau local ou du serveur empêchera cependant l'accès aux données.

---

## Séparation de l'application et des données

```text
Application Muni-Chien
        |
        v
Entity Framework Core
        |
        v
SQL Server
        |
        v
Base de données Muni-Chien
```

Cette séparation permettra de mettre à jour l'application sans devoir remplacer les données du client.

---

## Structure de l'application

### Présentation

- Fenêtres
- Écrans
- Formulaires
- Navigation
- Éléments visuels

Technologie prévue : WPF

### Logique métier

Cette partie contiendra notamment :

- Calcul des soldes
- Frais de retard
- Tarifs par municipalité
- Gestion des licences
- Gestion des chenils
- Fermeture annuelle

### Accès aux données

Technologie prévue : Entity Framework Core

### Base de données

Exemples de données permanentes :

- Propriétaires
- Chiens
- Licences
- Paiements
- Municipalités
- Avis
- Races
- Couleurs

---

## Utilisation simultanée

L'application doit supporter environ 6 à 7 utilisateurs travaillant simultanément.

Le système devra éviter autant que possible :

- L'écrasement accidentel de modifications
- Les données incohérentes
- Les doublons causés par deux opérations simultanées

Une stratégie de gestion de concurrence devra être définie pendant le développement.

---

## Sauvegardes

Le système devra idéalement permettre :

- Une sauvegarde manuelle
- Des sauvegardes automatiques
- Des noms contenant la date et l'heure
- La vérification du succès de la sauvegarde
- La restauration d'une sauvegarde

La stratégie exacte devra être confirmée avec le client.

---

## Sécurité

Les données réelles du client ne doivent jamais être placées sur GitHub.

Le dépôt GitHub doit uniquement contenir :

- Le code source
- La documentation
- Les scripts de création de base de données
- Des données fictives pour les tests si nécessaire

---

## Comptes utilisateurs

La nécessité d'avoir des comptes utilisateurs n'est pas encore confirmée.

Une gestion d'utilisateurs pourrait permettre :

- De connaître l'utilisateur ayant effectué une modification
- De limiter l'accès aux fonctions administratives
- De protéger la configuration
- De protéger la restauration ou la fermeture annuelle

---

## Déploiement

Chaque ordinateur devra avoir une copie installée de l'application Muni-Chien.

La base SQL sera centralisée.

Une mise à jour future de l'application devra pouvoir être déployée sans modifier ou perdre les données existantes.

---

## Architecture retenue actuellement

```text
Application :
C# / .NET

Interface :
WPF

Accès aux données :
Entity Framework Core

Base de données :
SQL Server Express

Nombre de postes :
Environ 6 à 7

Organisation :
Application installée sur chaque poste
+
Base de données centralisée sur le réseau local

Connexion Internet :
Non requise pour le fonctionnement normal
```

---

## Points restant à confirmer avec le client

- Quel ordinateur ou serveur hébergera la base de données
- Si le serveur reste allumé en permanence
- Si plusieurs utilisateurs modifient souvent les mêmes dossiers simultanément
- Si des comptes utilisateurs sont nécessaires
- Qui doit avoir accès à la configuration
- Qui doit pouvoir effectuer une restauration
- Qui doit pouvoir effectuer la fermeture annuelle
- Où les sauvegardes doivent être conservées
- La fréquence souhaitée des sauvegardes automatiques
- Si l'application doit pouvoir être utilisée à distance dans le futur

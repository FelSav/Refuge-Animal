# Analyse de l'interface de l'ancien Muni-Chien

Ce document décrit les principaux écrans de l'application Access actuelle, leur rôle, les actions disponibles et les améliorations envisagées pour la nouvelle version.

L'objectif est de comprendre le parcours utilisateur actuel avant de concevoir l'interface du futur Muni-Chien.

---

## Écran Recherche

### Rôle

L'écran Recherche permet de retrouver rapidement un propriétaire dans la municipalité actuellement sélectionnée.

Dans l'exemple analysé, la municipalité active est Roberval.

### Éléments affichés

L'écran contient :

- Un champ de recherche

- Un bouton de recherche

- Un filtre `Actif seulement`

- Un filtre `Tous`

- La date actuelle

- La municipalité active

### Résultats affichés

Les propriétaires sont affichés dans un tableau contenant les colonnes suivantes :

- Numéro de dossier

- Prénom

- Nom

- Numéro civique

- Rue

- Appartement

- Date du dernier avis

- Solde

- État

### Actions disponibles

Lorsqu'un propriétaire est sélectionné, l'utilisateur peut :

- Ouvrir son dossier avec le bouton `Détail`

- Accéder à son paiement avec le bouton `Paiement`

- Retourner à l'écran précédent

### Filtres

Deux modes semblent disponibles :

- `Actif seulement`

 - affiche probablement uniquement les propriétaires dont le dossier est actif

- `Tous`

 - affiche probablement les dossiers actifs et inactifs

Ce comportement devra être confirmé.

### Parcours utilisateur

Recherche

→ sélection d'un propriétaire

→ Détail

ou

Recherche

→ sélection d'un propriétaire

→ Paiement

### Points à améliorer dans la nouvelle version

- Interface plus moderne et plus lisible

- Barre de recherche plus claire

- Filtres plus explicites

- Possibilité de rechercher plusieurs types d'informations depuis le même champ

- Meilleure présentation du résultat sélectionné

- Tableau redimensionnable

- Affichage plus clair du statut du dossier

- Navigation plus intuitive vers le dossier et les paiements

## Écran Détail

### Rôle

L'écran `Détail` représente la fiche complète d'un propriétaire.

Il permet de consulter et modifier les informations du propriétaire, de voir ses chiens, leurs licences, son solde et les informations liées aux avis.

### Informations du propriétaire

Les informations suivantes sont affichées :

- Numéro de dossier

- Prénom

- Nom

- Date de naissance

- Statut `Propriétaire d'un chenil`

- Numéro civique

- Rue

- Appartement

- Municipalité

- Code postal

- Téléphone

- Courriel

- Commentaire

Certains champs sont modifiables directement dans le formulaire.

### Informations financières et avis

L'écran affiche également :

- Solde dû

- Date du dernier avis

- Code du dernier avis

Ces informations permettent à l'utilisateur de voir rapidement l'état financier et le suivi des avis du propriétaire.

### Gestion des chiens

Une section `Chien(s)` affiche tous les chiens associés au propriétaire.

Pour chaque chien, les informations suivantes sont disponibles :

- Nom

- Race

- Couleur

- Sexe

- Stérilisé

- Numéro de licence

Les chiens sont affichés sous forme de tableau.

Une nouvelle ligne vide permet également d'ajouter un chien directement au dossier.

### Licence

Le numéro de licence est actuellement enregistré directement dans la fiche du chien.

L'écran contient également un champ permettant de rechercher directement un numéro de licence.

### Actions disponibles

Les principales actions visibles sont :

- `Recherche`

 - Retour vers la recherche des propriétaires

- `Nouveau`

 - Création d'un nouveau dossier de propriétaire

- `Paiement`

 - Accès à la gestion des paiements du propriétaire

- Recherche par numéro de licence

Certains boutons utilisent uniquement une icône et leur rôle devra être confirmé.

### Parcours utilisateur

Recherche

→ sélection d'un propriétaire

→ Détail

Depuis Détail :

Détail

├── modifier les informations du propriétaire

├── consulter / modifier les chiens

├── ajouter un chien

├── consulter / modifier les licences

├── ajouter un commentaire

├── accéder aux paiements

├── rechercher une licence

└── retourner à la recherche

### Importance de cet écran

Cet écran semble être le cœur de la gestion quotidienne de Muni-Chien.

Il rassemble dans une seule fiche :

- le propriétaire

- ses coordonnées

- ses chiens

- ses licences

- son solde

- ses avis

- ses commentaires

### Points à améliorer dans la nouvelle version

- Séparer visuellement les informations du propriétaire, des chiens et de la facturation

- Rendre les boutons et actions plus explicites

- Éviter les boutons représentés uniquement par des icônes ambiguës

- Présenter chaque chien comme un élément clairement identifiable

- Ajouter un identifiant unique interne pour chaque chien

- Rendre la gestion des licences plus claire

- Afficher plus clairement le statut du dossier

- Améliorer la validation des champs

- Faciliter l'ajout et la modification d'un chien

- Conserver l'accès rapide aux paiements

## Écran Paiement

### Rôle

L'écran `Paiement` permet de consulter la situation financière d'un propriétaire et d'enregistrer un nouveau paiement.

Il est normalement ouvert à partir du dossier d'un propriétaire.

### Informations du propriétaire

L'écran affiche certaines informations permettant d'identifier le dossier :

- Nom

- Adresse

- Municipalité

- Date actuelle

### Résumé financier

L'écran présente les montants suivants :

- Solde dû

- Frais de retard

- Total

- Paiements déjà effectués

- Balance restante

La logique générale observée dans l'ancien système est :

Balance =

Solde dû

\+ Frais de retard

- Paiements effectués

### Historique des paiements

Une section `Paiement` affiche les paiements déjà associés au propriétaire.

Les colonnes visibles comprennent notamment :

- Numéro de reçu

- Date

- Montant

- Une information supplémentaire liée au paiement / à la municipalité

La signification exacte de cette dernière colonne devra être confirmée.

### Nouveau paiement

Une section `Nouveau paiement` permet d'inscrire un paiement.

Les informations visibles sont :

- Date

- Montant

- Case `Municipalité`

- Case `Chèque`

### Mode de paiement

L'ancien système semble principalement utiliser une case `Chèque` pour indiquer le type de paiement.

Cette gestion devra être revue dans la nouvelle version afin de déterminer les modes de paiement réellement nécessaires.

Exemples possibles à valider avec le client :

- Comptant

- Débit

- Crédit

- Chèque

- Autre

### Paiement lié à la municipalité

Une case `Municipalité` est disponible lors de l'inscription d'un paiement.

Sa signification exacte et son impact devront être confirmés pendant l'analyse du fonctionnement du logiciel.

### Actions disponibles

Deux actions principales sont visibles :

- `Inscrire`

 - Enregistre le nouveau paiement

- `Annuler`

 - Annule l'opération et retourne à l'écran précédent

### Frais de retard

Un petit bouton est présent à côté du montant des frais de retard.

Son rôle semble être lié à la suppression ou à l'annulation des frais de retard.

Ce comportement devra être confirmé avant de le documenter comme règle définitive.

### Parcours utilisateur

Détail

→ Paiement

→ Consultation du solde

→ Saisie d'un montant

→ Sélection des options de paiement

→ Inscrire

### Points à améliorer dans la nouvelle version

- Présenter plus clairement le calcul du montant restant

- Afficher un historique de paiements plus lisible

- Utiliser une vraie sélection de mode de paiement au lieu d'une simple case `Chèque`

- Clarifier le rôle de l'option `Municipalité`

- Rendre l'annulation des frais de retard plus explicite

- Afficher clairement le numéro de reçu après un paiement

- Ajouter des confirmations pour les opérations financières importantes

- Empêcher les montants invalides ou incohérents

## Écran / fonction Paiements journaliers

### Rôle

La fonction `Paiements journaliers` sert à produire la liste des paiements de la journée.

Cette liste semble être utilisée comme étape de fermeture ou de validation des paiements quotidiens.

### Comportement observé

Lorsqu'aucun paiement n'a été enregistré pour la journée, le logiciel affiche le message :

`Il n'y a pas eu de paiement aujourd'hui`

Aucun rapport n'est alors généré.

### Lien avec la fermeture annuelle

Le code VBA indique que la remise à zéro annuelle vérifie s'il reste des paiements journaliers non traités.

Si des paiements sont encore présents, la fermeture annuelle est bloquée et demande d'imprimer le rapport des paiements journaliers avant de continuer.

### Parcours utilisateur

Accueil

→ Paiements journaliers

Si aucun paiement du jour :

→ message d'information

→ retour à l'accueil

Si des paiements existent :

→ génération / affichage du rapport

→ traitement des paiements journaliers

### À confirmer

- Le contenu exact du rapport lorsqu'il y a des paiements

- Si le rapport est imprimé automatiquement ou affiché en aperçu

- Ce que représente exactement le champ `Z` dans la table Paiement

- Si les paiements sont marqués comme traités après impression

## Écran Configuration

### Rôle

L'écran `Configuration` regroupe les fonctions administratives et techniques de Muni-Chien.

Il donne accès à :

- Paramètres

- Sauvegarde de la base de données

- Restauration d'une copie de sauvegarde

- Remise à zéro annuelle

- Fermeture de l'écran

\---

## Paramètres

### Rôle

L'écran `Paramètres` permet de configurer les informations générales du refuge ainsi que certaines options du logiciel.

### Informations générales

Les informations visibles comprennent :

- Nom de l'organisme

- Adresse

- Municipalité

- Code postal

- Téléphone

### Options générales

Les options visibles comprennent :

- `Conserver les numéros de licences`

- Mode d'impression des reçus :

 - Aperçu

 - Imprimante par défaut

- Nombre de copies des avis

### Listes configurables

L'écran donne également accès à la gestion de plusieurs listes utilisées dans le logiciel :

- Avis

- Rue

- Race

- Municipalité

- Couleur

### Importance

Cet écran contient plusieurs paramètres qui influencent directement le comportement de l'application.

Par exemple, l'option `Conserver les numéros de licences` détermine si les numéros de licence doivent être conservés lors de la fermeture annuelle.

### Points à améliorer dans la nouvelle version

- Séparer clairement les paramètres généraux des données de référence

- Utiliser des sections ou onglets

- Ajouter des descriptions pour les options importantes

- Ajouter des validations sur les champs

- Restreindre l'accès à cette section aux utilisateurs autorisés si des comptes sont ajoutés

\---

## Sauvegarde de la base de données

### Rôle

La fonction `Sauvegarder de la base de données` permet de créer manuellement une copie complète des données de Muni-Chien.

### Fonctionnement observé

Lorsqu'elle est utilisée, le logiciel ouvre une fenêtre permettant à l'utilisateur de choisir :

- L'emplacement du fichier

- Le nom du fichier de sauvegarde

Le nom proposé par défaut est :

`backup.accdb`

### Fonctionnement technique

Le logiciel crée une nouvelle base Access puis copie les tables de données dans ce fichier.

### Points à améliorer dans la nouvelle version

- Utiliser des noms de sauvegarde contenant la date et l'heure

- Ajouter une confirmation de réussite

- Éviter l'écrasement accidentel d'une sauvegarde existante

- Prévoir éventuellement des sauvegardes automatiques

- Vérifier l'intégrité de la sauvegarde

\---

## Restauration d'une sauvegarde

### Rôle

La fonction `Restaurer une copie de sauvegarde` permet de sélectionner une ancienne base de données et de l'utiliser comme source de données.

### Fonctionnement observé

Le logiciel ouvre une fenêtre de sélection de fichier.

L'utilisateur choisit une copie de sauvegarde `.accdb`.

L'application remplace ensuite ses liens vers la base actuelle par les tables contenues dans la sauvegarde sélectionnée.

### Risques

Cette opération peut remplacer la base utilisée par l'application.

Dans la nouvelle version, une restauration devra être considérée comme une opération sensible.

### Points à améliorer dans la nouvelle version

- Demander une confirmation claire avant restauration

- Créer automatiquement une sauvegarde de la base actuelle avant restauration

- Vérifier que le fichier choisi est une sauvegarde valide

- Afficher clairement quelle sauvegarde sera restaurée

- Prévoir un mécanisme de retour en arrière en cas d'erreur

\---

## Remise à zéro annuelle

### Rôle

La fonction `Remise à zéro` permet de fermer l'année actuelle et de préparer Muni-Chien pour une nouvelle année.

### Confirmation utilisateur

Avant de commencer, le logiciel affiche un avertissement indiquant que la base sera remise à zéro pour commencer une nouvelle année.

L'utilisateur doit confirmer avec `Oui` ou annuler avec `Non`.

### Fonctionnement

La logique détaillée de cette opération est décrite dans `regles-metier.md`.

La procédure comprend notamment :

1\. Vérification des paiements journaliers non traités

2\. Demande de confirmation

3\. Sauvegarde de la base

4\. Conservation temporaire des soldes impayés

5\. Remise à zéro des soldes et frais

6\. Recalcul des montants de la nouvelle année

7\. Archivage des données financières

8\. Suppression des anciens paiements

9\. Report optionnel des soldes impayés

10\. Remise à zéro des licences si nécessaire

11\. Avancement des dates limites de paiement

### Importance

Il s'agit d'une des opérations les plus critiques de l'ancien Muni-Chien.

Elle modifie une grande quantité de données et ne doit jamais être reproduite dans la nouvelle version comme une simple suite d'opérations non sécurisées.

### Points à améliorer dans la nouvelle version

- Utiliser une transaction de base de données

- Créer automatiquement une sauvegarde avant l'opération

- Afficher clairement toutes les étapes qui seront effectuées

- Demander une confirmation renforcée

- Journaliser l'opération

- Empêcher deux utilisateurs de lancer la fermeture annuelle en même temps

- Permettre de connaître l'année courante de manière explicite

- Conserver davantage d'historique plutôt que supprimer certaines données

## Écran Avis

### Rôle

L'écran `Avis` sert à préparer et imprimer différents documents destinés aux propriétaires.

Il est associé à la municipalité actuellement sélectionnée.

### Types de documents

L'utilisateur peut choisir parmi plusieurs types d'impression :

- Avis

- Enveloppe (US #10)

- Étiquettes (Avery 137 192)

### Avis

Pour les avis, l'utilisateur peut sélectionner :

- Un code d'avis

- Le nombre de copies

Il peut ensuite choisir les destinataires.

### Destinataires

Plusieurs options sont disponibles :

- Tous les propriétaires ayant un solde à payer

- Un propriétaire précis

- Général

Pour les propriétaires ayant un solde, il est également possible de limiter la sélection selon une plage de rues :

- De la rue

- À la rue

### Quantité

Une quantité peut être indiquée selon le type d'impression sélectionné.

### Date limite

La date limite de paiement de la municipalité active est affichée sur l'écran.

Cette information semble être utilisée dans les avis.

### Enveloppes

L'utilisateur peut imprimer des enveloppes de format US #10.

Une option permet d'inclure l'adresse de l'expéditeur.

### Étiquettes

Le logiciel permet également d'imprimer des étiquettes Avery.

L'utilisateur peut choisir la position de la première étiquette utilisée sur la feuille afin de réutiliser une feuille déjà partiellement imprimée.

### Lien avec le suivi des avis

Lorsqu'un avis est produit, le logiciel peut mettre à jour :

- La date du dernier avis

- Le code du dernier avis

dans le dossier du propriétaire.

### Points à améliorer dans la nouvelle version

- Séparer clairement le choix du document et le choix des destinataires

- Afficher un aperçu avant impression

- Permettre de générer les documents en PDF

- Clarifier les notions de `Avis`, `Général` et `Code d'avis`

- Faciliter la sélection de plusieurs propriétaires

- Ajouter une confirmation du nombre de documents qui seront produits

- Conserver un historique plus détaillé des avis envoyés

## Écran Rapport

### Rôle

L'écran `Rapport` permet de générer différentes listes et statistiques sur les données de Muni-Chien.

### Listes disponibles

#### Chiens

Permet de produire une liste des chiens.

Un choix de tri est disponible.

Dans l'écran observé, le tri sélectionné est :

- Licence

D'autres critères peuvent être disponibles dans la liste déroulante.

#### Propriétaires

Permet de produire une liste des propriétaires.

Un choix de tri est disponible.

Dans l'écran observé, le tri sélectionné est :

- Nom

#### Soldes impayés

Permet de produire une liste des propriétaires ayant encore un montant à payer.

#### Frais de retard

Permet de produire une liste liée aux frais de retard.

#### Paiements

Permet de produire un rapport des paiements.

### Statistiques

Deux statistiques principales sont visibles :

- Sur les revenus

- Par race de chien

### Municipalité

Une municipalité peut être sélectionnée pour les statistiques.

Cela permet probablement de produire les résultats pour une municipalité précise.

### Impression

Un bouton situé au bas de l'écran permet de générer ou d'afficher le rapport sélectionné.

### Données financières

Les requêtes de l'ancien système permettent notamment d'agréger les revenus par :

- Municipalité

- Année

Les données courantes et les archives historiques peuvent être combinées pour produire ces statistiques.

### Points à améliorer dans la nouvelle version

- Utiliser un écran de rapports plus moderne avec catégories

- Afficher les critères de filtrage clairement

- Permettre un aperçu à l'écran

- Ajouter l'export PDF

- Ajouter éventuellement l'export CSV / Excel

- Permettre de filtrer par période

- Permettre de filtrer par municipalité

- Afficher les statistiques sous forme de tableaux ou graphiques lorsque pertinent

## Sous-écrans des Paramètres

### Municipalité

#### Rôle

L'écran `Municipalité` permet de gérer les municipalités utilisées dans Muni-Chien ainsi que leurs règles de tarification.

Chaque ligne représente une municipalité.

#### Informations disponibles

Pour chaque municipalité, on retrouve :

- Nom de la municipalité

- Code postal

- Date limite de paiement

- Montant par chien

- Montant pour chenil

- Frais de retard

- Option indiquant si le deuxième chien possède un tarif différent

- Montant du deuxième chien

#### Actions

L'utilisateur peut :

- Modifier directement les valeurs

- Ajouter une nouvelle municipalité

- Supprimer une municipalité

#### Importance

Cet écran contient plusieurs règles métier importantes du logiciel.

Les tarifs des chiens, les chenils, les frais de retard et les dates limites dépendent directement de ces valeurs.

#### Points à améliorer dans la nouvelle version

- Utiliser une fiche claire par municipalité

- Ajouter des validations sur les montants et les dates

- Empêcher la suppression d'une municipalité encore utilisée

- Afficher clairement l'option de tarif du deuxième chien

- Conserver un historique des modifications de tarifs si nécessaire

### Modèles d'avis

#### Rôle

L'écran `Avis` permet de créer et modifier les différents modèles de lettres utilisés lors de l'émission des avis.

#### Informations disponibles

Chaque modèle contient notamment :

- Code

- Titre

- Sous-titre

- Texte complet de l'avis

#### Utilisation

Le `Code d'avis` sélectionné dans l'écran d'émission d'avis semble correspondre à l'un de ces modèles.

Le logiciel peut ensuite enregistrer ce code dans le dossier du propriétaire comme `Code du dernier avis`.

#### Points à améliorer dans la nouvelle version

- Utiliser un véritable éditeur de modèles

- Permettre un aperçu du document

- Ajouter éventuellement des variables automatiques dans les modèles

 - nom du propriétaire

 - montant dû

 - date limite

 - municipalité

- Éviter de modifier accidentellement un modèle déjà utilisé

### Rues

#### Rôle

L'écran `Rue` contient la liste des rues reconnues pour la municipalité sélectionnée.

Dans l'exemple analysé, les rues affichées sont celles de Roberval.

#### Actions

L'utilisateur peut :

- Ajouter une rue

- Modifier le nom d'une rue

- Supprimer une rue

#### Utilisation

Cette liste est notamment utilisée :

- dans les adresses des propriétaires

- dans les recherches

- dans la sélection de plages de rues pour l'impression des avis

#### Points à améliorer dans la nouvelle version

- Ajouter une recherche

- Empêcher les doublons

- Associer clairement chaque rue à une municipalité

- Confirmer avant suppression si une rue est utilisée

### Races

#### Rôle

L'écran `Race` contient la liste des races pouvant être attribuées aux chiens.

#### Actions

L'utilisateur peut :

- Ajouter une race

- Modifier une race

- Supprimer une race

#### Utilisation

Cette liste est utilisée dans la fiche des chiens ainsi que dans certaines statistiques.

#### Points à améliorer dans la nouvelle version

- Ajouter une recherche

- Empêcher les doublons

- Trier automatiquement les races

- Éviter la suppression d'une race encore utilisée par des chiens

### Couleurs

#### Rôle

L'écran `Couleur` contient la liste des couleurs pouvant être attribuées aux chiens.

#### Actions

L'utilisateur peut :

- Ajouter une couleur

- Modifier une couleur

- Supprimer une couleur

#### Points à améliorer dans la nouvelle version

- Ajouter une recherche

- Empêcher les doublons

- Trier automatiquement les couleurs

- Éviter la suppression d'une couleur encore utilisée

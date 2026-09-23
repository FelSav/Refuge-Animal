# Écrans et parcours — MuniChien

Révision : 6 septembre 2026. Source : document du 5 septembre 2026, §§ 5, 8, 9 et 12–13. Les fonctionnalités sont issues du document; la répartition des écrans ci-dessous est une proposition à valider avec le client.

## Prototype frontend obligatoire avant développement complet

Préparer un prototype navigable avec données fictives avant de connecter toute la logique à la base. Faire valider navigation, écrans principaux, actions et messages par le Refuge Animal. La pile WPF reste un choix technique de l’équipe, pas une exigence officielle.

Utiliser l’identité orange, blanc, gris et tons sombres. Présenter les tâches courantes avec titres clairs, actions visibles et recherche rapidement accessible. Ne pas exposer la structure des tables à l’utilisateur.

## Écrans proposés

| Écran | Contenu et actions |
| --- | --- |
| Accueil | Recherche immédiate; accès dossiers, rapports et municipalités; bouton « Connexion administrateur ». Pas de connexion individuelle employé. |
| Recherche de chiens | Filtres combinables, tri, résultats, total de chiens, accès dossier, impression et PDF. |
| Dossier propriétaire | Numéro de dossier, identité, adresse, coordonnées, date de naissance existante, statut, commentaire distinct, chiens associés, paiements, solde et frais. |
| Fiche chien | Nom, race, sexe, couleur, naissance, âge calculé, poids précis kg, stérilisation, statut, date/raison d’inactivité, commentaire propre, licence courante et historique. |
| Licence / renouvellement | Numéro, émission, expiration, chien; renouvellement annuel avec possibilité de conserver le numéro; fiche imprimée au contenu fonctionnel conservé. |
| Remplacement de plaque | Plaque perdue ou endommagée; ancienne et nouvelle référence; récapitulatif du remplacement; historique conservé. |
| Paiements et solde | Montants dus, frais, paiements et solde expliqué; saisie manuelle; paiement direct municipalité si utilisé. |
| Municipalités et tarifs | Statut municipalité; tarifs premier/deuxième chien et chenil, date limite, frais de retard; historique cinq ans. Modification des paramètres sensibles sous accès administrateur. |
| Rapports et avis | Choix du rapport, filtres ajustables, aperçu, impression et export PDF. |
| Administration | Authentification; fermeture annuelle, restauration, suppressions définitives autorisées, paramètres sensibles et maintenance. |

## Recherche prioritaire

Combiner race et couleur avec autocomplétion, adresse officielle sélectionnée, numéro de licence, prénom/nom du propriétaire, numéro de dossier, âge calculé, poids numérique et statut. Afficher les actifs par défaut et permettre une vue uniquement inactive.

Le seuil et l’opérateur de poids sont modifiables : par exemple « poids supérieur à 20 kg ». Un exemple fictif de résultat est « 46 chiens ». Le total représente tous les chiens distincts correspondant aux filtres, même si la liste est paginée et même si plusieurs chiens ont le même propriétaire. Il ne compte pas les anciennes licences comme de nouveaux chiens.

Prévoir effacement des filtres, absence de résultat, chargement et erreur serveur. Les critères actifs restent visibles avant impression. La convention de filtrage par âge doit correspondre au calcul depuis la date de naissance.

## Saisie des adresses et références

À la saisie d’une adresse, proposer des adresses officielles et demander d’en sélectionner une; une saisie libre non validée ne doit pas être présentée comme officielle. Inclure numéro civique et appartement. Permettre l’ajout d’une rue absente par un parcours contrôlé, dont la validation reste à définir avec le fournisseur choisi.

Race et couleur : autocomplétion avec possibilité d’ajout. Éviter les doublons de références. Si le service d’adresses est indisponible, afficher un message compréhensible; le traitement d’une nouvelle adresse non validable reste à décider. Les consultations et autres opérations locales ne doivent pas être bloquées inutilement.

## Paiements et historique

Modes : comptant, carte, chèque et autre. Le mode « autre » rend la précision obligatoire. Montrer clairement ce qui est dû, payé et restant, ainsi que l’indicateur de paiement à la municipalité lorsqu’utilisé. La prise en charge des paiements partiels doit être validée avant de figer ce parcours.

Un propriétaire inactif ou un chien inactif reste consultable. L’inactivation d’un chien renseigne sa date automatiquement et propose une raison facultative. Les commentaires chien et propriétaire sont séparés et restent disponibles après fermeture annuelle.

## Rapports, avis et PDF

Reprendre tous les rapports utiles d’Access : chiens/licences, fiche de renouvellement, impayés et frais de retard, paiements par propriétaire et municipalité, statistiques de revenus, statistiques par race et recherches pertinentes. Inventorier les modèles existants pour éviter les omissions.

Chaque parcours permet d’ajuster les filtres avant aperçu, impression et export PDF. Moderniser la présentation sans changer le contenu fonctionnel attendu de la fiche de renouvellement. Les avis sont conservés; un éditeur de modèles d’avis n’est pas requis en première version.

## Modification concurrente

Lorsqu’un dossier est déjà en modification, le deuxième poste est empêché de le modifier et reçoit un message simple; le nom de l’employé n’est pas requis. Proposition de message : « Ce dossier est en cours de modification sur un autre poste. Vous pouvez le consulter et réessayer plus tard. »

Si le verrou expire ou si la version a changé, refuser l’enregistrement et demander un rechargement sans écraser la version enregistrée. Prévoir un avertissement explicite avant abandon de la saisie locale. La protection réelle est assurée par l’API, selon [architecture.md](architecture.md).

## Assistant de fermeture annuelle

1. Exiger la connexion administrateur et afficher le cycle concerné.
2. Préparer puis vérifier la sauvegarde obligatoire avant toute mutation sensible.
3. Présenter la synthèse des dossiers, soldes, paiements et éléments concernés.
4. Contrôler paiements et dettes; demander le choix de report lorsqu’il est nécessaire.
5. Expliquer les remises à zéro, archivages/suppressions validés et recalculs; demander une confirmation explicite.
6. Exécuter la fermeture de façon cohérente; empêcher les écritures concurrentes concernées et le lancement en double selon la conception proposée.
7. Afficher un bilan clair et rendre les erreurs visibles.

La fermeture renouvelle le cycle des licences sans inactiver automatiquement les chiens. Elle conserve les commentaires et les historiques encore soumis à conservation. Ne pas confondre ce changement de cycle avec un paiement ou un renouvellement individuel effectivement réalisé par le propriétaire.

La restauration est un parcours administrateur séparé, avec choix de sauvegarde, explication de l’état qui sera remplacé et confirmation renforcée. Elle ne doit pas être déclenchée trop facilement.

## Validation du prototype

Faire réaliser au client une recherche combinée avec total, la création d’un propriétaire et d’un chien, la sélection d’adresse, un renouvellement, un remplacement, un paiement, un PDF, l’accès à un inactif et une simulation de fermeture. Montrer aussi un conflit entre deux postes et une action sensible interdite sans connexion administrateur. Consigner corrections et validation avant le développement complet. Voir [mvp.md](mvp.md).

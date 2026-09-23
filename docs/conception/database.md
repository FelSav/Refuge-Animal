# Base de données — MuniChien

Révision : 6 septembre 2026. Référence : document de compréhension du 5 septembre 2026, §§ 3, 5, 7–8, 10 et annexe A. Ce modèle logique est une proposition de conception; les noms de tables et mécanismes SQL ne sont pas des exigences officielles. SQL Server est le choix technique actuel de l’équipe. Voir [decisions.md](decisions.md).

## Frontières et relations

La base MuniChien est indépendante de la base Fidélité. Seule l’API serveur y accède. Aucun schéma Fidélité n’est défini ici.

```text
Municipalité 1 ── N Propriétaire 1 ── N Chien 1 ── N Licence
      │                   │              ├── Race
      └── N Tarif         ├── N Paiement  └── Couleur
                          └── N Écriture financière (proposition)
```

Un propriétaire peut avoir plusieurs chiens; chaque chien appartient à un propriétaire. Les dossiers inactifs demeurent consultables. Les conditions autorisant un propriétaire sans chien doivent être précisées pendant la conception, sans bloquer la saisie progressive d’un dossier.

## Entités et données

| Entité proposée | Données et règles |
| --- | --- |
| Propriétaire | Identifiant interne; numéro de dossier existant conservé si possible; nom, prénom; numéro civique, appartement, rue, municipalité, code postal; téléphone, cellulaire, courriel disponibles; date de naissance existante conservée; statut actif/inactif; commentaire facultatif distinct de celui du chien. |
| Chien | Propriétaire; nom; race; sexe; couleur; date de naissance; poids précis en kg; stérilisation; statut; raison d’inactivité facultative; date d’inactivité enregistrée automatiquement; commentaire facultatif durable. |
| Licence | Chien; numéro; émission; expiration; statut; informations de renouvellement/remplacement; historique léger de cinq ans. |
| Municipalité | Identité; statut actif/inactif; paramètres municipaux et liens vers les versions de tarifs. |
| TarifMunicipal | Municipalité; période d’application proposée; tarif premier chien, deuxième chien, chenil; date limite; frais de retard. Historique de cinq ans. |
| Paiement | Propriétaire; date; montant; mode comptant/carte/chèque/autre; précision obligatoire pour « autre »; indicateur de paiement direct à une municipalité lorsqu’utilisé. |
| Race, Couleur | Référentiels avec autocomplétion et possibilité d’ajout. |
| Rue / Adresse | Référentiel normalisé, municipalité et adresse officielle sélectionnée; possibilité d’ajout d’une rue absente. Structure dépendante du fournisseur d’adresses à choisir. |
| Avis | Types/modèles utiles de l’ancien logiciel; modification du contenu par le personnel non requise en première version. |
| ÉcritureFinancière | Proposition : montants dus, frais, reports et ajustements nécessaires à l’explication du solde, avec références au cycle et aux règles appliquées. |
| Clôture / ArchiveAnnuelle | Proposition : cycle traité, synthèse archivée, choix de report, état d’exécution et référence de sauvegarde préalable. Contenu exact à déduire du fonctionnement validé. |

## Naissance, âge, poids et statuts

Enregistrer la date de naissance du chien; calculer son âge à une date de référence, sans conserver un âge figé qui deviendrait faux. Les filtres et rapports utilisent la même convention de calcul. Le poids utilise un type décimal précis en kilogrammes, pas une catégorie approximative.

Les dates de naissance et poids absents dans Access ne doivent pas être inventés : prévoir une valeur inconnue et une procédure de complément, dont les règles de saisie seront validées. Proposer des contrôles serveur contre dates futures et poids invalides. Le degré de précision et les champs obligatoires restent à définir.

L’inactivation conserve le chien et son historique. Elle renseigne automatiquement sa date; la raison est facultative. La fermeture annuelle ne rend pas automatiquement les chiens inactifs et n’efface aucun commentaire propriétaire ou chien.

## Licences et remplacement

Créer une véritable entité Licence, au lieu de ne conserver qu’un numéro sur Chien. Un renouvellement crée une nouvelle période historique sans supprimer le chien. Le même numéro peut être réutilisé d’une année à l’autre. Un remplacement de plaque perdue ou endommagée conserve la trace de l’ancienne plaque et du remplacement; un lien entre les enregistrements ou un événement de remplacement est une proposition technique.

Le numéro visible n’est pas la clé primaire. Conserver le contexte municipal nécessaire au contrôle des doublons, y compris après un changement de municipalité. La possibilité de numéros identiques simultanés dans deux municipalités est **à confirmer** (§§ 5.4 et 14). Ne pas figer une contrainte d’unicité globale ni autoriser tous les doublons par défaut. La portée de l’unicité et les chevauchements de validité doivent être validés avant finalisation des contraintes.

Les anciennes licences sont conservées cinq ans, puis supprimées selon la règle retenue. La date de départ du délai et le traitement des liens de remplacement restent à préciser. Ne pas purger une licence courante uniquement sur la base d’une date de création ancienne.

## Tarifs, paiements et soldes

Les tarifs sont configurables par municipalité et historisés cinq ans. Une modification ne doit pas modifier rétroactivement les montants déjà dus : proposition de versions datées et de montants appliqués conservés sur les écritures financières.

Le solde découle des montants dus, frais applicables, reports validés et paiements. Ne pas inventer de règles sur le troisième chien, le cumul chenil, les arrondis ou les frais : analyser Access et faire valider les règles exactes. Les paiements partiels restent à confirmer et ne constituent pas une décision acquise.

Les paiements anciens peuvent être supprimés pendant la fermeture annuelle seulement selon le fonctionnement validé, après sauvegarde et archivage prévus. Ce n’est ni une purge quotidienne ni une conservation arbitraire de cinq ans. Les archives doivent préserver les informations nécessaires aux soldes reportés.

## Intégrité et accès simultanés

Propositions : clés générées par le serveur/base, clés étrangères, contraintes métier, montants décimaux, transactions atomiques et jetons de version SQL Server `rowversion`. Le numéro de dossier métier reste distinct de la clé interne si nécessaire pour la migration.

Le verrou de dossier proposé dans [architecture.md](architecture.md) et la version doivent être contrôlés dans l’API lors de toute écriture. Un contrôle effectué seulement à l’ouverture de l’écran ne protège pas contre une saisie concurrente. Les changements liés au même dossier financier participent à la même protection.

## Recherche et comptage

Prévoir des index adaptés au numéro de dossier, numéro de licence, propriétaire, municipalité, race, couleur, statut, poids et date de naissance. La stratégie exacte est à mesurer sur les données migrées.

Les filtres combinent race, couleur, adresse officielle, licence, prénom, nom, dossier, poids avec opérateur/seuil variable, âge calculé et statut. Actifs par défaut; vue des inactifs disponible. Le total compte les **chiens distincts**, indépendamment des propriétaires, des anciennes licences et de la pagination. La requête de total reprend exactement les filtres des résultats.

## Conservation et sauvegardes

| Données | Règle officielle |
| --- | --- |
| Commentaires propriétaire/chien | Conservés; jamais supprimés par la fermeture annuelle. |
| Anciennes licences | Cinq ans, puis suppression selon règle de conservation retenue. |
| Historique des tarifs | Cinq ans, puis suppression. |
| Paiements | Gestion annuelle selon fermeture et archivage validés. |
| Sauvegardes | Quotidiennes, deux semaines glissantes; sauvegarde avant fermeture; restauration administrateur. |

La durée des sauvegardes ne remplace pas la durée des historiques métier. Les suppressions définitives sont des opérations sensibles administrateur; planification et déclenchement précis de purge restent à définir. Protéger les sauvegardes sur le serveur; copie externe recommandée à l’avenir.

## Migration depuis Access

1. Travailler sur une copie de `MC_data 2026.accdb`; ne jamais modifier l’original pendant l’analyse ou les essais. Examiner aussi `Muni-Chien Appli.accdb` pour les règles, rapports et traitements.
2. Documenter les correspondances de tables/champs, préserver numéros de dossier et relations utiles, normaliser sans changer arbitrairement le sens.
3. Transformer l’ancien numéro sur Chien en licence sans inventer dates ni historique absent; consigner les données inconnues et exceptions.
4. Ne pas reproduire mécaniquement les tables temporaires « Tampon ». Préserver les résultats métier nécessaires.
5. Comparer totaux, liens, soldes et échantillons. Volumes observés dans le document : environ 14 700 propriétaires, 17 400 chiens, 3 300 paiements et 23 municipalités; ce sont des repères, pas des totaux définitifs imposés.
6. Tester l’application et la restauration sur copie avant mise en service. Documenter les anomalies, corrections et procédure de retour.

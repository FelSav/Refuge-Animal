# MVP et validation — MuniChien

Révision : 6 septembre 2026. Référence : document du 5 septembre 2026, §§ 2, 5, 7–13. Ce fichier décrit le volet MuniChien uniquement. Le minimum global officiel comprend deux logiciels indépendants fonctionnels; livrer MuniChien seul ne signifie pas que l’ensemble du projet Refuge Animal est terminé.

## Résultat attendu

Remplacer l’utilisation quotidienne d’Access par MuniChien avec nouvelle base, API centrale et utilisation simultanée sur six postes. Les opérations essentielles sont réalisables manuellement, sans Acomba ni fusion avec Fidélité. Le cadre pédagogique du document ne garantit pas la livraison d’une solution complète directement exploitable en production.

## Périmètre MuniChien à livrer

- Propriétaires : dossier unique, numéro historique préservé si possible, coordonnées, adresse sélectionnée parmi des propositions officielles, statut et commentaire durable.
- Chiens uniquement : propriétaire associé, race/couleur avec autocomplétion et ajout, sexe, stérilisation, naissance, âge calculé, poids précis kg, statut, date automatique et raison facultative d’inactivité, commentaire distinct.
- Licences : émission, expiration, renouvellement annuel, même numéro possible d’une année à l’autre, remplacement de plaque perdue/endommagée, historique cinq ans et fiche imprimée de renouvellement.
- Municipalités : activation, tarifs configurables premier/deuxième chien et chenil, date limite, frais de retard, historique cinq ans.
- Finance manuelle : paiements comptant/carte/chèque/autre avec précision obligatoire, indicateur municipalité si utilisé, soldes et frais compréhensibles; paiements partiels à confirmer.
- Recherche : tous les filtres de § 5.7 combinables, poids à seuil variable, âge calculé, actifs par défaut, inactifs accessibles, total de chiens distincts.
- Rapports utiles d’Access, avis, filtres avant impression et export PDF.
- Fermeture annuelle administrateur avec sauvegarde, aperçu, choix de report des dettes lorsque requis, archivage/remises à zéro validés, recalcul et préservation des commentaires/historiques; aucune inactivation automatique des chiens.
- Sécurité serveur, protection contre l’écrasement concurrent, sauvegardes quotidiennes conservées deux semaines et restauration administrateur testée.
- Migration des données utiles depuis une copie Access, comparaison des résultats et absence de dépendance Access en production.

Cette liste ne réduit pas silencieusement le projet à un simple carnet de chiens. Les règles non détaillées et rapports existants doivent être analysés avant leur remplacement.

## Ordre de réalisation proposé

| Étape | Livrable et condition de passage |
| --- | --- |
| 1. Validation du besoin | Document officiel revu avec client/professeurs; décisions et questions suivies; Gantt séparé maintenu. |
| 2. Prototype frontend | Parcours principaux et cas d’erreur avec données fictives, présentés au client avant connexion de toute la logique aux données. |
| 3. Conception | Modèle MuniChien, contrats API, stratégie de concurrence, règles financières et conservation précisés. |
| 4. Premier parcours complet | Client → API → base : propriétaire, chien, recherche et refus d’une modification concurrente. |
| 5. Fonctions métier | Licences, remplacement, tarifs, finance, rapports/PDF et fermeture sécurisée. |
| 6. Exploitation et migration | Sauvegarde/restauration, import sur copie, contrôles de cohérence et essai simultané six postes. |
| 7. Recette | Scénarios ci-dessous exécutés, anomalies corrigées, validation client et documentation d’installation/utilisation/sauvegarde. |

Le prototype est une étape de validation, pas le MVP fonctionnel. C#/.NET/WPF/SQL Server restent les choix actuels de l’équipe et non des critères technologiques imposés par le client.

## Critères d’acceptation vérifiables

| Scénario | Résultat attendu | Référence |
| --- | --- | --- |
| Dossiers et migration | Numéros conservés si possible; données utiles, liens et soldes cohérents avec la copie Access; application autonome après migration. | MUN-001–004, § 10.1 |
| Naissance et poids | Âge calculé depuis la naissance, poids précis enregistré et filtrable; inconnues migrées non inventées. | MUN-005, MUN-010 |
| Commentaires et inactifs | Commentaires distincts conservés après clôture; dossier/chien inactif consultable avec historique. | MUN-004, MUN-006 |
| Licences | Renouvellement sans suppression du chien, même numéro possible, remplacement traçable, conservation cinq ans. | MUN-007, § 5.4 |
| Tarification | Premier/deuxième chien, chenil, échéance et frais conformes aux règles municipales validées; historique cinq ans. | MUN-008, § 5.5 |
| Paiements | Solde cohérent avec dû/frais/paiements; mode autre exige une précision; paiement municipal identifié si utilisé. | § 5.6 |
| Recherche combinée | Plusieurs filtres appliqués ensemble; total de chiens distincts correct, y compris plusieurs chiens par propriétaire et plusieurs licences par chien. | MUN-009–010 |
| Adresse | Sélection officielle proposée, gestion contrôlée d’une rue absente; aucune fausse validation si le service échoue. | MUN-011, § 5.8 |
| Rapports | Filtres respectés dans aperçu, impression et PDF; contenu de renouvellement conservé. | MUN-012 |
| Six postes | Dossiers différents créés simultanément sans doublons; deuxième modification d’un même dossier empêchée, sans écrasement même après déconnexion. | MUN-014, § 7.3 |
| Administration | Opérations quotidiennes sans comptes individuels; actions sensibles refusées par le serveur sans authentification administrateur. | MUN-013, § 8.1 |
| Fermeture | Sauvegarde préalable réussie, résumé et confirmation; report des dettes choisi si nécessaire; montants cohérents; chiens et commentaires préservés. | § 5.10 |
| Sauvegarde/restauration | Sauvegarde quotidienne, rotation deux semaines, restauration administrateur testée et cohérence des données vérifiée. | § 8.4 |

Tester aussi la frontière des cinq ans pour les historiques après validation de la date de référence, le seuil exact de poids, les anniversaires pour l’âge et la prévention d’une fermeture lancée deux fois. Ces scénarios constituent des vérifications attendues, pas des tests déjà exécutés.

## Hors dépendances du MVP

Acomba, fusion des deux logiciels, automatisation depuis la caisse et sauvegarde externe future ne conditionnent pas le fonctionnement de base. Ne pas créer ici d’écrans ou de modèle Fidélité. Les chats et autres animaux sont exclus de MuniChien. Un éditeur de contenu des avis n’est pas requis pour la première version.

## Points à résoudre progressivement

Confirmer la portée des numéros de licence et les paiements partiels. Préciser fournisseur d’adresses, règles exactes de clôture/archivage, dates de départ des conservations, rapports à reprendre et paramètres techniques serveur. Ces points n’empêchent pas le prototype; ils doivent être tranchés avant de figer les fonctions concernées. Voir [decisions.md](decisions.md).

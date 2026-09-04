\# Migration vers le nouveau Muni-Chien



Ce document contient les premières recommandations concernant la migration de l'ancien système Access vers le futur Muni-Chien.



Ces décisions sont provisoires et devront être validées après l'analyse complète du logiciel et des besoins du client.



\## Données à conserver



Les données suivantes semblent essentielles :



\- Propriétaires

\- Chiens

\- Municipalités

\- Paiements

\- Races

\- Couleurs

\- Paramètres utiles

\- Historique financier nécessaire

\- Informations liées aux avis si elles sont toujours utilisées



\## Éléments à restructurer



\### Licences



Actuellement, le numéro de licence est directement enregistré dans `Chien`.



La nouvelle architecture devrait étudier la possibilité de créer une table `Licence`.



Cela permettrait notamment de conserver un historique des licences et des renouvellements.



Exemple :



Propriétaire

&#x20;   ↓

Chien

&#x20;   ↓

Licence

&#x20;   ↓

Renouvellement / Paiement



\### Paiements



Les paiements sont actuellement associés au propriétaire.



Il faudra déterminer si la nouvelle version doit également permettre de savoir précisément quelle licence ou quelle opération correspond à chaque paiement.



\### Soldes et frais



Les soldes et frais de retard sont actuellement enregistrés directement dans le propriétaire.



Il faudra déterminer s'ils doivent rester ainsi ou être calculés à partir d'un historique de transactions.



\### Archives



Le fonctionnement actuel des archives devra être analysé afin de déterminer quelles données historiques doivent être conservées.



\## Éléments probablement inutiles dans la nouvelle architecture



Les éléments suivants semblent principalement liés au fonctionnement interne de Microsoft Access :



\- Tampon Adresse

\- Tampon année

\- Tampon solde

\- MSysCompactError



La nouvelle application devrait pouvoir effectuer ces traitements directement dans sa logique métier et sa base SQL sans avoir besoin de reproduire ces tables temporaires.



\## Migration des données



La migration devra probablement suivre cette logique :



Ancienne base Access

&#x20;       ↓

Extraction des données

&#x20;       ↓

Validation / nettoyage

&#x20;       ↓

Conversion vers le nouveau modèle

&#x20;       ↓

Import dans la nouvelle base

&#x20;       ↓

Vérification des données



\## À confirmer avec le client



Avant la migration finale, il faudra confirmer :



\- Quelles années historiques doivent être conservées

\- Si les anciens paiements doivent être conservés

\- Si les anciens numéros de licence doivent être conservés

\- Si les avis historiques doivent être conservés

\- Combien de postes utiliseront Muni-Chien

\- Si plusieurs utilisateurs travailleront simultanément

\- Comment les sauvegardes devront fonctionner dans la nouvelle version


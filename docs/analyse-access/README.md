\# Analyse de l'ancien Muni-Chien



Cette section documente le fonctionnement de l'application Muni-Chien actuellement utilisée par Refuge Animal.



L'objectif de cette analyse est de comprendre le fonctionnement du système existant avant de concevoir et développer sa nouvelle version.



\## Système actuel



Le logiciel actuel est basé sur Microsoft Access et est séparé en deux fichiers :



\- `Muni-Chien Appli.accdb`

&#x20; - Interface utilisateur

&#x20; - Formulaires

&#x20; - Requêtes

&#x20; - Rapports

&#x20; - Code VBA

&#x20; - Logique du logiciel



\- `MC\_data 2026.accdb`

&#x20; - Données des propriétaires

&#x20; - Chiens

&#x20; - Paiements

&#x20; - Municipalités

&#x20; - Avis

&#x20; - Archives

&#x20; - Paramètres

&#x20; - Tables temporaires



\## Objectifs de l'analyse



\- Identifier les données existantes

\- Comprendre les liens entre les tables

\- Comprendre les fonctionnalités du logiciel actuel

\- Identifier les règles métier importantes

\- Comprendre le fonctionnement de la fermeture annuelle

\- Déterminer quelles données doivent être conservées

\- Préparer la structure du futur Muni-Chien



\## Important



Les fichiers Access contenant les données réelles de Refuge Animal ne doivent jamais être ajoutés au dépôt GitHub.



L'analyse est effectuée uniquement sur des copies locales dédiées.



\## État actuel



Les éléments suivants ont déjà été analysés :



\- Structure générale de la base de données

\- Tables principales

\- Requêtes principales

\- Module VBA `Fonctions`

\- Sauvegarde et restauration

\- Fermeture annuelle

\- Calcul des soldes

\- Frais de retard

\- Tarification des chiens et des chenils



L'analyse détaillée des formulaires et de l'interface utilisateur reste à effectuer.


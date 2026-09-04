\# Relations de l'ancien Muni-Chien



La base Access ne contient pas de relations explicites visibles dans la fenêtre Relations.



Les liens entre les tables sont principalement gérés par les requêtes et la logique de l'application.



\## Relations confirmées



\### Propriétaire → Chien



`Propriétaire.No prop`



est utilisé avec :



`Chien.NoProp`



Un propriétaire peut donc avoir plusieurs chiens.



\### Propriétaire → Paiement



`Propriétaire.No prop`



est utilisé avec :



`Paiement.No de prop`



Un propriétaire peut avoir plusieurs paiements.



\### Municipalité → Propriétaire



`Municipalité.No\_municipalité`



est utilisé avec :



`Propriétaire.Municipalité`



Chaque propriétaire est associé à une municipalité.



\## Schéma logique simplifié



Propriétaire

│

├── possède → Chien

│

├── possède → Paiement

│

└── appartient à → Municipalité



Municipalité

│

├── définit le prix d'un chien

├── définit le prix du deuxième chien

├── définit le prix d'un chenil

├── définit la date limite de paiement

└── définit les frais de retard



\## Relations à confirmer



Certaines relations supplémentaires devront être confirmées pendant l'analyse des formulaires et des autres requêtes, notamment :



\- Municipalité → Rue

\- Avis → Propriétaire

\- Archives → Municipalité


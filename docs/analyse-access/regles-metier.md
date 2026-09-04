\# Règles métier de Muni-Chien



Ce document contient les règles importantes identifiées dans le fonctionnement de l'ancien logiciel.



\## Calcul du solde



Le logiciel calcule le solde réel d'un propriétaire selon la logique suivante :



Solde réel =

Solde dû

\+ Frais de retard

\- Paiements effectués



\## Prix des chiens



Le prix associé aux chiens dépend de la municipalité du propriétaire.



Une municipalité peut définir :



\- Un prix normal pour un chien

\- Un prix différent pour le deuxième chien



Lorsqu'un chien est ajouté ou supprimé, le solde dû du propriétaire est ajusté.



\## Deuxième chien



Certaines municipalités peuvent appliquer un tarif différent pour le deuxième chien.



La base contient un paramètre indiquant si le deuxième chien possède un tarif différent.



\## Chenil



Un propriétaire peut être identifié comme propriétaire d'un chenil.



Dans ce cas, le calcul basé sur le nombre de chiens est remplacé par un montant fixe défini par la municipalité.



Lorsqu'un propriétaire passe :



\- de chien normal vers chenil : son solde est recalculé avec le montant du chenil

\- de chenil vers chien normal : son solde est recalculé selon le nombre de chiens



\## Frais de retard



Un frais de retard peut être ajouté automatiquement lorsque :



\- Le propriétaire possède encore un solde

\- La date limite de paiement de la municipalité est dépassée

\- La municipalité possède un montant de frais de retard supérieur à zéro

\- Le dossier du propriétaire est actif



Avant la date limite, le frais de retard peut être remis à zéro.



\## Numéros de licence



Le numéro de licence est actuellement enregistré directement dans la fiche du chien.



Un paramètre permet de choisir si les numéros de licence sont conservés d'une année à l'autre.



Si cette option est désactivée, les numéros de licence sont remis à `NULL` pendant la fermeture annuelle.



\## Fermeture annuelle



Avant de commencer une nouvelle année, le logiciel effectue une procédure de remise à zéro.



La procédure actuelle :



1\. Vérifie que les paiements journaliers ont été traités

2\. Demande confirmation à l'utilisateur

3\. Crée une sauvegarde de la base

4\. Conserve temporairement les soldes impayés

5\. Remet les soldes et frais de retard à zéro

6\. Recalcule les montants dus selon les chiens

7\. Applique les tarifs de chenil

8\. Archive les données financières de l'année

9\. Supprime les anciens paiements

10\. Demande si les soldes impayés doivent être reportés

11\. Efface les licences si l'option de conservation est désactivée

12\. Avance la date limite de paiement d'une année



\## Avis



Lorsqu'un avis est envoyé, le logiciel peut enregistrer :



\- La date du dernier avis

\- Le code du dernier avis



Ces informations sont associées au propriétaire.



\## Sauvegarde



Une sauvegarde est automatiquement demandée avant une fermeture annuelle.



L'ancien système crée un nouveau fichier Access contenant une copie des tables de données.


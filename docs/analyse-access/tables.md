\# Tables de l'ancien Muni-Chien



Cette page présente les principales tables présentes dans `MC\_data 2026.accdb`.



\## Tables principales



| Table | Rôle actuel | État pour la future migration |

|---|---|---|

| Propriétaire | Contient les dossiers et coordonnées des propriétaires | À conserver et restructurer |

| Chien | Contient les informations des chiens ainsi que leur numéro de licence actuel | À conserver et restructurer |

| Paiement | Contient les paiements effectués par les propriétaires | À conserver et restructurer |

| Municipalité | Contient les municipalités ainsi que leurs tarifs et règles | À conserver |

| Avis | Contient les informations utilisées pour les avis | À analyser davantage |

| Archives | Contient des informations financières historiques par année et municipalité | À analyser |

| Race | Liste des races de chiens | À conserver |

| Couleur | Liste des couleurs | À conserver |

| Rue | Liste de rues associées aux municipalités | À confirmer |

| Paramètre | Paramètres généraux de l'application | À restructurer |

| Municipalité en cours | Définit les municipalités actuellement actives/utilisées | À analyser |



\## Tables temporaires



| Table | Rôle |

|---|---|

| Tampon Adresse | Stockage temporaire utilisé dans certaines opérations |

| Tampon année | Stockage temporaire utilisé pendant les opérations annuelles |

| Tampon solde | Stockage temporaire des soldes avant la remise à zéro annuelle |



Ces tables semblent être des tables techniques de travail et ne devraient probablement pas être reproduites directement dans la nouvelle architecture.



\## Table technique Access



| Table | Rôle |

|---|---|

| MSysCompactError | Table technique liée à Microsoft Access |



Cette table ne devra pas être migrée.



\## Particularité importante



Il n'existe actuellement aucune table dédiée aux licences.



Le numéro de licence est directement enregistré dans la table `Chien`.



Dans la future version, il faudra déterminer s'il est préférable de créer une véritable entité `Licence` afin de conserver un historique des licences et renouvellements.


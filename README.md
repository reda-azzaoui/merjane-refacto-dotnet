# Merjane Inventory

API de suivi d'inventaire de Merjane : traiter une commande met à jour le stock de ses produits et notifie les clients.

## Consignes

* Ignorez les migrations BDD.
* Ne pas modifier les classes qui ont un commentaire : `// WARN: Should not be changed during the exercise`.
* Pour lancer les tests : `dotnet test`.

## Prérequis et commandes

Le SDK .NET 10 est requis (voir `global.json`).

```bash
dotnet test
```

```bash
dotnet run --project src/Merjane.Inventory.Api
```

Point d'entrée : `POST /orders/{orderId}/processOrder`.

| Réponse | Signification |
|---|---|
| `200` | Commande traitée, corps `{ "id": 1 }`. |
| `400` | Identifiant de commande non numérique. |
| `404` | Commande inconnue. |

En développement, Swagger UI est disponible sur `/swagger`, et la description OpenAPI sur `/openapi/v1.json`.

## Architecture

Architecture DDD en couches, les dépendances allant vers le domaine :

```text
Api  ->  Application  ->  Domain
Api  ->  Infrastructure  ->  Application
```

| Projet | Rôle |
|---|---|
| `Merjane.Inventory.Domain` | Agrégats `Order` et `Product`, value object `SeasonPeriod`, événements de domaine, contrats des dépôts. Aucune dépendance. |
| `Merjane.Inventory.Application` | Cas d'usage `ProcessOrderHandler`, traduction des événements en notifications. |
| `Merjane.Inventory.Infrastructure` | EF Core, dépôts, unité de travail, implémentation des notifications. |
| `Merjane.Inventory.Api` | Endpoints Minimal API, composition de l'application. |

Les classes `WARN` gardent leur nom et leur namespace d'origine (`Refacto.DotNet.Controllers.Services`), seul leur emplacement a changé.

Pour ajouter une famille de produits : créer une classe dérivée de `Product`, implémenter `FulfillOneUnit` et déclarer sa valeur de discriminateur dans `ProductConfiguration`.

## Règles métier

Ces règles reproduisent à l'identique le comportement du code d'origine.

| Famille | Vendu si | Sinon |
|---|---|---|
| `NORMAL` | En stock | Délai annoncé si `leadTime > 0`, sinon rien |
| `SEASONAL` | En stock et saison ouverte, bornes exclues | Voir ci-dessous |
| `EXPIRABLE` | En stock et date d'expiration postérieure à aujourd'hui minuit | Stock retiré et notification d'expiration |

Pour un produit `SEASONAL` non vendu, dans cet ordre :

1. Si la date de réapprovisionnement dépasse la fin de saison : stock retiré, produit notifié indisponible.
2. Si la saison n'a pas commencé : produit notifié indisponible, stock conservé.
3. Sinon : délai annoncé.

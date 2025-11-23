# ECommerci.tn – Plateforme E?Commerce ASP.NET Core (.NET 8)

## 1. Vue d’ensemble
Solution e?commerce moderne en ASP.NET Core (.NET 8) avec identité, gestion produits, panier, commandes, paiements Stripe, promotions, notifications temps réel, audit et tableau de bord administrateur. Interface et messages en français.

## 2. Principales fonctionnalités
- Authentification & rôles (Admin, Manager, User) via ASP.NET Core Identity
- Inscription avec nom d’utilisateur + email, profil modifiable (email, username, téléphone, mot de passe optionnel)
- Gestion produits (CRUD, image, catégories, stock, corbeille)
- Panier dynamique + calcul remises + livraison
- Commandes (infos client, suivi statut, impression facture, historique)
- Paiements Stripe (Checkout) + mode manuel de test (simulation) + annulation / succès
- Système de promotions (pourcentage, livraison gratuite, frais) + badge visuel + résumé sur panier
- Notifications temps réel (SignalR / hub) + cloche + page dédiée + marquage “lu” / “tout lire”
- Audit log (actions CRUD, suppression, création promotions, etc.) avec stockage des anciennes valeurs
- Tableau de bord Admin / Manager (statistiques, stocks faibles, commandes récentes, activités)
- Gestion rôles & utilisateurs (création, édition, réinitialisation mot de passe, suppression)
- Stock & alertes (faible, rupture)
- Recherche produits + historique recherche utilisateur
- Impression commande (vue dédiée optimisée print)

## 3. Pile technologique
| Domaine | Outils |
|---------|-------|
| Backend | ASP.NET Core .NET 8, MVC/Razor Views |
| Auth | ASP.NET Core Identity |
| DB | SQL Server LocalDB (migrations EF Core) |
| Temps réel | SignalR Hub Notifications |
| Paiement | Stripe Checkout (clé publique/privée) |
| Front | Bootstrap 5, Font Awesome, Chart.js, DataTables, JS custom |
| Logs & Audit | EF + tables AuditLog |

## 4. Structure — dossiers clés
- `Controllers/` : logique MVC (Account, Product, Panier, Orders, Payment, Discount, Dashboard…)
- `Models/` : entités (Product, Category, Orders, Discount, Notification, AuditLog...)
- `ViewModels/` : modèles de transfert (auth, dashboard, panier, promotion…)
- `Views/` : Razor Views (Layout, Partials, Components)
- `Services/` : services métier (DiscountService, NotificationService, StripePaymentService, EmailService)
- `wwwroot/js|css` : scripts (notifications, datatables, currency formatter), styles, images
- `Migrations/` : migrations EF Core (évolution schéma)

## 5. Mise en route
### Prérequis
- .NET 8 SDK
- SQL Server / LocalDB
- Clés Stripe (Dashboard Stripe > Developers > API keys)

### Étapes
1. Cloner le dépôt
2. Configurer `appsettings.Development.json` :
```json
"ConnectionStrings": {"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerciDb;Trusted_Connection=True;"},
"Stripe": {"PublicKey": "pk_test_xxx", "SecretKey": "sk_test_xxx"}
```
3. Restaurer & construire
```
dotnet restore
dotnet build
```
4. Appliquer les migrations
```
dotnet ef database update
```
5. Lancer
```
dotnet run
```
6. Créer les rôles (une fois) — exemple rapide (à adapter / seed) :
```csharp
await roleManager.CreateAsync(new IdentityRole("Admin"));
await roleManager.CreateAsync(new IdentityRole("Manager"));
await roleManager.CreateAsync(new IdentityRole("User"));
```
7. Créer un utilisateur admin et lui attribuer le rôle.

## 6. Configuration & variables
| Clé | Description |
|-----|-------------|
| Stripe:PublicKey | Clé publique pour Checkout |
| Stripe:SecretKey | Clé secrète paiement |
| ASPNETCORE_ENVIRONMENT | Development / Production |

## 7. Sécurité / Bonnes pratiques
- Ne pas committer les clés Stripe réelles.
- Forcer HTTPS (en production).
- Vérifier validation côté serveur pour promo & paiement.
- Mettre en place rate?limiting sur endpoints sensibles si exposé public.
- Journaliser erreurs critiques (Serilog recommandé en évolution).

## 8. Paiement Stripe — flux
1. Création Session Checkout (POST serveur `/Payment/CreateCheckoutSession`)
2. Redirection Stripe hébergée
3. Webhook (à implémenter en production) pour validation définitive
4. Mise à jour statut commande (Paid / Confirmed / Delivered)

## 9. Promotions / Discounts
- Type % : réduction sur prix unitaire
- Livraison gratuite : remplace frais par 0
- Logiciel calcule prix réduit + économies par ligne
- Panneau admin: création, modification, activation/désactivation, suppression

## 10. Notifications temps réel
- Hub SignalR push nouvelles notifications (ex: nouveau user, commande)
- Dropdown + page liste avec filtres (lues / non lues / catégories)
- API endpoints lecture / marquer tout comme lu

## 11. Audit Log
- Stocke : ActionType, EntityType, EntityId, EntityName, OldValues, NewValues, User, IP, Timestamp
- Utilisé pour traçabilité (suppression produit, modification promo, etc.)

## 12. Impression commande
- Vue `Orders/Print.cshtml` sans layout, CSS optimisé pour A4
- Totaux + adresse + statut

## 13. Internationalisation / langue
- Interface FR (libellés nettoyés — suppression caractères mal encodés)
- Possibilité future : resx / IStringLocalizer si multi?langue requis.

## 14. Extension future
- Webhooks Stripe (paiement asynchrone)
- Mode multi?devises (adapter CurrencyFormatter)
- Tests unitaires (xUnit) pour services Discount & Notifications
- Cache produits populaires (MemoryCache / Redis)
- API REST publique (versionnement / Swagger)

## 15. Contribution
1. Fork / branche feature
2. Respect conventions C# (PascalCase, DI via constructeur)
3. Pull Request avec description claire (FR ou EN)
4. Ajouter tests si logique critique

## 16. Scripts utiles
| Action | Commande |
|--------|----------|
| Migrations (add) | `dotnet ef migrations add NomMigration` |
| Migrations (update) | `dotnet ef database update` |
| Lancer appli | `dotnet run` |

## 17. Résolution problèmes courants
| Problème | Solution rapide |
|----------|----------------|
| Caractères "??" | Fichiers encodés ISO ? sauvegarder UTF?8 sans BOM |
| Erreur paiement Stripe 404/415 | Vérifier URL endpoint & headers JSON |
| Images non affichées | Vérifier chemin `wwwroot/images/` & droits fichier |
| Notifications non push | Vérifier hub SignalR + JS côté client (console) |
| Statuts commande incohérents | Vérifier enum `OrderStatus` & logique mise à jour |


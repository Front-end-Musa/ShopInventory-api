# Analytics backend

See [the frontend integration guide](../ShopInventory-frontend/ANALYTICS.md) for the overview contract, deployment settings, sales export format, availability semantics, and browser verification.

From this backend directory:

```sh
dotnet ef database update --project ShopInventory.Api
dotnet test ShopInventory.Analytics.Tests
dotnet run --project ShopInventory.Api --launch-profile https
```

The additive InventoryAnalytics migration was applied locally during implementation. Restart any existing API process to load the new controller. Sales integration requires an external source to publish the normalized daily export; unavailable sales are never represented as zero activity.

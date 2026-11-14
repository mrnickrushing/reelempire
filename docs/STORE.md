# Store, purchases and App Store Connect

App Store Connect app **Reel Empire: Idle Tycoon**, Apple ID 6834551337, bundle id `com.rushingtech.reel`, team
PH4AKDQ4Q7. Nothing here submits the app for review: that is done on the website when the game is ready.

## In-app purchases

Products are `Sim.Products` (`Assets/Scripts/Core/Sim.Store.cs`); `Tools/appstore/iap.py` has the same list.

| Product id | Type | Price | Contents |
|---|---|---|---|
| reelempire.gems.small | Consumable | $1.99 | 100 gems |
| reelempire.gems.medium | Consumable | $4.99 | 300 gems |
| reelempire.gems.large | Consumable | $9.99 | 700 gems |
| reelempire.gems.huge | Consumable | $19.99 | 1,600 gems |
| reelempire.starter | Non-consumable | $2.99 | Deckhand's Pack: 250 gems (with the purchase only) and an extra porter and truck in every harbour |
| reelempire.ledger | Non-consumable | $7.99 | Captain's Ledger: all income doubled |
| reelempire.fleet | Non-consumable | $4.99 | Second Boat Out: a second voyage slot |
| reelempire.nightcrew | Non-consumable | $4.99 | Night Crew: 4 more hours of away earnings |
| reelempire.season | Non-renewing subscription | $7.99 | The premium track of the season running when it is bought |

Purchases go through `Assets/Scripts/Game/App/Store.cs`: RevenueCat on device (compiled when the package defines
`REELEMPIRE_REVENUECAT`), a test store with pretend purchases in the Editor only, and nothing at all on a device whose
store isn't working (never free). `Sim.BeginPurchase` notes a purchase before it starts, `Sim.Grant` pays out once per
App Store transaction, `Sim.ReconcilePurchases` delivers one that was charged but never reported, and
`Sim.RestoreOwned` brings the one-time products back (SHOP > Restore purchases).

RevenueCat: project **Reel Empire** (`4fd96cbf`), App Store app `app75547cba02` for `com.rushingtech.reel`, using the
team's in-app purchase key (8V5W657NH4). Its public Apple SDK key is `RevenueCatStore.ApiKey` (made to ship in the
app). The game buys and restores by product id, so no entitlements or offerings are needed; once the products exist
in App Store Connect, import them in RevenueCat (Product catalog > Products > Import) so its charts name them.

## Game Center

`Assets/Scripts/Game/Platform/GameCenter.cs` reports five leaderboards (`reelempire.board.earnings`, `.species`,
`.stars`, `.heaviest`, `.derby`) and every in-game achievement (`reelempire.ach.<id>`, points = its gems). They are
created from `docs/appstore/gamecenter.json` (pictures in `docs/appstore/gamecenter/`, painted by
`Tools/art/gc_badges.py`); `GameCenterTests` fails if the file and `Sim.Achievements` drift apart.

## Play statistics

`Tools/stats-worker` is deployed as the Cloudflare Worker `reelempire-stats`
(https://reelempire-stats.mrnickrushing.workers.dev, D1 database `reelempire-stats`); see its README to query it.

## Setting App Store Connect up

The App Store Connect API key lives only in the Codemagic app **train** (variable group `ios_credentials`), so the
jobs run there from the train repo's branch `codemagic/reelempire`, reading this repo:

- `reelempire-setup` runs `Tools/appstore/` in order: `capabilities.py` (iCloud, Sign in with Apple, Game Center on
  the app ID), `iap.py`, `gamecenter.py`, `listing.py` (`docs/appstore/listing.json` and `docs/appstore/screens/`).
  Every script leaves what is already there alone, so the job is safe to run again.
- `reelempire-testflight` downloads the Xcode project exported by `Tools/ios-export.sh` (release asset `ios-latest`),
  numbers the build, signs and uploads it for internal testers only. Ask before running it.

Screenshots: Play mode, then `Tools/store-shot.sh <iphone|ipad> out.png` (2868x1320 and 2752x2064), saved as JPG in
`docs/appstore/screens/`. The app icon is `Assets/Icon/icon.png` (the koi model on a sea background).

App Privacy to declare on the website: linked to the player, for app functionality: Name (the harbour master's name,
on the device and in iCloud only), User ID (RevenueCat app user id), Purchase History (RevenueCat); not linked, for
analytics: Device ID (the hashed statistics id), Product Interaction and Performance Data. No tracking. Keep it in
step with `docs/privacy.html`.

Site (GitHub Pages, branch `gh-pages`, copied from `docs/`): https://mrnickrushing.github.io/reelempire/ with
`privacy.html` and `support.html`.

# Reel Empire: Idle Tycoon — working notes for coding agents

A fishing harbour idle tycoon for iPhone and iPad (landscape), Unity **6000.0.85f1**, URP. Rushing Technologies.
Design: `docs/DESIGN.md`; the genre research behind it: `docs/RESEARCH.md`; the free models to use: `docs/ASSETS.md`.
Original names and places only, nothing recreated from an existing game. Free models only (CC0 first; CC BY credited
in `CREDITS.md` and in the game). **No ads, ever.** The project was started from Crosswind (`~/crosswind`): the
platform code, tools and conventions are the same.

- `Assets/Scripts/Core` (no Unity dependency): `Content` (berths, goods, harbours, tuning), `Content.Species` (`Book`:
  16 species per harbour), `GameState` (the save; every class `[Serializable]`), `Sim` (the tick: boats, haulers, hall,
  trucks; taps), `Sim.Chain` (rates, costs, buying, skippers, the hall's lines), `Sim.Economy` (multipliers, the
  bottleneck, idle rate, away earnings), `Sim.Book`, `Sim.Conditions` (24-minute day, weather every 3 h, four-week
  seasons, all from the real time), `Goals` (the guide), `Sim.Achievements`, `Sim.Store` (products, grants once per
  transaction), `Sim.Reminders`. Fish are counted in units worth one coin fresh; each harbour has its own cash.
- `Assets/Tests/EditMode`: `SimTests`, `SaveTests`, `StoreTests`, `PacingTests` (a greedy player; prints the first
  hours: run it after any tuning change). Build sessions with `TestGame.New()` (fixed clock).
- `Assets/Scripts/Game` (everything built from code at runtime; `GameRoot` boots in any scene): `World/Layout` (sea on
  the left, quay along x = 0, hall, road, market), `World/HarbourView` (draws the simulation), `World/Scenery` (each
  thing uses `Models.Kit(id)` when `Resources/Models/<id>` exists, else its own shape), `Shaders/Water`,
  `UI/Hud` (cash, chain bar, cards), `Hud.Signs` (level signs, TAP bubbles, chips, toast, guide), `Hud.Sheet` (the
  upgrade sheet), `Hud.Rate`, `UI/LoadingScreen`, `App/Store` (RevenueCat on device, a test store in the Editor),
  `Platform/` (Sign in with Apple, iCloud save, reminders, Game Center).
- Look (2026-11-13, deliberately unlike Last Tide and Crosswind): a bright toy diorama and a cut-paper interface.
  World: Kenney kits converted by `Tools/art/models.py` (`python3 -I Tools/art/kenney.py <kits>` first) into
  `Resources/Models/<id>.fbx`, drawn with `Materials.Kit` (`KitTint` lifts the industrial kit's dark buildings);
  dock hands are four frozen strides swapped by `Strider`; no specular highlights (`_SPECULARHIGHLIGHTS_OFF`) and
  reflection intensity 0.15, or the HDRI's sun glares off the ground. Interface: `Tools/art/ui_art.py` paints cream
  panels, candy buttons, pills, tags, the ribbon, badge, coin, gem, star and bubble, all with a navy outline and a hard
  drop shadow; fonts Lilita One (headings, numbers) and Fredoka SemiBold; text on paper is `Ui.Ink`, text on colour
  is white with `Stroke()`. Layout: cash pill top-left with the pace tab under it, gems top-right, the harbour's name
  on a ribbon, the guide card left, the chain as four round badges along the bottom, the upgrade sheet rising from
  the bottom (`Hud.Sheet`). Keep `CREDITS.md` in step with what ships.
- Buildings and docks: `Tools/art/props.py` fetches single models from poly.pizza (CC0 / CC BY only, authors kept in
  `art-src/free/props/manifest.json`); `models.py` converts them. The hall is Quaternius's Big Barn, the market a
  Barn with stalls, piers are `dock` pieces end to end; they are painted darker than the Kenney boats, so they are
  drawn through `Models.Kit(..., tint)` (`Scenery.Lift`). The App Store Connect app exists: Apple ID 6834551337.
- Fish: `Tools/art/fish.py fetch|render|species` downloads the fish models (poly.pizza; refuses anything not CC0 / CC
  BY), renders each with `Tools/blender/icon_render.py`, and makes one picture per species (its model's picture,
  hue-shifted by the table in the script) in `Resources/UI/Species/<harbour>_<name>.png` (`Hud.SpeciesPicture`).
- Screens (`UI/Hud.Screens.cs`, round buttons in the bottom corners): BOOK (tiles by harbour; uncaught ones are
  silhouettes with when they bite), AWARDS (claim gems), SHOP (time warps for gems, the one-time purchases, gem packs,
  restore), MORE (sound, music, battery saver, reminders, statistics, Sign in with Apple, name, iCloud, Game Center,
  rate, privacy, support, credits, delete account). `Frame` + `Entry` build them; the in-game credits are
  `CreditsText`: keep them in step with `CREDITS.md`.
- The long game (`Core/Sim.Progress.cs`): `Renovate` (all eight berths + cash; the harbour starts again at x2, up to
  five times; pays renown = 40 x sqrt(lifetime / 1e9), +2% income everywhere each), `OpenNextHarbour` (after one
  renovation, paid in the newest harbour's cash), `Travel`, and the Tide Boost (`UseBoost`: three free charges, one
  back every 4 h, x2 for an hour each, 8 h at most; away earnings ignore it). UI: the ribbon opens HARBOURS
  (`Hud.ShowHarbours`), the x2 badge under the gems is the boost. Each water has its own colours and plants
  (`World/Layout.cs` `Theme.For`). Money reads K, M, B, T, aa, ab... (`Format.Money`).
- First-run tutorial (`UI/Hud.Tutorial.cs`, `GameState.tutorialStep` 0/1/2): Marta's welcome card, then her speech
  bubble (lines in `Coach`, keyed by guide step up to `berth3`) beside the pointing hand, a goodbye card, SKIP at any
  time. Her portrait is rendered from the kit character (`art-src/free/faces/jobs.json` through `icon_render.py`).
- Not done yet (see DESIGN.md milestones): the logbook (spending renown), crew cards, voyages, daily
  gifts and tasks, derbies, the season pass, the trophy hall, the iCloud "newer save" offer
  (`Hud.OfferCloud` is empty), the privacy and support pages, App Store Connect setup.

## Verify every change

```bash
Tools/core-test.sh                           # Core tests without Unity
Tools/core-test.sh --logger "console;verbosity=detailed" --filter "Name~FirstHours"   # the pacing timeline
unity pipeline install --project-path .     # once per clone (local dev tool, never commit it)
unity open . --until-ready
Tools/unity-recompile.sh
Tools/unity-capture.sh out.png 8
```

Keep `com.unity.pipeline` out of the committed `Packages/manifest.json`; don't commit `Packages/packages-lock.json`.
Only one Unity Editor open at a time on this machine.

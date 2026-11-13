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
- Not done yet (see DESIGN.md milestones): renovation and renown, crew cards, boosts, voyages, the
  book and shop screens, settings and accounts UI, more harbours, derbies, the season pass, the tutorial, App Store
  Connect setup. The rating prompt's App Store id is a placeholder until the app exists.

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

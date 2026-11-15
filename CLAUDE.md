# Reel Empire: Idle Tycoon — working notes for coding agents

A fishing harbour idle tycoon for iPhone and iPad (landscape), Unity **6000.0.85f1**, URP. Rushing Technologies.
Design: `docs/DESIGN.md`; the genre research behind it: `docs/RESEARCH.md`; the free models to use: `docs/ASSETS.md`.
Original names and places only, nothing recreated from an existing game. Free models only (CC0 first; CC BY credited
in `CREDITS.md` and in the game). **Ads are rewarded only, never forced**: one plays only when the player taps for it. The project was started from Crosswind (`~/crosswind`): the
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
  Memorable fish get their own shape (no pike drawn as a swordfish): trout, kingfish (pike, musky, barracuda, wahoo),
  halibut and the skate seen from above (`PITCH`), seahorse, parrotfish, eels. Sturgeon, trevally, toothfish and oarfish
  are cut from one paid Higgsfield sheet (`fish.py ai`, `art-src/free/fish/ai_sheet.png`; regenerating it costs credits).
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
- Daily play (`Core/Sim.Live.cs`, the TODAY page in `Hud.Screens`): the seven-day gift (`ClaimGift`; a missed day
  starts the week again), three daily tasks counted from the day's start (`RollTasks` in `Tick`), voyages (one boat
  away for 30 min / 2 h / 8 h, a second with `reelempire.fleet`; the first one sent asks for notifications) and the
  Skipper's Logbook: eight perks that open at renown thresholds (`Logbook(id)`; nothing is spent).
- Crew (`Core/Sim.Crew.cs`, the CREW page): twelve cards, three per stage, from the crew chest (free every 4 h or 40
  gems; odds 70/22/7/1 printed on the page; a sure Epic by the tenth chest). One card stands duty per stage
  (`Assign`, `State.duty`) and multiplies it through `Fx`; copies raise its level (to 10). Portraits are the Kenney
  mini characters rendered by `art-src/free/faces/crew_jobs.json`. Each hand on duty has a skill (`Sim.Skills`).
- Longer rhythms (`Core/Sim.Season.cs`): the trophy hall (`Mount` a caught Rare / Epic / Legendary from its BOOK tile:
  +1% / 3% / 8% income everywhere, `TrophyMult`), the season pass (four weeks, 30 tiers, `AddSeasonPoints` from levels,
  species, tasks, voyages and derby goals; `reelempire.season` buys the premium track of the season running), and the
  weekend derby (Friday to Sunday local time; `DerbyPoints` per fish landed while playing, five goals, a placing among
  49 rivals whose scores come from the derby's number and how far through it is: `RivalScore`). Their pages open from
  TODAY. `Hud.OfferCloud` asks before a newer iCloud save replaces this device's.
- Retention (`docs/RETENTION.md` is the research, DESIGN.md "Keeping players" the checklist; `Core/Sim.Hooks.cs`):
  generated guide steps after the written ones (`Generated`, `PrepareGenerated`), the drifting crate (`TickBonus`,
  `ClaimBonus`; a chest model and a gold bubble in the harbour), `CollectAwayDoubled` (free once a day), `WelcomeHome`
  after three days away, `Unlocked(feature)` (nav buttons and TODAY sections appear as the harbour grows; news shown
  once, `State.seenFeatures`), `StarterOfferDue` (never on the first day), `Affords`. Feel: `CoinBurst`, `Haptics`
  (`Plugins/iOS/ReelEmpireHaptics.mm`), `HoldButton` on UPGRADE, `BigCatch`. Reminders: two at most, none between
  22:00 and 08:00. Don't add: forced or interstitial ads, energy, pay-only walls, punishing streaks, offers at launch, fake players.
- Clarity (`Core/Sim.Clarity.cs`, `UI/Hud.Target.cs`): every upgrade says what it does for the whole harbour
  (`UpgradeVerdict`: "+X% for the whole harbour" or what holds it back, measured by `PaceAfter`), the sheet's
  `LimitNote` (boats, porters and market set how MANY sell; the hall sets VALUE), the hall's lines with their pace and
  BEST NOW (`PaceWithGoods`, `BestGoods`), a skill's gain before it is used (`SkillGain`, the `_probe` stage), a pinned
  species from its BOOK tile (`Pin`, `Plan`: where, when, bait and the next useful step; the TARGET card under the goal,
  DO IT puts the bait on or sails there; let go once landed), what a purchase would be worth here (`PurchaseBenefit`, the
  shop's green line), and the starter pack's timing test (`StarterVariant` by `State.cohort`: "later_day" or "chain",
  offered once a line has run and an order is filled). Settings' "Labels over the harbour" FEWER shows only the slowest
  part, the open sheet and the best upgrade for its price (PlayerPrefs `re_compact_signs`). Telemetry: Target.pinned/
  applied/landed, Skill.used, Starter.offered, Labels.changed.
- Depth (`Core/Sim.Depth.cs`): each species fights in its own way (`StyleOf`: Steady, Runner, Darter, Stayer;
  legends are Stayers; `Fight.Warning` telegraphs a run, the HUD shivers the float and creaks). Customers come to know
  the harbour (`State.ties`, `TieLevel`: regular 2 / friend 5 / partner 10 orders, +10/20/35%, a line when they become
  friends) and each has a favourite line (`FavouriteOf`, `Order.favourite`, +25%). Fleet presets (`SavePreset`,
  `UsePreset`: baits and crew on duty, on the CREW page). From Boat Shed level 2 its crew mends the worst worn boat every
  20 minutes of play (`TickMend`, `Mended`). Recruit tokens (`State.tokens`: one a chest, 1-5 a derby placing, two for
  a copy past the top level; `Recruit` a chosen card for 6/15/40/100). Derby formats turn weekly (`FormatOf`:
  Classic, Variety, Hand-Line, Big Ones; rivals and goals scale by `ScaleOf`; the "other skippers are part of the game"
  line stays). A renovated harbour specialises (`SetFocus`: collecting, processing, voyages, transport; a change
  waits a day; `FocusFx`). Telemetry: Preset.saved/used, Crew.recruited, Focus.chosen, Customer.closer.
- Meta (`Core/Sim.Meta.cs`, `UI/Hud.Meta.cs`, `World/Decor.cs`): harbour paint (`Cosmetics`, one per part: boats,
  docks, market, lighthouse, bunting; gems only, never earnings; SEE IT in the shop tries it on the harbour itself via
  `HarbourView.PreviewId` with the HUD faded out). Each season has a theme (`SeasonThemes`, `SeasonTheme`) whose paint
  set is the premium reward at tiers 10/20/30 (the last `_gilt`). Collections (`Collections`, COLLECTIONS page from
  TODAY) and the town (`Town`, by `ordersDone`) put decorations in the harbour (`Decor`, built by `Decor.Build`).
  A record catch by hand opens a card with SHARE (`Platform/Share.cs` + `Plugins/iOS/ReelEmpireShare.mm`, the share
  sheet) and MOUNT IT. Voyages chart fishing grounds (`Grounds`, three per route; all three: +25% voyage cash).
  Chefs' requests (`OrderKind.Hand`: one fish landed by hand, PIN to make it the target) teach a recipe (+10% on that
  line in that harbour, `Recipes`). Telemetry: Paint.previewed/bought/worn, Collection.claimed, Record.caught/shared/
  mounted, Voyage.found, Recipe.learned, Target.pinned (from an order).
- Buildings that explain the chain (`World/Facilities.cs`): the fish hall (belt from the quay through the open door,
  `Belt`, running while there's fish to work; outside it the gear of each line owned: ice tables, cleaning trough,
  fillet shed, smokehouse with smoking chimneys, cannery tanks and tins; a fish painted on the roof slope that faces the
  water), the fish auction (boxes on the floor, buyers, the auctioneer's stand, price board), and milestone gear on the
  hall (awnings 10, vents 25, hoist 50, gilded fish 100, pennants 200), market (buyers, lights, van, clock, bunting),
  quay (hand trucks, box stacks, crane, weigh station) and each pier (boxes, net rack, davit, lamp, pennant).
  `HarbourView.GearKey` redraws when one is reached. Each water builds its own way (`Theme`: wall/roof colour and
  finish, market roof, quay surface, snow) and has a landmark (`World/Landmarks.cs` at `Layout.Landmark`/`WaterMark`,
  kept clear of trees and rocks): boathouse, water mill (`Spin`), cottages and crab pots, stilt huts and lanterns,
  container crane and stacks, timber church and stockfish racks.
- Trophy hall (`World/TrophyHall.cs`, `UI/Hud.Trophy.cs`): a little museum in every harbour (`Facilities.Museum` at
  `Layout.TrophyHall`; tap it, the Book's TROPHIES button, or MOUNT IT on a record/rare card) opens a room built at
  `TrophyHall.Origin`, turned to the camera's yaw. Every mounted fish hangs on a board (rarity frame, brass plate with
  weight and the day it went up from `State.trophyNotes`; older mounts say "an old catch"), the just-mounted one framed
  gold with NEW; `Sim.HallStar` stands in the glass case (tap a board to change it); WALLS cycles `Sim.HallWalls`.
  `CameraRig.LimX/LimZ/LimD` are swapped for the room and restored on BACK. Species pictures are square with empty
  space above and below the fish, so they are sized by width (`TrophyHall.Fit`).
- Fishing by hand happens in the world (`World/AnglerView.cs`, rules and HUD in `UI/Hud.Angling.cs` + `Core/Sim.Angling.cs`):
  FISH takes the camera to the south end of the quay (its limits locked there, restored on ✕), the rest of the HUD
  steps aside (`TryClear`) and the whole screen is the one button (`PressArea` on a full-screen image under the cards).
  The angler holds a `LineRenderer` rod that bows with the line's pull; the line sags when slack and runs straight
  and red near breaking; the cast strip (pale/green/gold, `Near`..`Far` metres out) and its mark lie on the water;
  dark species outlines swim under it (the big ones out in the gold), one darts to the float on a bite and becomes
  the hooked fish; a landed fish is swung up and held over the angler with a rarity glow. A record still opens its card.
- Not done yet (see DESIGN.md milestones): the privacy and support pages, App Store Connect setup.

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

# Reel Empire: Idle Tycoon — game design

A fishing harbour idle tycoon for iPhone and iPad (landscape), Unity 6000.0.85f1, URP. Rushing
Technologies. Original names, places and characters only; nothing recreated from an existing game. Free models only
(CC0 first, CC BY credited in `CREDITS.md` and in the game). **No ads, ever**: purchases only.

Research behind this document: `docs/RESEARCH.md` (idle tycoon conventions, the existing fishing idle games and what
their players complain about, with sources).

## The pitch

Start with one rowboat on a quiet lake. Catch fish, haul them up the dock, clean and pack them, sell them at the
market. Hire skippers so it runs by itself, come back to a full till, and grow the harbour into a fleet that works six
waters from the lake to the Arctic. Every species caught goes into the book; the biggest hang in the trophy hall.

What sets it apart from the fishing idle games already in the store (hook clickers, seafood factories, carry-arcade
shops, cute villages, aquariums):

- a grounded 3D harbour diorama where the whole chain is on one screen, left to right: sea, dock, hall, market;
- a real choice in the processing hall (fresh, fillet, smoked, canned) against daily market prices;
- a species book driven by the real time of day, weather and season (bonuses, never penalties);
- a trophy hall as the visible, lasting reward;
- no ads at all: everything other games gate behind an ad is free on a timer here.

## Pillars

1. **The loop is always visible.** Boats, haulers, hall and market on one screen; the weakest stage pulses and the
   guide points at it. A bottleneck is always solved by an upgrade the player can afford soon, never by a purchase.
2. **Something good every 30 seconds early, every session later.** First upgrade inside 10 seconds, first milestone
   inside 2 minutes, first skipper (free) inside 4.
3. **Generous free play.** Free Tide Boosts, free chests on a timer, a free daily double on the welcome-back card.
   Crew cards come mostly from play; odds are printed; a pity counter guarantees the good ones.
4. **Fair purchases.** One-time permanent upgrades, a season pass, gems. No weekly subscription, no crew that only
   money buys, no pop-up offers at launch.

## The production chain (one harbour)

```
Berths 1..8  ->  Dock (haulers)  ->  Hall: Clean -> Fillet / Smoke / Can  ->  Market (trucks)
generators       transport           value multiplier, product choice        sells
```

- **Berths.** Each berth is a boat fishing one water with its own species table. Levels 1-400. A trip takes
  `TripTime`, brings `Catch` fish worth `Value` each to the berth's crate (capacity `Deck`). Berth n's base value is
  8x berth n-1's; its unlock costs 12x the previous unlock.
- **Dock.** Haulers carry crates from the berths to the hall: `count x capacity / round-trip`.
- **Hall.** Lines unlock in order: Clean (x1.5 value), Fillet (x2.5), Smokehouse (x4), Cannery (x6). Each line has a
  level (throughput). The player picks what the hall makes; what the hall can't keep up with is sold fresh, so nothing
  ever stalls.
- **Market.** Trucks sell what the hall puts out: `count x capacity / round-trip`. The day's market price favours one
  product by +50% (changes at local midnight).

Throughput of the chain is the minimum of its stages; the HUD's chain bar shows each stage's rate per minute.

### Numbers

- Level cost: `C0 x r^level`; r = 1.08 for berths, 1.10 for dock, hall lines and market.
- Output grows linearly with level, times milestone multipliers at levels 10, 25, 50, 100, 150, 200, 250, 300, 350,
  400 (alternating x2 value and x2 speed; an extra hauler/truck/angler at 25, 100, 200).
- Buy x1 / x10 / x100 / NEXT (to the next milestone) / MAX, with the closed-form bulk cost.
- Numbers on screen: K, M, B, T, then aa, ab, ac...; three significant digits.
- Money is a `double`. Time to the next unlock should grow 1.2-1.5x per step (`PacingTests` prints the timeline).

## Systems

1. **Harbours.** Six waters, each its own 3D scene, species and cash: Stillwater Lake, Alder River, Gull Coast,
   Lantern Reef, The Deep, Frostmere (Arctic). Only the harbour on screen runs in real time; the others earn at their
   idle rate. Gems, crew, renown and trophies are global. Unlocking the next harbour costs cash plus stars earned from
   the current one's milestone list.
2. **Renovation** (per harbour, up to 5 times): resets that harbour's levels and cash for a permanent x2 / x4 / x8 /
   x16 / x32 income multiplier there. The panel shows exactly what is kept and gained.
3. **Renown** (global): `floor(40 x sqrt(lifetime cash / 1e9))`, paid as the difference at each renovation; every
   point is +2% income everywhere. Spent in the **Skipper's Logbook** (research): offline hours, boost length, ability
   cooldowns, cheaper berths, voyage speed.
4. **Skippers and crew.** A skipper per stage is hired for cash and runs it without taps (the first is free). Crew
   cards (Common, Rare, Epic, Legendary) are assigned to a stage: a passive bonus and an active ability (1-5 minutes,
   10-60 minute cooldown). Cards come from the free chest (every 4 hours), voyages, quests, the pass and events;
   copies level them; a sure Epic by the tenth chest; odds printed on the chest.
5. **Species book.** 16 species per harbour in five rarities. Some only bite at night, in rain, in a storm or in one
   season. First catch pays gems; a finished page is +10% in that harbour. Each species keeps its record weight.
6. **Trophy hall.** Record fish are mounted; each mounted trophy gives a small permanent bonus.
7. **Conditions.** Real day and night, weather rolled every 3 hours from the date (the same for everyone, no server),
   four-week seasons. Each adds bite chances or a small bonus.
8. **Tide Boost.** Free: x2 income for an hour, three charges, one back every 4 hours, stacking to 8 hours. Rare x5 /
   x10 boosts from chests and events multiply with it.
9. **Voyages.** Send a spare boat for 30 minutes, 2 hours or 8 hours: cash, gems, cards, boosts. Starting the first
   one is when the game asks to send notifications.
10. **Quests.** A guide chain that teaches the next thing, three daily tasks, a 7-day gift calendar.
11. **Derbies** (events). Three to five days, a small separate harbour with its own cash and five milestone rewards,
    ranked against 49 simulated rivals (no server) and a Game Center leaderboard. First one on day 2, then weekly.
12. **Season pass.** Four weeks, 30 tiers, free and premium tracks; points from upgrades, catches and quests.
13. **Away earnings.** 2 hours to start; the Logbook adds up to 6; Night Crew adds 4. The welcome-back card shows the
    breakdown: COLLECT, DOUBLE (free once a day, then gems).
14. **Game Center.** Leaderboards: lifetime earnings, renown, species caught, heaviest fish, derby score. An
    achievement for every harbour, renovation, book page and the big milestones.
15. **Accounts.** Sign in with Apple (names the harbour master, ties purchases to the account), the save in the
    player's iCloud, account deletion, restore purchases. No server of our own beyond anonymous play statistics.
16. **Rating prompt.** Once, at a happy moment, as in every Rushing Technologies game.

## Depth: decisions on top of the chain

Added after the first TestFlight build, when the game was "upgrade and wait" and little else:

- **Bait** (`Sim.Bait`): each dock picks what it fishes with from level 10, free to change. Groundbait for money
  (x1.5, nothing rare), spinners and chum for rare fish, glow bait for the night, rain, storm and season fish.
- **Orders** (`Sim.Orders`): from the fourth dock the town asks for things, three at a time: so much of a product (run
  that line), so many of a species, so many uncommon or rare fish (bait matters). Nothing expires. Cash, gems, notes.
- **Crew skills** (`Sim.Skills`): the hand on duty at each stage has a burst to call on from the main screen, then
  rests. It pays at the stage that is holding the harbour up; the Epic and Legendary ones always pay. Never counted
  in away earnings or time warps.
- **Works** (`Sim.Works`): five plots round the harbour, three levels each, in that harbour's cash: Net Loft, Boat
  Shed, Ice House, Auction Stand, Lighthouse Lamp. Cleared by a renovation.
- **Research** (`Sim.Works`): notes from filled orders and new species buy twelve lasting improvements in three
  branches at the chart room. Never lost.
- **Fishing by hand** (`Sim.Angling`, `Fight`): cast for the gold water, strike on the bite, hold to reel and ease
  off when the fish runs. Unlimited (no energy). Better odds than the boats, heavier fish (records), counts for the
  book, orders and the derby, pays about eight seconds of the harbour's pace times the fish's rarity.

## Purchases

| Product id | Type | Price | What it gives |
|---|---|---|---|
| reelempire.gems.small | Consumable | $1.99 | 100 gems |
| reelempire.gems.medium | Consumable | $4.99 | 300 gems |
| reelempire.gems.large | Consumable | $9.99 | 700 gems |
| reelempire.gems.huge | Consumable | $19.99 | 1,600 gems |
| reelempire.starter | Non-consumable | $2.99 | Deckhand's Pack: 250 gems, an extra porter and an extra truck in every harbour, for good |
| reelempire.ledger | Non-consumable | $7.99 | Captain's Ledger: all income doubled, every harbour, for good |
| reelempire.nightcrew | Non-consumable | $4.99 | Night Crew: 4 more hours of away earnings and a free DOUBLE every time |
| reelempire.fleet | Non-consumable | $4.99 | Second voyage slot, for good |
| reelempire.season | Non-renewing | $7.99 | The premium track of the current season |

Gems are also earned by playing and are spent on time warps (1 h / 4 h / 24 h of income), crew chests, finishing a
voyage, an extra DOUBLE. Not in 1.0: a subscription, a piggy bank, event passes.

## Reminders (local notifications, at most four planned, never two within ten minutes)

Docks full (away cap reached); voyage back; free chest ready; Tide Boost fully recharged; derby ends in 3 hours;
tomorrow's gift.

## Pacing targets

First hour: tap the first catch (0:10), berth 1 level 10 (1-2 min), berth 2 and the free skipper (2-4), the dock
becomes the bottleneck and the guide shows it, Clean line and berth 3 with the first Tide Boost (4-8), market skipper
and the first scripted rare fish (8-15), Fillet and product choice, berths 4-5, first voyage and the notification ask
(15-30), first crew card and ability, berth 6, the away cap explained (30-60).

First week: first renovation after 2-3 hours of play (day 1), first derby and the Smokehouse (day 2), Alder River and
the Logbook (day 3), trophy hall (day 6-7), Cannery and harbours 3-4 (days 8-18).

## Keeping players (from `docs/RETENTION.md`)

What the research says a game of this kind needs, and where it is here:

- **The whole chain on one screen, the weakest stage marked**: the harbour and the chain badges.
- **A win every 10-20 seconds at first; the first skipper is free**: the guide's first steps, with Marta talking.
- **Always a next goal, with a bar**: the guide never runs out (`Sim.Generated`), the next berth's sign shows how
  far the cash has come, every level sign turns green when it can be bought.
- **One new thing at a time**: the book, awards, TODAY, the Tide Boost, the crew, voyages, the derby, the season and
  renovation each appear when the harbour has grown into them (`Sim.Unlocked`), with a word of news.
- **Coming back pays, staying away costs nothing**: away earnings to a stated cap (3 hours, 8 by fifty stars, more
  with the logbook and Night Crew), a welcome-back card that says what the money buys, a free DOUBLE every day, a
  welcome for someone away three days or more, a gift that never resets for a missed day.
- **Timers of every length**: a drifting crate every two to four minutes of play (`ClaimBonus`), the Tide Boost and
  crew chest every four hours, voyages of 30 minutes to 8 hours, the day's gift and tasks, the weekend derby, the
  four-week season.
- **Things to finish**: the species book (a finished page is +10% in that water), trophies, awards (+1% each),
  the crew, six waters and their renovations.
- **Feel**: coins fly to the counter, the counter rolls and jumps, haptics, a rising sound on a held UPGRADE, a card
  for a rare catch.
- **Reminders**: only about things that have happened in the harbour, at most two, none at night, asked for after the
  first away earnings are collected; never a reward for allowing them.

Deliberately not done, because players resent them or Apple forbids them: forced ads, energy, walls only money
clears, rewards that vanish unless bought, streaks that punish a missed day, offers at launch or on the first day,
fake countdowns, invented players passed off as real (the derby says its skippers belong to the game).

## Art

A look of its own, unlike Last Tide (realistic scans) and Crosswind (dark navy plates, condensed type):

- **World:** a bright low-poly toy diorama from the Kenney kits (CC0): turquoise water with foam, a sandy shore,
  chunky boats, matte colours, no glare. Day by default; night is a bright moonlit blue.
- **Interface:** cut paper and stickers. Cream panels and candy-coloured buttons with a thick navy outline and a hard
  drop shadow; rounded lettering (Lilita One, Fredoka); the harbour's name on a coral ribbon; round badges for the
  chain; the upgrade sheet rises from the bottom. Painted by `Tools/art/ui_art.py`.
- Loading screen: the sea, sun rays, the name, a progress bar with the coin, a tip and the Rushing Technologies logo.

## Architecture

As Crosswind: `Assets/Scripts/Core` has no Unity dependency (the save, the rules, tests that run under `dotnet test`);
`Assets/Scripts/Game` builds everything from code at runtime. Randomness is a seed in the save.

## Milestones

1. Core: chain, costs, skippers, away earnings, save; pacing test.
2. The harbour on screen: world, boats, haulers, trucks, HUD, upgrade sheet, loading screen.
3. Depth: renovation, renown and logbook, crew cards and chests, species book, boosts, voyages, quests and dailies.
4. Platform: store, Sign in with Apple, iCloud, reminders, Game Center, rating prompt, settings.
5. More harbours, derbies, season pass, trophy hall, conditions.
6. Art pass with the free models, sound, tutorial, App Store listing.

## Lessons

The bigger parts of the game (crew, fishing by hand, works, bait, orders, research, voyages, renovation) are each
introduced once by a card from Marta when they open (`Hud.Tutorial.cs`, `Lessons`), with a button that goes straight
there. More → Settings → How it works lists them all again, and says what opens the ones still locked. Smaller
things keep a one-line banner.

Signs over the world never sit under the fixed HUD or off the screen (`Hud.ClearOfHud`); the works are small hammer
tags so a phone is not covered in labels. The camera frames the same width of harbour on every screen
(`CameraRig.Create`: distance 325 / aspect).

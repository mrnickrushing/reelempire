# Fishing Idle Tycoon: research report and recommended design

**Method.** I read the Kongregate/Game Developer "Math of Idle Games" series (parts I–III), Kolibri Games' own blog and help centre, GameAnalytics, PocketGamer.biz/AppQuantum deconstructions, Mobile Free to Play, two "900 seconds with…" first-session teardowns, App Store listings (ratings, IAP price lists and the reviews they show) for about 20 games, and r/incremental_games threads.

**Gaps you should know about.** Fandom wikis (Idle Miner, Egg Inc) returned HTTP 402 to automated fetches, so some exact per-rank numbers below come from secondary guides and are marked "(secondary)". I found no Deconstructor of Fun or GameRefinery piece specific to idle tycoons. Where I give my own recommendation rather than a sourced fact, it is labelled **Rec**.

---

## 0. Key takeaways

1. **The genre's top complaint is ads.** Every fishing idle game I checked has review complaints about forced ads, ads that fail to pay out, or ad-gated progress: Hooked Inc, Idle Fish 2, Fishing Frenzy, Eatventure, Idle Sea Park, Seafood Inc. A **no-ads game is a real differentiator**, provided free boosts are generous.
2. **The second complaint is managers or cards locked behind purchases or weak gacha odds** (Seafood Inc, Hooked Inc, Eatventure). Kolibri rebuilt Super Managers ("2.0") because only **1 in 10 daily players owned more than one** ([Kolibri](https://kolibrigames.com/blog/introducing-super-manager-2-0/), [TrendHunter](https://www.trendhunter.com/trends/idle-miner-tycoon)).
3. **Show the whole loop on one screen.** Kolibri's lesson: four stages (Mine → Transport → Sell → Upgrade) visible at once. Prototype D1 retention was **63–81%** against a typical ~40%. Average session is about **10 minutes** ([GameAnalytics](https://www.gameanalytics.com/blog/making-a-hit-idle-game-eight-lessons-from-kolibri-games)).
4. **The economy is exponential cost against roughly linear output with milestone multipliers.** Cost(next) = base × r^owned with r ≈ 1.07–1.15. Output gets x2 at owned-count milestones ([Math of Idle Games I](https://gamedeveloper.com/design/the-math-of-idle-games-part-i)).
5. **Fishing idle games fall into five crowded groups** (§A). The open space is a **3D harbour diorama with a real catch → haul → process → sell chain, a meaningful species collection, and no ads.**
6. **Naming risk.** "IDLE FACTORY TYCOON", "IDLE BANK TYCOON", "IDLE RESTAURANT TYCOON" and others are registered US trademarks with "IDLE" disclaimed ([trademark record](https://trademarkregistration.app/Trademark/Details/87932686)). I could not confirm the owner or any enforcement. **Rec:** avoid the "Idle ___ Tycoon" title pattern and "Hooked".

---

## A. Existing fishing idle games (stay distinct from these)

| Title (developer) | Core loop | What sets it apart | App Store US rating | Complaints shown | Monetisation notes |
|---|---|---|---|---|---|
| **Hooked Inc: Fishing Games** (Lion Studios), [link](https://apps.apple.com/US/app/id1436213906) | Swipe/tap to cast from a boat; crew auto-fish; new regions and boats | Big brand (41.9M Play installs); rare and secret fish need a legendary rod; daily multiplayer tournaments; seasonal crew | 4.7 (504K) | Ads when opening chests; ads interrupt big-fish moments; crew only via premium currency; Facebook nagging; startup freezes | No Ads $5.99, Starter $4.99, Season Buoy $3.99, gems $0.99–$9.99, weekend packs |
| **Idle Fish 2 / Fish Idle: Hooked Tycoon** (Multicast Games), [link](https://apps.apple.com/US/app/id1534396279) | Hook fish, upgrade boat and fisherman, zones and arks | Fish farms, enchanting, black/green markets, winter zone with a freezing boat | 4.8 (5.7K) | XP walls; long unfreeze timers with no speed-up; ad-dependent; subscription too expensive; dull late game | **VIP $4.99/week** after a 3-day trial, $14.99/month, $99.99/year; crystals $1.99–$19.99 |
| **Fishing Frenzy: Idle Hooked Inc** (CyberJoy), [link](https://apps.apple.com/app/id6502436718) | Sail out, hire and train anglers, upgrade fish box and ship, seafood restaurant | Breeding base, aquarium, harbour, PvP pets, arena | 4.8 (11K) | Ad frequency grows as you progress; forced ads at milestones; ad removal is a weekly sub; slow areas; asks for prestige | Club VIP **$9.99/week**, "Earnings privileges" $4.99, "Bank" $4.99 |
| **Idle Fishing Story** (Metamoki), [link](https://apps.apple.com/app/id1454313096) | Boats catch → dock carries → truck sells (a 3-link chain); captains run things offline | Father/daughter story; weekly events in themed locations; new fish market | 4.6 (~2K) | Dock backlog bottlenecks felt like a nudge to pay; main game slower than events; content runs out | Gems $1.99–$19.99; event boat offers $2.99–$4.99 |
| **Idle Fishing Village Tycoon** (Buff Studio), [link](https://apps.apple.com/us/app/-/id1619717984) | Boat → docks/storage → carts/trucks → fish market → restaurants and food trucks | Cute animal "Friends" as workers; stamps unlock the next island (prestige); no forced ads | 4.9 (348) | Load failures; stiff camera movement | Several $2.99 passes, packages $0.79–$16.99 |
| **Seafood Inc** (Dovi Games), [link](https://apps.apple.com/us/app/-/id6450953468) | Boats → crane → conveyor → packaging factory | City scenes (London, NY), investor and merchant orders | 4.6 (4.6K) | Manager cards scarce and paywalled ("bad faith"); slow without ads; content drought | Piggy bank $1.99 (+$2.99 upgrade), VIP Club $9.99/$19.99, Premium Member $5.99 |
| **My Fish Shop** (Supercent), [link](https://apps.apple.com/us/app/id6752633322) | Idle-arcade: walk, carry catch, process, stock shelves, serve customers | Boss fish, shipwrecks, card system | 4.5 (2.7K) | Level cap stuck at 26; card screen freezes | No-ads $3.99/$6.99, pearls $0.99–$49.99 |
| **Mastery Fisherman** (Onigiri), [link](https://apps.apple.com/us/app/id6499314497) | Idle-arcade: catch → fillet machine → pack → tables and drive-thru | Restaurant service | 4.7 (203) | Crash after one minute | No Ads $3.99, ad-skip tickets up to $39.99 |
| **My Fishing Market Idle Empire** (Tummy Games), [link](https://apps.apple.com/us/app/id6738889482) | Fleet → processing → restaurants → cargo export | 60+ species, tuna boats, export routes | 2.8 (11) | Stuck tasks; unlock bugs; customer pathing | Diamonds $1.99–$49.99, first offer $19.99 |
| **Fishing Boat Tycoon** (Jinshi), [link](https://apps.apple.com/us/app/-/id6467106875) | Fish factory | n/a | 2.3 (7) | Negative balances; camera bugs | VIP $19.99, battle pass $9.99 |
| **Idle Sea Park** (Playstack), [link](https://apps.apple.com/app/id1605972092) | Aquarium park; feed fish; visitors pay | Otters, dolphins; fish diary | 4.7 (946) | Rewarded ads don't pay out; pay-to-win; "done in an hour" | **Offline Extension Park Manager $8.99**, earnings boosts $4.99/$8.99, Remove ads $14.99 |
| **Idle Fishing Empire / Fisher Tycoon** (AlexPlay), [link](https://apps.apple.com/us/app/id1499358800) | Clicker plus passive; Game Center | n/a | 4.5 (12) | Ads needed even to open earned chests | **Total income x2 $5.99** |
| **Fishing Clash** (Ten Square Games) | Real-time sport fishing, PvP, collection | Rarity tiers: Common (grey, max level 12), Rare (11), Epic (9), Mythical (8), Legendary (6), plus Special event lures and Mysterious fish ([FAQ](https://tensquaregames.helpshift.com/hc/en/3-fishing-clash/faq/38-fish-rarity)) | n/a | n/a | Web store took 30% of bookings in May 2025 ([TSG](https://tensquaregames.com/2025/06/13/tsg-store-hits-record-high-share-in-game-bookings-as-the-company-gets-closer-to-its-long-term-target/)) |

**The five groups:**
- **Hook clickers:** Hooked Inc, Idle Fish 2, Fishing Frenzy, Idle Fishing Story.
- **Seafood factory:** Seafood Inc, My Fishing Market, Fishing Boat Tycoon.
- **Idle-arcade carry:** My Fish Shop, Mastery Fisherman.
- **Cute village:** Buff Studio.
- **Aquarium:** Idle Sea Park.

**Rec, to stay distinct:**
- A grounded 3D harbour diorama (not cartoon animals, not a person carrying stacks).
- Product choice in processing (fresh, fillet, smoked, canned).
- A collection book driven by real time, weather and season.
- A trophy hall or aquarium as the visible prestige trophy.
- No ads at all.

---

## 1. Core loop and production chains

- **Idle Miner Tycoon (Kolibri).** Shafts mine ore → elevator lifts it → warehouse transporters carry it to market. Stage levels are independent, so the shared throughput is the minimum of the three.
  - Guides advise keeping the elevator ahead of the shafts and the warehouse about 10–15 levels relative to the elevator ([AppGamer](https://www.appgamer.com/idle-miner-tycoon/strategy-guide/idle-miner-hints-and-tips), [BlueStacks](https://www.bluestacks.com/blog/game-guides/idle-miner-tycoon/idt-tips-tricks-en.html)) (secondary).
  - Each deeper shaft yields **about 500× the cash** of the one above it ([Kolibri](https://kolibrigames.com/blog/lets-dig-in)).
- **Idle Factory Tycoon.** Warehouse imports raw input → up to **20 workstations** per factory → market. "Input Missing" signals a starved station. The rule of thumb is to keep active stations, warehouse workers and market workers roughly equal ([LevelWinner](https://www.levelwinner.com/idle-factory-tycoon-beginners-guide-tips-cheats-strategies-boost-your-earnings/)).
- **Idle Bank Tycoon.** Marketing (customers in) → service windows (rate) → vault (carts to the safe) → main hall (extra per customer). Upgrades give reputation XP, which unlocks managers (level 6) and Business Mode (level 7) ([PocketGamer.biz](https://www.pocketgamer.biz/game-analysis-deconstructing-idle-bank-tycoon-by-kolibri-games/)).
- **Idle Fishing Story.** Boats → dock → truck. Players notice and complain when the dock backs up (App Store review).
- **AdVenture Capitalist / Cookie Clicker.** Parallel generators with no chain. Derivative chains (generators that make generators) give polynomial growth ([Part II](https://gamedeveloper.com/disciplines/the-math-of-idle-games-part-ii)).

**How bottlenecks work.** A stage's throughput is level-driven: capacity × speed × workers. The weakest stage limits the whole chain. The fix is always an upgrade on the starved stage, and the UI should point at it: a red "!", a full-capacity bar, idle workers. Players like resolving these. They hate bottlenecks that feel like a paywall (Idle Fishing Story review).

---

## 2. Cost and income formulas

From [Math of Idle Games I](https://gamedeveloper.com/design/the-math-of-idle-games-part-i):

- **Next cost:** `cost = base × r^owned`
- **Production:** `prod = base_prod × owned × multipliers`
- **Bulk cost for n units:** `b × r^k × (r^n − 1)/(r − 1)`
- **Max affordable (the MAX button):** `floor( log( c(r−1)/(b·r^k) + 1 ) / log r )`
- **AdCap example:** Lemonade Stand r = **1.07**, base cost 4 (really 3.738), 1.67/s. Cost rises exponentially while output rises linearly or polynomially, so "early on production exceeds costs; eventually costs become prohibitive".
- **Eric Guan's example:** cost **×1.15 per level** against output **×1.1 per level** ([Substack](https://ericguan.substack.com/p/idle-game-design-principles)).
- **Derivative Clicker:** 5 × 1.1^n.

**Milestones:**
- AdCap: speed doubles (time halved) at **25, 50, 100, 200, 300, 400** owned, and profit multipliers come after 400 ([PocketGamer](https://www.pocketgamer.com/adventure-capitalist/adventure-capitalist-tips-and-cheats-for-making-money/), [Mistplay](https://www.mistplay.com/blog/adventure-capitalist-guide)).
- Kongregate's model: optimal-buy spikes land on the 25/50 multipliers. Their prestige sheet adds multipliers at 300/400/500 and a 16× at 500.
- Eatventure: station milestones double income and add more workers.

**Buy x1 / x10 / x100 / MAX** (or "next milestone") is standard. The article notes players "usually buy in bulk to simplify the decision".

**Why ×1.1–1.2 steps work:** Guan cites just-noticeable difference. Changes under about 20% are not felt, so rewards must step up multiplicatively.

---

## 3. Managers and automation

- **AdCap.** Buying a manager for cash automates one business. Managers come from the shop and gold; angel-bought managers give cost cuts, for example oil companies 10% cheaper after 100 angels ([Steam guide](https://steamcommunity.com/app/346900/discussions/0/618458030681519409)).
- **Idle Miner regular managers.** Hired for cash per stage, in three ranks: Junior, Senior, Executive. Rank scales the active ability, for example upgrade-cost cuts of **40% (Junior) up to 80% (Executive)** (secondary, [LevelWinner](https://www.levelwinner.com/?p=10177)). Abilities are timed with a cooldown; one secondary source says 30 s active and 5 min cooldown.
- **Idle Miner Super Managers.**
  - Rarities Rare, Epic and Legendary, with active and passive abilities. Examples: "Duration 2 min / Cooldown 20 min", "60 s / 15 min", and "elevator upgrade cost −90% for 5 min" ([Kolibri FAQ](https://kolibri-games.helpshift.com/hc/en/3-idle-miner-tycoon/faq/509-everett-bloomfield)).
  - Three gem colours do three jobs: green hires, blue upgrades actives, red unlocks passives ([FAQ](https://kolibri-games.helpshift.com/hc/cs/3-idle-miner-tycoon/faq/39-what-are-super-managers-and-how-are-they-different-from-normal-managers)).
  - Super Manager 2.0 moved to **fragments from tokens earned in normal play and events**, because ownership was too low.
  - Players time abilities to fire on shaft, elevator and warehouse together ([Kolibri](https://kolibrigames.com/blog/lets-dig-in)).
- **Idle Bank.** Manager cards from loot cases, levelled with "bulbs" and ranked up with duplicate cards. Cases come from ads (5 per day, one every 2 h), quests and premium.

**What players hate:** managers or crew only for premium currency (Hooked Inc review), scarce cards (Seafood Inc), and low gacha odds (Eatventure). Apple requires apps with loot boxes to disclose the odds (App Review Guideline 3.1.1). Buff Studio's listing shows odds per item.

---

## 4. Prestige and reset

Prestige currency formulas ([Math of Idle Games III](https://gamedeveloper.com/design/the-math-of-idle-games-part-iii)):

| Game | Formula | To double your prestige currency |
|---|---|---|
| AdVenture Capitalist (angels) | p = 150·√(lifetime/10^15); **each angel +2% profit** ([guide](https://gameplay.tips/guides/adventure-capitalist-basic-guide-angels-events-suits-badges.html)) | Earn about 3–4× the previous run |
| Cookie Clicker | p = ∛(lifetime/10^12) | About 8× |
| Egg Inc | Δp = (run/10^6)^0.14 | About 128× |
| Realm Grinder | p = (√(1+8·max/10^12) − 1)/2 | 4× |

Other prestige patterns:
- **Kongregate's sample run** prestiges at **120 minutes**.
- **Egg Inc:** prestige unlocks with the first soul-egg mission. Beginners aim to double soul eggs each prestige; the Prestige Bonus upgrade gives +10% eggs ([guide](https://www.androidcentral.com/tips-and-tricks-egg-inc)).
- **Idle Miner:** prestige is **per mine, up to 6 times**, each with a larger multiplier. It costs cash, and the other mines, Super Cash and the remaining cash carry over ([FAQ](https://kolibri-games.helpshift.com/hc/en/3-idle-miner-tycoon/faq/48-how-does-the-prestige-system-work)).
- **Idle Miner continents:** several continents, each with several mines and a prestige panel showing which mine has the best multiplier. A research tree gives permanent "all mines +X%" bonuses ([Kolibri](https://kolibrigames.com/blog/lets-dig-in)).
- **Idle Bank:** completing all milestones unlocks a move to a new bank, and **soft currency carries over** for a fast start.
- **Buff Studio:** collect stamps to sail to a new island.
- **Telfer's GDC 2019 takeaway:** Idle Miner fixed the "after a few prestiges everything looks the same" problem with **visually new maps you keep**, instead of wiping everything ([MF2P](https://mobilefreetoplay.com/gdc-2019-deconstructing-idle-miner-tycoon/)).
- **Players** praise prestige that is transparent and arrives early. "It usually takes forever to get to the first prestige" is a common complaint (r/incremental_games).

---

## 5. Offline earnings

- **Egg Inc.** Each silo stores about 1 h. The standard permit allows 2 silos at half the online rate. The **Pro Permit ($9.99)** allows 10 silos at the full rate ([wiki excerpt via search](https://egg-inc.fandom.com/wiki/Grain_Silo)). Kongregate notes Egg Inc limits players to "2 hours of offline play".
- **Cookie Clicker.** Offline only after a prestige upgrade: 5% of output for 1 h, rising to 91% for over 7 days with all upgrades ([wiki](https://cookieclicker.wiki.gg/wiki/Prestige)).
- **Eatventure.** Hourglass and Knife vault items extend offline duration and value (up to about ×10.66 and ×10) ([guide](https://www.talkandroid.com/23261-guide-to-eatventure-vault-items-2023/)).
- **Idle Bank.** Welcome-back screen: claim ×1, ×2 for an ad, or ×3 for hard currency.
- **Idle Sea Park** sells an "Offline Extension Park Manager" for $8.99. **Mine Empire** gives 6 h with premium.
- **Guan's staggered caps:** 20 min, 5 h and 2 days on different resources, so players who check in hourly and players who check in daily are both rewarded.
- **The typical ladder is 2 h base, 4–8 h through upgrades, 12–24 h for premium.**

---

## 6. Boosts, events, dailies, quests, collections

**Boosts**
- Idle Miner boosts range from **2× for 1 h to rare 2,000×**. Ad boosts stack in hours and multiply rather than add (2× with 10× gives 20×) ([Kolibri](https://kolibrigames.com/blog/lets-dig-in)).
- Over half of all boosts come from **expeditions** (up to 5 a day). Players hoard boosts slightly more than they receive ([Kolibri economy](https://kolibrigames.com/blog/economy-in-free-to-play-mobile-games-part-2/)).
- In one hour of Idle Miner, players collectively received **316 boosts and 100 chests** ([Kolibri](https://kolibrigames.com/blog/what-happens-in-one-hour-of-idle-miner-tycoon/)).
- Ad boosts elsewhere: Idle Bank 2× for 2 h per ad; Idle Factory and Idle Miner 2× for 4 h per ad.

**Events**
- Idle Miner Event Mines run about a week, with a few days' rest in between. They use their own ore and currency and award Participation, Bronze, Silver, Gold or Elite badges. A **monthly Event Season** gathers points across events. The Chrono Mine has league leaderboards with rewards delivered the next day ([AppGamer](https://www.appgamer.com/idle-miner-tycoon/strategy-guide/what-are-events-in-idle-miner-tycoon-good-for), [FAQ](https://kolibri-games.helpshift.com/hc/en/3-idle-miner-tycoon/faq/338-what-is-the-chrono-mine)).
- Revenue jumped visibly after Kolibri added events (Telfer). AppQuantum puts it as "no events, no game" ([AppQuantum 2025](https://appquantum.com/news/october-2025/once-again-on-how-to-make-tycoons-in-2025-with-links-and-examples.html)).
- Idle Bank's limited-time event unlocks on **day 2**.
- Idle Office Tycoon has an **offline leaderboard tied to map progress**, which needs no server.

**Daily systems**
- Login rewards and daily missions are standard (Buff Studio, Hooked Inc).
- Idle Bank daily deals refresh every few hours, or up to 3 times a day for 10 gems.
- Eatventure gives a gem per star earned.

**Collections**
- Idle Miner has worker collectibles. Hooked Inc has secret and rare fish. Fishing Clash has rarity tiers with upgrade levels. Idle Sea Park has a fish diary.

---

## 7. Monetisation catalogue (observed USD prices)

| Item | Examples |
|---|---|
| Gem / premium packs | Idle Miner Super Cash $1.99–$19.99; AdCap gold 20/55/115/240 for $1.99/$4.99/$9.99/$19.99; Supermarket 80/500/1200 gems for $0.99/$4.99/$9.99; up to $49.99 (Supercent, Tummy) |
| Starter / first-time pack | Idle Miner $4.99; AdCap $3.99 and $7.99; Hooked $4.99; Fishing Frenzy $2.99 |
| **Permanent income multiplier** | Idle Miner "Double Cash" **$6.99**; AlexPlay "Total income x2" $5.99; Eatventure "Permanent Boost" $5.39; Sea Park $4.99/$8.99 |
| Permanent automation / offline | Egg Inc Pro Permit **$9.99**; Supermarket "General Manager" $9.99; Sea Park offline manager $8.99 |
| Season / battle pass | Idle Miner Premium Pass **$9.99**; Eatventure event pass $10.99; Hooked "Season Buoy" $3.99; Supermarket Event VIP $4.99; Buff passes $2.99 |
| Subscription | AdCap Gold Sub $9.99; **Egg Inc ULTRA $3.99/month, Pro $7.99/month** ([Medium](https://medium.com/@kevin_67107/egg-inc-goes-ultra-c70c306e2217)); Idle Fish 2 VIP $4.99/week (criticised); Fishing Frenzy $9.99/week (criticised) |
| Piggy bank | Egg Inc $3.99/$5.99; Seafood Inc $1.99 (+$2.99 upgrade); Fishing Frenzy "Bank" $4.99 |
| Chests / gacha | Idle Miner chest offer $2.99; Eatventure chests $2.19–$13.99 |
| Event and limited offers | Idle Miner $2.99/$19.99; Eatventure $5.39/$26.99 |
| Time warps | Bought with gems in AdCap's shop |

**What players treat as fair:**
- One-time permanent multipliers and automation.
- Passes and starter packs.
- Piggy banks.
- Optional purchases on top of earnable free boosts.
- Kolibri's argument: selling time is not pay-to-win when there is no PvP ([Kolibri](https://kolibrigames.com/blog/economy-in-free-to-play-mobile-games-part-2/)).
- A Reddit poster says they would pay $10–20 once, but sets a $3 ceiling for small unlocks ([r/incremental_games](https://libreddit.neb.cx/r/incremental_games/comments/1epowcl/what_makes_an_idle_game_fun)).

**What players call predatory:**
- Weekly subscriptions, especially when used for ad removal.
- Managers available only for premium currency.
- Low gacha odds.
- Ads forced after a purchase.
- Paywalls that look like balancing errors.
- "Progress impossible without paying."

**Replacing rewarded ads in a no-ads game.** Every placement where competitors use a rewarded ad needs a free equivalent:
- A **2× boost** becomes free "Tide Boost" charges that refill in real time and stack to a cap.
- **Welcome-back ×2** becomes a free daily double, then gems for more.
- **Ad chests** become free chests on a timer.
- **Daily-deal refresh** costs gems.
- **The "suitcase of money" on the floor** becomes a free tappable bonus fish or crate in the world. Teardowns liked these as micro-rewards ([900s My Perfect Hotel](https://martinjurasek.substack.com/p/900-seconds-with-my-perfect-hotel)).
- **Make each free reward worth time.** Jurasek criticises ad rewards worth "1.3 minutes" of progress for a 30–60 s ad ([900s Idle Theme Park](https://martinjurasek.substack.com/p/900-seconds-with-idle-theme-park)). A free tap can be small, but a free boost should be worth about 1–4 hours.

---

## 8. Retention and pacing

**Benchmarks**
- AppQuantum idle-tycoon benchmark: **D1 50%, D3 35%, D7 20%, D14 15%, D30 5%**. Idle Bank reached D1 48%. Drop-offs: slow progress on days 1–3, and **no new mechanics on days 7–30** ([PocketGamer.biz](https://www.pocketgamer.biz/game-analysis-deconstructing-idle-bank-tycoon-by-kolibri-games/)).
- All-genre medians are roughly D1 22–26%, D7 4–10%, D30 1–4%, with top quartile/decile at D1 40%+ ([Playio summary](https://blog.playio.co/d1-d7-d30-retention-benchmarks-2026)).
- AppQuantum calls day 7 the point where "fast food" tycoons collapse.

**Sessions**
- Kolibri: about 10 min per session.
- Guan: the first session runs 15–60 min, then about one check-in an hour, then a few a day, then weekly. Pacing should follow that decay.

**First session (from teardowns)**
- Idle Theme Park locks input for the first minute (a criticism). Missions appear at about 3 min, then the player is "left alone". Boosters weren't found until **14 min**.
- My Perfect Hotel hands staff over "after a couple of minutes", when manual chores become the bottleneck.
- AdCap's developers warn against teaching "12 mechanics" in the first session instead of one great one.
- Reddit: a game that is hard to navigate in the first 10 minutes gets uninstalled.

**Notifications**
- Ask for permission in context, when the player starts their first valuable timer, behind a custom pre-prompt.
- Batch notifications that land close together. Stop sending a type after 2 are ignored ([Adrian Crook](https://adriancrook.com/?p=4922)).
- Send only when there is a reason. Start with 1–2 types and measure ([Meta](https://developers.facebook.com/documentation/games/retain/notifications/best-practices)).

---

## 9. Visual and UX conventions

- **One-screen loop.** Idle Miner is a vertical cross-section; Telfer argues for a grid/diorama view that changes visibly as you progress. In landscape 3D, let the chain flow **left to right** (sea → dock → hall → market) under a fixed three-quarter camera with clamped pan and pinch zoom.
- **Upgrade panel.** Tap a station to open a bottom sheet showing:
  - level and progress to the next milestone;
  - current versus next stats (green deltas);
  - buy ×1/×10/×100/MAX/NEXT MILESTONE;
  - manager slot and ability button with a cooldown ring.
- **Number format.** K, M, B, T, then **aa (10^15), ab (10^18), ac …**, as in Idle Miner and AppQuantum titles ([AppQuantum FAQ](https://appquantum.helpshift.com/hc/en/20-animal-valley/faq/688-aa-ab-ac---what-do-the-numbers-mean)). Show 3 significant digits (12.4M). Offer a scientific-notation toggle (r/idleslayer asks for it).
- **Feedback ("juice").**
  - Coins fly to the counter.
  - Floating "+$" on each delivery.
  - The counter rolls rather than jumps.
  - Bursts and sound at milestone levels.
  - Workers visibly speed up when boosted.
  - Satisfying sound and tactile feedback are cited as return drivers (AppQuantum 2025).
  - Don't overdo it: too much juice hides information ([Wayline](https://www.wayline.io/blog/juice-overload-sensory-feedback-hurts-gameplay)).
- **Game Center.** AlexPlay uses leaderboards and achievements. Use it for lifetime earnings, collection count, biggest fish and event score.

---

## 10. Fishing-specific opportunities

- **Species book.** 5 rarity tiers plus event-only fish, after Fishing Clash's grey/blue/green/purple/gold. Each species records size (weight), first-catch reward and best-size record.
- **Real-time conditions.** Day/night, weather and a 4-week season rotation change which rare fish bite. Make conditions **bonuses, never punishments**: Idle Fish 2's freezing boat drew complaints.
- **Locations as maps.** Lake → River → Coast → Reef → Deep Sea → Arctic, each visually distinct.
- **Boats and docks** as the "shafts", with more boats and better gear per berth.
- **Processing with choice.** Sell fresh, fillet, smoke or can. Daily market prices nudge which product pays best, as a light strategy layer.
- **Trophy hall / aquarium.** Display mounted record fish and live tanks; each displayed trophy gives a small permanent bonus. This is the cosmetic, visible prestige meta.

---

# 11. Recommended design (ready for the design doc)

## 11.1 Production chain, one harbour per map (Rec)

```
[Fishing berths 1..8] → [Dock haulers/crane] → [Processing hall: Clean → Fillet/Smoke/Can lines] → [Market trucks / Fish restaurant]
   (like shafts)          (like the elevator)       (value multiplier; product choice)               (like the warehouse; sells)
```

- **Berths (generators).** Each berth is a boat plus anglers fishing one water depth with its own species table.
  - Base income ×8 per berth.
  - Unlock cost ×12 of the previous berth, so each new berth takes a little longer.
  - Levels 1–400.
  - Stats: catch per trip, trip time, deck capacity.
- **Dock (transport).** Haulers or crane: capacity × speed × count. One shared stage.
- **Processing hall (value).** Lines unlock in order: Clean (×1.5 value), Fillet (×2.5), Smokehouse (×4), Cannery (×6). Each line has its own levels and throughput.
  - The player assigns which product the hall prioritises.
  - Unprocessed fish can still be sold raw, so nothing ever hard-stalls.
- **Market (sell).** Trucks or stalls with capacity × speed. A late-harbour restaurant gives a further ×2 on one product.

**Formulas**
- Level cost: `C0·r^L`, with r = **1.08** for berths and **1.10** for shared stages.
- Output: linear in level × milestone multipliers.
- Milestones at levels **10, 25, 50, 100, 150, 200, 250, 300, 350, 400**. Alternate ×2 speed with ×2 value, and add a worker at 25/100/200.
- Implement bulk and MAX with the closed forms in §2.
- Simulate the whole economy in a spreadsheet before tuning: time-to-next-unlock should grow about 1.2–1.5× per step.

**Bottleneck UI**
- The chain bar at the top shows each stage's throughput per minute.
- The weakest stage pulses.
- The guide arrow points at it.

## 11.2 Systems list

1. **Harbours (maps).** 6 harbours in the main path: Lake, River, Coast, Reef, Deep Sea, Arctic, each a new 3D scene.
   - Earlier harbours stay playable; only the harbour you're in earns.
   - Each harbour has its own cash. Gems and trophies are global.
2. **Renovation (per-harbour prestige, up to 5 times).**
   - Resets that harbour for a permanent ×2/×4/×8/×16/×32 multiplier there.
   - Costs cash, like Idle Miner.
   - The panel shows the multiplier you would get.
3. **Renown (global meta currency).**
   - `Renown = floor(40·√(lifetime_cash_all_harbours / 1e9))`, granted on renovation and counted as a delta.
   - Each point gives +2% global income (the AdCap pattern).
   - Spend it in a **Skipper's Logbook** skill tree: offline hours, boost length, manager cooldowns, cheaper berth unlocks.
4. **Managers ("Skippers" and "Foremen").**
   - Regular managers are hired with cash per stage and automate it.
   - Crew cards come in 4 rarities: Common, Rare, Epic, Legendary.
   - Actives last 1–5 min with 10–60 min cooldowns; passives unlock at levels 10/30/50.
   - **Cards come mostly from free sources:** a free chest every 4 h, quests, the pass free track and events. Duplicate fragments are guaranteed, with a pity counter, and odds are shown.
5. **Species book and records.**
   - 15–25 species per harbour.
   - First catch gives gems.
   - A completed page gives +10% to that harbour.
   - Size records go into Game Center.
6. **Trophy hall / aquarium (home scene).** Mount record fish for permanent small bonuses; purely visual flex otherwise.
7. **Conditions.** Real day/night, weather rolled every 3 h, and a 4-week season set. Each condition adds rare-fish bite chances, a small boost or a special catch, never a penalty.
8. **Boosts.**
   - Free Tide Boost charges: 2× income for 1 h. Hold 3 at once, one recharges every 4 h, stackable to 8 h.
   - Rare boosts (5×/10×) from chests and events, multiplying with everything else.
9. **Voyages (the expeditions equivalent).**
   - Send a spare boat for 30 min / 2 h / 8 h.
   - Returns cash, gems, cards and boosts.
   - Main free source of boosts. This is the long timer that triggers the notification prompt.
10. **Quests.** A visible chain of 3 at a time that teaches the next mechanic. Milestone bars per harbour give the next harbour's unlock.
11. **Daily.** 7-day login calendar (gems day 1, card chest day 3, 5× boost day 7, repeating), 3 daily tasks, 1 free daily-offer refresh.
12. **Events ("Derbies").**
    - 3–5 day mini-harbours with their own currency and 3–5 milestone tiers.
    - Rank against a Game Center leaderboard plus 49 simulated rivals for a bracket feel, so no server is needed.
    - The first event unlocks on day 2. Then one event a week, 2 days off.
13. **Season pass.** 4 weeks, 30 tiers, free and premium tracks. XP comes from upgrades, catches and quests.
14. **Offline.**
    - 2 h base.
    - Logbook research +1 h × 6, for 8 h.
    - Harbour Club subscription +4 h, for 12 h.
    - Permanent "Night Crew" purchase +4 h, for up to 16 h total.
    - Welcome-back screen shows the full breakdown, "Collect" (×1), "Double" (free once a day, then 20 gems) and Triple for gems.
15. **Game Center.** Leaderboards for lifetime cash, Renown, species count, heaviest fish per harbour and event score. Achievements for every species page, renovation and harbour.

## 11.3 Pacing targets

**First hour**
| Time | Beat |
|---|---|
| 0:00–0:20 | The camera drifts over the harbour; one boat is already fishing. A hand points at the first catch: tap, coins fly. The first upgrade can be bought within 10 s. No text walls. |
| 0:20–1:00 | Berth 1 reaches level 5. The dock hauler appears and the chain bar is introduced. |
| 1–2 min | The berth 1 level-10 milestone gives a burst and ×2. The first species page entry appears. |
| 2–4 min | Berth 2 unlocks. The **first manager is free** and the first berth automates. The dock becomes the bottleneck and the arrow points at it. |
| 4–8 min | The Clean line opens (×1.5 value) and berth 3 unlocks. The first free Tide Boost is handed over, with a one-step explanation. |
| 8–15 min | The market truck manager unlocks; quests take over teaching. The first rare fish arrives, scripted. |
| 15–30 min | The Fillet line and product choice open. Berths 4–5. The first Voyage starts (30 min) and the **notification pre-prompt** appears. Daily login is shown. |
| 30–60 min | The voyage returns with a card chest. The first crew card and active ability. Berth 6. The offline cap is explained before the player leaves. The starter pack is offered **once**, after a happy moment. |

**First week**
| Day | Beat |
|---|---|
| Day 1 | 2–3 sessions. The first renovation becomes available near the end of day 1 (about 2–3 h of total play), with a clearly shown ×2. |
| Day 2 | First Derby event. The Smokehouse opens. |
| Day 3 | Harbour 2 (River) unlocks, which is a visual refresh. The Logbook skill tree opens with first Renown. |
| Day 4–5 | Rarity ranks for crew. Weather rare fish become collectable. |
| Day 6–7 | Season pass tier ~8–10. Harbour 1 species page about 80% complete. Trophy hall unlocks to fill the day 7–30 "no new mechanics" gap. |
| Day 8–30 | Cannery, then the Restaurant. Harbours 3–4 around days 10 and 18. Arctic and Deep Sea later. Monthly season rotation. |

## 11.4 IAP catalogue, purchases only (Rec, USD tiers)

| Product | Price | Notes |
|---|---|---|
| Gems S / M / L / XL / XXL / Mega | $0.99 / $4.99 / $9.99 / $19.99 / $49.99 / $99.99 | Bonus % grows with size; first purchase of each is doubled |
| Starter Pack (one-time, 48 h) | $2.99 | Gems, 1 Epic crew card, 4 h of 5× boost |
| **Captain's Ledger** (permanent ×2 income, all harbours) | $7.99 | The fairest-regarded purchase in the genre (Idle Miner Double Cash $6.99) |
| **Night Crew** (permanent +4 h offline and free daily ×2 welcome-back) | $4.99 | |
| Fleet Expansion (permanent second voyage slot) | $4.99 | |
| Season Pass premium track (4 weeks) | $7.99 | $14.99 version adds 10 tiers |
| Derby Pass (per event) | $3.99 | Premium event track |
| **Harbour Club** subscription | $4.99/month (monthly only, no weekly) | 40 gems a day, +4 h offline, +1 free boost charge, a monthly cosmetic boat; cancel anytime; no gameplay walls |
| Piggy Bank (fills from play) | $3.99 | Opens once full |
| Event offers | $4.99 / $9.99 / $19.99 | Max 1 visible at a time, no pop-ups on launch |
| Gem sinks | n/a | Time warps (1 h/4 h/24 h of income), crew chests with odds shown and a pity counter, voyage finish, Double/Triple welcome-back, daily-offer refresh |

- **No ads, ever.** Make it a store-listing feature ("No ads. Ever.").
- **Avoid:** weekly subscriptions, premium-only managers, progression walls, and pop-up offers on launch.

## 11.5 Local notifications (no server needed)

Ask permission in context when the first voyage starts. **Max 2 per day.** Batch anything within 15 min of each other, and stop a type after 2 are ignored.

1. **Docks full:** offline cap reached. Fires at the cap time.
2. **Voyage returned:** a boat is back with cargo.
3. **Free chest ready:** every 4 h, sent only if the player hasn't been in for over 4 h.
4. **Tide Boost recharged:** all 3 charges full.
5. **Derby started**, then **Derby ends in 3 h** if milestones are unclaimed.
6. **Daily gift** at the player's usual play hour, only after a missed day.
7. **Rare conditions:** "Storm tonight: storm-only fish are biting". At most weekly.
8. **Season pass ending in 24 h**, only if premium rewards are unclaimed.

## 11.6 Telemetry to watch (Rec)

- Funnel steps at 1, 2, 5, 15, 30 and 60 min.
- Time to first renovation.
- Share of players hitting the offline cap.
- Bottleneck-stage frequency.
- Boost hoarding: Kolibri found hoarding normal.
- D1/D7/D30 against AppQuantum's 50/20/5 benchmark.

---

**Key sources**

- Idle-game math: https://gamedeveloper.com/design/the-math-of-idle-games-part-i · https://gamedeveloper.com/disciplines/the-math-of-idle-games-part-ii · https://gamedeveloper.com/design/the-math-of-idle-games-part-iii · https://ericguan.substack.com/p/idle-game-design-principles
- Kolibri and Idle Miner: https://www.gameanalytics.com/blog/making-a-hit-idle-game-eight-lessons-from-kolibri-games · https://kolibrigames.com/blog/lets-dig-in · https://kolibrigames.com/blog/economy-in-free-to-play-mobile-games-part-2/ · https://kolibri-games.helpshift.com/hc/en/3-idle-miner-tycoon/faq/48-how-does-the-prestige-system-work · https://mobilefreetoplay.com/gdc-2019-deconstructing-idle-miner-tycoon/
- Idle Bank deconstruction: https://www.pocketgamer.biz/game-analysis-deconstructing-idle-bank-tycoon-by-kolibri-games/
- First-session teardowns: https://martinjurasek.substack.com/p/900-seconds-with-idle-theme-park · https://martinjurasek.substack.com/p/900-seconds-with-my-perfect-hotel
- Notifications: https://adriancrook.com/?p=4922
- Reddit: https://libreddit.neb.cx/r/incremental_games/comments/1epowcl/what_makes_an_idle_game_fun
- Fishing Clash rarity: https://tensquaregames.helpshift.com/hc/en/3-fishing-clash/faq/38-fish-rarity
- Egg Inc ULTRA: https://medium.com/@kevin_67107/egg-inc-goes-ultra-c70c306e2217
- App Store listings: those linked in §A and §7.

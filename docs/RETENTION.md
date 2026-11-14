# Retention in idle/tycoon games: what the evidence supports

## How to read this

Hard public data on idle retention is thin. Studios publish headline D1 figures and qualitative lessons, almost never controlled experiments. I tag claims as:

- **[E]** evidenced: primary data, official docs, or a first-party statement
- **[P]** practitioner opinion: postmortems, deconstructions, trade press
- **[U]** my own knowledge of these games, not verified against a source this session

Gaps you should know about:
- I found no Naavik or Deconstructor of Fun teardown of Eatventure, My Perfect Hotel, Idle Lumber Empire or Cats & Soup, and nothing first-party from Codigames or Auxbrain.
- GDC Vault talks are paywalled; the Google Play notifications article and Adjust's idle guide refused the fetch.
- Reddit mirrors mostly failed, so player sentiment rests on search-result summaries and store-review excerpts. Treat it as anecdotal.
- No published benchmark exists for time-to-first-upgrade or time-to-first-automation. The numbers in section 1 are my targets, not measured norms.

One structural point for your game: in this genre, rewarded ads are the main "free boost" faucet (2x offline, 2x income for 4 hours, free chests). Kolibri reports 52% ad engagement and about eight ads watched per day ([GameAnalytics](https://www.gameanalytics.com/blog/making-a-hit-idle-game-eight-lessons-from-kolibri-games)) [E]. With no ads, you must replace that faucet with free timed boosts, or the game will feel stingier than its peers.

## 1. First session

**Evidence**
- Kolibri's Idle Miner Tycoon prototype hit 63–81% D1 with no 3D art, IAP, social, cloud save or long-term goals. The team concluded the core loop alone carries D1 and optimised that first ([GameAnalytics](https://www.gameanalytics.com/blog/making-a-hit-idle-game-eight-lessons-from-kolibri-games)) [E, self-reported, small early cohort].
- The same talk: show the whole loop (mine, transport, sell, upgrade) on one screen, each stage with a visible choke point. Your boats, dock, hall and market map onto this directly [P].
- Gold & Goblins hit 56% D1; its curve lets new players finish the first 5–10 levels within a day, and events unlock around level 7 ([PocketGamer.biz](http://www.pocketgamer.biz/the-ins-and-outs-of-retention-in-an-idle-game-with-merge-and-puzzle-elements/)) [E/P].
- Its later teardown credits a high density of short timers early in each level, and warns that upgrades which are quick but meaningless cause early disengagement ([PocketGamer.biz](https://www.pocketgamer.biz/deconstructing-a-100m-idle-game-what-5-years-of-gold-and-goblins-reveals-about-live-ops-and-retention/)) [P].
- General FTUE advice: teach only what the first hour needs, and let players act immediately ([Supersonic](https://supersonic.com/learn/blog/optimizing-ftue/), [Game Developer](https://gamedeveloper.com/design/best-practices-for-a-successful-ftue-first-time-user-experience-)) [P].

**Targets from playing the top games [U]**
- **First 60 seconds:** the player is tapping within about 5 seconds, with no account wall, long intro or permission prompts. First purchase lands in 10–20 seconds; 3–5 upgrades in the first minute, each visibly changing the scene.
- **First 5 minutes:** the first automation (manager) arrives at roughly 1–3 minutes in AdVenture Capitalist and Idle Miner Tycoon. Aim for a win every 10–20 seconds, a second production stage, and one number-jumping milestone (the x2 at 25 owned).
- **First 15 minutes:** the whole chain automated, a first chest, a first quest chain completed, a first tease of the next area. End on a visible goal about 80% funded, and show offline earnings before the player leaves.

**Why people quit in session one** [P/U]
- A stall before automation, where tapping stops paying.
- Tutorial hand-locking past the first couple of minutes.
- Popups or offers before the loop is understood.
- An early loop that is "flat, predictable, and unrewarding" (r/incremental_games and IncrementalDB reviews, anecdotal).
- A notification prompt at launch. Ask after the first offline-earnings collection instead.

## 2. Session hooks

No public A/B evidence isolates these; they are simply universal in the top games.

- **A next goal that is always visible.** Flow needs "knowing what to do next" and immediate feedback ([Elite Game Developers](https://elitegamedevelopers.substack.com/p/egd-news-111-retention-day-30)) [P].
- **Near-miss affordability.** Buy buttons fill as cash approaches the price; the next area shows a progress bar. Goal-gradient effect [P/U].
- **Milestone ladders per generator** (x2 at 25/50/100/200...), tuned so the best purchase keeps changing. Pecorella's spreadsheets model exactly this ([Kongregate](https://www.kongregate.com/pages/quest-for-progress-the-math-of-idle-games)) [P]. Gold & Goblins uses random x2/x3/x10/x100 upgrade multipliers for the same purpose [P].
- **Timers of several lengths.** Gold & Goblins is designed so there is "something to return to both short-term and long-term"; the teardown also warns that players who check too often and find nothing to do disengage [P]. A practical ladder [U]: 1–5 min (flying bonus), 30 min–2 h (boost expiry, free chest), 4–8 h (offline cap, expeditions), 24 h (daily), multi-day (event).
- **Unclaimed-reward badges.** Standard. Never badge something that is only a shop offer; players learn to distrust dots [U].

## 3. Return triggers

**Offline earnings**
- Pecorella: a short check-in gives a "celebratory moment", the pull grows with time away, and this acts as a "natural energy system" with no energy currency ([Game Developer](https://gamedeveloper.com/design/the-rise-of-games-you-mostly-don-t-play)) [P, from the person with the most idle data in 2015].
- Egg, Inc. states its offline cap openly, sells extensions, and never penalises absence: "players simply stop earning" ([PocketGamer.biz](https://www.pocketgamer.biz/egg-inc-idle-games-gold-standard/)) [P].
- Typical caps are 2–12 hours at base, upgradeable [U]. A cap covering a night's sleep by day 2–3 avoids the "I slept and wasted it" complaint.

**Welcome-back screen** [U]: time away, total earned as a counting number, what it can buy now ("enough for Dock level 12"), one collect button, an optional free doubler. Without ads, give the doubler as a daily charge or for a small soft-currency price.

**Notifications**
- Apple 4.5.4: push must not be required for the app to work, and promotions need explicit opt-in via in-app consent language plus an in-app opt-out. 5.1.2(i): you may not require push to access functionality or rewards ([Apple](https://developer.apple.com/app-store/review/guidelines/)) [E]. So never offer gems for enabling notifications, and keep sale notifications behind a separate toggle.
- Median opt-in is 44% on iOS versus 90% on Android; a starting cap is as many notifications per day as the player's daily opens ([Mistplay](https://business.mistplay.com/resources/mobile-game-retention-benchmarks), citing vendor data) [E, weak].
- Google Play data shows more daily notifications correlate with higher opt-out [E; from a search summary, the article itself would not load].
- Opted-in users retain better (55% vs 29% at 30 days, Urban Airship), but that is selection, not causation; Pushwoosh found no linear link between opt-in rate and D30 [E, mixed].
- I found no send-time or copy benchmark for games. Best practice [P/U]: state-based local notifications only ("Your hold is full", "The Trawler is back", "Festival ends in 3 hours"), at most 1–2 a day, none overnight, and stop after about 3 ignored in a row.

**Daily login and streaks**
- No controlled game-specific data found. The best evidence is Duolingo: making the streak easier to keep gave +3.3% D14 retention, +1% DAU, and +10.5% of daily learners on a streak ([Duolingo](https://blog.duolingo.com/improving-the-streak)) [E, different product category].
- Implication: forgiving streaks beat punishing ones. Use a 7-day calendar that advances on days played and never resets, or resets with a free freeze. A 30-day cumulative calendar suits long-term play [P/U].
- Gold & Goblins deliberately has no daily quests or checklist rewards and still earned over $100M ([PocketGamer.biz](http://www.pocketgamer.biz/the-ins-and-outs-of-retention-in-an-idle-game-with-merge-and-puzzle-elements/)), so dailies are not mandatory [E].

**Comeback rewards** [P/U]: after 3+ days away, grant a larger offline payout, a free timed boost and a "what's new" card. Hyper Hippo's remarketing claims +50% D7, but that is a vendor case study about paid re-engagement ([AppsFlyer](https://www.appsflyer.com/customers/hyper-hippo/)) [weak].

## 4. Mid-term (D3–D14)

- **Timers lengthen and players hit a wall.** The Gold & Goblins teardown calls this the fundamental idle problem. Their fix is a second progression layer: an event with fast progress for 10–40 minutes while core timers run (the "pendulum effect") ([PocketGamer.biz](https://www.pocketgamer.biz/deconstructing-a-100m-idle-game-what-5-years-of-gold-and-goblins-reveals-about-live-ops-and-retention/)) [P, well argued].
- **Wave-shaped difficulty**: rise, release, rise again (same source) [P].
- **Visual novelty at each reset.** Idle Miner Tycoon opens new mines and continents instead of forcing a full reset, so prestige does not look identical each time ([Mobile Free to Play](https://mobilefreetoplay.com/gdc-2019-deconstructing-idle-miner-tycoon/)) [P].
- **Staggered unlocks.** Eatventure's Invite Helper unlocks around day 4–5, and its collection layer (hat/body/hand gear with global bonuses) keeps paying while away ([PocketGamer.biz](http://www.pocketgamer.biz/eatventure-vs-cat-snack-bar-an-analysis-of-both-games/)) [P]. AppQuantum's tycoon guide: layer depth gradually; games with no reason to return churn "by Day 7" ([AppQuantum](https://appquantum.com/news/october-2025/once-again-on-how-to-make-tycoons-in-2025-with-links-and-examples.html)) [P].

A workable week-one schedule, synthesised from these games rather than published anywhere [U]:

| Day | New system |
|---|---|
| D1 | Core loop, managers, quests, first chest, offline earnings |
| D2 | Second area or harbour, boosts, daily reward |
| D3 | Collection or gear layer, first prestige offered |
| D4–5 | First limited event, helper or expedition timers |
| D6–7 | Season pass, research tree, leaderboard |

That is roughly 6–8 systems in week one, one new thing per day and never two at once. First prestige or new location should arrive when progress first slows, typically late D1 to D3 [U]. Players on r/incremental_games say that when they hit a stall, "they quit and never return" (anecdotal).

## 5. Long-term (D14–D90)

- **Events are the main lever.** Kolibri shipped over 160 updates since July 2016, about one a week, and events "often doubled revenue for a weekend" [E, self-reported]. Idle Miner Tycoon's event mines run about 5 days ([Kolibri help center](https://kolibri-games.helpshift.com/hc/en/3-idle-miner-tycoon/faq/45-how-does-the-event-mine-work/)) [E]. Eatventure rotates weekly events [P]. Gold & Goblins had no events at launch and credits adding them with starting real revenue [E/P].
- **Events reuse the core loop** with different balance and are more forgiving; mostly-completable-for-free events raise completion and motivation [P].
- **Season pass.** One-month seasons were standard by 2019; Idle Miner Tycoon's pass progresses through events and cost $9.99. GameRefinery judged it a "tacked-on" layer with weak rewards, and credited Clash of Clans' pass with a "huge impact" because of daily and weekly challenges ([GameRefinery](https://www.gamerefinery.com/battle-pass-trend-mobile-games/)) [P]. A pass needs its own tasks and rewards people want.
- **Collections.** Completionism is one of four D30 drivers in the Elite Game Developers piece (with social, live service and flow) [P]. For you: a fish compendium with set bonuses.
- **Social.** Egg, Inc.'s co-op contracts pool eggs toward a timed goal; Auxbrain reportedly keeps only the latest contract score to reduce burnout ([fan wiki](https://egg-inc.fandom.com/wiki/Contracts)) [weak]. Yet the PocketGamer piece notes Egg, Inc. "needs no social features" at its core, and Eatventure fakes its helpers. Leaderboards inside events are the cheap, proven version; real co-op is optional.
- **Meta-progression**: permanent research, station levels and prestige currency that make each loop faster [U].

## 6. Juice and feel

There is no public controlled evidence. I searched for A/B tests isolating coin fly-ups, haptics or number pops and found none. Claims that juice measurably lifts retention are assertions [P]. The nearest data: TapNation reports small UI and progression changes sometimes moved KPIs "drastically" while bigger gameplay changes did not ([GameAnalytics](https://www.gameanalytics.com/blog/how-to-improve-your-game-kpis-and-boost-revenue-lessons-from-bubble-sort)) [weak].

What the top games all do [U]:
- Coins fly to the counter, which ticks up.
- Number pops on every production cycle.
- Light haptic on purchase, heavier on milestones.
- Pitch-rising sounds on rapid buys.
- A full-screen burst on area unlock.
- Readable big numbers (K, M, B, T, then aa, ab).
- A hold-to-buy button that accelerates.
- Visible speed-up when a boost is active.

Treat it as table stakes for D1, not a long-term lever. Avoid screen shake on routine actions.

## 7. Goal systems

- **Sequential quest chain** that never empties: "Upgrade Dock to 25", "Own 3 managers", "Earn 1M". Generate it from templates scaled to the player's state, one active with the next previewed. Eatventure and the Idle Tycoon series use this as the spine [U].
- **Milestone ladders** on every generator (section 2) create dozens of micro-goals for free [P].
- **Achievements with tiers** feeding a permanent bonus; Cookie Clicker's achievements raise "milk", which multiplies production [U].
- **Event milestones and pass tiers** as the medium layer; **collection completion** as the long one.

Rule of thumb [U]: a goal completable in under 2 minutes, one in under 20, one this session, one this week, always visible.

## 8. Rewards cadence

- **Cookie Clicker golden cookie**: spawns between 300 and 900 seconds apart (average about 447 s), stays 13 s. Frenzy is x7 production for 77 s; Lucky gives the lesser of 15% of bank or 15 minutes of production. Upgrades halve the interval twice, to about 121 s ([wiki](https://cookieclicker.wiki.gg/wiki/Golden_Cookie)) [E]. The lesson: start rare, and let the player invest in frequency.
- **Egg, Inc.**: random bonus boxes of cash and premium currency without ads; drones fly over roughly every minute or so and artifacts raise their frequency [P/U, no exact rate found]. The piggy bank gains one golden egg per purchase and costs about $3 to break, per PocketGamer; widely seen as fair value.
- **Eatventure**: helper every 30 minutes for a 5-minute shift; only two chest sizes, and upgrades work per category so no duplicate hunt [P].
- **Idle Miner Tycoon**: expeditions of about 6 h, 12 h and 2 days (fan wiki) [weak].
- **Gacha**: Apple 3.1.1 requires odds disclosure before purchase for any paid randomised item [E]. A pity counter is best practice.

Suggested cadence for a no-ads game [U]: a flying bonus (gull or crate) every 2–4 minutes, a free chest every 4 hours, a daily free premium trickle, a surprise jackpot about once per session.

## 9. What players like and what they resent

**Praised**
- Optional rewards and visible generosity; Egg, Inc. reviews repeat "no forced ads" ([App Store](https://apps.apple.com/us/app/id993492744), [PocketGamer.biz](https://www.pocketgamer.biz/egg-inc-idle-games-gold-standard/)).
- No penalty for being away.
- Minimal UI and low monetisation pressure, credited for Gold & Goblins' longevity [P].
- Honest caps, small simple shops, value IAPs like the piggy bank.

**Resented, or flagged by researchers and regulators**
- Forced interstitials. You are already clear of this.
- Pay-to-skip walls, grinding and playing by appointment: the three dark patterns found in a 2025 heuristic study of casual mobile games ([Deceptive Design](https://www.deceptive.design/articles/finding-dark-patterns-in-casual-mobile-games-using-heuristic-evaluation)) [E, small study].
- Long timers that only IAP shortens, then lengthen again (section 4).
- Lost or expiring progress. Gold & Goblins' Treasure Hunt discards locked premium rewards if you leave without buying the pass; that is the kind of loss-framed FOMO to avoid.
- Confusing multi-tier currencies and misleading odds: the FTC's $20M Genshin Impact settlement (January 2025) required disclosed odds and exchange rates ([FTC](https://ftc.gov/business-guidance/blog/2025/01/level-tips-businesses-ftcs-settlement-genshin-impact-developer-hoyoverse)) [E].
- EU CPC principles (March 2025, non-binding) on purchased currencies: show real-money prices, don't obscure costs, don't force buying more currency than needed, protect children ([Linklaters](https://techinsights.linklaters.com/post/102k6t4/game-changer-eu-introduces-consumer-protection-guidance-for-in-game-virtual-curr)) [E].
- An ICPEN sweep of 473 games reportedly found 78% used at least one dark pattern [secondary source, unverified].
- Fake social. Eatventure's helpers look like players but aren't; this is a deception risk I would not copy.
- Apple specifics [E]: purchased currency may not expire (3.1.1); no forced rating (3.2.2); use Apple's rating API only, no custom prompt that gates stars (5.6.1); push rules as in section 3.

## 10. Benchmarks

| Source | D1 | D7 | D30 | Note |
|---|---|---|---|---|
| GameAnalytics 2026, all mobile, median | ~22% | ~4% | ~0.7% | 16k games [E] |
| Same, top 25% | ~30% | 6–7% | ~1.7% | |
| Same, top 10% | ~40% | 11–12% | ~4% | |
| AppsFlyer Q3 2022, simulation | 30.1% | 8.7% | 3.0% | via Mistplay [E] |
| GameAnalytics idle post, top 25% / top 10% | 39.4% / 45.6% | – | – | [E] |
| East Side Games idle targets | ~40% | ~20% | – | internal target; from a search summary, PDF too large to fetch [P] |
| Gold & Goblins | 56% | – | – | [E] |
| Idle Miner Tycoon / Idle Factory Tycoon | 63–81% prototype, ~75% claimed | – | – | self-reported [E, treat as outlier] |

Sources: [GameAnalytics 2026 summary](https://gamedevreports.substack.com/p/gameanalytics-mobile-and-pc-game), [GameAnalytics idle post](https://www.gameanalytics.com/blog/how-to-keep-players-engaged-and-coming-back-to-your-idle-game), [Mistplay](https://business.mistplay.com/resources/mobile-game-retention-benchmarks).

- **Engagement**: idle stickiness (DAU/MAU) is 18% versus 10.5% for hyper-casual, with 5.3 sessions a day and about 8-minute sessions [E]. Kolibri reports about 10-minute sessions. All-mobile top 10% is about 8 minutes, 9.6 sessions and 40 minutes a day.
- **Realistic targets for a new idle tycoon** (my judgement): D1 40% is good and 50%+ excellent; D7 12–15% good, 20% excellent; D30 4–6% good, 8–10% excellent; 5–6 sessions a day at 6–8 minutes. Organic and iOS cohorts run higher than paid ones.
- Disregard the "35–50 / 20–30 / 10–15" idle table circulating on vendor blogs; I could not trace it to data.

## Prioritised checklist

1. Whole production chain on one screen with a visible bottleneck per stage — evidence: strong
2. First purchase under 20 s, first manager in 1–3 min, no stall before automation — medium
3. Offline earnings with a counting welcome-back screen showing what it now affords — strong
4. Offline cap stated openly, covering a night by D2–3, no penalty for absence — medium
5. An always-visible next goal with a progress bar; quest chain that never empties — medium
6. Limited-time events reusing the core loop, weekly, 3–5 days each, completable free — strong
7. Event as a second fast layer for when core timers get long — medium
8. First prestige or new harbour at the first slowdown (late D1–D3), with new visuals — medium
9. One new system a day in week one, never two at once — medium
10. Free timed boosts replacing the rewarded-ad faucet (2x for 2–4 h, daily doubler) — medium
11. Milestone multipliers per generator, tuned so the best buy keeps changing — medium
12. Collection layer with permanent global bonuses and no duplicate hunt — medium
13. Flying bonus every 2–4 min, with frequency the player can upgrade — medium
14. Timer ladder: minutes, hours, daily, multi-day, each with something to collect — medium
15. State-based local notifications, at most 1–2 a day, asked for after the first offline payout — medium
16. Forgiving daily reward: 7-day cycle that doesn't reset, or a free streak freeze — medium
17. Wave-shaped difficulty, with relief after each wall — medium
18. Monthly season pass with its own tasks and wanted rewards — medium
19. Free premium-currency trickle from quests, achievements and events — medium
20. Tiered achievements feeding a permanent bonus — weak
21. Comeback package after 3+ days away — weak
22. Juice: coin fly-ups, number pops, haptics, pitch-rising sounds, unlock bursts — weak for retention, required for polish
23. Readable big numbers and hold-to-buy / buy-max — weak
24. Leaderboards inside events — weak
25. Low-pressure UI: no offers before D2, a small shop — medium
26. Real co-op goals, only if server budget allows — weak for this genre
27. Retention telemetry: funnel for the first 15 minutes, D1/D7/D30, time-to-first-manager — strong (you cannot tune without it)

## Avoid

- Notification prompts at launch, rewards for enabling push, promotional push without a separate opt-in (Apple 4.5.4, 5.1.2).
- Paid random chests without odds shown first (3.1.1); purchased currency that expires.
- Custom "rate us" gates or rewards for rating (3.2.2, 5.6.1).
- Walls that only money clears; timers that re-lengthen after each purchase.
- Punishing streak resets, and earned-but-locked rewards that vanish unless you pay.
- Fake players presented as real; fake countdowns; "only 1 left" on infinite digital goods.
- Layered currencies that hide real prices; packs sized so you must overbuy.
- Popups on launch before the offline collect; red badges on shop offers.
- Energy systems. Offline accumulation already paces sessions.

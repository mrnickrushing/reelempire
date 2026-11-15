using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    public enum BonusKind { Cash, Gems }
    public struct BonusPrize { public BonusKind Kind; public double Cash; public int Gems; }

    /// <summary>
    /// What keeps a sitting going and brings the next one: a guide that never runs out of steps, a crate drifting in
    /// the harbour every couple of minutes, the free DOUBLE on the welcome-back card, and a welcome for someone who
    /// has been away for days.
    /// </summary>
    public sealed partial class Sim
    {
        // ---- goals without end ----

        /// <summary>The step after the written ones: made from where the harbour stands, so there is always a next thing.</summary>
        GoalDef Generated()
        {
            var h = H;
            if (State.goalTarget <= 0) PrepareGenerated();
            int kind = State.goalKind; double target = State.goalTarget; int berth = Math.Min(State.goalBerth, h.berths.Count - 1);
            switch (kind)
            {
                case 0: return new GoalDef { Id = "g.berth", Text = "Take " + Content.Berths[berth].Name + " to level " + (int)target, Have = s => s.H.berths[Math.Min(berth, s.H.berths.Count - 1)].level, Need = target, Gems = 3, CashSeconds = 60, Point = Stage.Berth, PointBerth = berth };
                case 1: return new GoalDef { Id = "g.dock", Text = "Upgrade the porters to level " + (int)target, Have = s => s.H.dock, Need = target, Gems = 3, CashSeconds = 60, Point = Stage.Dock };
                case 2: return new GoalDef { Id = "g.market", Text = "Upgrade the market to level " + (int)target, Have = s => s.H.market, Need = target, Gems = 3, CashSeconds = 60, Point = Stage.Market };
                case 3: return new GoalDef { Id = "g.trips", Text = "Bring " + Format.Money(target - State.goalFrom) + " more trips home", Have = s => s.State.trips - s.State.goalFrom, Need = target - State.goalFrom, Gems = 4, CashSeconds = 90 };
                case 4: return new GoalDef { Id = "g.species", Text = "Catch " + (int)target + " different species", Have = s => s.SpeciesCaught, Need = target, Gems = 8, CashSeconds = 60 };
                default: return new GoalDef { Id = "g.open", Text = "Open the next berth", Have = s => s.H.berths.Count, Need = target, Gems = 5, CashSeconds = 120, Point = Stage.Berth, PointBerth = (int)target - 1 };
            }
        }

        /// <summary>Chooses the next made-up step: the next berth if there is one to open, else round the stages in turn.</summary>
        void PrepareGenerated()
        {
            var h = H;
            int n = State.goalIndex - Goals.Count;
            if (h.berths.Count < Content.Berths.Length && n % 3 == 0) { State.goalKind = 5; State.goalTarget = h.berths.Count + 1; return; }
            int kind = n % 5;
            if (kind == 4 && SpeciesCaught >= Book.Species.Count) kind = 3;
            State.goalKind = kind;
            switch (kind)
            {
                case 0:
                    int low = 0; for (int i = 1; i < h.berths.Count; i++) if (h.berths[i].level < h.berths[low].level) low = i;
                    if (h.berths[low].level >= Content.MaxLevel) { State.goalKind = 3; goto case 3; }
                    State.goalBerth = low; State.goalTarget = Math.Min(Content.MaxLevel, Math.Max(Content.NextMilestone(h.berths[low].level), h.berths[low].level + 5)); break;
                case 1: State.goalTarget = h.dock + 10 - h.dock % 10; break;
                case 2: State.goalTarget = h.market + 10 - h.market % 10; break;
                case 3: State.goalFrom = State.trips; State.goalTarget = State.trips + 200 * Math.Max(1, h.berths.Count); break;
                case 4: State.goalTarget = SpeciesCaught + 2; break;
            }
        }

        // ---- the drifting crate ----

        public const double BonusFirst = 75, BonusEveryMin = 110, BonusEveryMax = 230, BonusStays = 45;

        /// <summary>A crate is afloat in the harbour, waiting for a tap (it drifts away after a while).</summary>
        public bool BonusAfloat { get { return State.bonusLeft > 0; } }
        public float BonusLeft { get { return (float)State.bonusLeft; } }

        void TickBonus(double dt)
        {
            if (State.bonusLeft > 0) { State.bonusLeft -= dt; if (State.bonusLeft <= 0) State.bonusIn = NextBonusIn(); return; }
            if (State.bonusIn <= 0) State.bonusIn = BonusFirst;
            State.bonusIn -= dt;
            if (State.bonusIn <= 0) { State.bonusLeft = BonusStays; State.bonusSeed = (int)(Roll() * 1000000); BonusAppeared?.Invoke(); }
        }

        public event Action BonusAppeared;
        /// <summary>Seconds of play until the next crate (half as long with the Lookout's page of the logbook).</summary>
        double NextBonusIn() { return (BonusEveryMin + Roll() * (BonusEveryMax - BonusEveryMin)) * (Logbook("lookout") ? 0.5 : 1); }

        /// <summary>Takes the crate: mostly a few minutes of the harbour's earnings, sometimes gems.</summary>
        public BonusPrize? ClaimBonus()
        {
            if (!BonusAfloat) return null;
            State.bonusLeft = 0; State.bonusIn = NextBonusIn(); State.bonusTaken++;
            double r = Roll();
            var p = new BonusPrize();
            if (r < 0.75)
            {
                p.Kind = BonusKind.Cash;
                p.Cash = Math.Max(RewardRate(H) / (Boosted ? BoostMult : 1) * (180 + Roll() * 300), 20 * Def.Scale);
                Pay(H, p.Cash);
            }
            else { p.Kind = BonusKind.Gems; p.Gems = 2 + (int)(Roll() * 4); State.gems += p.Gems; }
            Touch();
            return p;
        }

        // ---- the welcome back ----

        public const int MissedGems = 30;
        public const double MissedAfter = 3 * 86400;

        /// <summary>The free DOUBLE of the day is still there (always, with the Night Crew, which is paid for). After it, a
        /// double is only ever for a watched advert (CollectAwayByAd).</summary>
        public bool DoubleFree { get { return Owns("reelempire.nightcrew") || State.doubleDay != Today; } }

        /// <summary>Collects the time away twice over with the free double of the day. False if it has been used.</summary>
        public bool CollectAwayDoubled(double seconds)
        {
            if (!DoubleFree) return false;
            State.doubleDay = Today;
            CollectAway(seconds, 2);
            return true;
        }

        /// <summary>Someone back after three days or more gets a welcome: gems.</summary>
        public int WelcomeHome(double seconds)
        {
            if (seconds < MissedAfter) return 0;
            State.gems += MissedGems;
            Touch();
            return MissedGems;
        }

        // ---- one new thing at a time ----

        /// <summary>
        /// The game opens up as the harbour grows, so a new player meets one thing at a time: the book with the first
        /// catch, TODAY (the gift, tasks) once a skipper is aboard, the crew at the third berth, voyages at the fourth,
        /// the derby at the fifth, the season at the sixth, renovation and other waters when all eight are open.
        /// Anything already used stays open (an older save, or a harbour that was renovated).
        /// </summary>
        public bool Unlocked(string feature)
        {
            int berths = MostBerths; bool veteran = State.renovations > 0 || State.harbours.Count > 1;
            bool coached = State.goalIndex >= 12;       // Marta's twelve steps come first: nothing new is thrown in while she is teaching
            switch (feature)
            {
                case "book": return State.trips > 0;
                case "awards": return State.goalIndex >= 3 || veteran;
                case "today": return State.goalIndex >= 5 || veteran;
                case "boost": return State.goalIndex >= 7 || State.boostUsedAt.Count > 0 || veteran;
                case "crew": return berths >= 3 || State.crew.Count > 0 || veteran;
                case "voyages": return berths >= 4 || State.voyagesDone > 0 || State.voyages.Count > 0 || veteran;
                case "derby": return berths >= 5 || veteran;
                case "season": return berths >= 6 || veteran;
                case "harbours": return berths >= 7 || veteran;
                case "bait": return BestBerthLevel >= 10 && coached || veteran;
                case "works": return berths >= 3 && coached || veteran;
                case "angling": return berths >= 2 && coached || veteran;
                case "wear": return berths >= 3 && coached || veteran;
                case "research": return State.notes > 0 || State.research.Count > 0 || veteran;
                case "orders": return berths >= 4 || State.ordersDone > 0 || State.orders.Count > 0 || veteran;
            }
            return true;
        }
        public static readonly string[] Features = { "book", "awards", "today", "boost", "crew", "voyages", "derby", "season", "harbours", "bait", "orders", "works", "research", "angling", "wear" };
        public static string FeatureNews(string feature)
        {
            switch (feature)
            {
                case "book": return "NEW: the Species Book. Every fish you land goes in.";
                case "awards": return "NEW: Awards. Each one pays gems and +1% income for good.";
                case "today": return "NEW: Today. A gift every day, and three tasks.";
                case "boost": return "NEW: the Tide Boost. Double income for an hour, for a short advert.";
                case "crew": return "NEW: the Crew. Open your free chest, then use their skills!";
                case "voyages": return "NEW: Voyages. Send a spare boat out from TODAY.";
                case "derby": return "NEW: the Weekend Derby. See TODAY.";
                case "season": return "NEW: the Season. Thirty tiers of rewards, in TODAY.";
                case "harbours": return "NEW: Renovation and new waters. Tap the harbour's name.";
                case "angling": return "NEW: Fish by hand! Tap FISH for bigger, rarer catches.";
                case "works": return "NEW: Works. Build on the plots round the harbour.";
                case "research": return "NEW: Research. Spend your notes at the chart room.";
                case "wear": return "NEW: Wear. Boats wear as they sail: mend them with a wrench.";
                case "bait": return "NEW: Bait. A level 10 dock can choose what it fishes with.";
                case "orders": return "NEW: Orders. The town wants fish: see the board at the market.";
            }
            return null;
        }

        /// <summary>The Deckhand's Pack is offered once, and not on the first day: only to someone who came back.</summary>
        public bool StarterOfferDue { get { return !State.starterOffered && !Owns("reelempire.starter") && State.firstDay >= 0 && Today > State.firstDay && State.played >= 1200; } }

        /// <summary>What the cash would buy right now: the nudge on the welcome-back card and under the counter.</summary>
        public string Affords(double cash)
        {
            var h = H;
            if (h.berths.Count < Content.Berths.Length && cash >= BerthUnlockCost(h)) return "Enough to open " + Content.Berths[h.berths.Count].Name + "!";
            var neck = Bottleneck(h);
            double keep = h.cash; h.cash = cash;
            int n; string what;
            if (neck == Stage.Berth) { int best = 0; for (int i = 1; i < h.berths.Count; i++) if (h.berths[i].level < h.berths[best].level) best = i; n = Affordable(h, Stage.Berth, best); what = Content.Berths[best].Name; }
            else { n = Affordable(h, neck); what = neck == Stage.Dock ? "the porters" : "the market"; }
            h.cash = keep;
            return n > 0 ? "Enough to upgrade " + what + (n > 1 ? " " + n + " times." : ".") : null;
        }
    }
}

using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>A pinned species: where it lives, when it bites, what to fish with and the next useful thing to do.</summary>
    public sealed class TargetPlan
    {
        public SpeciesDef Species;
        public string Where, When, Bait, Next;
        public bool Open, Here, BitingNow, Caught;
        public int BaitIndex = -1;      // the bait to put on (index into Sim.Baits), -1: none needed
        public int Berth = -1;          // the dock to put it on, -1: no dock can take it yet
        public bool CanApply { get { return BaitIndex >= 0 && Berth >= 0; } }
    }

    /// <summary>
    /// Making the harbour's numbers readable: what an upgrade does to the whole harbour (not only its own stage), what
    /// limits quantity and what limits value, what a crew skill would do right now and what holds it back, a plan for
    /// a pinned species, what each purchase would be worth at the player's own pace, and when the starter pack is shown.
    /// </summary>
    public sealed partial class Sim
    {
        public static string StageName(Stage s) { return s == Stage.Berth ? "the boats" : s == Stage.Dock ? "the porters" : s == Stage.Hall ? "the hall" : "the market"; }

        static void Bump(HarbourState h, Stage stage, int berth, int n)
        {
            if (stage == Stage.Berth) h.berths[berth].level += n; else if (stage == Stage.Dock) h.dock += n; else if (stage == Stage.Hall) h.hall += n; else h.market += n;
        }

        /// <summary>The harbour's steady pace (cash a second, every stage working) after <paramref name="n"/> more levels.</summary>
        public double PaceAfter(Stage stage, int berth, int n)
        {
            var h = H; n = Math.Max(0, Math.Min(n, Content.MaxLevel - LevelOf(h, stage, berth)));
            Bump(h, stage, berth, n);
            try { return RewardRate(h); } finally { Bump(h, stage, berth, -n); }
        }

        /// <summary>What buying <paramref name="n"/> levels does to the whole harbour, in a line: the gain, or what will
        /// hold it back. <paramref name="gain"/> is the share the steady pace rises by.</summary>
        public string UpgradeVerdict(Stage stage, int berth, int n, out double gain)
        {
            var h = H; double now = RewardRate(h), after = PaceAfter(stage, berth, n);
            gain = now > 0 ? after / now - 1 : 0;
            if (n <= 0) { gain = 0; return null; }
            if (gain >= 0.005) return "+" + Pct(gain) + " for the whole harbour";
            // It does more at its own stage, but something else sets the pace.
            Bump(h, stage, berth, n);
            Stage limit; try { limit = Bottleneck(h); } finally { Bump(h, stage, berth, -n); }
            if (stage == Stage.Hall) return h.goods == 0 ? "Fresh fish skip the hall: run a line to make this count" : "The hall is keeping up already: more won't add value yet";
            return "Hardly changes earnings: " + StageName(limit) + " " + (limit == Stage.Berth ? "land" : limit == Stage.Dock ? "carry" : "sell") + " only " + Format.Money(RateOf(h, limit)) + "/s";
        }

        static string Pct(double g) { return g >= 10 ? Format.Money(g * 100) + "%" : Math.Round(g * 100, g < 0.1 ? 1 : 0).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%"; }

        double RateOf(HarbourState h, Stage s) { return s == Stage.Berth ? FleetRate(h, true) : s == Stage.Dock ? DockRate(h) : s == Stage.Hall ? HallRate(h) : MarketRate(h); }

        /// <summary>The difference between what limits quantity and what limits value, for the stage's sheet.</summary>
        public string LimitNote(Stage stage)
        {
            var h = H;
            if (stage == Stage.Hall)
            {
                double comes = Flow(h, true), worked = h.goods == 0 ? 0 : Math.Min(comes, HallRate(h));
                if (h.goods == 0) return "The hall sets the VALUE of each fish, not how many sell. Run a line to raise it.";
                return "Sets VALUE: " + Format.Money(worked) + " of " + Format.Money(comes) + "/s become " + Content.Goods[h.goods].Name.ToLowerInvariant() + "; " + (comes > worked ? "the rest sell fresh." : "all of them.");
            }
            var neck = Bottleneck(h);
            return "Boats, porters and market set how MANY sell. The slowest sets the pace: " + (neck == stage ? "this one." : StageName(neck) + ".");
        }

        /// <summary>What every line in the hall would earn the harbour a second right now, at its present levels.</summary>
        public double PaceWithGoods(int goods)
        {
            var h = H; if (goods < 0 || goods >= Content.Goods.Length) return 0;
            int keep = h.goods; h.goods = goods;
            try { return RewardRate(h); } finally { h.goods = keep; }
        }

        /// <summary>The line that earns most here right now, of those owned.</summary>
        public int BestGoods { get { int best = 0; double top = -1; for (int g = 0; g < Content.Goods.Length; g++) if (OwnsGoods(H, g)) { double p = PaceWithGoods(g); if (p > top + 1e-9) { top = p; best = g; } } return best; } }

        // ---- crew skills, previewed ----

        Stage _probe = (Stage)(-1); double _probeMult = 1;

        /// <summary>What calling on a stage's skill would add to the harbour's pace right now (as a share), and the stage
        /// that would then hold it back. Specials (rare fish, value, price) come on top and are not counted here.</summary>
        public double SkillGain(Stage stage, out Stage limit)
        {
            var h = H; limit = Bottleneck(h);
            var d = OnDuty(stage); if (d == null) return 0;
            bool was = _steady; _steady = true;
            try
            {
                double before = FullRate(h);
                _probe = stage; _probeMult = SkillOf(d).Mult;
                double after = FullRate(h);
                limit = Bottleneck(h);
                return before > 0 ? after / before - 1 : 0;
            }
            finally { _probe = (Stage)(-1); _steady = was; }
        }

        /// <summary>How much faster the harbour is earning this moment than at its steady pace (skills running together).</summary>
        public double SkillLift { get { var h = H; double steady = RewardRate(h); return steady > 0 ? FullRate(h) / steady - 1 : 0; } }

        public int SkillsRunning { get { int n = 0; for (int st = 0; st < 4; st++) if (SkillLeft((Stage)st) > 0) n++; return n; } }

        // ---- a pinned species ----

        public SpeciesDef Pinned { get { return string.IsNullOrEmpty(State.pinned) ? null : Book.Get(State.pinned); } }

        public bool Pin(string species)
        {
            if (string.IsNullOrEmpty(species)) { State.pinned = ""; Touch(); return true; }
            if (Book.Get(species) == null) return false;
            State.pinned = species; var e = Entry(species); State.pinnedCount = e != null ? e.count : 0; Touch();
            return true;
        }

        /// <summary>The pinned species has been landed since it was pinned.</summary>
        public bool PinnedLanded { get { var sp = Pinned; if (sp == null) return false; var e = Entry(sp.Id); return e != null && e.count > State.pinnedCount; } }

        /// <summary>The plan for catching a species: harbour, conditions, bait, and the next useful action.</summary>
        public TargetPlan Plan(SpeciesDef sp)
        {
            var p = new TargetPlan { Species = sp, Caught = Entry(sp.Id) != null };
            var def = Content.Harbour(sp.Harbour);
            int hi = State.harbours.FindIndex(x => x.id == sp.Harbour);
            p.Where = def.Name; p.Open = hi >= 0; p.Here = hi == State.at;
            p.When = BiteText(sp.When); p.BitingNow = Biting(sp);
            // The bait: Glow Bait if the hour or the weather keeps it away, Spinners or Chum for the rarer fish, else worms.
            int want = sp.When != Bite.Any && !p.BitingNow ? 3 : sp.Rarity >= Rarity.Epic ? 4 : sp.Rarity >= Rarity.Rare ? 2 : 0;
            p.Bait = want == 0 ? "Any bait (Worms bring the most)" : Baits[want].Name;
            if (!p.Open) { p.Next = "Open " + def.Name + " first, from HARBOURS."; return p; }
            if (!p.Here) { p.Next = "Sail to " + def.Name + ": tap the harbour's name."; return p; }
            var h = H;
            if (want > 0)
            {
                // The dock that can take it, highest level first; Chum wanted but not there yet: Spinners will do.
                for (int tryBait = want; tryBait >= 0; tryBait = tryBait == 4 ? 2 : -1)
                {
                    int best = -1;
                    for (int i = 0; i < h.berths.Count; i++) if (BaitUnlocked(h, i, tryBait) && (best < 0 || h.berths[i].level > h.berths[best].level)) best = i;
                    if (best >= 0)
                    {
                        if (h.berths[best].bait == tryBait || HasBait(h, tryBait)) { p.Next = p.BitingNow ? "Biting now: fish by hand (FISH) or let the " + Baits[tryBait].Name + " docks bring it." : "The " + Baits[tryBait].Name + " is on. Wait for it, or fish by hand."; return p; }
                        p.BaitIndex = tryBait; p.Berth = best; p.Bait = Baits[tryBait].Name;
                        p.Next = "Put " + Baits[tryBait].Name + " on " + Content.Berths[best].Name + ".";
                        return p;
                    }
                    if (tryBait != 4) break;
                }
                int need = BaitLevel(Baits[want == 4 ? 2 : want]); int top = 0; for (int i = 1; i < h.berths.Count; i++) if (h.berths[i].level > h.berths[top].level) top = i;
                p.Next = "Raise " + Content.Berths[top].Name + " to level " + need + " (now " + h.berths[top].level + ") for " + Baits[want == 4 ? 2 : want].Name + (p.BitingNow ? ", or fish by hand now: it's biting." : ".");
                return p;
            }
            p.Next = p.BitingNow ? "Biting now: fish by hand (FISH) or let the boats bring it." : "It bites " + p.When.ToLowerInvariant() + ".";
            return p;
        }

        bool HasBait(HarbourState h, int bait) { foreach (var b in h.berths) if (b.bait == bait) return true; return false; }

        /// <summary>Does the plan's next step: puts the bait on the dock it names.</summary>
        public bool ApplyPlan(TargetPlan p) { return p != null && p.CanApply && SetBait(p.Berth, p.BaitIndex); }

        // ---- what a purchase would be worth to this player ----

        /// <summary>A purchase's benefit at the player's present pace, in a line (null: nothing to add to the blurb).</summary>
        public string PurchaseBenefit(string product)
        {
            var h = H;
            switch (product)
            {
                case "reelempire.ledger":
                {
                    double pace = 0; foreach (var hh in State.harbours) pace += RewardRate(hh);
                    if (Owns(product)) pace /= 2;
                    return pace > 0 ? "At your pace now: +" + Format.Money(pace * 60) + " a minute, +" + Format.Money(pace * 3600) + " an hour." : null;
                }
                case "reelempire.nightcrew":
                {
                    double idle = 0; foreach (var hh in State.harbours) idle += IdleRate(hh);
                    double extra = idle * 4 * 3600;
                    return "Away limit " + Format.Time(AwayCap) + " ▸ " + Format.Time(AwayCap + (Owns(product) ? 0 : 4 * 3600)) + (extra > 0 ? ": up to +" + Format.Money(extra) + " more each night" : "") + ", and every DOUBLE on the welcome-back card is free.";
                }
                case "reelempire.fleet":
                {
                    var v = Voyages[Voyages.Length - 1];
                    return "Two voyages out at once: a second " + v.Name + " brings about +" + Format.Money(VoyageCash(v)) + " and " + v.Gems + " gems.";
                }
                case "reelempire.starter":
                    return "In each of your " + State.harbours.Count + (State.harbours.Count == 1 ? " harbour" : " harbours") + ": porters " + h.haulers.Count + " ▸ " + (h.haulers.Count + (Owns(product) ? 0 : 1)) + ", trucks " + h.trucks.Count + " ▸ " + (h.trucks.Count + (Owns(product) ? 0 : 1)) + ".";
                case "reelempire.season":
                {
                    int left = 0; for (int t = 0; t < SeasonTiers; t++) if (!SeasonClaimed(t, true)) left++;
                    return "The premium reward on all " + SeasonTiers + " tiers, " + left + " still to take, " + SeasonTier + " reached so far.";
                }
            }
            return null;
        }

        // ---- the starter pack's timing ----

        /// <summary>Which way this save's starter-pack test goes (fixed per save by its cohort): "later_day" offers it on a
        /// later day after twenty minutes of play, "chain" once the harbour runs a processing line and an order has been filled.</summary>
        public string StarterVariant { get { return State.cohort < 50 ? "later_day" : "chain"; } }

        bool StarterMilestone { get { return H.goodsOwned > 1 && State.ordersDone >= 1; } }
    }
}

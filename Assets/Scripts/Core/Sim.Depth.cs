using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>How a fish fights on a hand line (see <see cref="Fight"/>).</summary>
    public enum FightStyle { Steady, Runner, Darter, Stayer }

    /// <summary>A customer the harbour has delivered to, and how often.</summary>
    [Serializable]
    public sealed class Tie { public string who; public int delivered; }

    /// <summary>A saved fleet set-up: the bait at each dock and who stands duty at each stage.</summary>
    [Serializable]
    public sealed class Preset { public bool saved; public List<int> baits = new List<int>(); public List<string> duty = new List<string>(); }

    /// <summary>The weekend derby's rules this time.</summary>
    public enum DerbyFormat { Classic, Variety, HandLine, BigOnes }

    /// <summary>
    /// Depth: fish that fight in their own ways, customers who come to know the harbour (and their favourite lines),
    /// saved fleet set-ups, the Boat Shed's crew mending boats by itself, recruit tokens to pick a crew card, derbies
    /// whose rules change each weekend, and what a renovated harbour specialises in.
    /// </summary>
    public sealed partial class Sim
    {
        // ---- fighting styles ----

        public static readonly string[] StyleNames = { "Steady", "Runner", "Darter", "Stayer" };
        public static readonly string[] StyleHints =
        {
            "Steady: a constant pull. Reel in short holds.",
            "Runner: rare, long, sudden runs. Let go the moment it bolts.",
            "Darter: short bursts, over and over. Tap, don't hold.",
            "Stayer: a long fight. Stay patient in the green.",
        };

        /// <summary>A species' fighting style: fixed by its name, the same for everyone.</summary>
        public static FightStyle StyleOf(SpeciesDef sp)
        {
            if (sp == null) return FightStyle.Steady;
            if (sp.Rarity == Rarity.Legendary) return FightStyle.Stayer;
            uint h = 2166136261;
            foreach (char c in sp.Id) { h ^= c; h *= 16777619; }
            return (FightStyle)(h % 4);
        }

        // ---- customers ----

        /// <summary>How well a customer knows the harbour: 0 new, 1 regular (2 orders), 2 friend (5), 3 partner (10).</summary>
        public static readonly int[] TieAt = { 0, 2, 5, 10 };
        public static readonly string[] TieNames = { "New", "Regular", "Friend", "Partner" };
        public static readonly double[] TieBonus = { 0, 0.1, 0.2, 0.35 };
        public const double FavouriteBonus = 0.25;

        /// <summary>Each customer's favourite line (an index into Content.Goods), in the order of <see cref="Customers"/>.</summary>
        static readonly int[] Favourites = { 2, 1, 3, 2, 0, 4, 0, 3, 2, 4, 1, 4 };

        /// <summary>What a customer says once they count the harbour a friend, and once a partner.</summary>
        static readonly string[] FriendLines =
        {
            "The Heron Inn puts your harbour's name on its menu board.",
            "Marta ladles you a bowl of chowder on the house, every time.",
            "The smokery shows you how they cure the oak chips.",
            "The ferry café keeps a table by the window for your crew.",
            "The Gull & Anchor pins your best catch's photo over the bar.",
            "The school kitchen's children draw your boats for the wall.",
            "The Saturday Market gives your stall the corner pitch.",
            "The lighthouse keeper lends you the old charts of the banks.",
            "Captain Brannock tells you where the big ones lie on a spring tide.",
            "The harbour master waves your boats through first.",
            "Pip names the fish cart after your harbour.",
            "The cannery shop stacks your tins in the window.",
        };

        public int CustomerIndex(string who) { return Array.IndexOf(Customers, who); }
        public int FavouriteOf(string who) { int i = CustomerIndex(who); return i < 0 ? 0 : Favourites[i]; }

        public int Delivered(string who) { var t = State.ties.Find(x => x.who == who); return t == null ? 0 : t.delivered; }
        public int TieLevel(string who) { int n = Delivered(who), lv = 0; for (int i = 1; i < TieAt.Length; i++) if (n >= TieAt[i]) lv = i; return lv; }

        /// <summary>What a customer's tie and favourite add to an order's pay.</summary>
        public double OrderBonus(Order o)
        {
            double b = 1 + TieBonus[TieLevel(o.who)];
            if (o.kind == (int)OrderKind.Product && o.favourite) b += FavouriteBonus;
            return b;
        }

        /// <summary>Counts a delivery. Returns the line the customer says if this one made them a friend (or partner).</summary>
        string Deliver(string who)
        {
            if (string.IsNullOrEmpty(who)) return null;
            var t = State.ties.Find(x => x.who == who);
            if (t == null) { t = new Tie { who = who }; State.ties.Add(t); }
            int before = TieLevel(who); t.delivered++;
            int after = TieLevel(who);
            if (after == before) return null;
            int i = CustomerIndex(who);
            if (after == 2 && i >= 0) return FriendLines[i];
            if (after == 3) return who + " signs on as a partner: their orders pay " + Math.Round(TieBonus[3] * 100) + "% more, and each one brings a gem more.";
            return who + " is a regular now: their orders pay " + Math.Round(TieBonus[after] * 100) + "% more.";
        }

        /// <summary>The last thing a customer said when they came to know the harbour better (shown once by the HUD).</summary>
        public string TieNews;

        // ---- fleet presets ----

        public static readonly string[] PresetNames = { "Earning", "Hunting", "Orders", "Derby" };

        Preset PresetAt(int i)
        {
            while (State.presets.Count < PresetNames.Length) State.presets.Add(new Preset());
            return State.presets[i];
        }

        public bool PresetSaved(int i) { return i >= 0 && i < PresetNames.Length && PresetAt(i).saved; }

        /// <summary>Saves this harbour's baits and the crew on duty under a preset.</summary>
        public bool SavePreset(int i)
        {
            if (i < 0 || i >= PresetNames.Length) return false;
            var p = PresetAt(i); var h = H;
            p.saved = true; p.baits.Clear(); p.duty.Clear();
            foreach (var b in h.berths) p.baits.Add(b.bait);
            foreach (var d in State.duty) p.duty.Add(d ?? "");
            Touch();
            return true;
        }

        /// <summary>Puts a preset on: each dock's bait where it is unlocked here, and its crew where they are owned.
        /// Returns how many things changed.</summary>
        public int UsePreset(int i)
        {
            if (!PresetSaved(i)) return 0;
            var p = PresetAt(i); var h = H; int n = 0;
            for (int b = 0; b < h.berths.Count && b < p.baits.Count; b++)
                if (h.berths[b].bait != p.baits[b] && SetBait(b, p.baits[b])) n++;
            for (int s = 0; s < 4 && s < p.duty.Count; s++)
            {
                string id = p.duty[s];
                if (State.duty[s] == id) continue;
                if (string.IsNullOrEmpty(id) ? Assign((Stage)s, null) : Assign((Stage)s, id)) n++;
            }
            Touch();
            return n;
        }

        /// <summary>A line on what a preset holds.</summary>
        public string PresetText(int i)
        {
            if (!PresetSaved(i)) return "Empty: set the baits and crew as you like them, then SAVE.";
            var p = PresetAt(i); var names = new List<string>();
            var counts = new Dictionary<int, int>();
            foreach (int b in p.baits) counts[b] = counts.TryGetValue(b, out int c) ? c + 1 : 1;
            foreach (var kv in counts) names.Add(kv.Value + " " + Baits[Math.Max(0, Math.Min(Baits.Length - 1, kv.Key))].Name);
            int crew = 0; foreach (var d in p.duty) if (!string.IsNullOrEmpty(d)) crew++;
            return string.Join(", ", names) + "  ·  " + crew + " on duty";
        }

        // ---- the Boat Shed mends by itself ----

        /// <summary>From Boat Shed level 2 its crew mends the worst worn boat by itself every so often while you play.</summary>
        public const int AutoMendShed = 2;
        public const double AutoMendEvery = 20 * 60;

        public bool AutoMends(HarbourState h) { return WorkLevel(h, "shipyard") >= AutoMendShed; }
        public double AutoMendIn { get { return Math.Max(0, State.mendIn); } }

        void TickMend(HarbourState h, double dt)
        {
            if (!AutoMends(h) || !Unlocked("wear")) return;
            if (State.mendIn <= 0) State.mendIn = AutoMendEvery;
            State.mendIn -= dt;
            if (State.mendIn > 0) return;
            State.mendIn = AutoMendEvery;
            int worst = -1;
            for (int i = 0; i < h.berths.Count; i++) if (Worn(h, i) && (worst < 0 || h.berths[i].wear > h.berths[worst].wear)) worst = i;
            if (worst < 0) return;
            h.berths[worst].wear = 0; State.autoMended++;
            Mended?.Invoke(worst);
            Touch();
        }

        /// <summary>A boat the Boat Shed's crew put right by itself.</summary>
        public event Action<int> Mended;

        // ---- recruit tokens ----

        /// <summary>Tokens a card costs to recruit by rarity (Common, Rare, Epic, Legendary).</summary>
        public static int RecruitCost(CrewDef d) { return d.Rarity == Rarity.Common ? 6 : d.Rarity == Rarity.Rare ? 15 : d.Rarity == Rarity.Epic ? 40 : 100; }

        /// <summary>Takes a chosen card for tokens: a new hand, or a copy toward its next level.</summary>
        public bool Recruit(string id)
        {
            var d = CrewById(id); if (d == null) return false;
            var own = Owned(id);
            if (own != null && own.level >= CrewMaxLevel) return false;
            int cost = RecruitCost(d);
            if (State.tokens < cost) return false;
            State.tokens -= cost;
            GiveCard(d);
            Touch();
            return true;
        }

        /// <summary>Adds a card: a new hand (straight to work if the stage has nobody), else a copy. A copy beyond the
        /// last level turns into tokens.</summary>
        void GiveCard(CrewDef card)
        {
            var own = Owned(card.Id);
            if (own == null)
            {
                State.crew.Add(new CrewState { id = card.Id });
                if (OnDuty(card.Stage) == null) State.duty[(int)card.Stage] = card.Id;
                return;
            }
            if (own.level >= CrewMaxLevel) { State.tokens += 2; return; }
            own.copies++;
            while (own.level < CrewMaxLevel && own.copies >= CopiesFor(own.level)) { own.copies -= CopiesFor(own.level); own.level++; }
        }

        // ---- derby formats ----

        public static readonly string[] FormatNames = { "Classic Derby", "Variety Derby", "Hand-Line Derby", "Big Ones Derby" };
        public static readonly string[] FormatRules =
        {
            "Every fish counts, the rarer the more: 1, 3, 10, 40 and 200 points.",
            "Each kind of fish counts once this weekend, for 20 times its usual points. Fish every water.",
            "Only fish you land by hand count, for three times the points.",
            "Only rare fish or better count, for twice the points.",
        };
        /// <summary>How the rivals' and the goals' scores scale with the format (fewer fish count in some).</summary>
        static readonly double[] FormatScale = { 1, 0.35, 0.2, 0.6 };

        public static DerbyFormat FormatOf(long derby) { return (DerbyFormat)(int)(((derby % 4) + 4) % 4); }
        public DerbyFormat DerbyFormatNow { get { return FormatOf(DerbyIndex); } }
        public static double ScaleOf(long derby) { return FormatScale[(int)FormatOf(derby)]; }

        /// <summary>A derby goal's points in this weekend's format.</summary>
        public long DerbyGoal(int i) { return Math.Max(10, (long)Math.Round(DerbyGoals[i] * ScaleOf(DerbyIndex) / 10) * 10); }

        /// <summary>Points a catch is worth in this derby's format (0: it doesn't count).</summary>
        public long DerbyPointsFor(SpeciesDef sp, bool byHand)
        {
            int basic = DerbyPoints[(int)sp.Rarity];
            switch (DerbyFormatNow)
            {
                case DerbyFormat.Variety: return State.derbyKinds.Contains(sp.Id) ? 0 : basic * 20;
                case DerbyFormat.HandLine: return byHand ? basic * 3 : 0;
                case DerbyFormat.BigOnes: return sp.Rarity >= Rarity.Rare ? basic * 2 : 0;
                default: return basic;
            }
        }

        void DerbyCatch(SpeciesDef sp, bool byHand)
        {
            if (!DerbyOn) return;
            RollDerby();
            long pts = DerbyPointsFor(sp, byHand);
            if (DerbyFormatNow == DerbyFormat.Variety && !State.derbyKinds.Contains(sp.Id)) State.derbyKinds.Add(sp.Id);
            State.derbyScore += pts;
        }

        // ---- renovation focus ----

        public static readonly string[] Focuses = { "collect", "process", "voyage", "transport" };
        public static readonly string[] FocusNames = { "Collecting", "Processing", "Voyages", "Transport" };
        public static readonly string[] FocusBlurbs =
        {
            "Boats land 25% more, and hand casts bring rarer fish.",
            "The hall works 50% faster: more of the catch becomes worth more.",
            "Voyages come home a quarter sooner, with 25% more aboard.",
            "Porters and trucks move 25% more.",
        };
        public const double FocusChangeEvery = 86400;

        public string FocusOf(HarbourState h) { return h.renovations > 0 ? h.focus ?? "" : ""; }
        bool Focused(HarbourState h, string f) { return FocusOf(h) == f; }

        /// <summary>Whether the focus can be changed now (free, once a day, in a renovated harbour).</summary>
        public double FocusChangeIn(HarbourState h) { return Math.Max(0, h.focusAt + FocusChangeEvery - RealNow()); }

        /// <summary>Picks or changes a renovated harbour's focus. The first pick is free at any time; a change waits a day.</summary>
        public bool SetFocus(string f)
        {
            var h = H;
            if (h.renovations < 1 || Array.IndexOf(Focuses, f) < 0 || h.focus == f) return false;
            if (!string.IsNullOrEmpty(h.focus) && FocusChangeIn(h) > 0) return false;
            h.focus = f; h.focusAt = RealNow();
            Touch();
            return true;
        }

        /// <summary>The focus's share on a named effect (1 = none).</summary>
        double FocusFx(HarbourState h, string what)
        {
            switch (FocusOf(h))
            {
                case "collect": return what == "catch" ? 1.25 : 1;
                case "process": return what == "hall" ? 1.5 : 1;
                case "transport": return what == "dock" || what == "market" ? 1.25 : 1;
                default: return 1;
            }
        }
    }
}

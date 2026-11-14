using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>Something to build on a plot by the harbour: three levels, paid in that harbour's cash.</summary>
    public sealed class WorkDef
    {
        public string Id, Name, Does;
        public double Cost;             // the first level, before the harbour's scale; each level after costs sixty times the last
        public Func<int, string> Says;  // what a level gives, in a few words
    }

    /// <summary>A piece of research: bought once with notes, kept for good in every harbour.</summary>
    public sealed class ResearchDef
    {
        public string Id, Name, Blurb, Branch;
        public int Notes;
    }

    /// <summary>
    /// The harbour's works and the chart room's research. Works stand on plots round the harbour and belong to it
    /// (a renovation clears them with everything else); research is paid for in notes, which come from filled
    /// orders and from every species landed for the first time, and is never lost.
    /// </summary>
    public sealed partial class Sim
    {
        public const int WorkLevels = 3;
        public static readonly WorkDef[] Works =
        {
            new WorkDef { Id = "netloft", Name = "Net Loft", Does = "Bigger nets and room to dry them", Cost = 4e3, Says = l => "+" + l * 15 + "% catch, crates hold " + (l + 1) + "x" },
            new WorkDef { Id = "shipyard", Name = "Boat Shed", Does = "Boats kept trim sail faster and wear slower", Cost = 3e4, Says = l => "trips " + l * 10 + "% shorter, hulls last " + l * 50 + "% longer" },
            new WorkDef { Id = "icehouse", Name = "Ice House", Does = "Fresh fish keep, and sell for more", Cost = 2.5e5, Says = l => "fresh fish +" + l * 25 + "%, the hall floor holds " + (l + 1) + "x" },
            new WorkDef { Id = "auction", Name = "Auction Stand", Does = "Buyers bid against each other", Cost = 2e7, Says = l => "+" + l * 10 + "% on every sale" },
            new WorkDef { Id = "lamp", Name = "Lighthouse Lamp", Does = "Lit, it draws the big fish in", Cost = 2e8, Says = l => "rare fish " + (l == 1 ? "half as often again" : l == 2 ? "twice as often" : "three times as often") },
        };

        public static readonly ResearchDef[] Research =
        {
            new ResearchDef { Id = "hooks", Branch = "FISHING", Name = "Sharper Hooks", Blurb = "Every boat lands 10% more", Notes = 3 },
            new ResearchDef { Id = "finder", Branch = "FISHING", Name = "Reading the Water", Blurb = "Rare fish bite a quarter more often", Notes = 6 },
            new ResearchDef { Id = "baitlore", Branch = "FISHING", Name = "Bait Lore", Blurb = "Baits unlock at half the dock level", Notes = 10 },
            new ResearchDef { Id = "deeplines", Branch = "FISHING", Name = "Deep Lines", Blurb = "Epic and legendary fish twice as often", Notes = 20 },
            new ResearchDef { Id = "barrows", Branch = "HARBOUR", Name = "Oiled Barrows", Blurb = "Porters carry 15% more", Notes = 3 },
            new ResearchDef { Id = "knives", Branch = "HARBOUR", Name = "Sharp Knives", Blurb = "The hall works 15% faster", Notes = 6 },
            new ResearchDef { Id = "coldchain", Branch = "HARBOUR", Name = "Cold Chain", Blurb = "The market sells 15% faster", Notes = 10 },
            new ResearchDef { Id = "rota", Branch = "HARBOUR", Name = "A Proper Rota", Blurb = "Crew rest a quarter less between skills", Notes = 20 },
            new ResearchDef { Id = "haggle", Branch = "TRADE", Name = "Haggling", Blurb = "Orders pay a quarter more", Notes = 3 },
            new ResearchDef { Id = "regulars", Branch = "TRADE", Name = "Regulars", Blurb = "A fourth place on the order board", Notes = 6 },
            new ResearchDef { Id = "word", Branch = "TRADE", Name = "Word of Mouth", Blurb = "New orders come every six minutes", Notes = 10 },
            new ResearchDef { Id = "builder", Branch = "TRADE", Name = "Master Builder", Blurb = "Works cost a fifth less", Notes = 20 },
        };

        // ---- works ----

        public int WorkLevel(HarbourState h, string id)
        {
            int i = Array.FindIndex(Works, w => w.Id == id);
            return i >= 0 && i < h.works.Count ? h.works[i] : 0;
        }

        /// <summary>What the next level of a work costs (infinity when it is finished).</summary>
        public double WorkCost(HarbourState h, string id)
        {
            int i = Array.FindIndex(Works, w => w.Id == id); int lv = WorkLevel(h, id);
            if (i < 0 || lv >= WorkLevels) return double.PositiveInfinity;
            return Works[i].Cost * Math.Pow(60, lv) * DefOf(h).Scale * (Researched("builder") ? 0.8 : 1);
        }

        public bool BuildWork(string id)
        {
            var h = H; int i = Array.FindIndex(Works, w => w.Id == id);
            double cost = WorkCost(h, id);
            if (i < 0 || h.cash < cost) return false;
            while (h.works.Count < Works.Length) h.works.Add(0);
            h.cash -= cost; h.works[i]++;
            State.worksBuilt++;
            AddSeasonPoints(10);
            Touch();
            return true;
        }

        /// <summary>How many works stand in a harbour, levels added up.</summary>
        public int WorkTotal(HarbourState h) { int n = 0; foreach (int l in h.works) n += l; return n; }

        // ---- research ----

        public bool Researched(string id) { return State.research.Contains(id); }

        /// <summary>Why a piece of research can't be bought yet, or null when it can.</summary>
        public string ResearchBlocked(ResearchDef r)
        {
            if (Researched(r.Id)) return "Known";
            int i = Array.IndexOf(Research, r);
            if (i > 0 && Research[i - 1].Branch == r.Branch && !Researched(Research[i - 1].Id)) return "After " + Research[i - 1].Name;
            if (State.notes < r.Notes) return "Needs " + r.Notes + " notes";
            return null;
        }

        public bool DoResearch(string id)
        {
            var r = Array.Find(Research, x => x.Id == id);
            if (r == null || ResearchBlocked(r) != null) return false;
            State.notes -= r.Notes; State.research.Add(r.Id);
            Touch();
            return true;
        }

        /// <summary>Whether there is research to be had for the notes in hand.</summary>
        public bool ResearchReady { get { foreach (var r in Research) if (ResearchBlocked(r) == null) return true; return false; } }

        // ---- what they do ----

        /// <summary>How often each rarity bites on a bait in a harbour, out of 1000: the lamp and the research raise the rare ones.</summary>
        public int[] OddsFor(HarbourState h, BaitDef bait)
        {
            var basis = bait.Odds ?? Content.RarityOdds;
            double rare = 1 + new[] { 0, 0.5, 1, 2 }[Math.Min(3, WorkLevel(h, "lamp"))];
            if (Researched("finder")) rare *= 1.25;
            double great = rare * (Researched("deeplines") ? 2 : 1);
            if (rare == 1 && great == 1) return basis;
            var odds = (int[])basis.Clone(); int moved = 0;
            for (int i = 2; i < odds.Length; i++)
            {
                if (basis[i] == 0) continue;                    // a bait that brings nothing rare still brings nothing rare
                int now = (int)Math.Round(basis[i] * (i >= 3 ? great : rare));
                moved += now - basis[i]; odds[i] = now;
            }
            odds[0] = Math.Max(0, odds[0] - moved);             // the extra rare fish come out of the common ones
            return odds;
        }

        /// <summary>The average worth of a trip in a harbour on a bait against one plain common fish.</summary>
        public double TripMean(HarbourState h, BaitDef bait)
        {
            var odds = OddsFor(h, bait); double m = 0, all = 0;
            for (int i = 0; i < odds.Length; i++) { m += odds[i] * Content.RarityValue[i]; all += odds[i]; }
            return all <= 0 ? bait.Catch : m / all * bait.Catch;
        }

        public int BaitLevel(BaitDef b) { return Researched("baitlore") && b.Level > 1 ? (b.Level + 1) / 2 : b.Level; }
        public int OrderSlotCount { get { return OrderSlots + (Researched("regulars") ? 1 : 0); } }
        public double OrderGap { get { return Researched("word") ? 360 : OrderEvery; } }
    }
}

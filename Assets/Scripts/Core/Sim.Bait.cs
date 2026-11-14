using System;

namespace ReelEmpire.Core
{
    /// <summary>What a dock fishes with. A bait changes how much comes up and which fish take it.</summary>
    public sealed class BaitDef
    {
        public string Id, Name, Blurb;
        public int Level;               // the dock level that unlocks it
        public double Catch = 1;        // how much a trip brings against worms
        public int[] Odds;              // how often each rarity bites out of 1000 (null: the usual odds)
        public bool AnyTime;            // night, rain, storm and season fish take it whenever
    }

    /// <summary>
    /// Bait: each dock chooses one, free to change. Groundbait is for money, spinners and chum for rare fish (the
    /// derby, the trophies), glow bait for the fish the hour or the weather would otherwise keep away.
    /// </summary>
    public sealed partial class Sim
    {
        public static readonly BaitDef[] Baits =
        {
            new BaitDef { Id = "worm", Name = "Worms", Blurb = "Whatever is biting.", Level = 1 },
            new BaitDef { Id = "bread", Name = "Groundbait", Blurb = "Shoals of small fish: half as much again, but nothing rare.", Level = 10, Catch = 1.5, Odds = new[] { 850, 150, 0, 0, 0 } },
            new BaitDef { Id = "lure", Name = "Spinners", Blurb = "A lighter catch with three times the rare fish.", Level = 25, Catch = 0.8, Odds = new[] { 640, 200, 130, 27, 3 } },
            new BaitDef { Id = "glow", Name = "Glow Bait", Blurb = "Night, rain, storm and season fish bite at any hour.", Level = 50, Catch = 0.9, AnyTime = true },
            new BaitDef { Id = "chum", Name = "Chum", Blurb = "For the giants: ten times the epics and legends.", Level = 100, Catch = 0.6, Odds = new[] { 600, 200, 100, 90, 10 } },
        };

        public bool BaitUnlocked(HarbourState h, int berth, int bait) { return bait >= 0 && bait < Baits.Length && h.berths[berth].level >= BaitLevel(Baits[bait]); }

        /// <summary>The bait a dock is using (worms if the one chosen is no longer unlocked, as after a renovation).</summary>
        public BaitDef BaitOf(HarbourState h, int berth)
        {
            int b = h.berths[berth].bait;
            return BaitUnlocked(h, berth, b) ? Baits[b] : Baits[0];
        }

        public bool SetBait(int berth, int bait)
        {
            var h = H;
            if (berth < 0 || berth >= h.berths.Count || !BaitUnlocked(h, berth, bait)) return false;
            h.berths[berth].bait = bait;
            Touch();
            return true;
        }

        /// <summary>The average worth of a trip on this bait against one plain common fish.</summary>
        public static double BaitMean(BaitDef b)
        {
            var odds = b.Odds ?? Content.RarityOdds; double m = 0;
            for (int i = 0; i < odds.Length; i++) m += odds[i] / 1000.0 * Content.RarityValue[i];
            return m * b.Catch;
        }

        /// <summary>The next bait a dock will unlock, or null when it has them all.</summary>
        public BaitDef NextBait(HarbourState h, int berth)
        {
            foreach (var b in Baits) if (h.berths[berth].level < BaitLevel(b)) return b;
            return null;
        }
    }
}

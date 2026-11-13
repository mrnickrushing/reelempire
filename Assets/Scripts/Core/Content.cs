using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

    /// <summary>When a species bites. Conditions only ever add fish: nothing is taken away in fair weather.</summary>
    public enum Bite { Any, Day, Night, Rain, Storm, Spring, Summer, Autumn, Winter }

    public sealed class SpeciesDef
    {
        public string Id, Name, Harbour;
        public Rarity Rarity;
        public Bite When;
        public float MinKg, MaxKg;
    }

    /// <summary>One of the six waters. Every harbour has its own cash; <see cref="Scale"/> sizes its numbers.</summary>
    public sealed class HarbourDef
    {
        public string Id, Name, Water, Blurb;
        public double Scale;            // every value and cost here is multiplied by this
        public double UnlockCost;       // in the previous harbour's cash
        public int UnlockStars;         // stars needed in the previous harbour
    }

    public sealed class BerthDef
    {
        public string Name, Boat;
        public double UnlockCost;       // before the harbour's scale
        public double LevelCost;        // cost of the first level-up
        public double Catch;            // value of a trip's catch at level 1
        public float Trip;              // seconds out and back
    }

    /// <summary>What the processing hall can turn fish into.</summary>
    public sealed class GoodsDef
    {
        public string Id, Name, Line;
        public double Value;            // value multiplier of what goes through
        public double Speed;            // share of the hall's pace this line works at
        public double UnlockCost;       // before the harbour's scale
    }

    /// <summary>The numbers of the game. Tuning lives here; <c>PacingTests</c> prints what it does to the first hours.</summary>
    public static class Content
    {
        public const double BerthGrowth = 1.08, StageGrowth = 1.10, StageOutput = 1.08;
        public const int MaxLevel = 400;
        public static readonly int[] Milestones = { 10, 25, 50, 100, 150, 200, 250, 300, 350, 400 };
        /// <summary>Which milestones halve a berth's trip instead of doubling its catch.</summary>
        public static readonly int[] SpeedMilestones = { 25, 100 };
        /// <summary>Stage levels that add a hauler or a truck.</summary>
        public static readonly int[] CrewMilestones = { 10, 25, 50, 100, 200 };

        public const double StartCash = 10;
        public const float HaulRound = 6f, TruckRound = 8f;
        public const double DockBase = 3.5, HallBase = 2.5, MarketBase = 4.5;       // units a second at level 1
        public const double DockCost = 18, HallCost = 40, MarketCost = 25;          // first level-up
        public const double DeckTrips = 4;                                          // a berth's crate holds this many trips
        public const double PileSeconds = 45;                                       // the hall's floor holds this long of the dock's pace
        public const double SkipperBerth = 3, SkipperDock = 400, SkipperMarket = 2500;   // berth: times its unlock cost (the first is free)
        public const double FreshDay = 1.5;                                         // the market's product of the day sells for this much more

        public static readonly BerthDef[] Berths =
        {
            new BerthDef { Name = "Old Jetty", Boat = "Rowboat", UnlockCost = 0, LevelCost = 4, Catch = 3, Trip = 3 },
            new BerthDef { Name = "Reed Landing", Boat = "Skiff", UnlockCost = 120, LevelCost = 28, Catch = 30, Trip = 5 },
            new BerthDef { Name = "Stone Quay", Boat = "Dory", UnlockCost = 2400, LevelCost = 450, Catch = 290, Trip = 6 },
            new BerthDef { Name = "Net Wharf", Boat = "Cutter", UnlockCost = 60e3, LevelCost = 10e3, Catch = 3100, Trip = 8 },
            new BerthDef { Name = "Long Pier", Boat = "Drifter", UnlockCost = 1.8e6, LevelCost = 260e3, Catch = 31e3, Trip = 10 },
            new BerthDef { Name = "Crane Dock", Boat = "Trawler", UnlockCost = 60e6, LevelCost = 8e6, Catch = 300e3, Trip = 12 },
            new BerthDef { Name = "Deepwater Mole", Boat = "Longliner", UnlockCost = 2.4e9, LevelCost = 280e6, Catch = 3e6, Trip = 15 },
            new BerthDef { Name = "Fleet Basin", Boat = "Factory Ship", UnlockCost = 110e9, LevelCost = 11e9, Catch = 32e6, Trip = 20 },
        };

        public static readonly GoodsDef[] Goods =
        {
            new GoodsDef { Id = "fresh", Name = "Fresh", Line = "Ice Table", Value = 1, Speed = 1, UnlockCost = 0 },
            new GoodsDef { Id = "cleaned", Name = "Cleaned", Line = "Cleaning Line", Value = 1.5, Speed = 1, UnlockCost = 600 },
            new GoodsDef { Id = "fillet", Name = "Fillets", Line = "Fillet Line", Value = 2.5, Speed = 0.7, UnlockCost = 250e3 },
            new GoodsDef { Id = "smoked", Name = "Smoked", Line = "Smokehouse", Value = 4, Speed = 0.45, UnlockCost = 200e6 },
            new GoodsDef { Id = "canned", Name = "Canned", Line = "Cannery", Value = 6, Speed = 0.3, UnlockCost = 150e9 },
        };

        public static readonly HarbourDef[] Harbours =
        {
            new HarbourDef { Id = "lake", Name = "Stillwater Lake", Water = "Lake", Scale = 1, Blurb = "A quiet lake, an old jetty and one rowboat." },
            new HarbourDef { Id = "river", Name = "Alder River", Water = "River", Scale = 400, UnlockCost = 5e9, UnlockStars = 8, Blurb = "Fast water under the alders: trout, salmon and sturgeon." },
            new HarbourDef { Id = "coast", Name = "Gull Coast", Water = "Coast", Scale = 2e5, UnlockCost = 4e12, UnlockStars = 10, Blurb = "Salt wind, crab pots and the morning fleet." },
            new HarbourDef { Id = "reef", Name = "Lantern Reef", Water = "Reef", Scale = 1e8, UnlockCost = 3e15, UnlockStars = 12, Blurb = "Clear warm shallows full of colour." },
            new HarbourDef { Id = "deep", Name = "The Deep", Water = "Open ocean", Scale = 5e10, UnlockCost = 2e18, UnlockStars = 14, Blurb = "Blue water, long lines and very big fish." },
            new HarbourDef { Id = "arctic", Name = "Frostmere", Water = "Arctic", Scale = 3e13, UnlockCost = 2e21, UnlockStars = 16, Blurb = "Ice floes, northern lights and the cold-water giants." },
        };

        public static HarbourDef Harbour(string id) { return Array.Find(Harbours, h => h.Id == id); }
        public static int HarbourIndex(string id) { return Array.FindIndex(Harbours, h => h.Id == id); }

        /// <summary>How often each rarity bites (out of 1000) and what it does to that trip's worth.</summary>
        public static readonly int[] RarityOdds = { 800, 150, 40, 9, 1 };
        public static readonly double[] RarityValue = { 1, 2, 4, 10, 50 };
        public static readonly int[] RarityGems = { 1, 2, 5, 10, 25 };      // first catch
        /// <summary>The average worth of a trip against a plain one, from the two tables above.</summary>
        public static double RarityMean
        {
            get { double m = 0; for (int i = 0; i < RarityOdds.Length; i++) m += RarityOdds[i] / 1000.0 * RarityValue[i]; return m; }
        }

        /// <summary>How many of <see cref="Milestones"/> a level has reached.</summary>
        public static int MilestonesAt(int level) { int n = 0; foreach (var m in Milestones) if (level >= m) n++; return n; }
        public static int NextMilestone(int level) { foreach (var m in Milestones) if (level < m) return m; return MaxLevel; }
        public static int CrewAt(int level) { int n = 1; foreach (var m in CrewMilestones) if (level >= m) n++; return n; }
    }
}

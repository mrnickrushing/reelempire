using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>One step of the guide: what to do, how far along it is, and what it pays.</summary>
    public sealed class GoalDef
    {
        public string Id, Text;
        public Func<Sim, double> Have;
        public double Need = 1;
        public int Gems;
        public double CashSeconds;      // pays this many seconds of the harbour's full pace (at least Floor)
        public double Floor;
        public Stage Point; public int PointBerth = -1;     // what the guide's arrow points at
    }

    public sealed partial class Sim
    {
        public static readonly List<GoalDef> Goals = new List<GoalDef>
        {
            new GoalDef { Id = "cast", Text = "Tap the boat to go fishing", Have = s => s.State.trips, Need = 1, Floor = 5, Point = Stage.Berth, PointBerth = 0 },
            new GoalDef { Id = "haul", Text = "Tap the hauler to fetch the catch", Have = s => s.State.hauls, Need = 1, Floor = 5, Point = Stage.Dock },
            new GoalDef { Id = "sell", Text = "Tap the truck to sell at the market", Have = s => s.State.sales, Need = 1, Floor = 10, Point = Stage.Market },
            new GoalDef { Id = "level5", Text = "Upgrade the Old Jetty to level 5", Have = s => s.H.berths[0].level, Need = 5, Floor = 20, Point = Stage.Berth, PointBerth = 0 },
            new GoalDef { Id = "skipper", Text = "Hire a skipper for the Old Jetty (the first is free)", Have = s => s.H.berths[0].skipper ? 1 : 0, Need = 1, Gems = 5, Point = Stage.Berth, PointBerth = 0 },
            new GoalDef { Id = "level10", Text = "Reach level 10 at the Old Jetty: the catch doubles", Have = s => s.H.berths[0].level, Need = 10, Floor = 60, Point = Stage.Berth, PointBerth = 0 },
            new GoalDef { Id = "berth2", Text = "Open the second berth", Have = s => s.H.berths.Count, Need = 2, Gems = 5, Point = Stage.Berth, PointBerth = 1 },
            new GoalDef { Id = "dock5", Text = "Upgrade the dock to level 5", Have = s => s.H.dock, Need = 5, CashSeconds = 20, Floor = 100, Point = Stage.Dock },
            new GoalDef { Id = "dockskipper", Text = "Hire a dock foreman: the haulers work by themselves", Have = s => s.H.dockSkipper ? 1 : 0, Need = 1, Gems = 5, Point = Stage.Dock },
            new GoalDef { Id = "clean", Text = "Buy the Cleaning Line in the hall", Have = s => s.OwnsGoods(s.H, 1) ? 1 : 0, Need = 1, CashSeconds = 30, Point = Stage.Hall },
            new GoalDef { Id = "marketskipper", Text = "Hire a market manager: the trucks drive by themselves", Have = s => s.H.marketSkipper ? 1 : 0, Need = 1, Gems = 10, Point = Stage.Market },
            new GoalDef { Id = "berth3", Text = "Open the third berth", Have = s => s.H.berths.Count, Need = 3, CashSeconds = 30, Point = Stage.Berth, PointBerth = 2 },
            new GoalDef { Id = "species5", Text = "Catch 5 different species", Have = s => s.SpeciesCaught, Need = 5, Gems = 10 },
            new GoalDef { Id = "hall10", Text = "Upgrade the hall to level 10", Have = s => s.H.hall, Need = 10, CashSeconds = 40, Point = Stage.Hall },
            new GoalDef { Id = "berth4", Text = "Open the fourth berth", Have = s => s.H.berths.Count, Need = 4, CashSeconds = 40, Point = Stage.Berth, PointBerth = 3 },
            new GoalDef { Id = "fillet", Text = "Buy the Fillet Line", Have = s => s.OwnsGoods(s.H, 2) ? 1 : 0, Need = 1, Gems = 10, Point = Stage.Hall },
            new GoalDef { Id = "l25", Text = "Reach level 25 at any berth: its boat sails twice as fast", Have = s => s.BestBerthLevel, Need = 25, CashSeconds = 60 },
            new GoalDef { Id = "berth5", Text = "Open the fifth berth", Have = s => s.H.berths.Count, Need = 5, CashSeconds = 60, Point = Stage.Berth, PointBerth = 4 },
            new GoalDef { Id = "species10", Text = "Catch 10 different species", Have = s => s.SpeciesCaught, Need = 10, Gems = 15 },
            new GoalDef { Id = "berth6", Text = "Open the sixth berth", Have = s => s.H.berths.Count, Need = 6, CashSeconds = 90, Point = Stage.Berth, PointBerth = 5 },
            new GoalDef { Id = "smoke", Text = "Build the Smokehouse", Have = s => s.OwnsGoods(s.H, 3) ? 1 : 0, Need = 1, Gems = 15, Point = Stage.Hall },
            new GoalDef { Id = "l100", Text = "Reach level 100 at any berth", Have = s => s.BestBerthLevel, Need = 100, Gems = 20 },
            new GoalDef { Id = "berth8", Text = "Open all eight berths", Have = s => s.H.berths.Count, Need = 8, Gems = 25 },
            new GoalDef { Id = "can", Text = "Build the Cannery", Have = s => s.OwnsGoods(s.H, 4) ? 1 : 0, Need = 1, Gems = 25, Point = Stage.Hall },
            new GoalDef { Id = "renovate", Text = "Renovate the harbour: tap its name at the top", Have = s => s.State.renovations, Need = 1, Gems = 50 },
            new GoalDef { Id = "river", Text = "Open Alder River", Have = s => s.State.harbours.Count, Need = 2, Gems = 50 },
        };

        public GoalDef Goal { get { return State.goalIndex < Goals.Count ? Goals[State.goalIndex] : null; } }
        public double GoalHave { get { var g = Goal; return g == null ? 0 : Math.Min(g.Need, g.Have(this)); } }
        public bool GoalDone { get { var g = Goal; return g != null && g.Have(this) >= g.Need; } }
        public double GoalCash(GoalDef g) { return Math.Max(g.Floor * Def.Scale, g.CashSeconds * FullRate(H)); }
        public int BestBerthLevel { get { int n = 0; foreach (var b in H.berths) n = Math.Max(n, b.level); return n; } }

        /// <summary>Takes the finished step's reward and moves to the next. False if it isn't done.</summary>
        public bool ClaimGoal()
        {
            var g = Goal;
            if (g == null || !GoalDone) return false;
            double cash = GoalCash(g);
            if (g.CashSeconds > 0 || g.Floor > 0) Pay(H, cash);
            State.gems += g.Gems;
            State.goalIndex++;
            Touch();
            return true;
        }

        /// <summary>The name the harbour master goes by (friends and the welcome card).</summary>
        public void Rename(string name)
        {
            name = (name ?? "").Trim();
            if (name.Length > 16) name = name.Substring(0, 16);
            State.playerName = name;
            Touch();
        }

        /// <summary>Stars across every harbour: the measure of how far the empire has come.</summary>
        public int Stars { get { int n = 0; foreach (var h in State.harbours) n += h.stars + h.renovations * 25; return n; } }
    }
}

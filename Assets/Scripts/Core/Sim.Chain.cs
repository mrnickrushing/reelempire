using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>The chain's numbers: what each stage does at its level, what the next level costs, and buying.</summary>
    public sealed partial class Sim
    {
        // ---- berths ----

        public double CatchOf(HarbourState h, int berth)
        {
            var d = Content.Berths[berth]; int lv = h.berths[berth].level;
            int doubles = 0;
            foreach (var m in Content.Milestones) if (lv >= m && Array.IndexOf(Content.SpeedMilestones, m) < 0) doubles++;
            return d.Catch * DefOf(h).Scale * lv * Math.Pow(2, doubles) * Fx(h, "catch");
        }

        public float TripOf(HarbourState h, int berth)
        {
            float t = Content.Berths[berth].Trip; int lv = h.berths[berth].level;
            foreach (var m in Content.SpeedMilestones) if (lv >= m) t *= 0.5f;
            return t;
        }

        public double DeckOf(HarbourState h, int berth) { return CatchOf(h, berth) * Content.DeckTrips; }

        /// <summary>Units a second a berth lands on average (rare fish included).</summary>
        public double BerthRate(HarbourState h, int berth) { return CatchOf(h, berth) * Content.RarityMean / TripOf(h, berth); }

        public double BerthUnlockCost(HarbourState h) { return h.berths.Count >= Content.Berths.Length ? double.PositiveInfinity : Content.Berths[h.berths.Count].UnlockCost * DefOf(h).Scale * Fx(h, "berthcost"); }

        public double SkipperCost(HarbourState h, Stage stage, int berth = 0)
        {
            if (!State.freeSkipperUsed) return 0;
            double s = DefOf(h).Scale;
            switch (stage)
            {
                case Stage.Berth: return Math.Max(60, Content.Berths[berth].UnlockCost * Content.SkipperBerth) * s;
                case Stage.Dock: return Content.SkipperDock * s;
                case Stage.Market: return Content.SkipperMarket * s;
            }
            return double.PositiveInfinity;
        }

        public bool HasSkipper(HarbourState h, Stage stage, int berth = 0)
        {
            return stage == Stage.Berth ? h.berths[berth].skipper : stage == Stage.Dock ? h.dockSkipper : stage == Stage.Market ? h.marketSkipper : true;
        }

        // ---- shared stages ----

        static double StageOut(double baseRate, int level) { return baseRate * Math.Pow(Content.StageOutput, level - 1) * Math.Pow(2, Content.MilestonesAt(level)); }

        public double DockRate(HarbourState h) { return StageOut(Content.DockBase, h.dock) * DefOf(h).Scale * Fx(h, "dock"); }
        /// <summary>Units a second the hall works on the line it is running.</summary>
        public double HallRate(HarbourState h) { return StageOut(Content.HallBase, h.hall) * DefOf(h).Scale * Content.Goods[h.goods].Speed * Fx(h, "hall"); }
        public double MarketRate(HarbourState h) { return StageOut(Content.MarketBase, h.market) * DefOf(h).Scale * Fx(h, "market"); }

        public double HaulLoad(HarbourState h) { return DockRate(h) * Content.HaulRound / h.haulers.Count; }
        public double TruckLoad(HarbourState h) { return MarketRate(h) * Content.TruckRound / h.trucks.Count; }
        public double PileCap(HarbourState h) { return DockRate(h) * Content.PileSeconds; }
        public double PileRoom(HarbourState h) { return Math.Max(0, PileCap(h) - h.raw - h.done); }

        /// <summary>What a unit sells for as this product today (before income multipliers).</summary>
        public double GoodsValue(HarbourState h, int goods) { return Content.Goods[goods].Value * (goods == GoodsOfTheDay ? Content.FreshDay : 1); }

        /// <summary>The product the market pays half as much again for today.</summary>
        public int GoodsOfTheDay { get { return (int)(Today % Content.Goods.Length); } }

        /// <summary>Whole days since 1970 at the player's clock (tests set <see cref="UtcOffset"/> to 0).</summary>
        public long Today { get { return (long)Math.Floor((RealNow() + UtcOffset) / 86400.0); } }
        public double UtcOffset;

        void FitCrew(HarbourState h)
        {
            int n = Content.CrewAt(h.dock); while (h.haulers.Count < n) h.haulers.Add(new Carrier());
            n = Content.CrewAt(h.market); while (h.trucks.Count < n) h.trucks.Add(new Carrier());
        }

        // ---- levels and what they cost ----

        public int LevelOf(HarbourState h, Stage stage, int berth = 0)
        {
            return stage == Stage.Berth ? h.berths[berth].level : stage == Stage.Dock ? h.dock : stage == Stage.Hall ? h.hall : h.market;
        }

        double FirstCost(HarbourState h, Stage stage, int berth)
        {
            double s = DefOf(h).Scale;
            switch (stage)
            {
                case Stage.Berth: return Content.Berths[berth].LevelCost * s;
                case Stage.Dock: return Content.DockCost * s;
                case Stage.Hall: return Content.HallCost * s;
                default: return Content.MarketCost * s;
            }
        }

        static double Growth(Stage stage) { return stage == Stage.Berth ? Content.BerthGrowth : Content.StageGrowth; }

        /// <summary>What <paramref name="n"/> more levels cost from where the stage is now.</summary>
        public double LevelCost(HarbourState h, Stage stage, int berth = 0, int n = 1)
        {
            int lv = LevelOf(h, stage, berth);
            n = Math.Min(n, Content.MaxLevel - lv);
            if (n <= 0) return double.PositiveInfinity;
            double r = Growth(stage);
            return FirstCost(h, stage, berth) * Math.Pow(r, lv - 1) * (Math.Pow(r, n) - 1) / (r - 1) * Fx(h, "levelcost");
        }

        /// <summary>How many levels the harbour's cash buys now.</summary>
        public int Affordable(HarbourState h, Stage stage, int berth = 0)
        {
            int lv = LevelOf(h, stage, berth);
            double r = Growth(stage);
            double b = FirstCost(h, stage, berth) * Math.Pow(r, lv - 1) * Fx(h, "levelcost");
            int n = (int)Math.Floor(Math.Log(h.cash * (r - 1) / b + 1) / Math.Log(r) + 1e-9);
            return Math.Max(0, Math.Min(n, Content.MaxLevel - lv));
        }

        /// <summary>Buys up to <paramref name="n"/> levels (all or nothing). True if bought.</summary>
        public bool Upgrade(Stage stage, int berth = 0, int n = 1)
        {
            var h = H;
            if (stage == Stage.Berth && (berth < 0 || berth >= h.berths.Count)) return false;
            int lv = LevelOf(h, stage, berth);
            n = Math.Min(n, Content.MaxLevel - lv);
            if (n <= 0) return false;
            double cost = LevelCost(h, stage, berth, n);
            if (h.cash + 1e-9 < cost) return false;
            h.cash -= cost;
            int before = Content.MilestonesAt(lv);
            switch (stage)
            {
                case Stage.Berth: h.berths[berth].level += n; break;
                case Stage.Dock: h.dock += n; break;
                case Stage.Hall: h.hall += n; break;
                default: h.market += n; break;
            }
            FitCrew(h);
            int gained = Content.MilestonesAt(lv + n) - before;
            if (gained > 0) { h.stars += gained; MilestoneReached?.Invoke(stage, berth, lv + n); }
            Touch();
            return true;
        }

        /// <summary>A stage crossed a milestone level (10, 25, 50...).</summary>
        public event Action<Stage, int, int> MilestoneReached;

        public bool UnlockBerth()
        {
            var h = H;
            double cost = BerthUnlockCost(h);
            if (h.cash + 1e-9 < cost) return false;
            h.cash -= cost;
            h.berths.Add(new BerthState());
            h.stars++;
            Touch();
            return true;
        }

        public bool HireSkipper(Stage stage, int berth = 0)
        {
            var h = H;
            if (stage == Stage.Hall || HasSkipper(h, stage, berth)) return false;
            if (stage == Stage.Berth && berth >= h.berths.Count) return false;
            double cost = SkipperCost(h, stage, berth);
            if (h.cash + 1e-9 < cost) return false;
            h.cash -= cost;
            State.freeSkipperUsed = true;
            if (stage == Stage.Berth) h.berths[berth].skipper = true; else if (stage == Stage.Dock) h.dockSkipper = true; else h.marketSkipper = true;
            Touch();
            return true;
        }

        public bool OwnsGoods(HarbourState h, int goods) { return (h.goodsOwned & (1 << goods)) != 0; }
        public double GoodsCost(HarbourState h, int goods) { return Content.Goods[goods].UnlockCost * DefOf(h).Scale; }

        /// <summary>Buys a processing line (in order) and switches the hall to it.</summary>
        public bool BuyGoods(int goods)
        {
            var h = H;
            if (goods <= 0 || goods >= Content.Goods.Length || OwnsGoods(h, goods) || !OwnsGoods(h, goods - 1)) return false;
            double cost = GoodsCost(h, goods);
            if (h.cash + 1e-9 < cost) return false;
            h.cash -= cost;
            h.goodsOwned |= 1 << goods; h.goods = goods; h.stars++;
            Touch();
            return true;
        }

        public bool SetGoods(int goods)
        {
            var h = H;
            if (goods < 0 || goods >= Content.Goods.Length || !OwnsGoods(h, goods)) return false;
            h.goods = goods;
            Touch();
            return true;
        }
    }
}

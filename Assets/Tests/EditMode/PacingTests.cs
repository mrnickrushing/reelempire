using System;
using System.Collections.Generic;
using NUnit.Framework;
using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    /// <summary>A sensible player, played fast: prints when each thing happens so tuning can be read off a timeline.</summary>
    public class PacingTests
    {
        static string Clock(double s) { return ((int)s / 3600) + ":" + ((int)s % 3600 / 60).ToString("00") + ":" + ((int)s % 60).ToString("00"); }

        /// <summary>One decision: the cheapest useful thing the cash buys. Returns what it did, or null.</summary>
        public static string Act(Sim s)
        {
            var h = s.H;
            // Skippers as soon as they can be paid for.
            for (int i = 0; i < h.berths.Count; i++) if (!h.berths[i].skipper && s.HireSkipper(Stage.Berth, i)) return "skipper berth " + (i + 1);
            if (!h.dockSkipper && s.HireSkipper(Stage.Dock)) return "skipper dock";
            if (!h.marketSkipper && s.HireSkipper(Stage.Market)) return "skipper market";
            if (h.berths.Count < Content.Berths.Length && s.UnlockBerth()) return "BERTH " + h.berths.Count;
            for (int g = 1; g < Content.Goods.Length; g++) if (!s.OwnsGoods(h, g) && s.BuyGoods(g)) return "LINE " + Content.Goods[g].Name;
            var neck = s.Bottleneck(h);
            if (neck == Stage.Dock) return s.Upgrade(Stage.Dock) ? "" : null;
            if (neck == Stage.Market) return s.Upgrade(Stage.Market) ? "" : null;
            // The fleet is what holds things back: the level that adds most income for its cost; the hall when it is behind.
            if (h.goods > 0 && s.HallRate(h) < s.Flow(h, true) && s.LevelCost(h, Stage.Hall) < h.cash * 0.5 && s.Upgrade(Stage.Hall)) return "";
            int best = -1; double bestWorth = 0;
            for (int i = 0; i < h.berths.Count; i++)
            {
                if (h.berths[i].level >= Content.MaxLevel) continue;
                double worth = s.BerthRate(h, i) / h.berths[i].level / s.LevelCost(h, Stage.Berth, i);
                if (worth > bestWorth) { bestWorth = worth; best = i; }
            }
            // Save for the next berth rather than sink cash into levels that pay back slower than it would.
            if (best >= 0 && s.Upgrade(Stage.Berth, best)) return "";
            return null;
        }

        [Test]
        public void FirstHours()
        {
            var s = TestGame.New();
            var log = new List<string>();
            s.MilestoneReached += (st, b, lv) => { if (st == Stage.Berth && (lv == 10 || lv == 25 || lv == 50 || lv == 100)) log.Add(Clock(s.State.played) + "  berth " + (b + 1) + " level " + lv); };
            double t = 0, nextPrint = 300;
            while (t < 4 * 3600)
            {
                // Taps: a player sends what stands idle about once a second.
                var h = s.H;
                for (int i = 0; i < h.berths.Count; i++) if (!h.berths[i].skipper) s.TapBerth(i);
                if (!h.dockSkipper) s.TapDock();
                if (!h.marketSkipper) s.TapMarket();
                for (int n = 0; n < 20; n++) { var did = Act(s); if (did == null) break; if (did != "") log.Add(Clock(t) + "  " + did); }
                s.Tick(1); t += 1;
                if (t >= nextPrint)
                {
                    log.Add(Clock(t) + "  -- cash " + Format.Money(h.cash) + ", " + Format.Money(s.FullRate(h)) + "/s (idle " + Format.Money(s.IdleRate(h)) + "), neck " + s.Bottleneck(h) + ", dock " + h.dock + " hall " + h.hall + " market " + h.market + ", berths " + string.Join("/", h.berths.ConvertAll(b => b.level.ToString())) + ", stars " + h.stars);
                    nextPrint *= 2;
                }
            }
            foreach (var l in log) Console.WriteLine(l);
            Assert.That(s.H.berths.Count, Is.GreaterThanOrEqualTo(4), "four berths inside four hours");
            Assert.That(s.IdleRate(s.H), Is.GreaterThan(0), "the harbour runs by itself");
        }
    }
}

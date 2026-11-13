using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>Multipliers, what the harbour earns by itself, the weakest stage, and earnings while away.</summary>
    public sealed partial class Sim
    {
        /// <summary>A named effect on a harbour (1 = none): "catch", "dock", "hall", "market", "levelcost", "berthcost".
        /// Crew, the logbook and trophies add to these.</summary>
        public double Fx(HarbourState h, string what) { return 1; }

        /// <summary>Everything that multiplies a sale.</summary>
        public double IncomeMult(HarbourState h)
        {
            double m = Math.Pow(2, h.renovations);
            if (Owns("reelempire.ledger")) m *= 2;
            return m;
        }

        /// <summary>How much faster everything moves just now (boosts that speed the harbour up).</summary>
        public double SpeedMult(HarbourState h) { return 1; }

        /// <summary>Units a second the boats with skippers land.</summary>
        public double FleetRate(HarbourState h, bool all = false)
        {
            double n = 0;
            for (int i = 0; i < h.berths.Count; i++) if (all || h.berths[i].skipper) n += BerthRate(h, i);
            return n;
        }

        /// <summary>The stage holding the harbour back (as if every stage had its skipper).</summary>
        public Stage Bottleneck(HarbourState h)
        {
            double fleet = FleetRate(h, true), dock = DockRate(h), market = MarketRate(h);
            if (fleet <= dock && fleet <= market) return Stage.Berth;
            return dock <= market ? Stage.Dock : Stage.Market;
        }

        /// <summary>Units a second through the whole chain.</summary>
        public double Flow(HarbourState h, bool all = false)
        {
            double f = Math.Min(FleetRate(h, all), DockRate(h));
            return Math.Min(f, MarketRate(h));
        }

        /// <summary>Cash a second the harbour makes with nobody tapping: only what skippers run counts.</summary>
        public double IdleRate(HarbourState h)
        {
            if (!h.dockSkipper || !h.marketSkipper) return 0;
            return ValueOf(h, Flow(h)) * IncomeMult(h);
        }

        /// <summary>Cash a second if every stage ran flat out (what the HUD shows as the harbour's pace).</summary>
        public double FullRate(HarbourState h) { return ValueOf(h, Flow(h, true)) * IncomeMult(h); }

        double ValueOf(HarbourState h, double flow)
        {
            if (h.goods == 0) return flow * GoodsValue(h, 0);
            double worked = Math.Min(flow, HallRate(h));
            return worked * GoodsValue(h, h.goods) + (flow - worked) * GoodsValue(h, 0);
        }

        // ---- away ----

        public const double AwayBase = 2 * 3600;
        public double AwayCap { get { return AwayBase + (Owns("reelempire.nightcrew") ? 4 * 3600 : 0); } }

        /// <summary>What the harbour on screen made in <paramref name="seconds"/> away (nothing under a minute).</summary>
        public double AwayEarnings(double seconds)
        {
            if (seconds < 60) return 0;
            return IdleRate(H) * Math.Min(seconds, AwayCap);
        }

        /// <summary>Pays the time away: the harbour on screen gets <paramref name="mult"/> times its share, the others theirs.</summary>
        public double CollectAway(double seconds, double mult = 1)
        {
            double got = AwayEarnings(seconds) * mult;
            Pay(H, got);
            if (seconds >= 60)
                for (int i = 0; i < State.harbours.Count; i++)
                    if (i != State.at) Pay(State.harbours[i], IdleRate(State.harbours[i]) * Math.Min(seconds, AwayCap));
            Touch();
            return got;
        }
    }
}

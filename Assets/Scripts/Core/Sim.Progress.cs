using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>
    /// The long game: renovating a harbour (it starts again, earning twice as much for good), renown (a share of
    /// everything ever earned, paid at each renovation: +2% income everywhere per point), opening the next water, and
    /// the Tide Boost (free, on a timer: double income for an hour a charge).
    /// </summary>
    public sealed partial class Sim
    {
        public const int MaxRenovations = 5;
        public const double RenovationBase = 100e9, RenovationGrowth = 40, HarbourCost = 2e12, RenownEach = 0.02;

        /// <summary>What the next renovation of a harbour costs, in its own cash.</summary>
        public double RenovationCost(HarbourState h) { return h.renovations >= MaxRenovations ? double.PositiveInfinity : RenovationBase * Math.Pow(RenovationGrowth, h.renovations) * DefOf(h).Scale; }

        /// <summary>Why the harbour can't be renovated yet, or null.</summary>
        public string RenovationBlocked(HarbourState h)
        {
            if (h.renovations >= MaxRenovations) return "Renovated as far as it goes";
            if (h.berths.Count < Content.Berths.Length) return "Open all eight docks first";
            if (h.cash < RenovationCost(h)) return "Needs " + Format.Money(RenovationCost(h));
            return null;
        }

        /// <summary>Renown the player would be paid if they renovated now.</summary>
        public int RenownWaiting { get { return Math.Max(0, (int)Math.Floor(40 * Math.Sqrt(State.lifetime / 1e9)) - State.renownPaid); } }

        /// <summary>Rebuilds the harbour on screen from the first jetty: everything it earns from now on is doubled.</summary>
        public bool Renovate()
        {
            var h = H;
            if (RenovationBlocked(h) != null) return false;
            int renown = RenownWaiting;
            State.renown += renown; State.renownPaid += renown;
            var fresh = GameState.NewHarbour(Content.HarbourIndex(h.id));
            fresh.renovations = h.renovations + 1;
            State.harbours[State.at] = fresh;
            FitCrew(fresh);
            State.renovations++;
            Touch();
            return true;
        }

        // ---- the next water ----

        public HarbourDef NextHarbour { get { return State.harbours.Count < Content.Harbours.Length ? Content.Harbours[State.harbours.Count] : null; } }
        /// <summary>The cost of the next harbour, in the cash of the newest one already open.</summary>
        public double NextHarbourCost { get { var last = State.harbours[State.harbours.Count - 1]; return HarbourCost * DefOf(last).Scale; } }

        public string NextHarbourBlocked
        {
            get
            {
                if (NextHarbour == null) return "Every water is open";
                var last = State.harbours[State.harbours.Count - 1];
                if (last.renovations < 1) return "Renovate " + DefOf(last).Name + " once first";
                if (last.cash < NextHarbourCost) return "Needs " + Format.Money(NextHarbourCost) + " at " + DefOf(last).Name;
                return null;
            }
        }

        public bool OpenNextHarbour()
        {
            if (NextHarbourBlocked != null) return false;
            var last = State.harbours[State.harbours.Count - 1];
            last.cash -= NextHarbourCost;
            State.harbours.Add(GameState.NewHarbour(State.harbours.Count));
            FitCrew(State.harbours[State.harbours.Count - 1]);
            State.at = State.harbours.Count - 1;
            Touch();
            return true;
        }

        /// <summary>Goes to another harbour that is open.</summary>
        public bool Travel(int index)
        {
            if (index < 0 || index >= State.harbours.Count || index == State.at) return false;
            State.at = index;
            Touch();
            return true;
        }

        // ---- the Tide Boost ----

        public const int BoostCharges = 3;
        public const double BoostEvery = 4 * 3600, BoostLasts = 3600, BoostCap = 8 * 3600, BoostMult = 2;

        public bool Boosted { get { return State.boostUntil > RealNow(); } }
        public double BoostLeft { get { return Math.Max(0, State.boostUntil - RealNow()); } }

        /// <summary>Charges ready now (they come back one every four hours, up to three).</summary>
        public int BoostReady
        {
            get
            {
                if (State.boostUsedAt.Count == 0) return BoostCharges;
                int back = 0; double now = RealNow();
                foreach (double t in State.boostUsedAt) if (now - t >= BoostEvery) back++;
                return BoostCharges - State.boostUsedAt.Count + back;
            }
        }

        /// <summary>Seconds until the next charge is back (0: one is ready, or all are).</summary>
        public double BoostNextIn
        {
            get
            {
                double now = RealNow(), best = double.PositiveInfinity;
                foreach (double t in State.boostUsedAt) if (now - t < BoostEvery) best = Math.Min(best, BoostEvery - (now - t));
                return double.IsInfinity(best) ? 0 : best;
            }
        }

        /// <summary>Seconds until every charge is back.</summary>
        public double BoostFullIn
        {
            get { double now = RealNow(), worst = 0; foreach (double t in State.boostUsedAt) worst = Math.Max(worst, BoostEvery - (now - t)); return Math.Max(0, worst); }
        }

        public bool UseBoost()
        {
            double now = RealNow();
            State.boostUsedAt.RemoveAll(t => now - t >= BoostEvery);
            if (State.boostUsedAt.Count >= BoostCharges) return false;
            double lasts = BoostLasts * (Logbook("tide") ? 1.5 : 1);
            if (BoostLeft + lasts > BoostCap + 1) return false;
            State.boostUsedAt.Add(now);
            State.boostUntil = Math.Max(State.boostUntil, now) + lasts;
            Touch();
            return true;
        }
    }
}

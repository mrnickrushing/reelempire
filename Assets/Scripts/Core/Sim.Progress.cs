using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>
    /// The long game: renovating a harbour (it starts again, earning twice as much for good), renown (a share of
    /// everything ever earned, paid at each renovation: +2% income everywhere per point), opening the next water, and
    /// the Tide Boost (double income for an hour, for a watched advert).
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

        // Double income for an hour, only ever for a watched advert (BoostByAd) or from the paid season track: nothing
        // doubles for free. It stacks to eight hours.
        public const double BoostLasts = 3600, BoostCap = 8 * 3600, BoostMult = 2;

        public bool Boosted { get { return State.boostUntil > RealNow(); } }
        public double BoostLeft { get { return Math.Max(0, State.boostUntil - RealNow()); } }
    }
}

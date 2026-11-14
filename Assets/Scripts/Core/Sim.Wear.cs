using System;

namespace ReelEmpire.Core
{
    /// <summary>
    /// Wear: a boat's hull wears as it sails. Past half worn it lands less (down to seven tenths when worn out), but
    /// it never stops. A wrench puts a boat right. Wrenches come from the day's gift, from filled orders, for gems,
    /// or for a watched advert; the Boat Shed makes hulls last longer. Nothing wears while the game is closed.
    /// </summary>
    public sealed partial class Sim
    {
        /// <summary>Seconds of sailing from a fresh hull to a worn-out one, before the Boat Shed.</summary>
        public const double WearSeconds = 3 * 3600;
        public const float WornAt = 0.5f, WornCatch = 0.7f;
        public const int WrenchGems = 10, WrenchesHeld = 30, AdWrenchBelow = 5;

        /// <summary>How much longer hulls last here: half as long again for each level of the Boat Shed.</summary>
        public double Durability(HarbourState h) { return 1 + 0.5 * WorkLevel(h, "shipyard"); }

        /// <summary>0 fresh .. 1 worn out.</summary>
        public float WearOf(HarbourState h, int berth) { return h.berths[berth].wear; }
        public bool Worn(HarbourState h, int berth) { return h.berths[berth].wear > WornAt; }
        public int WornBoats { get { int n = 0; var h = H; for (int i = 0; i < h.berths.Count; i++) if (Worn(h, i)) n++; return n; } }

        /// <summary>What a worn boat still lands: all of it up to half worn, then down to <see cref="WornCatch"/>.</summary>
        public static double WearFactor(float wear)
        {
            if (wear <= WornAt) return 1;
            return 1 - (1 - WornCatch) * Math.Min(1, (wear - WornAt) / (1 - WornAt));
        }

        void WearTrip(HarbourState h, int berth)
        {
            if (!Unlocked("wear")) return;
            var b = h.berths[berth];
            b.wear = (float)Math.Min(1, b.wear + Content.Berths[berth].Trip / (WearSeconds * Durability(h)));
        }

        /// <summary>Spends a wrench on a boat: good as new. False with no wrench, or nothing to mend.</summary>
        public bool Repair(int berth)
        {
            var h = H; if (berth < 0 || berth >= h.berths.Count) return false;
            var b = h.berths[berth];
            if (b.wear < 0.02f || State.wrenches <= 0) return false;
            State.wrenches--; b.wear = 0; State.repairs++;
            Touch();
            return true;
        }

        /// <summary>Every worn boat in the harbour, the worst first, while the wrenches last. Returns how many.</summary>
        public int RepairAll()
        {
            int n = 0; var h = H;
            while (State.wrenches > 0)
            {
                int worst = -1;
                for (int i = 0; i < h.berths.Count; i++) if (h.berths[i].wear > 0.02f && (worst < 0 || h.berths[i].wear > h.berths[worst].wear)) worst = i;
                if (worst < 0 || h.berths[worst].wear <= WornAt * 0.5f || !Repair(worst)) break;
                n++;
            }
            return n;
        }

        public void AddWrenches(int n) { State.wrenches = Math.Min(WrenchesHeld, State.wrenches + n); Touch(); }

        public bool BuyWrench()
        {
            if (State.gems < WrenchGems || State.wrenches >= WrenchesHeld) return false;
            State.gems -= WrenchGems; AddWrenches(1);
            return true;
        }

        /// <summary>A wrench for a watched advert. False if it wasn't on offer.</summary>
        public bool WrenchByAd()
        {
            if (!AdOffered(AdReward.Wrench)) return false;
            AddWrenches(1); CountAd();
            return true;
        }
    }
}

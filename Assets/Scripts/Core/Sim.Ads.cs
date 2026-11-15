using System;

namespace ReelEmpire.Core
{
    /// <summary>What a watched advert can pay for. Always the player's choice, never shown unasked.</summary>
    public enum AdReward { Double, Boost, Wrench }

    /// <summary>
    /// Rewarded adverts: the only way to double without paying (a second DOUBLE of the time away in a day, after the
    /// free one; a Tide Boost), a few times a day. The rules are here; showing the advert is
    /// <c>Game/Platform/Ads</c>.
    /// </summary>
    public sealed partial class Sim
    {
        public const int AdsPerDay = 6;

        public int AdsLeft { get { return State.adDay == Today ? Math.Max(0, AdsPerDay - State.adCount) : AdsPerDay; } }

        double BoostHour { get { return BoostLasts * (Logbook("tide") ? 1.5 : 1); } }

        /// <summary>Whether this reward can be offered right now (something to gain, and adverts left today).</summary>
        public bool AdOffered(AdReward reward)
        {
            if (AdsLeft <= 0) return false;
            switch (reward)
            {
                case AdReward.Double: return !DoubleFree;                       // the free one of the day comes first
                case AdReward.Wrench: return Unlocked("wear") && State.wrenches < AdWrenchBelow;      // for someone running short, not for hoarding
                case AdReward.Boost: return BoostLeft + BoostHour <= BoostCap + 1;
            }
            return false;
        }

        void CountAd()
        {
            if (State.adDay != Today) { State.adDay = Today; State.adCount = 0; }
            State.adCount++;
            Touch();
        }

        /// <summary>Collects the time away twice over for a watched advert. False if it wasn't on offer.</summary>
        public bool CollectAwayByAd(double seconds)
        {
            if (!AdOffered(AdReward.Double)) return false;
            CollectAway(seconds, 2);
            CountAd();
            return true;
        }

        /// <summary>A Tide Boost for a watched advert. False if it wasn't on offer.</summary>
        public bool BoostByAd()
        {
            if (!AdOffered(AdReward.Boost)) return false;
            State.boostUntil = Math.Max(State.boostUntil, RealNow()) + BoostHour;
            CountAd();
            return true;
        }
    }
}

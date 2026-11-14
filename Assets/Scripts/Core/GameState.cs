using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    [Serializable]
    public sealed class BerthState
    {
        public int level = 1;
        public bool skipper;            // sails by itself
        public bool sailing;            // out on a trip
        public float trip;              // seconds left of it
        public double crate;            // fish waiting on the quay for the haulers (units)
        public int bait;                // index into Sim.Baits
    }

    /// <summary>A hauler or a truck on its round.</summary>
    [Serializable]
    public sealed class Carrier
    {
        public bool busy;
        public float t;                 // seconds into the round
        public double load, value;      // units carried and what they will sell for (trucks)
        public bool turned;             // the round's halfway work is done
    }

    [Serializable]
    public sealed class HarbourState
    {
        public string id;
        public double cash;
        public double earned;           // since the last renovation
        public List<BerthState> berths = new List<BerthState>();
        public int dock = 1, hall = 1, market = 1;
        public bool dockSkipper, marketSkipper;
        public List<Carrier> haulers = new List<Carrier>();
        public List<Carrier> trucks = new List<Carrier>();
        public double raw;              // fish on the hall floor, not yet worked
        public double done, doneValue;  // worked fish waiting for a truck, and what they are worth
        public int goods;               // which line the hall runs (index into Content.Goods)
        public int goodsOwned = 1;      // bit per line bought
        public int renovations;
        public int stars;               // milestones reached here
        public List<int> works = new List<int>();      // the level of each of Sim.Works
    }

    [Serializable]
    public sealed class SpeciesEntry { public string id; public int count; public float best; }

    /// <summary>The save: plain fields and lists so JsonUtility can write it. Every class it holds is [Serializable].</summary>
    [Serializable]
    public sealed class GameState
    {
        public int version = 1;
        public uint seed = 2463534242;
        public double savedAt;          // real time of the last save, seconds
        public double played;           // seconds in the game
        public string playerName = "";
        public int gems;
        public double lifetime;         // everything ever earned, in lake money (each harbour's cash divided by its scale)
        public int at;                  // the harbour on screen
        public List<HarbourState> harbours = new List<HarbourState>();
        public List<SpeciesEntry> species = new List<SpeciesEntry>();
        public long trips, fish, taps, hauls, sales;
        public List<string> achieved = new List<string>();
        public int goalIndex;
        public int tutorialStep;
        public bool freeSkipperUsed;
        public int renown, renownPaid, renovations;
        public double boostUntil;       // real time the Tide Boost runs out
        public List<double> boostUsedAt = new List<double>();
        public long giftDay = -1; public int giftStreak;
        /// <summary>Seconds added to the clock for day and night, so a new game starts in the morning.</summary>
        public double dayShift;
        public long taskDay = -1; public int taskClaimed; public List<double> taskBase = new List<double>();
        public long levelsBought, voyagesDone;
        public List<Voyage> voyages = new List<Voyage>();
        public List<CrewState> crew = new List<CrewState>();
        public List<string> duty = new List<string> { "", "", "", "" };     // who stands at each stage (Stage order)
        public double chestAt;          // real time the free crew chest is ready
        public int chestsOpened, chestsSinceEpic;
        public List<string> trophies = new List<string>();
        public int goalKind, goalBerth; public double goalTarget; public long goalFrom;
        public double bonusIn, bonusLeft; public int bonusSeed; public long bonusTaken;
        public long doubleDay = -1;
        public bool starterOffered;
        public long skillsUsed, worksBuilt;
        public int notes; public List<string> research = new List<string>();
        public List<Order> orders = new List<Order>(); public double orderAt; public long ordersDone;
        public long firstDay = -1;      // the day the game was first played
        public List<string> seenFeatures = new List<string>();
        public long seasonIndex = -1, seasonPoints, seasonPremiumIndex = -1; public int seasonFree, seasonPaid;
        public long derbyIndex = -1, derbyScore, derbyLastIndex = -1, derbyLastScore, derbyPrizeFor = -1; public int derbyGoals, derbyBest;

        // The shop.
        public List<string> owned = new List<string>();
        public List<string> transactions = new List<string>();
        public List<PendingPurchase> pending = new List<PendingPurchase>();

        public static GameState New()
        {
            var s = new GameState();
            s.harbours.Add(NewHarbour(0));
            return s;
        }

        public static HarbourState NewHarbour(int index)
        {
            var def = Content.Harbours[index];
            var h = new HarbourState { id = def.Id, cash = Content.StartCash * def.Scale };
            h.berths.Add(new BerthState());
            h.haulers.Add(new Carrier());
            h.trucks.Add(new Carrier());
            return h;
        }

        /// <summary>Makes an older or damaged save playable.</summary>
        public void Repair()
        {
            if (harbours == null) harbours = new List<HarbourState>();
            if (harbours.Count == 0) harbours.Add(NewHarbour(0));
            if (species == null) species = new List<SpeciesEntry>();
            if (owned == null) owned = new List<string>();
            if (transactions == null) transactions = new List<string>();
            if (pending == null) pending = new List<PendingPurchase>();
            if (playerName == null) playerName = "";
            if (achieved == null) achieved = new List<string>();
            if (boostUsedAt == null) boostUsedAt = new List<double>();
            if (taskBase == null) taskBase = new List<double>();
            if (voyages == null) voyages = new List<Voyage>();
            if (crew == null) crew = new List<CrewState>();
            if (trophies == null) trophies = new List<string>();
            if (seenFeatures == null) seenFeatures = new List<string>();
            if (duty == null) duty = new List<string>();
            while (duty.Count < 4) duty.Add("");
            if (seed == 0) seed = 2463534242;
            at = Math.Max(0, Math.Min(at, harbours.Count - 1));
            foreach (var h in harbours)
            {
                if (h.berths == null) h.berths = new List<BerthState>();
                if (h.berths.Count == 0) h.berths.Add(new BerthState());
                if (h.haulers == null) h.haulers = new List<Carrier>();
                if (h.trucks == null) h.trucks = new List<Carrier>();
                h.dock = Math.Max(1, h.dock); h.hall = Math.Max(1, h.hall); h.market = Math.Max(1, h.market);
                h.goodsOwned |= 1;
                foreach (var b in h.berths) b.level = Math.Max(1, Math.Min(Content.MaxLevel, b.level));
            }
        }
    }
}

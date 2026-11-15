using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    public sealed class SeasonReward { public int Gems; public double WarpHours, BoostHours; public bool Chest; public string Text; }

    /// <summary>
    /// The longer rhythms, all on the device's clock with no server: the trophy hall (rare fish mounted for a lasting
    /// income bonus), the four-week season pass (thirty tiers, a free and a premium reward on each) and the weekend
    /// derby (points for every fish landed from Friday to Sunday, milestones, and a ranking against 49 invented
    /// rivals whose scores are worked out from the date).
    /// </summary>
    public sealed partial class Sim
    {
        // ---- the trophy hall ----

        public static double TrophyBonus(Rarity r) { return r == Rarity.Rare ? 0.01 : r == Rarity.Epic ? 0.03 : r == Rarity.Legendary ? 0.08 : 0; }
        public bool Mounted(string species) { return State.trophies.Contains(species); }
        public bool CanMount(SpeciesDef sp) { return sp.Rarity >= Rarity.Rare && Entry(sp.Id) != null && !Mounted(sp.Id); }

        /// <summary>Hangs a rare catch in the trophy hall: a little more income everywhere, for good.</summary>
        public bool Mount(string species)
        {
            var sp = Book.Get(species);
            if (sp == null || !CanMount(sp)) return false;
            State.trophies.Add(species);
            Touch();
            return true;
        }

        public double TrophyMult { get { double m = 1; foreach (var id in State.trophies) { var sp = Book.Get(id); if (sp != null) m += TrophyBonus(sp.Rarity); } return m; } }
        public int TrophiesWaiting { get { int n = 0; foreach (var e in State.species) { var sp = Book.Get(e.id); if (sp != null && CanMount(sp)) n++; } return n; } }

        // ---- the season pass ----

        public const int SeasonTiers = 30;
        public long SeasonIndex { get { return (long)Math.Floor(RealNow() / SeasonLength); } }
        public static int TierNeed(int tier) { return 120 + tier * 30; }     // points to go from this tier to the next

        void RollSeason()
        {
            if (State.seasonIndex == SeasonIndex) return;
            State.seasonIndex = SeasonIndex; State.seasonPoints = 0; State.seasonFree = 0; State.seasonPaid = 0;
        }

        /// <summary>Season points come from playing: a level bought, a new species, a task, a voyage, a derby milestone.</summary>
        public void AddSeasonPoints(int n) { RollSeason(); State.seasonPoints += n; }

        public int SeasonTier
        {
            get
            {
                RollSeason();
                int t = 0; long left = State.seasonPoints;
                while (t < SeasonTiers && left >= TierNeed(t)) { left -= TierNeed(t); t++; }
                return t;
            }
        }
        /// <summary>Points into the present tier, and what it needs.</summary>
        public void SeasonProgress(out long have, out int need)
        {
            RollSeason();
            int t = 0; long left = State.seasonPoints;
            while (t < SeasonTiers && left >= TierNeed(t)) { left -= TierNeed(t); t++; }
            have = left; need = t >= SeasonTiers ? 0 : TierNeed(t);
        }

        public bool SeasonPremium { get { RollSeason(); return State.seasonPremiumIndex == SeasonIndex; } }

        public static SeasonReward SeasonRewardFor(int tier, bool premium)
        {
            int n = tier + 1;
            if (!premium)
            {
                if (n % 5 == 0) return new SeasonReward { Gems = 20, Text = "20 gems" };
                if (n % 2 == 0) return new SeasonReward { WarpHours = 0.5, Text = "Half an hour's earnings" };
                return new SeasonReward { Gems = 5, Text = "5 gems" };
            }
            if (n % 10 == 0) return new SeasonReward { Gems = 150, Text = "150 gems" };
            if (n % 5 == 0) return new SeasonReward { Chest = true, Gems = 30, Text = "A crew chest and 30 gems" };
            if (n % 3 == 0) return new SeasonReward { BoostHours = 2, Text = "Two hours of Tide Boost" };
            if (n % 2 == 0) return new SeasonReward { WarpHours = 2, Text = "Two hours' earnings" };
            return new SeasonReward { Gems = 20, Text = "20 gems" };
        }

        public bool SeasonClaimed(int tier, bool premium) { RollSeason(); return ((premium ? State.seasonPaid : State.seasonFree) & (1 << tier)) != 0; }
        public bool SeasonCanClaim(int tier, bool premium) { return tier < SeasonTier && !SeasonClaimed(tier, premium) && (!premium || SeasonPremium); }

        public SeasonReward ClaimSeason(int tier, bool premium)
        {
            if (tier < 0 || tier >= SeasonTiers || !SeasonCanClaim(tier, premium)) return null;
            var r = SeasonRewardFor(tier, premium);
            if (premium) State.seasonPaid |= 1 << tier; else State.seasonFree |= 1 << tier;
            Give(r);
            Touch();
            return r;
        }

        void Give(SeasonReward r)
        {
            State.gems += r.Gems;
            if (r.WarpHours > 0) Pay(H, Math.Max(IdleRate(H) / (Boosted ? BoostMult : 1), FullRate(H) * 0.25) * r.WarpHours * 3600);
            if (r.BoostHours > 0) State.boostUntil = Math.Max(State.boostUntil, RealNow()) + r.BoostHours * 3600;
            if (r.Chest) { State.gems += ChestGems; OpenChest(true); }
        }

        public int SeasonWaiting { get { int n = 0; for (int t = 0; t < SeasonTier; t++) { if (SeasonCanClaim(t, false)) n++; if (SeasonCanClaim(t, true)) n++; } return n; } }

        // ---- the weekend derby ----

        public const int DerbyRivals = 49;
        public static readonly int[] DerbyGoals = { 200, 1000, 4000, 12000, 30000 };
        public static readonly int[] DerbyGoalGems = { 10, 15, 25, 40, 60 };
        public static readonly int[] DerbyPoints = { 1, 3, 10, 40, 200 };       // by rarity of the fish landed
        const double Week = 7 * 86400, DerbyLength = 3 * 86400;
        const double DerbyStartsAt = 86400;     // 1 January 1970 was a Thursday: the derby opens on Friday, local time

        double LocalNow { get { return RealNow() + UtcOffset; } }
        /// <summary>The number of the derby running now, or of the last one that ran.</summary>
        public long DerbyIndex { get { return (long)Math.Floor((LocalNow - DerbyStartsAt) / Week); } }
        double DerbyStart(long index) { return DerbyStartsAt + index * Week; }
        public bool DerbyOn { get { double t = LocalNow - DerbyStart(DerbyIndex); return t >= 0 && t < DerbyLength; } }
        public double DerbyLeft { get { return DerbyOn ? DerbyStart(DerbyIndex) + DerbyLength - LocalNow : 0; } }
        public double DerbyIn { get { return DerbyOn ? 0 : DerbyStart(DerbyIndex + 1) - LocalNow; } }

        void RollDerby()
        {
            if (State.derbyIndex == DerbyIndex) return;
            // Keep the last one's result until its prize is taken.
            if (State.derbyScore > 0 && State.derbyPrizeFor < State.derbyIndex) { State.derbyLastIndex = State.derbyIndex; State.derbyLastScore = State.derbyScore; }
            State.derbyIndex = DerbyIndex; State.derbyScore = 0; State.derbyGoals = 0; State.derbyKinds.Clear();
        }

        public long DerbyScore { get { RollDerby(); return State.derbyScore; } }

        /// <summary>A rival's score: where they will finish (between a few hundred and tens of thousands, fixed by
        /// the derby's number), times how far through the weekend it is, each at their own pace.</summary>
        public long RivalScore(long derby, int rival, double fraction)
        {
            uint h = Hash(derby * 131 + rival * 7919 + 17);
            double final = 300 * Math.Pow(180, (h % 10000) / 10000.0) * ScaleOf(derby);
            double pace = 0.7 + (h >> 16) % 100 / 100.0 * 0.8;
            return (long)(final * Math.Pow(Math.Max(0, Math.Min(1, fraction)), pace));
        }

        public static readonly string[] RivalNames =
        {
            "Old Barnaby", "Saltwater Sue", "Captain Hobbs", "Minnow Mike", "Lucky Lena", "The Heron", "Big Tuna Tom", "Pearl Diver", "Driftwood Dan", "Nettie Hook",
            "Stormy Pete", "Reel Rosa", "Kelp King", "Anchor Annie", "Gull Gary", "Tidepool Tess", "Barnacle Bill", "Cora Finn", "Harbour Hank", "Misty Maren",
            "Jolly Jib", "Squall Sam", "Bay Betty", "Marlin Max", "Sandy Shoals", "Fathom Fred", "Luna Tide", "Rusty Reel", "Wavey Wren", "Skipper Sol",
            "Briny Bea", "Dory Dave", "Coral Kate", "Trawler Ted", "Nora Knot", "Float Flo", "Lobster Lou", "Keel Kit", "Sprat Jack", "Foghorn Fay",
            "Harpoon Hal", "Shelly Sands", "Oyster Ollie", "Mona Moor", "Reef Rae", "Bosun Bo", "Windy Wade", "Cove Clara", "Pike Paddy",
        };

        /// <summary>The player's place among the fifty, for a derby and a score (1 is first).</summary>
        public int DerbyRank(long derby, long score, double fraction)
        {
            int rank = 1;
            for (int i = 0; i < DerbyRivals; i++) if (RivalScore(derby, i, fraction) > score) rank++;
            return rank;
        }
        public int DerbyRankNow { get { return DerbyRank(DerbyIndex, DerbyScore, 1 - DerbyLeft / DerbyLength); } }

        public bool DerbyGoalDone(int i) { return DerbyScore >= DerbyGoal(i); }
        public bool DerbyGoalClaimed(int i) { RollDerby(); return (State.derbyGoals & (1 << i)) != 0; }
        public bool ClaimDerbyGoal(int i)
        {
            if (i < 0 || i >= DerbyGoals.Length || !DerbyGoalDone(i) || DerbyGoalClaimed(i)) return false;
            State.derbyGoals |= 1 << i; State.gems += DerbyGoalGems[i]; AddSeasonPoints(40);
            Touch();
            return true;
        }

        /// <summary>Recruit tokens that come with a derby placing.</summary>
        public static int DerbyTokens(int rank) { return rank <= 3 ? 5 : rank <= 10 ? 3 : 1; }

        public static int DerbyPrize(int rank) { return rank <= 1 ? 100 : rank <= 3 ? 60 : rank <= 10 ? 30 : rank <= 25 ? 15 : 5; }

        /// <summary>A finished derby whose placing prize hasn't been taken: its number, or -1.</summary>
        public long DerbyPrizeWaiting
        {
            get
            {
                RollDerby();
                if (!DerbyOn && State.derbyScore > 0 && State.derbyPrizeFor < State.derbyIndex) return State.derbyIndex;
                if (State.derbyLastScore > 0 && State.derbyPrizeFor < State.derbyLastIndex) return State.derbyLastIndex;
                return -1;
            }
        }

        /// <summary>Takes the placing prize of the last finished derby. Returns the place (0: nothing to take).</summary>
        public int ClaimDerbyPrize()
        {
            long d = DerbyPrizeWaiting; if (d < 0) return 0;
            long score = d == State.derbyIndex ? State.derbyScore : State.derbyLastScore;
            int rank = DerbyRank(d, score, 1);
            State.gems += DerbyPrize(rank); State.tokens += DerbyTokens(rank); State.derbyPrizeFor = d; State.derbyLastScore = 0;
            if (rank < State.derbyBest || State.derbyBest == 0) State.derbyBest = rank;
            Touch();
            return rank;
        }
    }
}

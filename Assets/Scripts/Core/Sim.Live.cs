using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    [Serializable]
    public sealed class Voyage { public int kind; public double doneAt; }

    public sealed class VoyageDef { public string Name, Blurb; public double Seconds, CashHours; public int Gems; }
    public sealed class GiftDef { public string Name; public int Gems; public double WarpHours; }
    public sealed class TaskDef { public string Id, Text; public double Need; public int Gems; public Func<GameState, double> Count; }
    public sealed class LogbookDef { public string Id, Name, Blurb; public int Renown; }

    /// <summary>
    /// What brings a player back each day, all on the device's own clock: the seven-day gift, three daily tasks,
    /// voyages (a boat away for half an hour to eight hours, back with cash and gems) and the Skipper's Logbook
    /// (lasting perks that open as renown grows; nothing is spent).
    /// </summary>
    public sealed partial class Sim
    {
        // ---- the gift ----

        public static readonly GiftDef[] Gifts =
        {
            new GiftDef { Name = "10 gems", Gems = 10 },
            new GiftDef { Name = "Half an hour's earnings", WarpHours = 0.5 },
            new GiftDef { Name = "15 gems", Gems = 15 },
            new GiftDef { Name = "An hour's earnings", WarpHours = 1 },
            new GiftDef { Name = "25 gems", Gems = 25 },
            new GiftDef { Name = "Two hours' earnings", WarpHours = 2 },
            new GiftDef { Name = "60 gems", Gems = 60 },
        };

        public bool GiftReady { get { return State.giftDay != Today; } }
        /// <summary>Which of the seven gifts is next. It moves on with every day played and never starts again: a missed
        /// day costs nothing.</summary>
        public int GiftIndex { get { return State.giftStreak % Gifts.Length; } }
        public double GiftIn { get { return 86400 - ((RealNow() + UtcOffset) % 86400 + 86400) % 86400; } }

        public GiftDef ClaimGift()
        {
            if (!GiftReady) return null;
            int i = GiftIndex;
            var g = Gifts[i];
            State.gems += g.Gems;
            if (g.WarpHours > 0) Pay(H, Math.Max(IdleRate(H) / (Boosted ? BoostMult : 1), FullRate(H) * 0.25) * g.WarpHours * 3600);
            if (Unlocked("wear")) AddWrenches(2);        // two wrenches with every day's gift
            State.giftStreak = i + 1; State.giftDay = Today;
            Touch();
            return g;
        }

        // ---- daily tasks ----

        public static readonly TaskDef[] Tasks =
        {
            new TaskDef { Id = "trips", Text = "Bring 60 trips home", Need = 60, Gems = 5, Count = s => s.trips },
            new TaskDef { Id = "levels", Text = "Buy 25 levels", Need = 25, Gems = 5, Count = s => s.levelsBought },
            new TaskDef { Id = "sales", Text = "Sell 40 truckloads", Need = 40, Gems = 5, Count = s => s.sales },
        };

        void RollTasks()
        {
            if (State.taskDay == Today && State.taskBase.Count == Tasks.Length) return;
            State.taskDay = Today; State.taskClaimed = 0;
            State.taskBase.Clear();
            foreach (var t in Tasks) State.taskBase.Add(t.Count(State));
        }

        public double TaskHave(int i) { RollTasks(); return Math.Min(Tasks[i].Need, Tasks[i].Count(State) - State.taskBase[i]); }
        public bool TaskDone(int i) { return TaskHave(i) >= Tasks[i].Need; }
        public bool TaskClaimed(int i) { RollTasks(); return (State.taskClaimed & (1 << i)) != 0; }

        public bool ClaimTask(int i)
        {
            if (i < 0 || i >= Tasks.Length || !TaskDone(i) || TaskClaimed(i)) return false;
            State.taskClaimed |= 1 << i; State.gems += Tasks[i].Gems; AddSeasonPoints(30);
            Touch();
            return true;
        }

        // ---- voyages ----

        public static readonly VoyageDef[] Voyages =
        {
            new VoyageDef { Name = "Along the Shore", Blurb = "Half an hour out and back", Seconds = 30 * 60, CashHours = 0.4, Gems = 3 },
            new VoyageDef { Name = "Past the Point", Blurb = "Two hours to the far banks", Seconds = 2 * 3600, CashHours = 1.5, Gems = 8 },
            new VoyageDef { Name = "The Long Haul", Blurb = "Eight hours: a full hold", Seconds = 8 * 3600, CashHours = 5, Gems = 25 },
        };

        public int VoyageSlots { get { return 1 + (Owns("reelempire.fleet") ? 1 : 0); } }
        public double VoyageSeconds(VoyageDef v) { return v.Seconds * (Logbook("charts") ? 0.8 : 1); }
        public double VoyageLeft(Voyage v) { return Math.Max(0, v.doneAt - RealNow()); }
        /// <summary>What a voyage of this kind brings home, at the harbour's present pace.</summary>
        public double VoyageCash(VoyageDef v) { return Math.Max(RewardRate(H) / (Boosted ? BoostMult : 1) * 0.5, 2 * Def.Scale) * v.CashHours * 3600; }

        public bool StartVoyage(int kind)
        {
            if (kind < 0 || kind >= Voyages.Length || State.voyages.Count >= VoyageSlots) return false;
            State.voyages.Add(new Voyage { kind = kind, doneAt = RealNow() + VoyageSeconds(Voyages[kind]) });
            Touch();
            return true;
        }

        /// <summary>Takes a returned boat's cargo. Returns the cash (0 if it isn't back).</summary>
        public double CollectVoyage(int index)
        {
            if (index < 0 || index >= State.voyages.Count || VoyageLeft(State.voyages[index]) > 0) return 0;
            var def = Voyages[State.voyages[index].kind];
            double cash = VoyageCash(def);
            State.voyages.RemoveAt(index);
            Pay(H, cash); State.gems += def.Gems; State.voyagesDone++; AddSeasonPoints(25);
            Touch();
            return cash;
        }

        public bool VoyageBack { get { foreach (var v in State.voyages) if (VoyageLeft(v) <= 0) return true; return false; } }

        // ---- the logbook ----

        public static readonly LogbookDef[] LogbookPages =
        {
            new LogbookDef { Id = "watch1", Renown = 25, Name = "Night Watch", Blurb = "The harbour earns for 1 more hour while you're away" },
            new LogbookDef { Id = "tide", Renown = 60, Name = "Tide Tables", Blurb = "A Tide Boost lasts 90 minutes" },
            new LogbookDef { Id = "lookout", Renown = 100, Name = "The Lookout", Blurb = "Crates drift into the harbour twice as often" },
            new LogbookDef { Id = "watch2", Renown = 150, Name = "Second Watch", Blurb = "1 more hour of away earnings" },
            new LogbookDef { Id = "charts", Renown = 300, Name = "Better Charts", Blurb = "Voyages take a fifth less time" },
            new LogbookDef { Id = "watch3", Renown = 600, Name = "Dog Watch", Blurb = "2 more hours of away earnings" },
            new LogbookDef { Id = "yard", Renown = 1200, Name = "Friends at the Yard", Blurb = "New docks cost 15% less" },
            new LogbookDef { Id = "watch4", Renown = 2500, Name = "Harbour Lights", Blurb = "2 more hours of away earnings" },
            new LogbookDef { Id = "trade", Renown = 5000, Name = "Bulk Orders", Blurb = "Every level costs 10% less" },
        };

        public bool Logbook(string id) { var p = Array.Find(LogbookPages, x => x.Id == id); return p != null && State.renown >= p.Renown; }
        double LogbookAwayHours { get { return (Logbook("watch1") ? 1 : 0) + (Logbook("watch2") ? 1 : 0) + (Logbook("watch3") ? 2 : 0) + (Logbook("watch4") ? 2 : 0); } }

        /// <summary>Whether something on the TODAY page is waiting to be taken.</summary>
        public bool TodayWaiting
        {
            get
            {
                // Only what the page can show: the season and the derby count once they have opened.
                if (GiftReady || VoyageBack) return true;
                if (Unlocked("orders") && OrdersReady > 0) return true;
                if (Unlocked("season") && SeasonWaiting > 0) return true;
                if (Unlocked("derby"))
                {
                    if (DerbyPrizeWaiting >= 0) return true;
                    for (int i = 0; i < DerbyGoals.Length; i++) if (DerbyGoalDone(i) && !DerbyGoalClaimed(i)) return true;
                }
                for (int i = 0; i < Tasks.Length; i++) if (TaskDone(i) && !TaskClaimed(i)) return true;
                return false;
            }
        }
    }
}

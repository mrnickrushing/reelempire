using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    public class SimTests
    {
        [Test]
        public void ATappedBoatBringsFishHomeAndTheChainSellsThem()
        {
            var s = TestGame.New();
            double start = s.H.cash;
            Assert.That(s.TapBerth(0), Is.True);
            Assert.That(s.TapBerth(0), Is.False, "it is out already");
            s.Tick(Content.Berths[0].Trip + 0.5);
            Assert.That(s.H.berths[0].crate, Is.GreaterThan(0));
            Assert.That(s.TapDock(), Is.True);
            s.Tick(Content.HaulRound + 0.5);
            Assert.That(s.H.raw, Is.GreaterThan(0));
            Assert.That(s.TapMarket(), Is.True);
            s.Tick(Content.TruckRound + 0.5);
            Assert.That(s.H.cash, Is.GreaterThan(start));
            Assert.That(s.State.sales, Is.EqualTo(1));
        }

        [Test]
        public void NothingMovesWithoutATapOrASkipper()
        {
            var s = TestGame.New();
            s.Tick(120);
            Assert.That(s.State.trips, Is.EqualTo(0));
            Assert.That(s.IdleRate(s.H), Is.EqualTo(0));
        }

        [Test]
        public void TheFirstSkipperIsFreeAndTheBoatThenSailsByItself()
        {
            var s = TestGame.New();
            Assert.That(s.SkipperCost(s.H, Stage.Berth, 0), Is.EqualTo(0));
            Assert.That(s.HireSkipper(Stage.Berth, 0), Is.True);
            Assert.That(s.SkipperCost(s.H, Stage.Dock), Is.GreaterThan(0), "only the first");
            s.Tick(30);
            Assert.That(s.State.trips, Is.GreaterThan(3));
        }

        [Test]
        public void ACrateOnlyHoldsSoMuch()
        {
            var s = TestGame.New();
            s.HireSkipper(Stage.Berth, 0);
            s.Tick(600);
            Assert.That(s.H.berths[0].crate, Is.LessThanOrEqualTo(s.DeckOf(s.H, 0) * 60), "rare fish can overfill a little, never without end");
            long trips = s.State.trips;
            s.Tick(60);
            Assert.That(s.State.trips, Is.EqualTo(trips), "the boat waits while the crate is full");
        }

        [Test]
        public void BulkCostIsTheSumOfTheLevels()
        {
            var s = TestGame.New();
            var h = s.H;
            double ten = s.LevelCost(h, Stage.Berth, 0, 10), sum = 0;
            for (int i = 0; i < 10; i++) { sum += s.LevelCost(h, Stage.Berth, 0); h.berths[0].level++; }
            Assert.That(ten, Is.EqualTo(sum).Within(sum * 1e-9));
        }

        [Test]
        public void AffordableIsExactlyWhatTheCashBuys()
        {
            var s = TestGame.New();
            var h = s.H;
            foreach (double cash in new[] { 0.0, 3.9, 4, 50, 1234, 9e6 })
            {
                h.cash = cash;
                int n = s.Affordable(h, Stage.Berth, 0);
                if (n > 0) Assert.That(s.LevelCost(h, Stage.Berth, 0, n), Is.LessThanOrEqualTo(cash * (1 + 1e-9)));
                if (n < Content.MaxLevel - 1) Assert.That(s.LevelCost(h, Stage.Berth, 0, n + 1), Is.GreaterThan(cash));
            }
        }

        [Test]
        public void MilestonesDoubleTheCatchOrTheSpeed()
        {
            var s = TestGame.New();
            var h = s.H;
            h.berths[0].level = 9; double c9 = s.CatchOf(h, 0);
            h.berths[0].level = 10; Assert.That(s.CatchOf(h, 0), Is.EqualTo(c9 / 9 * 10 * 2).Within(1e-6));
            float t = s.TripOf(h, 0);
            h.berths[0].level = 25; Assert.That(s.TripOf(h, 0), Is.EqualTo(t / 2).Within(1e-4));
        }

        [Test]
        public void UpgradingPastAMilestoneGivesAStarAndCrew()
        {
            var s = TestGame.New();
            s.H.cash = 1e9;
            int stars = s.H.stars; int seen = 0;
            s.MilestoneReached += (st, b, lv) => seen++;
            Assert.That(s.Upgrade(Stage.Dock, 0, 9), Is.True);
            Assert.That(s.H.dock, Is.EqualTo(10));
            Assert.That(s.H.stars, Is.EqualTo(stars + 1));
            Assert.That(s.H.haulers.Count, Is.EqualTo(2));
            Assert.That(seen, Is.EqualTo(1));
        }

        [Test]
        public void TheHallMakesFishWorthMoreAndTheRestSellsFresh()
        {
            var s = TestGame.New();
            var h = s.H; h.cash = 1e6;
            Assert.That(s.BuyGoods(2), Is.False, "lines are bought in order");
            Assert.That(s.BuyGoods(1), Is.True);
            Assert.That(h.goods, Is.EqualTo(1));
            h.raw = 1000;
            s.Tick(1);
            Assert.That(h.done, Is.GreaterThan(0));
            Assert.That(h.doneValue / h.done, Is.EqualTo(s.GoodsValue(h, 1)).Within(1e-9));
            Assert.That(s.SetGoods(0), Is.True);
            Assert.That(s.SetGoods(3), Is.False);
        }

        [Test]
        public void TheHarbourEarnsWhileAwayUpToItsCap()
        {
            var s = TestGame.New();
            var h = s.H; h.cash = 1e7;
            s.HireSkipper(Stage.Berth, 0); s.HireSkipper(Stage.Dock); s.HireSkipper(Stage.Market);
            double rate = s.IdleRate(h);
            Assert.That(rate, Is.GreaterThan(0));
            Assert.That(s.AwayEarnings(30), Is.EqualTo(0), "not for half a minute");
            Assert.That(s.AwayEarnings(3600), Is.EqualTo(rate * 3600).Within(1e-6));
            Assert.That(s.AwayEarnings(86400), Is.EqualTo(rate * s.AwayCap).Within(1e-6));
            double before = h.cash;
            s.CollectAway(3600, 2);
            Assert.That(h.cash - before, Is.EqualTo(rate * 7200).Within(1e-6));
        }

        [Test]
        public void WhatTheGameEarnsLiveMatchesItsIdleRate()
        {
            var s = TestGame.New();
            var h = s.H; h.cash = 1e7;
            s.HireSkipper(Stage.Berth, 0); s.HireSkipper(Stage.Dock); s.HireSkipper(Stage.Market);
            s.Upgrade(Stage.Berth, 0, 20); s.Upgrade(Stage.Dock, 0, 12); s.Upgrade(Stage.Market, 0, 12);
            s.Tick(120);
            double before = h.earned;
            s.Tick(1800);
            double live = (h.earned - before) / 1800, idle = s.IdleRate(h);
            Assert.That(live, Is.EqualTo(idle).Within(idle * 0.2), "live " + live + " against idle " + idle);
        }

        [Test]
        public void EveryHarbourHasItsSixteenSpeciesAndOneOfEachRarityBitesAnyTime()
        {
            foreach (var d in Content.Harbours)
            {
                var all = Book.In(d.Id);
                Assert.That(all.Count, Is.EqualTo(16), d.Id);
                Assert.That(all.Exists(x => x.Rarity == Rarity.Common && x.When == Bite.Any), d.Id);
            }
            var ids = new HashSet<string>();
            foreach (var sp in Book.Species) Assert.That(ids.Add(sp.Id), Is.True, sp.Id);
        }

        [Test]
        public void AFirstCatchPaysGemsOnce()
        {
            var s = TestGame.New();
            s.HireSkipper(Stage.Berth, 0);
            s.Tick(4);
            Assert.That(s.SpeciesCaught, Is.EqualTo(1));
            Assert.That(s.State.gems, Is.GreaterThan(0));
        }

        [Test]
        public void TheGuidePaysAndMovesOn()
        {
            var s = TestGame.New();
            Assert.That(s.ClaimGoal(), Is.False);
            s.TapBerth(0); s.Tick(5);
            Assert.That(s.GoalDone, Is.True);
            double cash = s.H.cash;
            Assert.That(s.ClaimGoal(), Is.True);
            Assert.That(s.H.cash, Is.GreaterThan(cash));
            Assert.That(s.State.goalIndex, Is.EqualTo(1));
        }

        [Test]
        public void RemindersAreFewAndApart()
        {
            var s = TestGame.New();
            var plan = s.ReminderPlan();
            Assert.That(plan.Count, Is.InRange(1, 4));
            for (int i = 1; i < plan.Count; i++) Assert.That(plan[i].In - plan[i - 1].In, Is.GreaterThanOrEqualTo(600));
        }

        [Test]
        public void TheWeatherAndTheClockComeFromTheTime()
        {
            var s = TestGame.New(0);
            Assert.That(s.IsNight, Is.True, "dawn begins at 0");
            double t = Sim.DayLength * 0.4; s.RealNow = () => t;
            Assert.That(s.IsNight, Is.False);
            t = Sim.DayLength * 0.9;
            Assert.That(s.IsNight, Is.True);
            s.ForcedWeather = (int)Weather.Storm;
            Assert.That(s.Biting(Book.Get("lake.muskellunge")), Is.True);
            s.ForcedWeather = (int)Weather.Clear;
            Assert.That(s.Biting(Book.Get("lake.muskellunge")), Is.False);
        }
    }

    public class ProgressTests
    {
        static Sim Rich()
        {
            var s = TestGame.New();
            s.H.cash = 1e15;
            while (s.H.berths.Count < Content.Berths.Length) s.UnlockBerth();
            return s;
        }

        [Test]
        public void RenovatingStartsTheHarbourAgainAtDoubleIncomeAndPaysRenown()
        {
            var s = TestGame.New();
            Assert.That(s.Renovate(), Is.False);
            s = Rich(); s.State.lifetime = 1e12;
            double m = s.IncomeMult(s.H);
            Assert.That(s.RenovationBlocked(s.H), Is.Null);
            Assert.That(s.Renovate(), Is.True);
            Assert.That(s.H.berths.Count, Is.EqualTo(1));
            Assert.That(s.H.renovations, Is.EqualTo(1));
            Assert.That(s.State.renown, Is.EqualTo(1264));
            Assert.That(s.IncomeMult(s.H), Is.EqualTo(m * 2 * (1 + 1264 * Sim.RenownEach)).Within(1e-6));
            Assert.That(s.RenownWaiting, Is.EqualTo(0), "paid once");
        }

        [Test]
        public void TheNextWaterOpensAfterARenovationAndHasItsOwnCash()
        {
            var s = Rich();
            Assert.That(s.OpenNextHarbour(), Is.False, "not before a renovation");
            s.Renovate();
            s.H.cash = s.NextHarbourCost;
            Assert.That(s.OpenNextHarbour(), Is.True);
            Assert.That(s.State.harbours.Count, Is.EqualTo(2));
            Assert.That(s.H.id, Is.EqualTo("river"));
            Assert.That(s.H.cash, Is.EqualTo(Content.StartCash * 400));
            Assert.That(s.Travel(0), Is.True);
            Assert.That(s.H.id, Is.EqualTo("lake"));
            s.TapBerth(0); s.Tick(5);
            Assert.That(Book.Get(s.State.species[0].id).Harbour, Is.EqualTo("lake"));
        }

        [Test]
        public void TheTideBoostDoublesIncomeForAnHourAndRecharges()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            double m = s.IncomeMult(s.H);
            Assert.That(s.BoostReady, Is.EqualTo(3));
            Assert.That(s.UseBoost(), Is.True);
            Assert.That(s.IncomeMult(s.H), Is.EqualTo(m * 2));
            Assert.That(s.UseBoost(), Is.True); Assert.That(s.UseBoost(), Is.True);
            Assert.That(s.BoostLeft, Is.EqualTo(3 * 3600).Within(1));
            Assert.That(s.UseBoost(), Is.False, "three charges");
            t += 3 * 3600 + 1;
            Assert.That(s.Boosted, Is.False);
            Assert.That(s.BoostReady, Is.EqualTo(0));
            t += 3600;
            Assert.That(s.BoostReady, Is.EqualTo(3));
            Assert.That(s.UseBoost(), Is.True);
        }
    }

    public class FormatTests
    {
        [Test]
        public void MoneyReadsAsIdleGamesCount()
        {
            Assert.That(Format.Money(999), Is.EqualTo("999"));
            Assert.That(Format.Money(1234), Is.EqualTo("1.23K"));
            Assert.That(Format.Money(15.6e6), Is.EqualTo("15.6M"));
            Assert.That(Format.Money(2e12), Is.EqualTo("2T"));
            Assert.That(Format.Money(4.99e15), Is.EqualTo("4.99aa"));
            Assert.That(Format.Money(3e18), Is.EqualTo("3ab"));
            Assert.That(Format.Money(1e93), Is.EqualTo("1ba"));
        }
    }

    public class SaveTests
    {
        /// <summary>JsonUtility silently drops a field whose class isn't [Serializable]: walk the save and fail on one.</summary>
        [Test]
        public void EverythingInTheSaveIsSerializable()
        {
            var seen = new HashSet<Type>();
            Walk(typeof(GameState), seen);
        }

        static void Walk(Type t, HashSet<Type> seen)
        {
            if (t.IsPrimitive || t == typeof(string) || t.IsEnum || !seen.Add(t)) return;
            if (t.IsGenericType) { foreach (var a in t.GetGenericArguments()) Walk(a, seen); return; }
            if (t.IsArray) { Walk(t.GetElementType(), seen); return; }
            Assert.That(t.IsSerializable, Is.True, t.Name + " is in the save but not [Serializable]");
            foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Instance)) Walk(f.FieldType, seen);
        }

        [Test]
        public void ADamagedSaveIsRepaired()
        {
            var st = new GameState { harbours = null, species = null, owned = null, at = 7 };
            st.Repair();
            var s = new Sim(st);
            Assert.That(s.H.berths.Count, Is.EqualTo(1));
            s.TapBerth(0); s.Tick(10);
        }
    }

    public class StoreTests
    {
        [Test]
        public void APurchaseIsGrantedOncePerTransaction()
        {
            var s = TestGame.New();
            Assert.That(s.Grant("reelempire.gems.small", "t1"), Is.True);
            Assert.That(s.Grant("reelempire.gems.small", "t1"), Is.False);
            Assert.That(s.State.gems, Is.EqualTo(100));
        }

        [Test]
        public void TheLedgerDoublesIncomeAndComesBackOnRestoreWithoutGems()
        {
            var s = TestGame.New();
            double m = s.IncomeMult(s.H);
            Assert.That(s.RestoreOwned(new[] { "reelempire.ledger", "reelempire.starter", "reelempire.gems.small" }), Is.EqualTo(2));
            Assert.That(s.IncomeMult(s.H), Is.EqualTo(m * 2));
            Assert.That(s.State.gems, Is.EqualTo(0));
        }

        [Test]
        public void TheStarterPackAddsAHaulerAndATruckForGood()
        {
            var s = TestGame.New();
            s.Grant("reelempire.starter", "t");
            Assert.That(s.State.gems, Is.EqualTo(250));
            Assert.That(s.H.haulers.Count, Is.EqualTo(2));
            Assert.That(s.H.trucks.Count, Is.EqualTo(2));
        }

        [Test]
        public void ATimeWarpPaysIdleEarningsForGems()
        {
            var s = TestGame.New();
            var h = s.H; h.cash = 1e7; s.State.gems = 25;
            Assert.That(s.BuyWarp(0), Is.False, "nothing to warp while nothing runs by itself");
            s.HireSkipper(Stage.Berth, 0); s.HireSkipper(Stage.Dock); s.HireSkipper(Stage.Market);
            double before = h.cash, rate = s.IdleRate(h);
            Assert.That(s.BuyWarp(0), Is.True);
            Assert.That(h.cash - before, Is.EqualTo(rate * 3600).Within(1e-6));
            Assert.That(s.State.gems, Is.EqualTo(5));
            Assert.That(s.BuyWarp(1), Is.False);
        }

        [Test]
        public void NightCrewAddsFourHoursAway()
        {
            var s = TestGame.New();
            double cap = s.AwayCap;
            s.Grant("reelempire.nightcrew", "t");
            Assert.That(s.AwayCap, Is.EqualTo(cap + 4 * 3600));
        }

        [Test]
        public void APurchaseCutShortIsPaidOutLater()
        {
            var s = TestGame.New();
            s.BeginPurchase("reelempire.gems.medium");
            var got = s.ReconcilePurchases(new[] { new StoreReceipt("reelempire.gems.medium", "tx9", 1_700_000_050) });
            Assert.That(got.Count, Is.EqualTo(1));
            Assert.That(s.State.gems, Is.EqualTo(300));
            Assert.That(s.HasPendingPurchases, Is.False);
        }
    }
}

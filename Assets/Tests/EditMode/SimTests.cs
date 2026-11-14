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
            Assert.That(plan.Count, Is.InRange(1, 2));
            foreach (var r in plan) { double local = (1_700_000_000 + r.In) % 86400; Assert.That(local, Is.InRange(8 * 3600 - 1, 22 * 3600), "none at night"); }
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

    public class LiveTests
    {
        [Test]
        public void TheGiftComesOnceADayAndAMissedDayCostsNothing()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            Assert.That(s.GiftReady, Is.True);
            Assert.That(s.ClaimGift().Gems, Is.EqualTo(10));
            Assert.That(s.ClaimGift(), Is.Null, "once a day");
            t += 86400;
            Assert.That(s.GiftIndex, Is.EqualTo(1));
            s.ClaimGift();
            t += 86400;
            Assert.That(s.ClaimGift().Gems, Is.EqualTo(15));
            t += 3 * 86400;
            Assert.That(s.GiftIndex, Is.EqualTo(3), "a missed day costs nothing: the week carries on");
        }

        [Test]
        public void DailyTasksCountFromTheStartOfTheDay()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            s.H.cash = 1e6;
            Assert.That(s.TaskHave(1), Is.EqualTo(0));
            s.Upgrade(Stage.Berth, 0, 30);
            Assert.That(s.TaskDone(1), Is.True);
            int gems = s.State.gems;
            Assert.That(s.ClaimTask(1), Is.True);
            Assert.That(s.ClaimTask(1), Is.False);
            Assert.That(s.State.gems, Is.EqualTo(gems + 5));
            t += 86400;
            Assert.That(s.TaskHave(1), Is.EqualTo(0), "a new day");
            Assert.That(s.TaskClaimed(1), Is.False);
        }

        [Test]
        public void AVoyageComesBackWithCashAndGems()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            Assert.That(s.StartVoyage(0), Is.True);
            Assert.That(s.StartVoyage(1), Is.False, "one boat to spare");
            Assert.That(s.CollectVoyage(0), Is.EqualTo(0), "not back yet");
            t += 30 * 60 + 1;
            Assert.That(s.VoyageBack, Is.True);
            double cash = s.H.cash; int gems = s.State.gems;
            Assert.That(s.CollectVoyage(0), Is.GreaterThan(0));
            Assert.That(s.H.cash, Is.GreaterThan(cash));
            Assert.That(s.State.gems, Is.EqualTo(gems + 3));
            s.Grant("reelempire.fleet", "t");
            Assert.That(s.StartVoyage(0) && s.StartVoyage(2), Is.True, "two with the second boat");
        }

        [Test]
        public void TheLogbookOpensWithRenown()
        {
            var s = TestGame.New();
            double cap = s.AwayCap;
            Assert.That(s.Logbook("watch1"), Is.False);
            s.State.renown = 150;
            Assert.That(s.Logbook("watch1") && s.Logbook("tide") && s.Logbook("watch2"), Is.True);
            Assert.That(s.AwayCap, Is.EqualTo(cap + 2 * 3600));
            s.UseBoost();
            Assert.That(s.BoostLeft, Is.EqualTo(5400).Within(1));
        }
    }

    public class CrewTests
    {
        [Test]
        public void TheFreeChestComesEveryFourHoursAndTheFirstHandGoesOnDuty()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            var card = s.OpenChest();
            Assert.That(card, Is.Not.Null);
            Assert.That(s.OnDuty(card.Stage), Is.EqualTo(card));
            Assert.That(s.OpenChest(), Is.Null, "not twice");
            t += Sim.ChestEvery + 1;
            Assert.That(s.OpenChest(), Is.Not.Null);
            Assert.That(s.OpenChest(true), Is.Null, "no gems");
            s.State.gems = 40;
            Assert.That(s.OpenChest(true), Is.Not.Null);
            Assert.That(s.State.gems, Is.EqualTo(0));
        }

        [Test]
        public void TheTenthChestWithoutAnEpicHoldsOne()
        {
            var s = TestGame.New(); s.State.gems = 100000;
            int since = 0, worst = 0;
            for (int i = 0; i < 400; i++)
            {
                var c = s.OpenChest(true);
                if (c.Rarity >= Rarity.Epic) since = 0; else since++;
                worst = Math.Max(worst, since);
            }
            Assert.That(worst, Is.LessThan(Sim.ChestPity));
            Assert.That(s.State.crew.Count, Is.EqualTo(12), "400 chests find everyone");
        }

        [Test]
        public void ACardOnDutyRaisesItsStageAndCopiesRaiseItsLevel()
        {
            var s = TestGame.New();
            double dock = s.DockRate(s.H);
            s.State.crew.Add(new CrewState { id = "greta" });
            Assert.That(s.Assign(Stage.Berth, "greta"), Is.False, "her stage is the dock");
            Assert.That(s.Assign(Stage.Dock, "greta"), Is.True);
            Assert.That(s.DockRate(s.H), Is.EqualTo(dock * 1.2).Within(1e-9));
            s.Owned("greta").level = 5;
            Assert.That(s.DockRate(s.H), Is.EqualTo(dock * (1 + 0.2 * 2)).Within(1e-9));
            Assert.That(s.Assign(Stage.Dock, null), Is.True);
            Assert.That(s.DockRate(s.H), Is.EqualTo(dock).Within(1e-9));
        }

        [Test]
        public void TheOddsAddUpAndEveryRarityHasACard()
        {
            int sum = 0; foreach (int o in Sim.ChestOdds) sum += o;
            Assert.That(sum, Is.EqualTo(100));
            foreach (var r in new[] { Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary }) Assert.That(Array.Exists(Sim.Crew, c => c.Rarity == r), r.ToString());
        }
    }

    public class SeasonTests
    {
        [Test]
        public void ARareCatchCanBeMountedOnceForMoreIncome()
        {
            var s = TestGame.New();
            double m = s.IncomeMult(s.H);
            Assert.That(s.Mount("lake.northern_pike"), Is.False, "not caught yet");
            s.State.species.Add(new SpeciesEntry { id = "lake.northern_pike", count = 1, best = 9 });
            s.State.species.Add(new SpeciesEntry { id = "lake.bluegill", count = 1, best = 0.3f });
            Assert.That(s.TrophiesWaiting, Is.EqualTo(1));
            Assert.That(s.Mount("lake.bluegill"), Is.False, "too common for the wall");
            Assert.That(s.Mount("lake.northern_pike"), Is.True);
            Assert.That(s.Mount("lake.northern_pike"), Is.False);
            Assert.That(s.IncomeMult(s.H), Is.EqualTo(m * 1.01).Within(1e-9));
        }

        [Test]
        public void SeasonPointsRaiseTheTierAndRewardsAreClaimedOnce()
        {
            var s = TestGame.New();
            Assert.That(s.SeasonTier, Is.EqualTo(0));
            s.AddSeasonPoints(Sim.TierNeed(0) + Sim.TierNeed(1) + 5);
            Assert.That(s.SeasonTier, Is.EqualTo(2));
            int gems = s.State.gems;
            Assert.That(s.ClaimSeason(0, false).Gems, Is.EqualTo(5));
            Assert.That(s.ClaimSeason(0, false), Is.Null);
            Assert.That(s.ClaimSeason(2, false), Is.Null, "not reached");
            Assert.That(s.ClaimSeason(0, true), Is.Null, "premium not bought");
            s.Grant("reelempire.season", "tx");
            Assert.That(s.SeasonPremium, Is.True);
            Assert.That(s.ClaimSeason(0, true), Is.Not.Null);
            Assert.That(s.State.gems, Is.EqualTo(gems + 5 + 20));
        }

        [Test]
        public void ANewSeasonStartsFromNothingAndPremiumDoesNotCarryOver()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            s.Grant("reelempire.season", "tx"); s.AddSeasonPoints(5000);
            Assert.That(s.SeasonTier, Is.GreaterThan(5));
            t += Sim.SeasonLength;
            Assert.That(s.SeasonTier, Is.EqualTo(0));
            Assert.That(s.SeasonPremium, Is.False);
        }

        [Test]
        public void TheDerbyRunsFridayToSundayAndCountsFishLanded()
        {
            // 1970-01-02 was a Friday: a derby opens at 86400 and every 7 days after.
            double t = 86400 + 7 * 86400 * 2900 + 3600;       // an hour into a derby
            var s = TestGame.New(); s.RealNow = () => t;
            Assert.That(s.DerbyOn, Is.True);
            s.HireSkipper(Stage.Berth, 0); s.Tick(60);
            Assert.That(s.DerbyScore, Is.GreaterThan(0));
            long score = s.DerbyScore;
            t += 3 * 86400;                                   // Monday
            Assert.That(s.DerbyOn, Is.False);
            s.Tick(60);
            Assert.That(s.DerbyScore, Is.EqualTo(score), "nothing counts after Sunday");
            Assert.That(s.DerbyPrizeWaiting, Is.GreaterThanOrEqualTo(0));
            int gems = s.State.gems;
            int rank = s.ClaimDerbyPrize();
            Assert.That(rank, Is.InRange(1, 50));
            Assert.That(s.State.gems, Is.EqualTo(gems + Sim.DerbyPrize(rank)));
            Assert.That(s.ClaimDerbyPrize(), Is.EqualTo(0));
            t += 4 * 86400 + 60;                              // the next Friday
            Assert.That(s.DerbyOn, Is.True);
            Assert.That(s.DerbyScore, Is.EqualTo(0));
        }

        [Test]
        public void RivalsClimbThroughTheWeekendAndThereAreFortyNineNames()
        {
            var s = TestGame.New();
            Assert.That(Sim.RivalNames.Length, Is.EqualTo(Sim.DerbyRivals));
            for (int i = 0; i < Sim.DerbyRivals; i++)
            {
                Assert.That(s.RivalScore(7, i, 0), Is.EqualTo(0));
                Assert.That(s.RivalScore(7, i, 1), Is.GreaterThanOrEqualTo(s.RivalScore(7, i, 0.5)));
                Assert.That(s.RivalScore(7, i, 1), Is.InRange(299, 54001));
            }
            Assert.That(s.DerbyRank(7, 1_000_000, 1), Is.EqualTo(1));
            Assert.That(s.DerbyRank(7, 0, 1), Is.EqualTo(50));
        }
    }

    public class HookTests
    {
        [Test]
        public void TheGuideNeverRunsOutOfSteps()
        {
            var s = TestGame.New();
            s.H.cash = 1e30;
            s.State.goalIndex = Sim.Goals.Count - 1;
            for (int i = 0; i < 60; i++)
            {
                var g = s.Goal;
                Assert.That(g, Is.Not.Null, "step " + i);
                Assert.That(g.Need, Is.GreaterThan(0), g.Text);
                // Do whatever it asks, by the short road.
                s.HireSkipper(Stage.Berth, 0); s.HireSkipper(Stage.Dock); s.HireSkipper(Stage.Market);
                for (int k = 0; k < 400 && !s.GoalDone; k++)
                {
                    if (g.Id == "g.berth") s.Upgrade(Stage.Berth, g.PointBerth, 1);
                    else if (g.Id == "g.dock") s.Upgrade(Stage.Dock);
                    else if (g.Id == "g.market") s.Upgrade(Stage.Market);
                    else if (g.Id == "g.open") s.UnlockBerth();
                    else if (g.Id == "g.species") s.State.species.Add(new SpeciesEntry { id = Book.Species[s.State.species.Count].Id, count = 1 });
                    else if (g.Id == "g.trips") s.State.trips += 500;
                    else { s.State.goalIndex++; break; }
                }
                if (s.GoalDone) Assert.That(s.ClaimGoal(), Is.True, g.Text);
            }
            Assert.That(s.State.goalIndex, Is.GreaterThan(Sim.Goals.Count + 40));
        }

        [Test]
        public void ACrateDriftsInAfterAWhileAndPaysOnce()
        {
            var s = TestGame.New();
            int seen = 0; s.BonusAppeared += () => seen++;
            s.Tick(Sim.BonusFirst - 5);
            Assert.That(s.BonusAfloat, Is.False);
            s.Tick(10);
            Assert.That(s.BonusAfloat, Is.True);
            Assert.That(seen, Is.EqualTo(1));
            double cash = s.H.cash; int gems = s.State.gems; double boost = s.BoostLeft;
            var p = s.ClaimBonus();
            Assert.That(p, Is.Not.Null);
            Assert.That(s.H.cash > cash || s.State.gems > gems || s.BoostLeft > boost, Is.True);
            Assert.That(s.ClaimBonus(), Is.Null);
            s.Tick(Sim.BonusEveryMax + 1);
            Assert.That(s.BonusAfloat, Is.True, "another one comes");
            s.Tick(Sim.BonusStays + 1);
            Assert.That(s.BonusAfloat, Is.False, "it drifts away untaken");
        }

        [Test]
        public void TheDoubleIsFreeOnceADayThenCostsGems()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            var h = s.H; h.cash = 1e7;
            s.HireSkipper(Stage.Berth, 0); s.HireSkipper(Stage.Dock); s.HireSkipper(Stage.Market);
            double rate = s.IdleRate(h), before = h.cash;
            Assert.That(s.DoubleFree, Is.True);
            Assert.That(s.CollectAwayDoubled(3600), Is.True);
            Assert.That(h.cash - before, Is.EqualTo(rate * 7200).Within(1e-6));
            Assert.That(s.DoubleFree, Is.False);
            Assert.That(s.CollectAwayDoubled(3600), Is.False, "no gems");
            s.State.gems = 20;
            Assert.That(s.CollectAwayDoubled(3600), Is.True);
            Assert.That(s.State.gems, Is.EqualTo(0));
            t += 86400;
            Assert.That(s.DoubleFree, Is.True);
        }

        [Test]
        public void SomeoneBackAfterDaysIsWelcomed()
        {
            var s = TestGame.New();
            s.UseBoost(); s.UseBoost(); s.UseBoost();
            Assert.That(s.WelcomeHome(86400), Is.EqualTo(0));
            Assert.That(s.WelcomeHome(4 * 86400), Is.EqualTo(Sim.MissedGems));
            Assert.That(s.BoostReady, Is.EqualTo(3));
        }
    }

    public class OpeningUpTests
    {
        [Test]
        public void ANewGameShowsOneThingAtATime()
        {
            var s = TestGame.New();
            foreach (var f in Sim.Features) Assert.That(s.Unlocked(f), Is.False, f);
            s.TapBerth(0); s.Tick(5);
            Assert.That(s.Unlocked("book"), Is.True);
            Assert.That(s.Unlocked("crew"), Is.False);
            s.H.cash = 1e12;
            s.UnlockBerth(); s.UnlockBerth();
            Assert.That(s.Unlocked("crew"), Is.True);
            Assert.That(s.Unlocked("voyages"), Is.False);
            while (s.H.berths.Count < 8) s.UnlockBerth();
            foreach (var f in new[] { "voyages", "derby", "season", "harbours" }) Assert.That(s.Unlocked(f), Is.True, f);
            foreach (var f in Sim.Features) Assert.That(Sim.FeatureNews(f), Is.Not.Null, f);
        }

        [Test]
        public void ARenovatedHarbourKeepsEverythingOpen()
        {
            var s = TestGame.New();
            s.State.renovations = 1;
            foreach (var f in Sim.Features) if (f != "book") Assert.That(s.Unlocked(f), Is.True, f);
        }

        [Test]
        public void AwardsAndAFinishedPageRaiseIncomeAndStarsLengthenTheNight()
        {
            var s = TestGame.New();
            double m = s.IncomeMult(s.H), cap = s.AwayCap;
            s.State.achieved.Add("firstcatch"); s.State.achieved.Add("million");
            Assert.That(s.IncomeMult(s.H), Is.EqualTo(m * 1.02).Within(1e-9));
            foreach (var sp in Book.In("lake")) s.State.species.Add(new SpeciesEntry { id = sp.Id, count = 1 });
            Assert.That(s.IncomeMult(s.H), Is.EqualTo(m * 1.02 * 1.10).Within(1e-9));
            s.H.stars = 30;
            Assert.That(s.AwayCap, Is.EqualTo(cap + 3 * 3600));
            s.H.stars = 500;
            Assert.That(s.AwayCap, Is.EqualTo(cap + 5 * 3600), "to eight hours");
        }

        [Test]
        public void TheStarterPackIsNotOfferedOnTheFirstDay()
        {
            double t = 1_700_000_000;
            var s = TestGame.New(); s.RealNow = () => t;
            s.Tick(3000);
            Assert.That(s.StarterOfferDue, Is.False, "the first day");
            t += 86400;
            Assert.That(s.StarterOfferDue, Is.True);
            s.State.starterOffered = true;
            Assert.That(s.StarterOfferDue, Is.False);
        }

        [Test]
        public void TheWelcomeCardSaysWhatTheMoneyBuys()
        {
            var s = TestGame.New();
            Assert.That(s.Affords(0), Is.Null);
            Assert.That(s.Affords(s.BerthUnlockCost(s.H)), Does.Contain("Reed Landing"));
            Assert.That(s.H.cash, Is.EqualTo(Content.StartCash), "asking changes nothing");
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

        [Test]
        public void TodaysDotOnlyCountsWhatThePageShows()
        {
            var s = TestGame.New();
            s.ClaimGift();
            s.AddSeasonPoints(100000);                 // tiers earned long before the season page opens
            Assert.That(s.SeasonWaiting, Is.GreaterThan(0));
            Assert.That(s.Unlocked("season"), Is.False);
            Assert.That(s.TodayWaiting, Is.False);
        }

        [Test]
        public void ANewGameOpensInDaylight()
        {
            var s = TestGame.New(1_700_000_000 + 1300);      // late in the 24-minute day by the clock
            s.StartAtMorning();
            Assert.That(s.Night, Is.EqualTo(0));
            Assert.That(s.DayPhase, Is.EqualTo(0.08f).Within(0.001f));
        }
        static void Fish(Sim s, int trips)
        {
            for (int i = 0; i < trips; i++) { s.TapBerth(0); s.Tick(s.TripOf(s.H, 0) + 0.5); s.H.berths[0].crate = 0; }
        }

        [Test]
        public void GroundbaitBringsNothingRareAndSpinnersBringMore()
        {
            var s = TestGame.New();
            s.H.berths[0].level = 25;
            int rare = 0; s.Caught += (b, sp, kg, units, first) => { if (sp.Rarity >= Rarity.Rare) rare++; };
            Assert.That(s.SetBait(0, 1), Is.True);
            Fish(s, 600);
            Assert.That(rare, Is.EqualTo(0));
            Assert.That(s.SetBait(0, 2), Is.True);
            Fish(s, 600);
            Assert.That(rare, Is.GreaterThan(60));       // about 16% of trips, against 5% on worms
        }

        [Test]
        public void ABaitNeedsItsDockLevel()
        {
            var s = TestGame.New();
            Assert.That(s.SetBait(0, 1), Is.False);
            s.H.berths[0].level = 10;
            Assert.That(s.SetBait(0, 1), Is.True);
            Assert.That(s.BerthRate(s.H, 0), Is.GreaterThan(s.CatchOf(s.H, 0) * Content.RarityMean / s.TripOf(s.H, 0)));
            s.H.berths[0].level = 1;                      // as after a renovation: back to worms until it is earned again
            Assert.That(s.BaitOf(s.H, 0).Id, Is.EqualTo("worm"));
        }

        [Test]
        public void GlowBaitBringsNightFishByDay()
        {
            var s = TestGame.New(); s.StartAtMorning();
            s.H.berths[0].level = 50;
            int night = 0; s.Caught += (b, sp, kg, units, first) => { if (sp.When == Bite.Night) night++; };
            Fish(s, 400);
            Assert.That(night, Is.EqualTo(0));
            s.SetBait(0, 3);
            Fish(s, 400);
            Assert.That(night, Is.GreaterThan(0));
        }

        [Test]
        public void TheOrderBoardOpensWithThreeAndPaysWhenFilled()
        {
            var s = TestGame.New();
            while (s.H.berths.Count < 4) s.H.berths.Add(new BerthState());
            s.Tick(1);
            Assert.That(s.State.orders.Count, Is.EqualTo(Sim.OrderSlots));
            Assert.That(s.ClaimOrder(0), Is.EqualTo(0));              // not filled yet
            var o = s.State.orders[0]; o.have = o.need;
            Assert.That(s.OrdersReady, Is.EqualTo(1));
            double before = s.H.cash; int gems = s.State.gems;
            Assert.That(s.ClaimOrder(0), Is.GreaterThan(0));
            Assert.That(s.H.cash, Is.GreaterThan(before));
            Assert.That(s.State.gems, Is.GreaterThan(gems));
            Assert.That(s.State.orders.Count, Is.EqualTo(2));
        }

        [Test]
        public void FishLandedAndSoldCountTowardOrders()
        {
            var s = TestGame.New();
            while (s.H.berths.Count < 4) s.H.berths.Add(new BerthState());
            s.Tick(1);
            s.State.orders.Clear();
            s.State.orders.Add(new Order { kind = (int)OrderKind.Rarity, harbour = s.H.id, rarity = 0, need = 3, cashSeconds = 60 });
            s.State.orders.Add(new Order { kind = (int)OrderKind.Product, harbour = s.H.id, goods = 0, need = 5, cashSeconds = 60 });
            for (int i = 0; i < 4; i++) { s.TapBerth(0); s.Tick(s.TripOf(s.H, 0) + 0.5); }
            Assert.That(s.State.orders[0].have, Is.EqualTo(3));
            s.TapDock(); s.Tick(Content.HaulRound + 1); s.TapMarket(); s.Tick(Content.TruckRound + 1);
            Assert.That(s.State.orders[1].have, Is.GreaterThan(0));
        }
        [Test]
        public void ASkillRunsThenRestsAndNeverCountsWhileAway()
        {
            double now = 1_700_000_000;
            var s = new Sim(GameState.New()); s.RealNow = () => now;
            s.State.crew.Add(new CrewState { id = "finn" }); s.Assign(Stage.Berth, "finn");
            s.H.dockSkipper = s.H.marketSkipper = true; s.H.berths[0].skipper = true;
            double steady = s.BerthRate(s.H, 0), away = s.IdleRate(s.H);
            Assert.That(s.UseSkill(Stage.Berth), Is.True);
            Assert.That(s.BerthRate(s.H, 0), Is.EqualTo(steady * 2).Within(1e-9));
            Assert.That(s.IdleRate(s.H), Is.EqualTo(away).Within(1e-9));
            Assert.That(s.UseSkill(Stage.Berth), Is.False);                 // running
            now += 61;
            Assert.That(s.BerthRate(s.H, 0), Is.EqualTo(steady).Within(1e-9));
            Assert.That(s.UseSkill(Stage.Berth), Is.False);                 // resting
            now += 600;
            Assert.That(s.UseSkill(Stage.Berth), Is.True);
        }

        [Test]
        public void TheOldGroundsBringOnlyRareFish()
        {
            double now = 1_700_000_000;
            var s = new Sim(GameState.New()); s.RealNow = () => now;
            s.State.crew.Add(new CrewState { id = "brannock" }); s.Assign(Stage.Berth, "brannock");
            int common = 0; s.Caught += (b, sp, kg, units, first) => { if (sp.Rarity < Rarity.Rare) common++; };
            s.UseSkill(Stage.Berth);
            Fish(s, 12);
            Assert.That(common, Is.EqualTo(0));
        }
        [Test]
        public void WorksCostCashAndDoWhatTheySay()
        {
            var s = TestGame.New();
            double rate = s.BerthRate(s.H, 0); float trip = s.TripOf(s.H, 0); double deck = s.DeckOf(s.H, 0);
            Assert.That(s.BuildWork("netloft"), Is.False);                  // no money
            s.H.cash = 1e12;
            Assert.That(s.BuildWork("netloft"), Is.True);
            Assert.That(s.BerthRate(s.H, 0), Is.EqualTo(rate * 1.15).Within(1e-9));
            Assert.That(s.DeckOf(s.H, 0), Is.EqualTo(deck * 1.15 * 2).Within(1e-9));
            s.BuildWork("shipyard");
            Assert.That(s.TripOf(s.H, 0), Is.EqualTo(trip * 0.9f).Within(1e-5));
            double m = s.IncomeMult(s.H);
            s.BuildWork("auction");
            Assert.That(s.IncomeMult(s.H), Is.EqualTo(m * 1.1).Within(1e-9));
            double second = s.WorkCost(s.H, "netloft");
            Assert.That(second, Is.EqualTo(4e3 * 60).Within(1e-6));
            s.BuildWork("netloft"); s.BuildWork("netloft");
            Assert.That(s.BuildWork("netloft"), Is.False);                  // three levels and no more
            Assert.That(s.WorkTotal(s.H), Is.EqualTo(5));
        }

        [Test]
        public void TheLampBringsRareFishAndTheRateKnowsIt()
        {
            var s = TestGame.New();
            double rate = s.BerthRate(s.H, 0);
            s.H.cash = 1e15; s.BuildWork("lamp");
            var odds = s.OddsFor(s.H, Sim.Baits[0]);
            Assert.That(odds[2], Is.EqualTo(60));                           // 40 in a thousand, half as often again
            int sum = 0; foreach (int o in odds) sum += o;
            Assert.That(sum, Is.EqualTo(1000));
            Assert.That(s.BerthRate(s.H, 0), Is.GreaterThan(rate));
            Assert.That(s.OddsFor(s.H, Sim.Baits[1])[2], Is.EqualTo(0));    // groundbait still brings nothing rare
        }

        [Test]
        public void ResearchIsBoughtWithNotesInOrder()
        {
            var s = TestGame.New();
            Assert.That(s.DoResearch("hooks"), Is.False);
            s.State.notes = 9;
            Assert.That(s.DoResearch("finder"), Is.False);                  // Sharper Hooks comes first
            double rate = s.BerthRate(s.H, 0);
            Assert.That(s.DoResearch("hooks"), Is.True);
            Assert.That(s.BerthRate(s.H, 0), Is.EqualTo(rate * 1.1).Within(1e-9));
            Assert.That(s.DoResearch("finder"), Is.True);
            Assert.That(s.State.notes, Is.EqualTo(0));
            Assert.That(s.DoResearch("hooks"), Is.False);                   // once
            s.State.notes = 9; s.DoResearch("haggle"); s.DoResearch("regulars");
            Assert.That(s.OrderSlotCount, Is.EqualTo(4));
        }

        [Test]
        public void ANewSpeciesAndAFilledOrderEachTeachTheChartRoomSomething()
        {
            var s = TestGame.New();
            Fish(s, 1);
            Assert.That(s.State.notes, Is.EqualTo(1));
            s.State.orders.Add(new Order { kind = (int)OrderKind.Product, harbour = s.H.id, need = 1, have = 1, cashSeconds = 10 });
            s.ClaimOrder(0);
            Assert.That(s.State.notes, Is.EqualTo(2));
        }
        static FightState Play(Rarity r, uint seed, System.Func<Fight, bool> hold)
        {
            var f = new Fight(r, seed);
            for (int i = 0; i < 60 * 90 && f.State == FightState.Fighting; i++) f.Step(1f / 60, hold(f));
            return f.State;
        }

        [Test]
        public void AFishIsLandedByKeepingTheLineInTheBand()
        {
            // Someone who reels below the middle of the band and eases off above it lands fish of every size.
            foreach (Rarity r in System.Enum.GetValues(typeof(Rarity)))
                for (uint seed = 1; seed <= 20; seed++)
                    Assert.That(Play(r, seed, f => f.Tension < (f.Running ? 0.5f : 0.62f)), Is.EqualTo(FightState.Landed), r + " " + seed);
        }

        [Test]
        public void HoldingAllTheTimeSnapsTheLineAndNeverHoldingLosesTheFish()
        {
            Assert.That(Play(Rarity.Rare, 7, f => true), Is.EqualTo(FightState.Snapped));
            Assert.That(Play(Rarity.Rare, 7, f => false), Is.EqualTo(FightState.Escaped));
        }

        [Test]
        public void AHandCaughtFishGoesInTheBookAndPays()
        {
            var s = TestGame.New(); s.StartAtMorning();
            int better = 0;
            for (int i = 0; i < 400; i++) if (s.HandBite(1).Rarity >= Rarity.Rare) better++;
            Assert.That(better, Is.GreaterThan(60));                 // about a quarter, against a twentieth from the boats
            var sp = s.HandBite(0.5); double cash = s.H.cash;
            var c = s.LandByHand(sp);
            Assert.That(c.First, Is.True);
            Assert.That(s.Entry(sp.Id).count, Is.EqualTo(1));
            Assert.That(s.H.cash, Is.EqualTo(cash + c.Cash).Within(1e-9));
            Assert.That(s.State.handCaught, Is.EqualTo(1));
        }
        [Test]
        public void ASkillDoesNotInflateRewards()
        {
            double now = 1_700_000_000;
            var s = new Sim(GameState.New()); s.RealNow = () => now;
            foreach (var id in new[] { "finn", "pip", "nell", "dot" }) { s.State.crew.Add(new CrewState { id = id }); s.Assign(Sim.CrewById(id).Stage, id); }
            double reward = s.RewardRate(s.H);
            for (int st = 0; st < 4; st++) s.UseSkill((Stage)st);
            Assert.That(s.FullRate(s.H), Is.GreaterThan(reward));
            Assert.That(s.RewardRate(s.H), Is.EqualTo(reward).Within(1e-9));
        }

        [Test]
        public void NothingNewIsThrownInWhileTheGuideIsTeaching()
        {
            var s = TestGame.New();
            while (s.H.berths.Count < 3) s.H.berths.Add(new BerthState());
            s.H.berths[0].level = 10; s.State.goalIndex = 8;
            foreach (var f in new[] { "bait", "works", "angling" }) Assert.That(s.Unlocked(f), Is.False, f);
            s.State.goalIndex = 12;
            foreach (var f in new[] { "bait", "works", "angling" }) Assert.That(s.Unlocked(f), Is.True, f);
        }

        /// <summary>Three hours of someone using everything, in every harbour: nothing throws, nothing goes negative or not-a-number.</summary>
        [Test]
        public void ALongSessionWithEverythingInUseStaysSane()
        {
            double now = 1_700_000_000;
            var s = new Sim(GameState.New()); s.RealNow = () => now; s.StartAtMorning();
            s.State.goalIndex = 99; s.State.tutorialStep = 2;
            var rnd = new System.Random(42);
            for (int step = 0; step < 3 * 3600 / 2; step++)
            {
                now += 2; s.Tick(2); s.TickOthers(2);
                var h = s.H;
                s.TapBerth(rnd.Next(h.berths.Count)); s.TapDock(); s.TapMarket();
                switch (rnd.Next(14))
                {
                    case 0: s.Upgrade((Stage)rnd.Next(4), rnd.Next(h.berths.Count), 1 + rnd.Next(5)); break;
                    case 1: s.UnlockBerth(); break;
                    case 2: s.HireSkipper((Stage)rnd.Next(4), rnd.Next(h.berths.Count)); break;
                    case 3: s.SetBait(rnd.Next(h.berths.Count), rnd.Next(Sim.Baits.Length)); break;
                    case 4: for (int i = s.State.orders.Count - 1; i >= 0; i--) s.ClaimOrder(i); if (rnd.Next(30) == 0) s.DismissOrder(0); break;
                    case 5: s.UseSkill((Stage)rnd.Next(4)); break;
                    case 6: s.BuildWork(Sim.Works[rnd.Next(Sim.Works.Length)].Id); break;
                    case 7: s.DoResearch(Sim.Research[rnd.Next(Sim.Research.Length)].Id); break;
                    case 8: s.LandByHand(s.HandBite(rnd.NextDouble())); break;
                    case 9: s.OpenChest(); s.OpenChest(true); break;
                    case 10: s.BuyGoods(rnd.Next(Content.Goods.Length)); s.SetGoods(rnd.Next(Content.Goods.Length)); break;
                    case 11: if (s.Renovate()) { } else if (s.OpenNextHarbour()) { } break;
                    case 12: s.Travel(rnd.Next(s.State.harbours.Count)); break;
                    case 13: s.ClaimGoal(); s.ClaimBonus(); break;
                }
                if (step % 200 == 0) h.cash += s.FullRate(h) * 600 + 1e3 * s.DefOf(h).Scale;      // a welcome-back now and then
                foreach (var x in s.State.harbours)
                {
                    Assert.That(double.IsNaN(x.cash) || double.IsInfinity(x.cash) || x.cash < 0, Is.False, "cash at step " + step);
                    Assert.That(x.raw >= -1e-6 && x.done >= -1e-6, Is.True, "floor at step " + step);
                }
                Assert.That(s.State.gems >= 0 && s.State.notes >= 0, Is.True, "gems/notes at step " + step);
                Assert.That(s.State.orders.Count, Is.LessThanOrEqualTo(s.OrderSlotCount));
            }
            // The save goes out and comes back the same.
            Assert.That(s.State.species.Count, Is.GreaterThan(5));
            Assert.That(s.State.ordersDone, Is.GreaterThan(0));
        }
    }
}

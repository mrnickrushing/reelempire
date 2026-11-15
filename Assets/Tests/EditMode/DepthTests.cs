using System.Linq;
using NUnit.Framework;
using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    /// <summary>Fighting styles, customers and their favourites, fleet presets, the Boat Shed mending by itself,
    /// recruit tokens, derby formats and a renovated harbour's focus.</summary>
    public class DepthTests
    {
        [Test]
        public void EverySpeciesHasAFixedStyleAndTheyDiffer()
        {
            var styles = Book.Species.Select(Sim.StyleOf).ToList();
            Assert.That(styles.Distinct().Count(), Is.EqualTo(4), "all four styles turn up");
            foreach (var sp in Book.Species) Assert.AreEqual(Sim.StyleOf(sp), Sim.StyleOf(Book.Get(sp.Id)));
            Assert.IsTrue(Book.Species.Where(x => x.Rarity == Rarity.Legendary).All(x => Sim.StyleOf(x) == FightStyle.Stayer));
        }

        [Test]
        public void ADarterRunsOftenAndARunnerRarelyAndBothWarnFirst()
        {
            int Runs(FightStyle st)
            {
                var f = new Fight(Rarity.Rare, 5, st); int runs = 0; bool was = false, warned = false;
                for (int i = 0; i < 60 * 20; i++)
                {
                    f.Step(1f / 60, f.Tension < 0.55f);
                    if (f.State != FightState.Fighting) break;
                    if (f.Warning) warned = true;
                    if (f.Running && !was) { runs++; Assert.IsTrue(warned, st + ": a run comes after a warning"); warned = false; }
                    was = f.Running;
                }
                return runs;
            }
            Assert.Greater(Runs(FightStyle.Darter), Runs(FightStyle.Runner) * 2);
        }

        [Test]
        public void CustomersComeToKnowTheHarbourAndPayMore()
        {
            var s = TestGame.New();
            string who = "The Heron Inn";
            var o = new Order { kind = (int)OrderKind.Product, harbour = s.H.id, who = who, need = 1, have = 1, cashSeconds = 10 };
            double first = s.OrderCash(o);
            for (int i = 0; i < 5; i++) { s.State.orders.Add(new Order { kind = (int)OrderKind.Product, harbour = s.H.id, who = who, need = 1, have = 1, cashSeconds = 10 }); s.ClaimOrder(0); }
            Assert.AreEqual(2, s.TieLevel(who));
            StringAssert.Contains("menu board", s.TieNews, "a friend says so");
            Assert.AreEqual(first * 1.2, s.OrderCash(o), first * 1e-9);
            o.favourite = true;
            Assert.AreEqual(first * 1.45, s.OrderCash(o), first * 1e-9, "their favourite line pays more again");
        }

        [Test]
        public void ACustomerAsksForTheirFavouriteLineWhenTheHallHasIt()
        {
            var s = TestGame.New(); var h = s.H;
            h.goodsOwned = 0b111;      // fresh, cleaned, fillets
            int favs = 0;
            for (int i = 0; i < 200; i++)
            {
                s.State.orders.Clear(); s.State.orderAt = 0; s.State.ordersDone = 1;
                var o = typeof(Sim).GetMethod("NewOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(s, new object[] { (int)OrderKind.Product }) as Order;
                if (o.favourite) { favs++; Assert.AreEqual(s.FavouriteOf(o.who), o.goods); Assert.IsTrue(s.OwnsGoods(h, o.goods)); }
            }
            Assert.Greater(favs, 20);
        }

        [Test]
        public void APresetPutsBaitsAndCrewBack()
        {
            var s = TestGame.New(); var h = s.H; h.cash = 1e12;
            while (h.berths.Count < 3) s.UnlockBerth();
            foreach (var b in h.berths) b.level = 60;
            s.State.crew.Add(new CrewState { id = "finn" }); s.State.crew.Add(new CrewState { id = "odile" });
            s.Assign(Stage.Berth, "odile"); s.SetBait(0, 2); s.SetBait(1, 1);
            Assert.IsTrue(s.SavePreset(1));
            s.Assign(Stage.Berth, "finn"); s.SetBait(0, 0); s.SetBait(1, 0);
            Assert.AreEqual(3, s.UsePreset(1));
            Assert.AreEqual(2, h.berths[0].bait); Assert.AreEqual(1, h.berths[1].bait);
            Assert.AreEqual("odile", s.State.duty[(int)Stage.Berth]);
            Assert.AreEqual(0, s.UsePreset(0), "an empty preset changes nothing");
            StringAssert.Contains("1 on duty", s.PresetText(1));
        }

        [Test]
        public void TheBoatShedMendsAWornBoatByItselfFromLevelTwo()
        {
            var s = TestGame.New(); var h = s.H;
            s.State.goalIndex = 20; while (h.berths.Count < 6) { h.cash = 1e12; s.UnlockBerth(); }
            Assume.That(s.Unlocked("wear"));
            h.berths[2].wear = 0.9f;
            s.Tick(Sim.AutoMendEvery + 5);
            Assert.Greater(h.berths[2].wear, 0.5f, "no Boat Shed, no mending");
            while (h.works.Count < Sim.Works.Length) h.works.Add(0);
            h.works[System.Array.FindIndex(Sim.Works, w => w.Id == "shipyard")] = Sim.AutoMendShed;
            h.berths[2].wear = 0.9f;
            s.Tick(Sim.AutoMendEvery + 5);
            Assert.AreEqual(0, h.berths[2].wear, 1e-6);
            Assert.AreEqual(1, s.State.autoMended);
        }

        [Test]
        public void TokensRecruitAChosenCard()
        {
            var s = TestGame.New();
            s.State.chestAt = 0; s.OpenChest();
            Assert.AreEqual(1, s.State.tokens, "a chest brings a token");
            Assert.IsFalse(s.Recruit("augusta"));
            s.State.tokens = 40;
            Assert.IsTrue(s.Recruit("augusta"));
            Assert.IsNotNull(s.Owned("augusta"));
            Assert.AreEqual(0, s.State.tokens);
            s.State.tokens = 40; s.Recruit("augusta");
            Assert.AreEqual(1, s.Owned("augusta").copies);
            Assert.AreEqual(5, Sim.DerbyTokens(1));
        }

        [Test]
        public void TheDerbyChangesItsRulesEachWeek()
        {
            var s = TestGame.New();
            Assert.That(Enumerable.Range(0, 4).Select(i => Sim.FormatOf(i)).Distinct().Count(), Is.EqualTo(4));
            var rare = Book.In("lake").First(x => x.Rarity == Rarity.Rare);
            var common = Book.In("lake").First(x => x.Rarity == Rarity.Common);
            // Find a week of each format and check what counts.
            long baseIndex = s.DerbyIndex;
            switch (s.DerbyFormatNow)
            {
                case DerbyFormat.HandLine: Assert.AreEqual(0, s.DerbyPointsFor(rare, false)); Assert.AreEqual(30, s.DerbyPointsFor(rare, true)); break;
                case DerbyFormat.BigOnes: Assert.AreEqual(0, s.DerbyPointsFor(common, true)); Assert.AreEqual(20, s.DerbyPointsFor(rare, false)); break;
                case DerbyFormat.Variety: Assert.AreEqual(20, s.DerbyPointsFor(common, false)); break;
                default: Assert.AreEqual(10, s.DerbyPointsFor(rare, false)); break;
            }
            Assert.Less(s.RivalScore(1, 3, 1), s.RivalScore(4, 3, 1) * 10, "rivals scale with the format");
            Assert.Greater(s.DerbyGoal(0), 0);
        }

        [Test]
        public void AVarietyDerbyCountsEachKindOnce()
        {
            var s = TestGame.New();
            // Move the clock to a Friday of a Variety week.
            double t = 1_700_000_000;
            for (int d = 0; d < 60; d++)
            {
                var probe = TestGame.New(t + d * 86400.0);
                if (probe.DerbyOn && probe.DerbyFormatNow == DerbyFormat.Variety) { s = probe; break; }
            }
            Assume.That(s.DerbyFormatNow == DerbyFormat.Variety && s.DerbyOn);
            var sp = Book.In("lake").First(x => x.Rarity == Rarity.Common);
            s.LandByHand(sp); long once = s.DerbyScore;
            s.LandByHand(sp);
            Assert.AreEqual(once, s.DerbyScore);
            Assert.AreEqual(20, once);
        }

        [Test]
        public void ARenovatedHarbourPicksAFocusAndCanChangeItADayLater()
        {
            var s = TestGame.New(); var h = s.H;
            Assert.IsFalse(s.SetFocus("transport"), "not before a renovation");
            h.renovations = 1;
            double dock = s.DockRate(h);
            Assert.IsTrue(s.SetFocus("transport"));
            Assert.AreEqual(dock * 1.25, s.DockRate(h), dock * 1e-9);
            Assert.IsFalse(s.SetFocus("process"), "a change waits a day");
            double now = 1_700_000_000 + Sim.FocusChangeEvery + 1; s.RealNow = () => now;
            Assert.IsTrue(s.SetFocus("voyage"));
            Assert.AreEqual(s.DockRate(h), dock, dock * 1e-9);
            Assert.Less(s.VoyageSeconds(Sim.Voyages[0]), Sim.Voyages[0].Seconds);
        }
    }
}

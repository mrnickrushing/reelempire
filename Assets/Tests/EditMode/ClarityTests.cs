using ReelEmpire.Core;
using NUnit.Framework;

namespace ReelEmpire.Tests
{
    /// <summary>Upgrade verdicts for the whole harbour, what limits quantity and value, skill previews, a pinned
    /// species' plan, purchase previews and the starter pack's timing test.</summary>
    public class ClarityTests
    {
        static Sim Built()
        {
            var s = TestGame.New();
            var h = s.H;
            h.cash = 1e12;
            while (h.berths.Count < 3) s.UnlockBerth();
            foreach (var b in h.berths) b.level = 30;
            h.dock = 30; h.hall = 30; h.market = 30;
            return s;
        }

        [Test]
        public void AnUpgradeHeldBackByAnotherStageSaysSo()
        {
            var s = Built(); var h = s.H;
            h.market = 1;       // the market sells next to nothing: more boats can't help
            double gain;
            string v = s.UpgradeVerdict(Stage.Berth, 0, 10, out gain);
            Assert.Less(gain, 0.005);
            StringAssert.Contains("the market", v);
            v = s.UpgradeVerdict(Stage.Market, 0, 10, out gain);
            Assert.Greater(gain, 0.05, v);
            StringAssert.Contains("whole harbour", v);
        }

        [Test]
        public void TheHallSetsValueAndTheChainSetsQuantity()
        {
            var s = Built();
            StringAssert.Contains("VALUE", s.LimitNote(Stage.Hall));
            StringAssert.Contains("MANY", s.LimitNote(Stage.Market));
            s.H.goodsOwned |= 6; s.H.goods = 2;
            Assert.Greater(s.PaceWithGoods(2), s.PaceWithGoods(0), "fillets earn more than fresh fish");
            Assert.AreEqual(2, s.BestGoods);
        }

        [Test]
        public void ASkillPreviewSeesWhatHoldsItBack()
        {
            var s = Built(); var h = s.H;
            s.State.crew.Add(new CrewState { id = "finn" }); s.Assign(Stage.Berth, "finn");
            s.State.crew.Add(new CrewState { id = "dot" }); s.Assign(Stage.Market, "dot");
            h.market = 1;
            Stage limit;
            Assert.Less(s.SkillGain(Stage.Berth, out limit), 0.01, "the boats' skill can't get past the market");
            Assert.AreEqual(Stage.Market, limit);
            Assert.Greater(s.SkillGain(Stage.Market, out limit), 0.5, "the market's skill doubles the pace");
            Assert.AreEqual(0, s.SkillLift, 1e-9, "a preview leaves nothing running");
        }

        [Test]
        public void APinnedSpeciesHasAPlanAndIsLetGoWhenLanded()
        {
            var s = Built(); var h = s.H;
            var rare = Book.In("lake").Find(x => x.Rarity == Rarity.Rare && x.When == Bite.Any);
            Assert.IsTrue(s.Pin(rare.Id));
            var p = s.Plan(rare);
            Assert.AreEqual("Spinners", p.Bait);
            Assert.IsTrue(p.CanApply, p.Next);
            Assert.IsTrue(s.ApplyPlan(p));
            Assert.AreEqual(2, h.berths[p.Berth].bait);
            Assert.IsFalse(s.Plan(rare).CanApply, "already on");
            Assert.IsFalse(s.PinnedLanded);
            s.State.species.Add(new SpeciesEntry { id = rare.Id, count = 1 });
            Assert.IsTrue(s.PinnedLanded);

            var far = Book.In("reef")[0];
            StringAssert.Contains("Open", s.Plan(far).Next, "a water not open yet");
        }

        [Test]
        public void PurchasesShowWhatTheyWouldBeWorthHere()
        {
            var s = Built(); var h = s.H;
            h.dockSkipper = h.marketSkipper = true; foreach (var b in h.berths) b.skipper = true;
            StringAssert.Contains("a minute", s.PurchaseBenefit("reelempire.ledger"));
            StringAssert.Contains("more each night", s.PurchaseBenefit("reelempire.nightcrew"));
            StringAssert.Contains("Two voyages", s.PurchaseBenefit("reelempire.fleet"));
            StringAssert.Contains("porters", s.PurchaseBenefit("reelempire.starter"));
        }

        [Test]
        public void TheChainVariantOffersTheStarterOnceTheChainIsUnderstood()
        {
            var s = TestGame.New(); s.State.cohort = 80;
            Assert.AreEqual("chain", s.StarterVariant);
            s.Tick(1); s.State.played = 1300;
            Assert.IsFalse(s.StarterOfferDue, "no line run, no order filled");
            s.H.goodsOwned |= 2; s.State.ordersDone = 1;
            Assert.IsTrue(s.StarterOfferDue, "on the first day too, once it means something");
        }
    }
}

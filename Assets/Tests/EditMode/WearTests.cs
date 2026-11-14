using NUnit.Framework;
using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    public class WearTests
    {
        static Sim Veteran()
        {
            var s = TestGame.New();
            s.State.renovations = 1;        // everything is open to someone who has renovated
            return s;
        }

        [Test]
        public void AFreshHullLandsEverythingAndAWornOutOneSevenTenths()
        {
            Assert.AreEqual(1, Sim.WearFactor(0), 1e-9);
            Assert.AreEqual(1, Sim.WearFactor(Sim.WornAt), 1e-9);
            Assert.AreEqual(Sim.WornCatch, Sim.WearFactor(1), 1e-9);
            Assert.Greater(Sim.WearFactor(0.6f), Sim.WearFactor(0.9f));
        }

        [Test]
        public void NothingWearsUntilWearIsOpen()
        {
            var s = TestGame.New();
            Assert.IsFalse(s.Unlocked("wear"));
            s.H.berths[0].skipper = true;
            for (int i = 0; i < 2000; i++) s.Tick(1);
            Assert.AreEqual(0, s.H.berths[0].wear);
        }

        [Test]
        public void SailingWearsTheHullAndAWrenchMendsIt()
        {
            var s = Veteran();
            var b = s.H.berths[0]; b.skipper = true; s.H.dockSkipper = true; s.H.marketSkipper = true;
            for (int i = 0; i < 3000; i++) s.Tick(1);
            Assert.Greater(b.wear, 0.02f);
            int had = s.State.wrenches;
            Assert.IsTrue(s.Repair(0));
            Assert.AreEqual(0, b.wear);
            Assert.AreEqual(had - 1, s.State.wrenches);
            Assert.IsFalse(s.Repair(0), "nothing to mend");
        }

        [Test]
        public void AWornBoatEarnsLessAndNoWrenchNoRepair()
        {
            var s = Veteran();
            double fresh = s.BerthRate(s.H, 0);
            s.H.berths[0].wear = 1;
            Assert.AreEqual(fresh * Sim.WornCatch, s.BerthRate(s.H, 0), fresh * 1e-6);
            s.State.wrenches = 0;
            Assert.IsFalse(s.Repair(0));
            Assert.AreEqual(1, s.WornBoats);
        }

        [Test]
        public void TheBoatShedMakesHullsLast()
        {
            var s = Veteran();
            Assert.AreEqual(1, s.Durability(s.H), 1e-9);
        }

        [Test]
        public void AWrenchForAnAdvertOnlyWhenRunningShort()
        {
            var s = Veteran();
            s.State.wrenches = Sim.AdWrenchBelow;
            Assert.IsFalse(s.WrenchByAd());
            s.State.wrenches = 0;
            Assert.IsTrue(s.WrenchByAd());
            Assert.AreEqual(1, s.State.wrenches);
            Assert.AreEqual(Sim.AdsPerDay - 1, s.AdsLeft);
        }
    }
}

using NUnit.Framework;
using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    public class AdTests
    {
        [Test]
        public void TheFreeDoubleComesBeforeAnAdvert()
        {
            var s = TestGame.New();
            Assert.IsTrue(s.DoubleFree);
            Assert.IsFalse(s.AdOffered(AdReward.Double));
            Assert.IsFalse(s.CollectAwayByAd(600));
            Assert.IsTrue(s.CollectAwayDoubled(600));
            Assert.IsTrue(s.AdOffered(AdReward.Double));
            Assert.IsTrue(s.CollectAwayByAd(600));
            Assert.AreEqual(Sim.AdsPerDay - 1, s.AdsLeft);
        }

        [Test]
        public void ABoostByAdvertOnlyWhenTheChargesAreSpent()
        {
            var s = TestGame.New();
            Assert.IsFalse(s.AdOffered(AdReward.Boost));
            Assert.IsFalse(s.BoostByAd());
            while (s.UseBoost()) { }
            Assert.AreEqual(0, s.BoostReady);
            double before = s.BoostLeft;
            Assert.IsTrue(s.BoostByAd());
            Assert.Greater(s.BoostLeft, before + 60);
            Assert.AreEqual(0, s.BoostReady, "no charge is spent or given back");
        }

        [Test]
        public void TheBoostNeverRunsPastItsCap()
        {
            var s = TestGame.New();
            while (s.UseBoost()) { }
            for (int i = 0; i < 20; i++) s.BoostByAd();
            Assert.LessOrEqual(s.BoostLeft, Sim.BoostCap + 1);
        }

        [Test]
        public void SixADay()
        {
            var s = TestGame.New();
            s.CollectAwayDoubled(600);
            for (int i = 0; i < Sim.AdsPerDay; i++) Assert.IsTrue(s.CollectAwayByAd(600));
            Assert.AreEqual(0, s.AdsLeft);
            Assert.IsFalse(s.CollectAwayByAd(600));
        }
    }
}

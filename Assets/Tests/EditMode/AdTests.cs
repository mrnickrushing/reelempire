using NUnit.Framework;
using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    public class AdTests
    {
        [Test]
        public void TheDoubleIsAlwaysForAnAd()
        {
            var s = TestGame.New();
            Assert.IsFalse(s.DoubleFree, "no free double of the day");
            Assert.IsFalse(s.CollectAwayDoubled(600));
            Assert.IsTrue(s.AdOffered(AdReward.Double));
            Assert.IsTrue(s.CollectAwayByAd(600));
            Assert.AreEqual(Sim.AdsPerDay - 1, s.AdsLeft);
        }

        [Test]
        public void TheBoostIsOnlyEverForAnAdvert()
        {
            var s = TestGame.New();
            Assert.IsFalse(s.Boosted);
            Assert.IsTrue(s.AdOffered(AdReward.Boost));
            Assert.IsTrue(s.BoostByAd());
            Assert.AreEqual(Sim.BoostLasts, s.BoostLeft, 1);
            Assert.AreEqual(Sim.AdsPerDay - 1, s.AdsLeft);
        }

        [Test]
        public void ADoubleNeedsAnAdNotGems()
        {
            var s = TestGame.New();
            s.State.gems = 1000;
            Assert.IsFalse(s.CollectAwayDoubled(600), "gems never buy a double");
            Assert.AreEqual(1000, s.State.gems);
            Assert.IsTrue(s.CollectAwayByAd(600));
        }

        [Test]
        public void NothingFreeHandsOutABoost()
        {
            var s = TestGame.New();
            foreach (var g in Sim.Gifts) Assert.IsFalse(g.Name.Contains("Boost"), g.Name);
            for (int t = 0; t < Sim.SeasonTiers; t++) Assert.AreEqual(0, Sim.SeasonRewardFor(t, false).BoostHours, "free season track");
            for (int i = 0; i < 200; i++) { s.State.bonusLeft = 10; s.ClaimBonus(); }
            Assert.IsFalse(s.Boosted, "the drifting crates");
        }

        [Test]
        public void TheBoostNeverRunsPastItsCap()
        {
            var s = TestGame.New();
            for (int i = 0; i < 20; i++) s.BoostByAd();
            Assert.LessOrEqual(s.BoostLeft, Sim.BoostCap + 1);
        }

        [Test]
        public void SixADay()
        {
            var s = TestGame.New();
            for (int i = 0; i < Sim.AdsPerDay; i++) Assert.IsTrue(s.CollectAwayByAd(600));
            Assert.AreEqual(0, s.AdsLeft);
            Assert.IsFalse(s.CollectAwayByAd(600));
        }
    }
}

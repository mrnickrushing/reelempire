using System.Linq;
using NUnit.Framework;
using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    /// <summary>Paint, the season set, collections, the town, record notes, fishing grounds and chefs' requests.</summary>
    public class MetaTests
    {
        [Test]
        public void PaintIsBoughtWithGemsAndWornOnItsPart()
        {
            var s = TestGame.New();
            Assert.IsFalse(s.BuyCosmetic("boats_royal"), "no gems");
            s.State.gems = 500;
            double before = s.IncomeMult(s.H);
            Assert.IsTrue(s.BuyCosmetic("boats_royal"));
            Assert.AreEqual("boats_royal", s.Wearing("boats").Id);
            Assert.AreEqual(before, s.IncomeMult(s.H), 1e-12, "paint never changes earnings");
            Assert.IsFalse(s.BuyCosmetic("s0_gilt"), "season paint isn't sold");
            Assert.IsTrue(s.UseCosmetic(null, "boats"));
            Assert.IsNull(s.Wearing("boats"));
        }

        [Test]
        public void EverySeasonHasAPaintSetOnThePremiumTrack()
        {
            for (int theme = 0; theme < Sim.SeasonThemes.Length; theme++)
            {
                var looks = new[] { 9, 19, 29 }.Select(t => Sim.SeasonRewardFor(t, true, theme).Cosmetic).ToList();
                Assert.That(looks, Has.All.Not.Null, Sim.SeasonThemes[theme]);
                Assert.AreEqual(3, looks.Distinct().Count());
                StringAssert.EndsWith("_gilt", looks[2], "the last tier is the gilded piece");
                Assert.IsNull(Sim.SeasonRewardFor(29, false, theme).Cosmetic, "not on the free track");
            }
            var s = TestGame.New(); s.State.seasonIndex = s.SeasonIndex; s.State.seasonPremiumIndex = s.SeasonIndex; s.State.seasonPoints = 1_000_000;
            var r = s.ClaimSeason(9, true);
            Assert.IsTrue(s.OwnsCosmetic(r.Cosmetic));
            Assert.AreEqual(r.Cosmetic, s.Wearing("bunting").Id);
        }

        [Test]
        public void ACollectionPutsSomethingInTheHarbour()
        {
            var s = TestGame.New();
            var night = Sim.Collections.First(c => c.Id == "night");
            Assert.IsFalse(s.ClaimCollection("night"));
            foreach (var sp in Book.Species.Where(x => x.When == Bite.Night).Take(3)) s.State.species.Add(new SpeciesEntry { id = sp.Id, count = 1, best = 1 });
            Assert.AreEqual(1, s.CollectionsWaiting);
            int gems = s.State.gems;
            Assert.IsTrue(s.ClaimCollection("night"));
            Assert.AreEqual(gems + night.Gems, s.State.gems);
            CollectionAssert.Contains(s.Decor, "camp");
            Assert.IsFalse(s.ClaimCollection("night"), "once");
        }

        [Test]
        public void TheTownGrowsWithFilledOrders()
        {
            var s = TestGame.New();
            Assert.AreEqual("cafe", s.NextTown.Id);
            s.State.ordersDone = 15;
            CollectionAssert.IsSupersetOf(s.Decor, new[] { "cafe", "benches" });
            Assert.AreEqual("bandstand", s.NextTown.Id);
        }

        [Test]
        public void VoyagesFindGroundsAndACompleteRouteBringsMore()
        {
            var s = TestGame.New(); var h = s.H;
            Assert.AreEqual(9, Sim.Grounds.Length);
            s.State.voyages.Add(new Voyage { kind = 0, doneAt = 0 });
            s.CollectVoyage(0);
            Assert.AreEqual(1, s.FoundOn(0), "the first trip on a route always finds a ground");
            Assert.IsNotNull(s.NewGround);
            double plain = s.VoyageCash(Sim.Voyages[0]);
            foreach (var g in Sim.Grounds.Where(x => x.Voyage == 0)) if (!s.Found(g.Id)) s.State.grounds.Add(g.Id);
            Assert.IsTrue(s.RouteCharted(0));
            Assert.AreEqual(plain * 1.25, s.VoyageCash(Sim.Voyages[0]), plain * 1e-9);
        }

        [Test]
        public void AChefsRequestIsFilledByHandAndTeachesARecipe()
        {
            var s = TestGame.New(); var h = s.H; h.goodsOwned = 0b11;
            var sp = Book.In("lake").First(x => x.Rarity == Rarity.Uncommon && x.When == Bite.Any);
            s.State.orders.Add(new Order { kind = (int)OrderKind.Hand, harbour = h.id, who = "The Heron Inn", species = sp.Id, need = 1, goods = 1, cashSeconds = 60, gems = 5 });
            typeof(Sim).GetMethod("OrderCatch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(s, new object[] { h, sp, false });
            Assert.AreEqual(0, s.State.orders[0].have, "a boat's catch doesn't count");
            s.LandByHand(sp);
            Assert.AreEqual(1, s.State.orders[0].have);
            double value = s.GoodsValue(h, 1);
            Assert.Greater(s.ClaimOrder(0), 0);
            StringAssert.Contains("recipe", s.RecipeLearned);
            Assert.AreEqual(value * 1.1, s.GoodsValue(h, 1), value * 1e-9);
        }

        [Test]
        public void ARecordCatchKnowsWhereItStands()
        {
            var s = TestGame.New();
            var sp = Book.In("lake").First(x => x.Rarity == Rarity.Common);
            s.State.species.Add(new SpeciesEntry { id = sp.Id, count = 1, best = 0.5f });
            var c = s.LandByHand(sp);
            Assert.AreEqual(0.5f, c.Before, 1e-6);
            Assert.IsNotEmpty(s.RecordNote(sp));
        }
    }
}

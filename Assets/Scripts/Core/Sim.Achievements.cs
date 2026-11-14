using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    public sealed class AchievementDef
    {
        public string Id, Name, Blurb;
        public int Gems;
        public Func<Sim, double> Have;
        public double Need;
    }

    /// <summary>Achievements: mirrored to Game Center (ids <c>reelempire.ach.&lt;id&gt;</c>), each paying gems once claimed.</summary>
    public sealed partial class Sim
    {
        public static readonly List<AchievementDef> Achievements = new List<AchievementDef>
        {
            new AchievementDef { Id = "firstcatch", Name = "First Bite", Blurb = "Land your first fish", Gems = 5, Have = s => s.State.trips, Need = 1 },
            new AchievementDef { Id = "trips1k", Name = "Old Hand", Blurb = "Bring 1,000 trips home", Gems = 10, Have = s => s.State.trips, Need = 1000 },
            new AchievementDef { Id = "trips25k", Name = "Salt in the Blood", Blurb = "Bring 25,000 trips home", Gems = 25, Have = s => s.State.trips, Need = 25000 },
            new AchievementDef { Id = "berths4", Name = "Working Waterfront", Blurb = "Open four docks in one harbour", Gems = 10, Have = s => s.MostBerths, Need = 4 },
            new AchievementDef { Id = "berths8", Name = "Full Quay", Blurb = "Open all eight docks in one harbour", Gems = 25, Have = s => s.MostBerths, Need = 8 },
            new AchievementDef { Id = "skippers", Name = "Hands Off", Blurb = "Have a skipper at every stage of a harbour", Gems = 10, Have = s => s.FullyCrewed ? 1 : 0, Need = 1 },
            new AchievementDef { Id = "level100", Name = "Centurion", Blurb = "Take a dock to level 100", Gems = 15, Have = s => s.BestBerthLevel, Need = 100 },
            new AchievementDef { Id = "level400", Name = "As Far As It Goes", Blurb = "Take a dock to level 400", Gems = 50, Have = s => s.BestBerthLevel, Need = 400 },
            new AchievementDef { Id = "smokehouse", Name = "Up in Smoke", Blurb = "Build a Smokehouse", Gems = 10, Have = s => s.OwnsGoods(s.H, 3) ? 1 : 0, Need = 1 },
            new AchievementDef { Id = "cannery", Name = "Canned Goods", Blurb = "Build a Cannery", Gems = 20, Have = s => s.OwnsGoods(s.H, 4) ? 1 : 0, Need = 1 },
            new AchievementDef { Id = "species10", Name = "Keen Eye", Blurb = "Catch 10 species", Gems = 10, Have = s => s.SpeciesCaught, Need = 10 },
            new AchievementDef { Id = "species40", Name = "Naturalist", Blurb = "Catch 40 species", Gems = 25, Have = s => s.SpeciesCaught, Need = 40 },
            new AchievementDef { Id = "species96", Name = "The Whole Book", Blurb = "Catch every species", Gems = 100, Have = s => s.SpeciesCaught, Need = 96 },
            new AchievementDef { Id = "legend", Name = "The One They Talk About", Blurb = "Catch a legendary fish", Gems = 25, Have = s => s.LegendsCaught, Need = 1 },
            new AchievementDef { Id = "million", Name = "First Million", Blurb = "Earn 1M in all", Gems = 10, Have = s => s.State.lifetime, Need = 1e6 },
            new AchievementDef { Id = "trillion", Name = "Fish Baron", Blurb = "Earn 1T in all", Gems = 25, Have = s => s.State.lifetime, Need = 1e12 },
            new AchievementDef { Id = "trophy5", Name = "Wall of Fame", Blurb = "Mount 5 trophies", Gems = 20, Have = s => s.State.trophies.Count, Need = 5 },
            new AchievementDef { Id = "derbytop3", Name = "On the Podium", Blurb = "Finish a derby in the top three", Gems = 40, Have = s => s.State.derbyBest > 0 && s.State.derbyBest <= 3 ? 1 : 0, Need = 1 },
            new AchievementDef { Id = "crew6", Name = "All Hands", Blurb = "Sign on six of the crew", Gems = 20, Have = s => s.State.crew.Count, Need = 6 },
            new AchievementDef { Id = "crew12", Name = "Full Complement", Blurb = "Sign on all twelve of the crew", Gems = 50, Have = s => s.State.crew.Count, Need = 12 },
            new AchievementDef { Id = "voyages10", Name = "Sea Legs", Blurb = "Bring 10 voyages home", Gems = 15, Have = s => s.State.voyagesDone, Need = 10 },
            new AchievementDef { Id = "renovate1", Name = "Fresh Paint", Blurb = "Renovate a harbour", Gems = 25, Have = s => s.State.renovations, Need = 1 },
            new AchievementDef { Id = "renovate5", Name = "Master Builder", Blurb = "Renovate five times in all", Gems = 50, Have = s => s.State.renovations, Need = 5 },
            new AchievementDef { Id = "harbour2", Name = "Upriver", Blurb = "Open Alder River", Gems = 20, Have = s => s.State.harbours.Count, Need = 2 },
            new AchievementDef { Id = "harbour6", Name = "Six Waters", Blurb = "Open every harbour", Gems = 100, Have = s => s.State.harbours.Count, Need = 6 },
        };

        public int MostBerths { get { int n = 0; foreach (var h in State.harbours) n = Math.Max(n, h.berths.Count); return n; } }
        public bool FullyCrewed { get { foreach (var h in State.harbours) { if (!h.dockSkipper || !h.marketSkipper) continue; bool all = true; foreach (var b in h.berths) all &= b.skipper; if (all) return true; } return false; } }
        public int LegendsCaught { get { int n = 0; foreach (var e in State.species) { var d = Book.Get(e.id); if (d != null && d.Rarity == Rarity.Legendary) n++; } return n; } }

        public bool AchievementDone(AchievementDef a) { return a.Have(this) >= a.Need; }
        public bool AchievementClaimed(AchievementDef a) { return State.achieved.Contains(a.Id); }
        public int AchievementsToClaim { get { int n = 0; foreach (var a in Achievements) if (AchievementDone(a) && !AchievementClaimed(a)) n++; return n; } }

        public bool ClaimAchievement(string id)
        {
            var a = Achievements.Find(x => x.Id == id);
            if (a == null || !AchievementDone(a) || AchievementClaimed(a)) return false;
            State.achieved.Add(a.Id); State.gems += a.Gems;
            Touch();
            return true;
        }
    }
}

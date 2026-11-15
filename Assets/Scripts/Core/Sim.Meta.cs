using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>Paint for a part of the harbour: a tint and a hue shift on its models. Paint only, never earnings.</summary>
    public sealed class CosmeticDef
    {
        public string Id, Part, Name;
        public int Gems;                // 0: not sold (a season or collection reward)
        public float R = 1, G = 1, B = 1, Hue;
        public int Season = -1;         // the season theme that gives it, or -1
    }

    /// <summary>A small collection of fish to land; done, it puts something in the harbour for good.</summary>
    public sealed class CollectionDef
    {
        public string Id, Name, Blurb, Decor, DecorName;
        public int Gems, Need;
        public Func<Sim, int> Have;
    }

    /// <summary>A fishing ground a voyage can find, with the story the boat brings home.</summary>
    public sealed class GroundDef { public string Id, Name, Story; public int Voyage; }

    /// <summary>What the town builds as the harbour fills its orders.</summary>
    public sealed class TownDef { public string Id, Name, Blurb; public int Orders; }

    /// <summary>
    /// The meta layer: paint for the harbour (sold for gems, previewed in place; a season set on the pass), collections
    /// that put something in the harbour, record catches, fishing grounds found by voyages, a town that grows with the
    /// orders, and chefs' requests: a fish landed by hand teaches the hall a recipe.
    /// </summary>
    public sealed partial class Sim
    {
        // ---- paint ----

        public static readonly string[] Parts = { "boats", "docks", "market", "lighthouse", "bunting" };
        public static readonly string[] PartNames = { "Boat paint", "Dock boards", "Market canopies", "Lighthouse", "Quay bunting" };

        public static readonly string[] SeasonThemes = { "Spring Regatta", "Lantern Nights", "Harvest Tide", "Frost Fair" };

        public static readonly CosmeticDef[] Cosmetics =
        {
            new CosmeticDef { Id = "boats_seafoam", Part = "boats", Name = "Seafoam hulls", Gems = 120, Hue = 0.33f },
            new CosmeticDef { Id = "boats_royal", Part = "boats", Name = "Royal blue hulls", Gems = 120, Hue = 0.58f },
            new CosmeticDef { Id = "boats_rose", Part = "boats", Name = "Rose and cream", Gems = 150, Hue = 0.9f, R = 1.08f, G = 1.02f, B = 1.02f },
            new CosmeticDef { Id = "docks_white", Part = "docks", Name = "Whitewashed boards", Gems = 80, R = 1.95f, G = 1.92f, B = 1.85f },
            new CosmeticDef { Id = "docks_tar", Part = "docks", Name = "Tarred boards", Gems = 80, R = 0.85f, G = 0.78f, B = 0.72f },
            new CosmeticDef { Id = "market_lemon", Part = "market", Name = "Lemon awnings", Gems = 100, Hue = 0.14f, R = 1.55f, G = 1.5f, B = 1.45f },
            new CosmeticDef { Id = "market_berry", Part = "market", Name = "Berry awnings", Gems = 100, Hue = 0.74f, R = 1.55f, G = 1.5f, B = 1.45f },
            new CosmeticDef { Id = "lighthouse_blue", Part = "lighthouse", Name = "Blue-banded lighthouse", Gems = 90, Hue = 0.55f },
            new CosmeticDef { Id = "lighthouse_green", Part = "lighthouse", Name = "Green-banded lighthouse", Gems = 90, Hue = 0.33f },
            new CosmeticDef { Id = "bunting_harbour", Part = "bunting", Name = "Harbour bunting", Gems = 60, R = 0.86f, G = 0.24f, B = 0.2f },
            // The season sets: one for each theme, on the premium track at tiers 10, 20 and 30 (the last one gilded).
            new CosmeticDef { Id = "s0_bunting", Part = "bunting", Name = "Regatta pennants", Season = 0, R = 0.2f, G = 0.55f, B = 0.85f },
            new CosmeticDef { Id = "s0_boats", Part = "boats", Name = "Regatta stripes", Season = 0, Hue = 0.45f, R = 1.1f, G = 1.1f, B = 1.1f },
            new CosmeticDef { Id = "s0_gilt", Part = "lighthouse", Name = "Gilded lighthouse (Regatta)", Season = 0, R = 1.45f, G = 1.15f, B = 0.55f },
            new CosmeticDef { Id = "s1_bunting", Part = "bunting", Name = "Paper lanterns", Season = 1, R = 1f, G = 0.62f, B = 0.18f },
            new CosmeticDef { Id = "s1_boats", Part = "boats", Name = "Lantern red hulls", Season = 1, Hue = 0.95f, R = 1.12f, G = 0.95f, B = 0.9f },
            new CosmeticDef { Id = "s1_gilt", Part = "boats", Name = "Gilded fleet (Lantern Nights)", Season = 1, R = 1.4f, G = 1.12f, B = 0.55f },
            new CosmeticDef { Id = "s2_bunting", Part = "bunting", Name = "Harvest garlands", Season = 2, R = 0.85f, G = 0.5f, B = 0.12f },
            new CosmeticDef { Id = "s2_market", Part = "market", Name = "Harvest awnings", Season = 2, Hue = 0.07f, R = 1.6f, G = 1.45f, B = 1.3f },
            new CosmeticDef { Id = "s2_gilt", Part = "market", Name = "Gilded market (Harvest Tide)", Season = 2, R = 1.75f, G = 1.45f, B = 0.85f },
            new CosmeticDef { Id = "s3_bunting", Part = "bunting", Name = "Frost fair flags", Season = 3, R = 0.75f, G = 0.9f, B = 1f },
            new CosmeticDef { Id = "s3_docks", Part = "docks", Name = "Frosted boards", Season = 3, R = 1.75f, G = 1.9f, B = 2.05f },
            new CosmeticDef { Id = "s3_gilt", Part = "docks", Name = "Gilded boards (Frost Fair)", Season = 3, R = 2.1f, G = 1.75f, B = 0.95f },
        };

        public static CosmeticDef CosmeticById(string id) { return Array.Find(Cosmetics, c => c.Id == id); }
        public bool OwnsCosmetic(string id) { return State.cosmetics.Contains(id); }

        /// <summary>What a part of the harbour wears (null: its own colours).</summary>
        public CosmeticDef Wearing(string part)
        {
            int i = Array.IndexOf(Parts, part); if (i < 0) return null;
            while (State.wearing.Count < Parts.Length) State.wearing.Add("");
            var d = CosmeticById(State.wearing[i]);
            return d != null && OwnsCosmetic(d.Id) ? d : null;
        }

        public bool BuyCosmetic(string id)
        {
            var d = CosmeticById(id);
            if (d == null || d.Gems <= 0 || OwnsCosmetic(id) || State.gems < d.Gems) return false;
            State.gems -= d.Gems; State.cosmetics.Add(id);
            UseCosmetic(id);
            return true;
        }

        /// <summary>Puts an owned paint on its part (null id with a part: back to its own colours).</summary>
        public bool UseCosmetic(string id, string part = null)
        {
            var d = CosmeticById(id);
            if (d != null && !OwnsCosmetic(id)) return false;
            int i = Array.IndexOf(Parts, d != null ? d.Part : part); if (i < 0) return false;
            while (State.wearing.Count < Parts.Length) State.wearing.Add("");
            State.wearing[i] = d != null ? d.Id : "";
            Touch();
            return true;
        }

        public int SeasonTheme { get { return (int)(((SeasonIndex % SeasonThemes.Length) + SeasonThemes.Length) % SeasonThemes.Length); } }

        /// <summary>The season's paint at a premium tier (10, 20, 30), or null.</summary>
        public static CosmeticDef SeasonCosmetic(int theme, int tier)
        {
            int n = tier + 1; string suffix = n == 10 ? "bunting" : n == 20 ? null : n == 30 ? "gilt" : "";
            if (suffix == "") return null;
            if (suffix == null) return Array.Find(Cosmetics, c => c.Season == theme && c.Id != "s" + theme + "_bunting" && !c.Id.EndsWith("_gilt"));
            return CosmeticById("s" + theme + "_" + suffix);
        }

        // ---- collections ----

        static int Got(Sim s, Func<SpeciesDef, bool> f) { int n = 0; foreach (var e in s.State.species) { var sp = Book.Get(e.id); if (sp != null && f(sp)) n++; } return n; }

        public static readonly CollectionDef[] Collections =
        {
            new CollectionDef { Id = "lake_commons", Name = "Lake Regulars", Blurb = "Every common fish of Stillwater Lake", Need = 7, Gems = 20, Decor = "flowers", DecorName = "Flower boxes along the quay",
                Have = s => Got(s, x => x.Harbour == "lake" && x.Rarity == Rarity.Common) },
            new CollectionDef { Id = "night", Name = "Night Owls", Blurb = "Three fish that bite only at night", Need = 3, Gems = 30, Decor = "camp", DecorName = "A night-fishing camp on the shore",
                Have = s => Got(s, x => x.When == Bite.Night) },
            new CollectionDef { Id = "rain", Name = "Rain or Shine", Blurb = "Two fish that bite only in the rain", Need = 2, Gems = 30, Decor = "canoes", DecorName = "A canoe rack by the slipway",
                Have = s => Got(s, x => x.When == Bite.Rain) },
            new CollectionDef { Id = "predator", Name = "Big Predator", Blurb = "A personal best over 12 kg", Need = 1, Gems = 40, Decor = "statue", DecorName = "A fish statue at the head of the quay",
                Have = s => s.State.species.Exists(e => e.best >= 12) ? 1 : 0 },
            new CollectionDef { Id = "rares", Name = "Rare Trio", Blurb = "Three rare fish or better, from any water", Need = 3, Gems = 50, Decor = "flags", DecorName = "Pennants on the pier heads",
                Have = s => Got(s, x => x.Rarity >= Rarity.Rare) },
            new CollectionDef { Id = "river_commons", Name = "River Run", Blurb = "Every common fish of Alder River", Need = 7, Gems = 60, Decor = "barrels", DecorName = "A stack of salted-fish barrels",
                Have = s => Got(s, x => x.Harbour == "river" && x.Rarity == Rarity.Common) },
        };

        public bool CollectionDone(CollectionDef c) { return c.Have(this) >= c.Need; }
        public bool CollectionClaimed(string id) { return State.collections.Contains(id); }
        public int CollectionsWaiting { get { int n = 0; foreach (var c in Collections) if (CollectionDone(c) && !CollectionClaimed(c.Id)) n++; return n; } }

        public bool ClaimCollection(string id)
        {
            var c = Array.Find(Collections, x => x.Id == id);
            if (c == null || !CollectionDone(c) || CollectionClaimed(id)) return false;
            State.collections.Add(id); State.gems += c.Gems; AddSeasonPoints(30);
            Touch();
            return true;
        }

        /// <summary>What stands in the harbour: the decorations of claimed collections and what the town has built.</summary>
        public List<string> Decor
        {
            get
            {
                var d = new List<string>();
                foreach (var c in Collections) if (CollectionClaimed(c.Id)) d.Add(c.Decor);
                foreach (var t in Town) if (State.ordersDone >= t.Orders) d.Add(t.Id);
                return d;
            }
        }

        // ---- the town ----

        public static readonly TownDef[] Town =
        {
            new TownDef { Id = "cafe", Name = "The waterfront café", Blurb = "Tables and parasols by the road", Orders = 5 },
            new TownDef { Id = "benches", Name = "Benches and flower beds", Blurb = "Somewhere to watch the boats", Orders = 15 },
            new TownDef { Id = "bandstand", Name = "A bandstand", Blurb = "Music on market days", Orders = 30 },
            new TownDef { Id = "stalls", Name = "More market stalls", Blurb = "The market spills onto the road", Orders = 60 },
            new TownDef { Id = "fountain", Name = "The fountain", Blurb = "The town square's centrepiece", Orders = 100 },
        };

        /// <summary>The next thing the town will build, or null when it is all built.</summary>
        public TownDef NextTown { get { foreach (var t in Town) if (State.ordersDone < t.Orders) return t; return null; } }

        // ---- record catches ----

        /// <summary>What a record catch means for the book: a line for the record card.</summary>
        public string RecordNote(SpeciesDef sp)
        {
            var e = Entry(sp.Id); if (e == null) return "";
            float heaviest = 0; foreach (var x in State.species) if (x.best > heaviest) heaviest = x.best;
            if (e.best >= heaviest - 1e-4f) return "The heaviest fish in your whole book.";
            int rank = 1; foreach (var x in Book.In(sp.Harbour)) { var xe = Entry(x.Id); if (xe != null && xe.best > e.best) rank++; }
            string place = Content.Harbour(sp.Harbour).Name;
            return rank == 1 ? "The heaviest fish you've landed at " + place + "."
                : "The " + Ordinal(rank) + " heaviest fish you've landed at " + place + ".";
        }

        static string Ordinal(int n)
        {
            int t = n % 100; string suf = t >= 11 && t <= 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
            return n + suf;
        }

        // ---- fishing grounds ----

        public static readonly GroundDef[] Grounds =
        {
            new GroundDef { Voyage = 0, Id = "reedbed", Name = "The Reed Beds", Story = "Herons stood in the shallows like posts. The nets came up green with weed and silver with roach." },
            new GroundDef { Voyage = 0, Id = "millrace", Name = "The Old Mill Race", Story = "Under the broken wheel the water runs deep and cold. An old man fishing there swore by worms." },
            new GroundDef { Voyage = 0, Id = "willow", Name = "Willow Bend", Story = "A kingfisher followed the boat all the way home." },
            new GroundDef { Voyage = 1, Id = "banks", Name = "The Far Banks", Story = "Sandbars as long as streets. The crew ate their sandwiches on one while the tide turned." },
            new GroundDef { Voyage = 1, Id = "wreck", Name = "The Sunken Barge", Story = "A coal barge on the bottom, thick with fish. Someone's anchor is still caught on it." },
            new GroundDef { Voyage = 1, Id = "point", Name = "Lantern Point", Story = "A lamp burns on the point though nobody lives there. Nobody on the crew asked who lights it." },
            new GroundDef { Voyage = 2, Id = "shelf", Name = "The Shelf", Story = "Where the bottom drops away the water turns from green to ink. The lines came up heavy." },
            new GroundDef { Voyage = 2, Id = "fog", Name = "The Fog Banks", Story = "Two days in fog. They found the boat home by the sound of the market bell." },
            new GroundDef { Voyage = 2, Id = "whales", Name = "Whale Road", Story = "A whale surfaced beside the boat and looked at them for a long time." },
        };

        public bool Found(string ground) { return State.grounds.Contains(ground); }
        public int FoundOn(int voyage) { int n = 0; foreach (var g in Grounds) if (g.Voyage == voyage && Found(g.Id)) n++; return n; }
        /// <summary>A route whose three grounds are all found brings home a quarter more.</summary>
        public bool RouteCharted(int voyage) { return FoundOn(voyage) >= 3; }

        /// <summary>The ground the last voyage found (the HUD shows its story once), or null.</summary>
        public GroundDef NewGround;

        /// <summary>A voyage home may have found a new ground: the first of each route always does, later ones half the time.</summary>
        void Discover(int voyage)
        {
            var left = Array.FindAll(Grounds, g => g.Voyage == voyage && !Found(g.Id));
            if (left.Length == 0) return;
            if (FoundOn(voyage) > 0 && Roll() >= 0.5) return;
            var g = left[(int)(Roll() * left.Length)];
            State.grounds.Add(g.Id); NewGround = g;
        }

        // ---- chefs' requests and recipes ----

        /// <summary>Recipes the hall has learned, as "harbour:goods". Each adds a tenth to that line's value there.</summary>
        public const double RecipeValue = 0.1;
        public int Recipes(HarbourState h, int goods) { int n = 0; string k = h.id + ":" + goods; foreach (var r in State.recipes) if (r == k) n++; return n; }

        /// <summary>A chef's request: one fish of a kind landed by hand. Filled, it teaches a recipe for a line.</summary>
        Order ChefOrder(HarbourState h, Order o)
        {
            var pool = Book.In(h.id).FindAll(s => s.Rarity >= Rarity.Uncommon && s.Rarity <= Rarity.Rare && !State.orders.Exists(x => x.species == s.Id));
            if (pool.Count == 0) return null;
            var sp = pool[(int)(Roll() * pool.Count)];
            var owned = new List<int>(); for (int g = 1; g < Content.Goods.Length; g++) if (OwnsGoods(h, g)) owned.Add(g);
            o.kind = (int)OrderKind.Hand; o.species = sp.Id; o.need = 1;
            o.goods = owned.Count > 0 ? owned[(int)(Roll() * owned.Count)] : 1;
            o.cashSeconds = sp.Rarity == Rarity.Rare ? 600 : 360; o.gems = sp.Rarity == Rarity.Rare ? 8 : 5;
            return o;
        }
    }
}

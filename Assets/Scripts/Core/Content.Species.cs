using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>The species book: sixteen fish for each water (7 common, 4 uncommon, 3 rare, 1 epic, 1 legendary).</summary>
    public static partial class Book
    {
        public static readonly List<SpeciesDef> Species = new List<SpeciesDef>();
        static readonly Dictionary<string, SpeciesDef> ById = new Dictionary<string, SpeciesDef>();

        public static SpeciesDef Get(string id) { SpeciesDef s; return ById.TryGetValue(id, out s) ? s : null; }
        public static List<SpeciesDef> In(string harbour) { return Species.FindAll(s => s.Harbour == harbour); }

        static void Add(string harbour, Rarity r, string name, float min, float max, Bite when = Bite.Any)
        {
            var s = new SpeciesDef { Id = harbour + "." + name.ToLowerInvariant().Replace(' ', '_').Replace("'", ""), Name = name, Harbour = harbour, Rarity = r, When = when, MinKg = min, MaxKg = max };
            Species.Add(s); ById[s.Id] = s;
        }

        static Book()
        {
            const Rarity C = Rarity.Common, U = Rarity.Uncommon, R = Rarity.Rare, E = Rarity.Epic, L = Rarity.Legendary;
            Add("lake", C, "Bluegill", 0.1f, 0.9f); Add("lake", C, "Yellow Perch", 0.1f, 1.2f); Add("lake", C, "Roach", 0.1f, 1f);
            Add("lake", C, "Crappie", 0.2f, 1.5f); Add("lake", C, "Bream", 0.3f, 3f); Add("lake", C, "Tench", 0.5f, 4f); Add("lake", C, "Rudd", 0.1f, 1.1f);
            Add("lake", U, "Largemouth Bass", 0.5f, 6f); Add("lake", U, "Common Carp", 1f, 18f); Add("lake", U, "Walleye", 0.5f, 7f, Bite.Night); Add("lake", U, "Channel Catfish", 1f, 14f, Bite.Rain);
            Add("lake", R, "Northern Pike", 2f, 20f); Add("lake", R, "Mirror Carp", 3f, 28f, Bite.Summer); Add("lake", R, "Burbot", 0.5f, 8f, Bite.Winter);
            Add("lake", E, "Muskellunge", 5f, 30f, Bite.Storm); Add("lake", L, "Golden Koi", 4f, 16f);

            Add("river", C, "Brown Trout", 0.2f, 5f); Add("river", C, "Chub", 0.2f, 3f); Add("river", C, "Dace", 0.05f, 0.5f);
            Add("river", C, "Grayling", 0.2f, 2f); Add("river", C, "Barbel", 0.5f, 7f); Add("river", C, "Smallmouth Bass", 0.3f, 4f); Add("river", C, "Minnow", 0.01f, 0.05f);
            Add("river", U, "Rainbow Trout", 0.3f, 8f); Add("river", U, "River Eel", 0.3f, 5f, Bite.Night); Add("river", U, "Zander", 0.5f, 10f, Bite.Rain); Add("river", U, "Brook Char", 0.2f, 3f, Bite.Spring);
            Add("river", R, "Atlantic Salmon", 2f, 25f, Bite.Autumn); Add("river", R, "Steelhead", 2f, 15f); Add("river", R, "Wels Catfish", 5f, 90f, Bite.Night);
            Add("river", E, "White Sturgeon", 20f, 300f); Add("river", L, "Ghost Salmon", 6f, 30f, Bite.Storm);

            Add("coast", C, "Mackerel", 0.2f, 1.5f); Add("coast", C, "Herring", 0.1f, 0.6f); Add("coast", C, "Pollock", 0.5f, 9f);
            Add("coast", C, "Whiting", 0.2f, 2f); Add("coast", C, "Flounder", 0.3f, 4f); Add("coast", C, "Shore Crab", 0.05f, 0.3f); Add("coast", C, "Sardine", 0.02f, 0.2f);
            Add("coast", U, "Sea Bass", 0.5f, 9f); Add("coast", U, "Atlantic Cod", 1f, 30f, Bite.Winter); Add("coast", U, "Brown Crab", 0.3f, 3f); Add("coast", U, "Squid", 0.2f, 2f, Bite.Night);
            Add("coast", R, "Lobster", 0.5f, 6f); Add("coast", R, "Turbot", 2f, 18f, Bite.Rain); Add("coast", R, "Conger Eel", 5f, 60f, Bite.Night);
            Add("coast", E, "Halibut", 20f, 200f, Bite.Storm); Add("coast", L, "Blue Lobster", 1f, 7f);

            Add("reef", C, "Clownfish", 0.02f, 0.2f); Add("reef", C, "Blue Tang", 0.1f, 0.6f); Add("reef", C, "Butterflyfish", 0.05f, 0.4f);
            Add("reef", C, "Parrotfish", 0.5f, 6f); Add("reef", C, "Snapper", 0.5f, 8f); Add("reef", C, "Damselfish", 0.02f, 0.2f); Add("reef", C, "Wrasse", 0.2f, 3f);
            Add("reef", U, "Angelfish", 0.2f, 1.5f); Add("reef", U, "Grouper", 2f, 40f); Add("reef", U, "Reef Octopus", 0.5f, 6f, Bite.Night); Add("reef", U, "Triggerfish", 0.5f, 5f, Bite.Summer);
            Add("reef", R, "Lionfish", 0.3f, 1.2f, Bite.Night); Add("reef", R, "Barracuda", 2f, 25f); Add("reef", R, "Moray Eel", 2f, 30f, Bite.Rain);
            Add("reef", E, "Giant Trevally", 10f, 80f, Bite.Storm); Add("reef", L, "Lantern Seahorse", 0.01f, 0.05f, Bite.Night);

            Add("deep", C, "Skipjack Tuna", 2f, 18f); Add("deep", C, "Mahi-Mahi", 3f, 25f); Add("deep", C, "Bonito", 1f, 8f);
            Add("deep", C, "Flying Fish", 0.1f, 0.7f); Add("deep", C, "Albacore", 5f, 35f); Add("deep", C, "Amberjack", 5f, 50f); Add("deep", C, "Wahoo", 5f, 60f);
            Add("deep", U, "Yellowfin Tuna", 10f, 150f); Add("deep", U, "Sailfish", 20f, 90f, Bite.Day); Add("deep", U, "Giant Squid", 50f, 250f, Bite.Night); Add("deep", U, "Mako Shark", 40f, 300f, Bite.Rain);
            Add("deep", R, "Swordfish", 50f, 400f, Bite.Night); Add("deep", R, "Bluefin Tuna", 100f, 600f, Bite.Autumn); Add("deep", R, "Oarfish", 50f, 250f, Bite.Storm);
            Add("deep", E, "Blue Marlin", 100f, 800f); Add("deep", L, "Anglerfish King", 20f, 60f, Bite.Night);

            Add("arctic", C, "Arctic Char", 0.5f, 12f); Add("arctic", C, "Capelin", 0.02f, 0.06f); Add("arctic", C, "Polar Cod", 0.05f, 0.4f);
            Add("arctic", C, "Lumpfish", 0.5f, 8f); Add("arctic", C, "Snow Crab", 0.3f, 1.5f); Add("arctic", C, "Redfish", 0.5f, 10f); Add("arctic", C, "Northern Shrimp", 0.01f, 0.03f);
            Add("arctic", U, "Greenland Halibut", 2f, 40f); Add("arctic", U, "King Crab", 2f, 10f, Bite.Winter); Add("arctic", U, "Wolffish", 2f, 20f, Bite.Night); Add("arctic", U, "Arctic Skate", 1f, 12f, Bite.Rain);
            Add("arctic", R, "Sablefish", 2f, 25f); Add("arctic", R, "Giant Pacific Octopus", 10f, 70f, Bite.Night); Add("arctic", R, "Toothfish", 10f, 100f, Bite.Storm);
            Add("arctic", E, "Greenland Shark", 200f, 1000f); Add("arctic", L, "Aurora Char", 3f, 14f, Bite.Night);
        }
    }
}

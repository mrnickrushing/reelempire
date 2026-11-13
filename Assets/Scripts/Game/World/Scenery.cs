using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>The things in the harbour. Each uses a free model when one is in Resources/Models, else the game's own shape.</summary>
    public static class Scenery
    {
        static readonly Color[] Hulls = { Palette.Hex(0xC8553D), Palette.Hex(0x2F6FA8), Palette.Hex(0xE2B13C), Palette.Hex(0x3E8A6A), Palette.Hex(0xD9D6CC), Palette.Hex(0x8A4FA0), Palette.Hex(0x2B3A55), Palette.Hex(0xB23A48) };

        public static GameObject Boat(int berth, Transform parent)
        {
            float len = Layout.BoatLength(berth);
            var kit = Models.Kit("boat_" + berth, parent, Vector3.zero, 0, len);
            if (kit != null) return kit;
            return Models.Built("Boat " + berth, parent, mb =>
            {
                Color hull = Hulls[berth % Hulls.Length], deck = Palette.WoodPale, white = Palette.White;
                float w = len * 0.32f, d = len * 0.16f;
                mb.Paint();
                // The hull: a box amidships, tapering to the bow (+z).
                mb.Box(new Vector3(0, d * 0.3f, -len * 0.1f), new Vector3(w, d, len * 0.62f), hull, deck);
                var bow = new[] { new Vector2(-w / 2, len * 0.21f), new Vector2(w / 2, len * 0.21f), new Vector2(0, len * 0.5f) };
                mb.Prism(bow, -d * 0.2f, d * 0.8f, hull, deck);
                mb.Box(new Vector3(0, d * 0.86f, -len * 0.1f), new Vector3(w * 1.06f, d * 0.12f, len * 0.64f), white);      // the rail
                if (berth >= 1)
                {
                    float ch = len * 0.2f;
                    mb.Box(new Vector3(0, d * 0.8f + ch / 2, -len * 0.18f), new Vector3(w * 0.62f, ch, len * 0.26f), white, hull);     // wheelhouse
                    mb.Mat(0.8f, 0.2f);
                    mb.Box(new Vector3(0, d * 0.8f + ch * 0.62f, -len * 0.05f), new Vector3(w * 0.5f, ch * 0.3f, 0.06f), Palette.Glass);
                    mb.Paint();
                }
                if (berth >= 3)
                {
                    mb.Cylinder(new Vector3(0, d, len * 0.12f), new Vector3(0, d + len * 0.42f, len * 0.12f), 0.09f, 8, Palette.SteelDark);       // the mast
                    mb.Cylinder(new Vector3(0, d + len * 0.36f, len * 0.12f), new Vector3(0, d + len * 0.16f, len * 0.42f), 0.06f, 6, Palette.SteelDark); // the boom
                }
                else mb.Cylinder(new Vector3(w * 0.3f, d * 0.8f, len * 0.05f), new Vector3(w * 0.9f, d * 0.8f + len * 0.3f, len * 0.3f), 0.04f, 6, Palette.Wood);  // a rod
                mb.Matte();
            });
        }

        public static GameObject Crate(Transform parent)
        {
            var kit = Models.Kit("crate", parent, Vector3.zero, 0, 1.2f);
            if (kit != null) return kit;
            return Models.Built("Crate", parent, mb =>
            {
                mb.Surf = SurfaceKind.Planks;
                mb.Box(new Vector3(0, 0.45f, 0), new Vector3(1.2f, 0.9f, 1.2f), Palette.Wood, Palette.Hex(0x9FB4C4));
                mb.Surf = SurfaceKind.Auto;
            });
        }

        public static GameObject Pile(Transform parent, Color tint)
        {
            return Models.Built("Pile", parent, mb =>
            {
                mb.Surf = SurfaceKind.Planks;
                for (int i = 0; i < 6; i++)
                {
                    int layer = i < 3 ? 0 : i < 5 ? 1 : 2;
                    float x = layer == 0 ? (i - 1) * 1.05f : layer == 1 ? (i - 3.5f) * 1.05f : 0;
                    mb.Box(new Vector3(x, 0.4f + layer * 0.8f, 0), new Vector3(1, 0.8f, 1), Palette.Wood, tint);
                }
                mb.Surf = SurfaceKind.Auto;
            });
        }

        public static GameObject Hauler(Transform parent, int index)
        {
            var kit = Models.Kit("hauler", parent, Vector3.zero, 0, 1.8f);
            if (kit != null) return kit;
            return Models.Built("Hauler", parent, mb =>
            {
                Color coat = index % 2 == 0 ? Palette.Hex(0xE8A22A) : Palette.Hex(0x3C7AC8), skin = Palette.Hex(0xE0B090), boots = Palette.Hex(0x2C3A30);
                mb.Box(new Vector3(-0.14f, 0.4f, 0), new Vector3(0.2f, 0.8f, 0.24f), boots);
                mb.Box(new Vector3(0.14f, 0.4f, 0), new Vector3(0.2f, 0.8f, 0.24f), boots);
                mb.Box(new Vector3(0, 1.15f, 0), new Vector3(0.56f, 0.72f, 0.34f), coat);
                mb.Sphere(new Vector3(0, 1.72f, 0), Vector3.one * 0.21f, 10, skin);
                mb.Box(new Vector3(0, 1.9f, 0), new Vector3(0.44f, 0.12f, 0.44f), Palette.Hex(0xE8C23A));       // sou'wester
                // The barrow.
                mb.Box(new Vector3(0, 0.38f, 0.75f), new Vector3(0.8f, 0.1f, 0.9f), Palette.SteelDark);
                mb.Cylinder(new Vector3(-0.42f, 0.2f, 0.9f), new Vector3(0.42f, 0.2f, 0.9f), 0.2f, 10, Palette.Tyre);
            });
        }

        public static GameObject Truck(Transform parent)
        {
            var kit = Models.Kit("truck", parent, Vector3.zero, 0, 5.2f);
            if (kit != null) return kit;
            var go = Models.Built("Truck", parent, mb =>
            {
                Color cab = Palette.Hex(0x2F8FB8), bed = Palette.Hex(0xD8D4C8);
                mb.Paint();
                mb.Box(new Vector3(0, 1.35f, 1.6f), new Vector3(2.1f, 1.7f, 1.6f), cab);
                mb.Mat(0.8f, 0.2f);
                mb.Box(new Vector3(0, 1.75f, 2.41f), new Vector3(1.8f, 0.7f, 0.05f), Palette.Glass);
                mb.Paint();
                mb.Box(new Vector3(0, 0.75f, -0.9f), new Vector3(2.2f, 0.3f, 3.6f), Palette.SteelDark);
                mb.Box(new Vector3(-1.05f, 1.2f, -0.9f), new Vector3(0.1f, 0.6f, 3.6f), bed);
                mb.Box(new Vector3(1.05f, 1.2f, -0.9f), new Vector3(0.1f, 0.6f, 3.6f), bed);
                mb.Box(new Vector3(0, 1.2f, -2.65f), new Vector3(2.2f, 0.6f, 0.1f), bed);
                mb.Matte();
                foreach (float z in new[] { 1.5f, -1.9f })
                    foreach (float x in new[] { -1.0f, 1.0f })
                        mb.Cylinder(new Vector3(x - 0.16f, 0.45f, z), new Vector3(x + 0.16f, 0.45f, z), 0.45f, 12, Palette.Tyre);
            });
            var load = Models.Built("Load", go.transform, mb =>
            {
                mb.Surf = SurfaceKind.Planks;
                for (int i = 0; i < 3; i++) mb.Box(new Vector3(0, 1.35f, 0.3f - i * 1.15f), new Vector3(1.8f, 0.9f, 1.05f), Palette.Wood, Palette.Hex(0xDC9A4A));
                mb.Surf = SurfaceKind.Auto;
            });
            return go;
        }

        public static void Hall(Transform parent, HarbourState h)
        {
            Models.Built("Hall", parent, mb =>
            {
                Vector3 c = Layout.HallCentre, s = Layout.HallSize;
                Color wall = Palette.Hex(0xB9483A), trim = Palette.White, roof = Palette.Hex(0x4A5560);
                mb.Surf = SurfaceKind.Siding;
                mb.Box(c + new Vector3(0, s.y * 0.3f, 0), new Vector3(s.x, s.y * 0.6f, s.z), wall);
                mb.Surf = SurfaceKind.Tin;
                mb.GableRoof(c + new Vector3(0, s.y * 0.6f, 0), new Vector3(s.x, s.y * 0.4f, s.z), roof, wall, 0.6f);
                mb.Surf = SurfaceKind.Auto;
                // The big door onto the quay and the bay onto the yard.
                mb.Box(new Vector3(c.x - s.x / 2 - 0.05f, c.y + 2, 0), new Vector3(0.12f, 4, 6), Palette.Hex(0x2B2F36));
                mb.Box(new Vector3(c.x - s.x / 2 - 0.1f, c.y + 4.15f, 0), new Vector3(0.14f, 0.3f, 6.6f), trim);
                mb.Box(new Vector3(c.x + s.x / 2 + 0.05f, c.y + 2, -5), new Vector3(0.12f, 4, 5), Palette.Hex(0x2B2F36));
                mb.Lit();
                for (float z = -9; z <= 9; z += 4.5f)
                    if (Mathf.Abs(z) > 3.5f) mb.Box(new Vector3(c.x - s.x / 2 - 0.05f, c.y + 3.3f, z), new Vector3(0.1f, 1.3f, 1.8f), new Color(0.75f, 0.85f, 0.9f, 0));
                mb.Matte();
                // The smokehouse chimney.
                mb.Surf = SurfaceKind.Brick;
                mb.Box(c + new Vector3(4, s.y * 0.95f, 7), new Vector3(1.6f, s.y * 0.7f, 1.6f), Palette.Hex(0x8A4A3A));
                mb.Surf = SurfaceKind.Auto;
            });
        }

        public static void Market(Transform parent)
        {
            Models.Built("Market", parent, mb =>
            {
                Vector3 c = Layout.MarketCentre;
                Color wall = Palette.Hex(0xE8DCC0), roof = Palette.Hex(0x2F7A8A);
                mb.Surf = SurfaceKind.Brick;
                mb.Box(c + new Vector3(2, 3, 0), new Vector3(12, 6, 20), wall);
                mb.Surf = SurfaceKind.RoofTiles;
                mb.HipRoof(c + new Vector3(2, 6, 0), new Vector3(12, 3.2f, 20), roof, 0.8f);
                mb.Surf = SurfaceKind.Auto;
                // Awnings along the front, striped by stall.
                Color[] stripes = { Palette.Hex(0xD9483A), Palette.Hex(0xF2F0E6), Palette.Hex(0x2F6FA8), Palette.Hex(0xE2B13C) };
                for (int i = 0; i < 4; i++)
                {
                    float z = c.z - 7.5f + i * 5;
                    mb.Box(new Vector3(c.x - 5.4f, c.y + 3.2f, z), new Vector3(3, 0.16f, 4.4f), stripes[i]);
                    mb.Box(new Vector3(c.x - 5.6f, c.y + 0.6f, z), new Vector3(2, 1.2f, 3.8f), Palette.Wood, Palette.Hex(0x9FB4C4));
                    mb.Cylinder(new Vector3(c.x - 6.8f, c.y, z - 2.1f), new Vector3(c.x - 6.8f, c.y + 3.2f, z - 2.1f), 0.07f, 6, Palette.Steel);
                }
            });
        }

        public static void Surroundings(Transform parent, string harbour)
        {
            Models.Built("Surroundings", parent, mb =>
            {
                var rnd = new System.Random(harbour.GetHashCode());
                System.Func<float, float, float> R = (a, b) => a + (float)rnd.NextDouble() * (b - a);
                Color leaf = Palette.Hex(0x4F8A3C), leaf2 = Palette.Hex(0x3C7240), trunk = Palette.Hex(0x6A4A30);
                // Trees behind the quay and round the market, kept off the road and the buildings.
                for (int i = 0; i < 90; i++)
                {
                    float x = R(12, 120), z = R(-120, 120);
                    if (Mathf.Abs(z) < 16 && x < 84) continue;
                    if (x < 34 && Mathf.Abs(z) < 50) continue;
                    float hgt = R(4, 8), y = Layout.QuayY;
                    mb.Surf = SurfaceKind.Bark;
                    mb.Cylinder(new Vector3(x, y, z), new Vector3(x, y + hgt * 0.45f, z), 0.28f, 6, trunk);
                    mb.Surf = SurfaceKind.None;
                    var col = i % 3 == 0 ? leaf2 : leaf;
                    mb.Cylinder(new Vector3(x, y + hgt * 0.3f, z), new Vector3(x, y + hgt * 0.75f, z), hgt * 0.32f, hgt * 0.2f, 8, col);
                    mb.Cylinder(new Vector3(x, y + hgt * 0.6f, z), new Vector3(x, y + hgt, z), hgt * 0.22f, 0.02f, 8, MeshBuilder.Lighten(col, 0.08f));
                }
                // Rocks at the ends of the quay.
                mb.Surf = SurfaceKind.Stone;
                for (int i = 0; i < 26; i++)
                {
                    float z = (i % 2 == 0 ? 1 : -1) * R(49, 75), x = R(-7, 4);
                    float r = R(1.2f, 3.4f);
                    mb.Sphere(new Vector3(x, R(0, 0.8f), z), new Vector3(r, r * 0.7f, r * R(0.8f, 1.3f)), 7, Palette.Hex(0x7E7C78), false);
                }
                mb.Surf = SurfaceKind.Auto;
                // Mooring bollards and lamp posts along the quay.
                for (float z = -44; z <= 44; z += 11)
                {
                    mb.Shiny();
                    mb.Cylinder(new Vector3(0.6f, Layout.QuayY, z + 5.5f), new Vector3(0.6f, Layout.QuayY + 0.7f, z + 5.5f), 0.22f, 8, Palette.Black);
                    mb.Matte();
                    mb.Cylinder(new Vector3(8.4f, Layout.QuayY, z), new Vector3(8.4f, Layout.QuayY + 4.2f, z), 0.08f, 6, Palette.SteelDark);
                    mb.Lit();
                    mb.Sphere(new Vector3(8.4f, Layout.QuayY + 4.3f, z), Vector3.one * 0.28f, 8, new Color(1f, 0.92f, 0.7f, 0));
                    mb.Matte();
                }
            });
        }
    }
}

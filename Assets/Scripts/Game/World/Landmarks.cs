using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The one thing each water has that no other does, so arriving somewhere new looks new: Stillwater's boathouse on
    /// stilts, the Alder's water mill, the whitewashed cottages of Gull Coast, Lantern Reef's stilt huts and lanterns, the
    /// Deep's container crane, and Frostmere's timber church with racks of stockfish.
    /// </summary>
    public static class Landmarks
    {
        public static void Build(Transform parent, string harbour)
        {
            switch (harbour)
            {
                case "lake": Boathouse(parent); break;
                case "river": Mill(parent); break;
                case "coast": Cottages(parent); break;
                case "reef": StiltHuts(parent); break;
                case "deep": Gantry(parent); break;
                case "arctic": Church(parent); break;
            }
        }

        static float Y => Layout.QuayY;

        static void Boathouse(Transform parent)
        {
            var c = Layout.WaterMark;
            Models.Built("Boathouse", parent, mb =>
            {
                Color wall = Palette.Hex(0x7A4A34), roof = Palette.Hex(0x3E4A56), trim = Palette.White;
                // Stilts, a deck and a shed open to the water at its front.
                mb.Surf = SurfaceKind.Bark;
                for (int i = 0; i < 4; i++) for (int k = 0; k < 3; k++) mb.Cylinder(new Vector3(c.x - 4 + i * 2.6f, -2, c.z - 3 + k * 3), new Vector3(c.x - 4 + i * 2.6f, 1.4f, c.z - 3 + k * 3), 0.2f, 6, Palette.Hex(0x6A4A30));
                mb.Surf = SurfaceKind.Planks;
                mb.Box(new Vector3(c.x, 1.45f, c.z), new Vector3(9, 0.2f, 7.4f), Palette.Hex(0xC8925A));
                mb.Box(new Vector3((c.x + 4.5f + 0.5f) / 2, Y - 0.05f, c.z), new Vector3(0.5f - (c.x + 4.5f), 0.2f, 2), Palette.Hex(0xC8925A));   // the walk to the shore
                mb.Surf = SurfaceKind.WoodGrain;
                mb.Box(new Vector3(c.x + 1.5f, 3.4f, c.z - 3.4f), new Vector3(5.5f, 3.8f, 0.2f), wall);
                mb.Box(new Vector3(c.x + 1.5f, 3.4f, c.z + 3.4f), new Vector3(5.5f, 3.8f, 0.2f), wall);
                mb.Box(new Vector3(c.x + 4.2f, 3.4f, c.z), new Vector3(0.2f, 3.8f, 7f), wall);
                mb.Surf = SurfaceKind.Shingles;
                mb.Push().Translate(c.x + 1.5f, 5.3f, c.z).RotateY(90);
                mb.GableRoof(Vector3.zero, new Vector3(7.2f, 2.4f, 5.8f), roof, wall, 0.5f);
                mb.Pop();
                mb.Surf = SurfaceKind.Auto;
                mb.Box(new Vector3(c.x - 1.3f, 4.6f, c.z), new Vector3(0.1f, 0.3f, 7f), trim);
            });
            Models.Kit("canoe", parent, new Vector3(c.x + 0.5f, 0.05f, c.z - 1.2f), 0, 5.2f).AddComponent<Bobber>();
            Models.Kit("canoe", parent, new Vector3(c.x - 6.2f, 0.05f, c.z + 1.8f), 12, 4.8f).AddComponent<Bobber>();
        }

        static void Mill(Transform parent)
        {
            // The mill stands on a stone footing out in the stream, its wheel turning in the current, a plank walk to shore.
            var c = new Vector3(-8, Y, Layout.WaterMark.z);
            Models.Built("Water mill", parent, mb =>
            {
                mb.Surf = SurfaceKind.Stone;
                mb.Box(new Vector3(c.x, (Y - 2) / 2, c.z), new Vector3(8, Y + 2, 9), Palette.Hex(0x9A9488));
                mb.Box(c + new Vector3(0, 3, 0), new Vector3(7, 6, 8), Palette.Hex(0xB8B0A0));
                mb.Surf = SurfaceKind.Shingles;
                mb.Push().Translate(c + new Vector3(0, 6, 0)).RotateY(90);
                mb.GableRoof(Vector3.zero, new Vector3(8, 3.2f, 7), Palette.Hex(0x4A7356), Palette.Hex(0xB8B0A0), 0.5f);
                mb.Pop();
                mb.Surf = SurfaceKind.Auto;
                mb.Box(c + new Vector3(3.55f, 1.4f, 2), new Vector3(0.1f, 2.6f, 1.6f), Palette.Hex(0x4A3424));
                mb.Lit(); mb.Box(c + new Vector3(3.55f, 4.2f, -1.6f), new Vector3(0.1f, 1.1f, 1.2f), new Color(0.75f, 0.86f, 0.92f, 0)); mb.Matte();
                mb.Surf = SurfaceKind.Planks;
                mb.Box(new Vector3((c.x + 4 + 0.5f) / 2, Y - 0.05f, c.z + 2), new Vector3(0.5f - (c.x + 4), 0.2f, 1.8f), Palette.Wood);
                mb.Surf = SurfaceKind.Auto;
            });
            var wheel = Models.Built("Mill wheel", parent, mb =>
            {
                mb.Surf = SurfaceKind.WoodGrain;
                const float R = 3.1f;
                for (int i = 0; i < 12; i++)
                {
                    float a = i * Mathf.PI / 6, b = (i + 1) * Mathf.PI / 6;
                    foreach (float side in new[] { -0.55f, 0.55f })
                        mb.Cylinder(new Vector3(side, Mathf.Cos(a) * R, Mathf.Sin(a) * R), new Vector3(side, Mathf.Cos(b) * R, Mathf.Sin(b) * R), 0.12f, 5, Palette.Hex(0x6A4A30));
                    mb.Box(new Vector3(0, Mathf.Cos(a) * R, Mathf.Sin(a) * R), new Vector3(1.2f, 0.12f + Mathf.Abs(Mathf.Sin(a)) * 0.5f, 0.12f + Mathf.Abs(Mathf.Cos(a)) * 0.5f), Palette.Wood);
                    mb.Cylinder(Vector3.zero, new Vector3(0, Mathf.Cos(a) * R, Mathf.Sin(a) * R), 0.07f, 4, Palette.Hex(0x6A4A30));
                }
                mb.Cylinder(new Vector3(-0.9f, 0, 0), new Vector3(0.9f, 0, 0), 0.3f, 8, Palette.SteelDark);
                mb.Surf = SurfaceKind.Auto;
            });
            wheel.transform.position = new Vector3(c.x - 4.8f, 0.9f, c.z - 1);
            wheel.AddComponent<Spin>().DegreesPerSecond = new Vector3(-28, 0, 0);
            Models.Kit("crate", parent, new Vector3(c.x + 4.6f, Y, c.z + 3.6f), 20, 1.3f);
            Models.Kit("barrel", parent, new Vector3(c.x + 4.4f, Y, c.z - 3.2f), 0, 1.2f);
        }

        static void Cottages(Transform parent)
        {
            var c = Layout.Landmark;
            Color[] doors = { Palette.Hex(0x2F6FA8), Palette.Hex(0x3E8A6A), Palette.Hex(0xD9503E), Palette.Hex(0xE2B13C), Palette.Hex(0x2F6FA8) };
            Models.Built("Cottages", parent, mb =>
            {
                for (int i = 0; i < 5; i++)
                {
                    var p = c + new Vector3(-8 + i * 4.3f, 0, (i % 2) * 2.4f - 1);
                    mb.Surf = SurfaceKind.Brick;
                    mb.Box(p + new Vector3(0, 1.6f, 0), new Vector3(3.8f, 3.2f, 4.6f), Palette.Hex(0xF4F0E6));
                    mb.Surf = SurfaceKind.RoofTiles;
                    mb.GableRoof(p + new Vector3(0, 3.2f, 0), new Vector3(3.8f, 1.8f, 4.6f), Palette.Hex(0xC2643E), Palette.Hex(0xF4F0E6), 0.35f);
                    mb.Surf = SurfaceKind.Auto;
                    mb.Box(p + new Vector3(0.4f, 1, -2.32f), new Vector3(0.9f, 2, 0.08f), doors[i]);
                    mb.Lit(); mb.Box(p + new Vector3(-1f, 2.1f, -2.32f), new Vector3(0.8f, 0.8f, 0.08f), new Color(0.75f, 0.86f, 0.92f, 0)); mb.Matte();
                    mb.Surf = SurfaceKind.Brick; mb.Box(p + new Vector3(1.2f, 4.9f, 1.2f), new Vector3(0.5f, 1.4f, 0.5f), Palette.Hex(0xE8E2D4)); mb.Surf = SurfaceKind.Auto;
                }
            });
            // Crab pots stacked along the quay.
            Models.Built("Crab pots", parent, mb =>
            {
                for (int s = 0; s < 3; s++)
                    for (int l = 0; l < 3; l++)
                        for (int k = 0; k < 3 - l; k++)
                        {
                            var p = new Vector3(7.6f, Y + 0.35f + l * 0.66f, -30 + s * 26 + k * 0.9f + l * 0.45f);
                            mb.Box(p, new Vector3(0.8f, 0.6f, 0.8f), Palette.Hex(0x2B3A4A), Palette.Hex(0x3E8A6A));
                            mb.Box(p + new Vector3(0, 0.32f, 0), new Vector3(0.82f, 0.05f, 0.82f), Palette.Hex(0xD9503E));
                        }
            });
        }

        static void StiltHuts(Transform parent)
        {
            var c = Layout.WaterMark;
            Models.Built("Stilt huts", parent, mb =>
            {
                Vector3[] at = { c + new Vector3(-3, 0, -3), c + new Vector3(3.5f, 0, 3), c + new Vector3(-4, 0, 6.5f) };
                foreach (var p in at)
                {
                    mb.Surf = SurfaceKind.Bark;
                    foreach (float dx in new[] { -1.4f, 1.4f }) foreach (float dz in new[] { -1.4f, 1.4f }) mb.Cylinder(p + new Vector3(dx, -2, dz), p + new Vector3(dx, 1.6f, dz), 0.14f, 6, Palette.Hex(0x8A6A48));
                    mb.Surf = SurfaceKind.Planks; mb.Box(p + new Vector3(0, 1.65f, 0), new Vector3(4, 0.16f, 4), Palette.Hex(0xD6B07A));
                    mb.Surf = SurfaceKind.WoodGrain; mb.Box(p + new Vector3(0, 2.9f, 0), new Vector3(2.8f, 2.4f, 2.8f), Palette.Hex(0xEAD6AE));
                    mb.Surf = SurfaceKind.Shingles; mb.HipRoof(p + new Vector3(0, 4.1f, 0), new Vector3(3.2f, 2.4f, 3.2f), Palette.Hex(0xD6B464), 0.7f, 0.05f);
                    mb.Surf = SurfaceKind.Auto;
                    mb.Box(p + new Vector3(-1.42f, 2.6f, 0), new Vector3(0.06f, 1.8f, 0.9f), Palette.Hex(0x5A3A28));
                    mb.Lit(); mb.Sphere(p + new Vector3(-1.9f, 3.9f, 1.2f), Vector3.one * 0.28f, 8, new Color(1f, 0.6f, 0.25f, 0)); mb.Matte();
                }
                // Boardwalks joining them to each other and the shore.
                mb.Surf = SurfaceKind.Planks;
                mb.Box((at[0] + at[1]) / 2 + Vector3.up * 1.6f, new Vector3(1.2f, 0.12f, (at[1] - at[0]).magnitude), Palette.Hex(0xD6B07A));
                mb.Box((at[0] + at[2]) / 2 + Vector3.up * 1.6f, new Vector3(1.2f, 0.12f, (at[2] - at[0]).magnitude), Palette.Hex(0xD6B07A));
                var shore = new Vector3(2, Y - 0.05f, at[1].z);
                mb.Box((at[1] + shore) / 2 + Vector3.up * 0.8f, new Vector3((shore - at[1]).magnitude, 0.12f, 1.2f), Palette.Hex(0xD6B07A));
                mb.Surf = SurfaceKind.Auto;
            });
            // Lanterns strung along the quay: the reef's name.
            Models.Built("Lantern strings", parent, mb =>
            {
                Color[] glow = { new Color(1f, 0.55f, 0.25f, 0), new Color(1f, 0.85f, 0.35f, 0), new Color(0.95f, 0.35f, 0.4f, 0) };
                Color[] paper = { Palette.Hex(0xE8763A), Palette.Hex(0xF2C443), Palette.Hex(0xD9503E) };
                for (float z0 = -40; z0 < 40; z0 += 16)
                    for (int i = 1; i < 8; i++)
                    {
                        float t = i / 8f, sag = Mathf.Sin(t * Mathf.PI) * 1.2f;
                        var p = new Vector3(8.6f, Y + 3.6f - sag, z0 + t * 16);
                        int k = (i + (int)((z0 + 40) / 16)) % 3;
                        mb.Matte(); mb.Sphere(p, new Vector3(0.26f, 0.34f, 0.26f), 8, paper[k]);            // the paper, by day
                        mb.Lit(); mb.Sphere(p, new Vector3(0.275f, 0.1f, 0.275f), 8, glow[k]); mb.Matte();   // a band that glows at night
                        mb.Box(p + Vector3.up * 0.36f, new Vector3(0.14f, 0.08f, 0.14f), Palette.Hex(0x2B3A4A));
                    }
            });
        }

        static void Gantry(Transform parent)
        {
            var c = new Vector3(2f, Y, 45);
            Models.Built("Container crane", parent, mb =>
            {
                Color blue = Palette.Hex(0x2F6FA8);
                mb.Paint();
                foreach (float x in new[] { -1.5f, 5f }) foreach (float z in new[] { -3f, 3f })
                    mb.Box(c + new Vector3(x, 7, z), new Vector3(0.6f, 14, 0.6f), blue);
                foreach (float z in new[] { -3f, 3f }) mb.Box(c + new Vector3(1.75f, 13.6f, z), new Vector3(7.1f, 0.6f, 0.6f), blue);
                mb.Box(c + new Vector3(-6, 15, 0), new Vector3(22, 1, 1.4f), blue);       // the boom out over the water
                mb.Box(c + new Vector3(1.5f, 14.4f, 0), new Vector3(3, 2, 6.4f), Palette.White);       // the machinery house
                mb.Box(c + new Vector3(-12, 14, 0), new Vector3(1.6f, 1.2f, 1.6f), Palette.PaintYellow);      // the trolley
                mb.Cylinder(c + new Vector3(-12, 13.4f, 0), c + new Vector3(-12, 7, 0), 0.05f, 4, Palette.Black);
                mb.Matte();
                foreach (float x in new[] { -1.5f, 5f }) foreach (float z in new[] { -3f, 3f })
                    mb.Box(c + new Vector3(x, 0.3f, z), new Vector3(1.4f, 0.6f, 1.6f), Palette.SteelDark);
            });
            // Stacked containers and the fuel tanks on the rise behind the hall.
            var l = Layout.Landmark;
            string[] kinds = { "container_red", "container", "container_red", "container" };
            for (int i = 0; i < 4; i++)
                for (int k = 0; k < 2 + i % 2; k++)
                    Models.Kit(kinds[(i + k) % 4], parent, l + new Vector3(-6 + i * 3.2f, k * 2.6f, -3), 0, 6f, k % 2 == 0 ? new Color(1.2f, 1.2f, 1.2f) : new Color(1.4f, 1.45f, 1.6f));
            Models.Kit("tank", parent, l + new Vector3(6, 0, 4), 0, 7f, new Color(1.6f, 1.6f, 1.6f));
            Models.Kit("tank", parent, l + new Vector3(1, 0, 6), 0, 6f, new Color(1.6f, 1.6f, 1.6f));
        }

        static void Church(Transform parent)
        {
            var c = Layout.Landmark;
            Models.Built("Church", parent, mb =>
            {
                Color wall = Palette.Hex(0x9A3A30), snow = Palette.Hex(0xF2F6FA), trim = Palette.White;
                mb.Surf = SurfaceKind.WoodGrain;
                mb.Box(c + new Vector3(0, 2.5f, 0), new Vector3(6, 5, 10), wall);
                mb.Surf = SurfaceKind.None;
                mb.GableRoof(c + new Vector3(0, 5, 0), new Vector3(6, 3, 10), snow, wall, 0.5f);
                mb.Surf = SurfaceKind.WoodGrain;
                // The tower at the west end with its spire.
                mb.Box(c + new Vector3(0, 5, -6), new Vector3(3, 10, 3), wall);
                mb.Surf = SurfaceKind.None;
                mb.HipRoof(c + new Vector3(0, 10, -6), new Vector3(3, 5.5f, 3), snow, 0.25f, 0.02f);
                mb.Surf = SurfaceKind.Auto;
                mb.Box(c + new Vector3(0, 16.2f, -6), new Vector3(0.12f, 1.4f, 0.12f), Palette.SteelDark);
                mb.Box(c + new Vector3(0, 16.5f, -6), new Vector3(0.12f, 0.12f, 0.7f), Palette.SteelDark);
                mb.Lit();
                for (int i = 0; i < 3; i++) foreach (float side in new[] { -3.04f, 3.04f }) mb.Box(c + new Vector3(side, 2.8f, -2.5f + i * 2.8f), new Vector3(0.08f, 1.6f, 0.8f), new Color(1f, 0.85f, 0.5f, 0));
                mb.Matte();
                for (int i = 0; i < 4; i++) foreach (float x in new[] { -3.1f, 3.1f }) mb.Box(c + new Vector3(x, 2.5f, -4.9f + i * 3.3f), new Vector3(0.15f, 5, 0.15f), trim);
            });
            // Racks of stockfish drying in the cold air, down toward the quay.
            Models.Built("Stockfish racks", parent, mb =>
            {
                for (int r = 0; r < 3; r++)
                {
                    float x = 12.5f + r * 4.2f, z0 = 25, z1 = 32;
                    mb.Surf = SurfaceKind.Bark;
                    for (float z = z0; z <= z1 + 0.01f; z += 3.5f)
                    {
                        mb.Cylinder(new Vector3(x - 1, Y, z), new Vector3(x, Y + 3, z), 0.1f, 5, Palette.Hex(0x6A4A30));
                        mb.Cylinder(new Vector3(x + 1, Y, z), new Vector3(x, Y + 3, z), 0.1f, 5, Palette.Hex(0x6A4A30));
                    }
                    mb.Cylinder(new Vector3(x, Y + 2.95f, z0), new Vector3(x, Y + 2.95f, z1), 0.08f, 5, Palette.Hex(0x6A4A30));
                    mb.Surf = SurfaceKind.Auto;
                    for (float z = z0 + 0.4f; z < z1; z += 0.45f)
                        foreach (float side in new[] { -0.22f, 0.22f })
                            mb.Sphere(new Vector3(x + side, Y + 2.3f, z), new Vector3(0.09f, 0.6f, 0.14f), 6, Palette.Hex(0xC9B48A));
                }
            });
        }
    }

    /// <summary>Turns steadily (a mill wheel).</summary>
    public sealed class Spin : MonoBehaviour
    {
        public Vector3 DegreesPerSecond;
        void Update() { transform.Rotate(DegreesPerSecond * Time.deltaTime, Space.Self); }
    }
}

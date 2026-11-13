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
            var kit = Models.Kit("box", parent, Vector3.zero, 0, 1.25f);
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
            if (Resources.Load<Mesh>("Models/hand_0_idle") != null)
            {
                var go = new GameObject("Hauler");
                go.transform.SetParent(parent, false);
                var st = go.AddComponent<Strider>();
                string who = "hand_" + index % 3 + "_";
                st.Idle = Models.Kit(who + "idle", go.transform, Vector3.zero, 0, 1.9f);
                for (int i = 0; i < 4; i++) st.Steps[i] = Models.Kit(who + "w" + i, go.transform, Vector3.zero, 0, 1.9f);
                st.Show(false, 0);
                return go;
            }
            return Models.Built("Hauler", parent, mb =>
            {
                Color coat = index % 2 == 0 ? Palette.Hex(0xE8A22A) : Palette.Hex(0x3C7AC8), skin = Palette.Hex(0xE0B090), boots = Palette.Hex(0x2C3A30);
                mb.Box(new Vector3(-0.14f, 0.4f, 0), new Vector3(0.2f, 0.8f, 0.24f), boots);
                mb.Box(new Vector3(0.14f, 0.4f, 0), new Vector3(0.2f, 0.8f, 0.24f), boots);
                mb.Box(new Vector3(0, 1.15f, 0), new Vector3(0.56f, 0.72f, 0.34f), coat);
                mb.Sphere(new Vector3(0, 1.72f, 0), Vector3.one * 0.21f, 10, skin);
            });
        }

        public static GameObject Truck(Transform parent)
        {
            var kit = Models.Kit("truck", parent, Vector3.zero, 0, 6.2f);
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

        public static float HallYaw = 90, MarketYaw = -90;
        /// <summary>How much the industrial kit's dark buildings are brightened, a little toward warm.</summary>
        static readonly Color Lift = new Color(1.75f, 1.62f, 1.5f);

        public static void Hall(Transform parent, HarbourState h)
        {
            var hall = Models.Kit("hall", parent, new Vector3(Layout.HallCentre.x, Layout.QuayY, Layout.HallCentre.z), HallYaw, 21f);
            if (hall != null)
            {
                hall.GetComponent<MeshRenderer>().sharedMaterial = Materials.KitTint(Lift);
                Models.Kit("tank", parent, new Vector3(Layout.HallCentre.x + 2, Layout.QuayY, Layout.HallCentre.z + 16), 0, 6f);
                Models.Kit("container", parent, new Vector3(Layout.HallCentre.x + 9, Layout.QuayY, Layout.HallCentre.z + 15), 90, 6f);
                for (int i = 0; i < 4; i++) Models.Kit("barrel", parent, Layout.HallDoor + new Vector3(-1.2f + i % 2 * 1.3f, 0, -6.5f - i / 2 * 1.3f), i * 40, 1.3f);
                return;
            }
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
            var market = Models.Kit("market", parent, new Vector3(Layout.MarketCentre.x + 4, Layout.QuayY, Layout.MarketCentre.z + 2), MarketYaw, 19f);
            if (market != null)
            {
                market.GetComponent<MeshRenderer>().sharedMaterial = Materials.KitTint(Lift);
                for (int i = 0; i < 4; i++)
                {
                    float z = Layout.MarketCentre.z - 9f + i * 5.2f, x = Layout.MarketCentre.x - 8.5f;
                    Models.Kit("stall", parent, new Vector3(x, Layout.QuayY, z), 0, 3.6f);
                    Models.Kit(i % 2 == 0 ? "box_open" : "box", parent, new Vector3(x - 0.2f, Layout.QuayY, z + 0.6f), 90, 1.5f);
                    Models.Kit("fish", parent, new Vector3(x - 0.2f, Layout.QuayY + 0.95f, z - 0.9f), 30 + i * 50, 1.3f);
                    Models.Kit("barrel", parent, new Vector3(x + 1.2f, Layout.QuayY, z - 1.2f), 0, 1.2f);
                }
                Models.Kit("flag", parent, new Vector3(Layout.MarketCentre.x - 8.5f, Layout.QuayY, Layout.MarketCentre.z + 13), 0, 8f);
                return;
            }
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
            if (Resources.Load<Mesh>("Models/tree_a") != null) { KitSurroundings(parent, harbour); return; }
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

        static void KitSurroundings(Transform parent, string harbour)
        {
            var rnd = new System.Random(harbour.GetHashCode());
            System.Func<float, float, float> R = (a, b) => a + (float)rnd.NextDouble() * (b - a);
            var th = Theme.For(harbour);
            string[] trees = th.Trees;
            float y = Layout.QuayY;
            for (int i = 0; i < 150; i++)
            {
                float x = R(11, 150), z = R(-130, 130);
                if (Mathf.Abs(z + 5) < 7 && x < 66) continue;                                   // the road
                if (x < 42 && Mathf.Abs(z) < 24) continue;                                      // the hall and its yard
                if (x > 52 && x < 92 && Mathf.Abs(z - 3) < 20) continue;                        // the market
                if (x < 16 && Mathf.Abs(z) < 50) continue;                                      // the quay
                int kind = rnd.Next(100);
                if (kind < 62) { string tr = trees[rnd.Next(trees.Length)]; Models.Kit(tr, parent, new Vector3(x, y, z), R(0, 360), tr.StartsWith("rock") || tr.StartsWith("bush") ? R(3f, 6f) : R(6.5f, 11f)); }
                else if (kind < 80) Models.Kit(rnd.Next(2) == 0 ? "bush" : "bush_b", parent, new Vector3(x, y, z), R(0, 360), R(1.6f, 3f));
                else if (kind < 90) Models.Kit(rnd.Next(2) == 0 ? "rock_a" : "rock_d", parent, new Vector3(x, y, z), R(0, 360), R(1.5f, 3.5f));
                else if (!th.Ice) Models.Kit(rnd.Next(2) == 0 ? "flowers" : "grass", parent, new Vector3(x, y, z), R(0, 360), R(0.9f, 1.6f));
            }
            // Flowers and grass tufts near the buildings, where the eye rests.
            for (int i = 0; i < 40 && !th.Ice; i++)
                Models.Kit(i % 3 == 0 ? "flowers" : "grass", parent, new Vector3(R(11, 60), y, R(8, 30) * (i % 2 == 0 ? 1 : -1)), R(0, 360), R(0.8f, 1.4f));
            // Rocks where the quay ends, in and out of the water.
            for (int i = 0; i < 22; i++)
            {
                float z = (i % 2 == 0 ? 1 : -1) * R(49, 80), x = R(-9, 5);
                Models.Kit(i % 3 == 0 ? "rock_c" : i % 3 == 1 ? "rock_b" : "rocks_a", parent, new Vector3(x, x < 0 ? -0.6f : y - 0.3f, z), R(0, 360), R(3f, 7f));
            }
            // Buoys marking the channel, and lily pads in the shallows of the lake.
            for (int i = 0; i < 7; i++) Models.Kit("buoy", parent, new Vector3(-22 - i % 2 * 26, 0, -52 + i * 17), R(0, 360), 2.4f).AddComponent<Bobber>();
            if (th.Ice)
                for (int i = 0; i < 30; i++) Models.Built("Floe", parent, mb => { mb.Mat(0.02f); mb.Cylinder(Vector3.zero, new Vector3(0, 0.5f, 0), 1, 0.85f, 7, Palette.Hex(0xEAF4FA)); }).transform.SetPositionAndRotation(new Vector3(R(-70, -16), -0.15f, R(-70, 70)), Quaternion.Euler(0, R(0, 360), 0));
            if (th.Ice) foreach (Transform t in parent) if (t.name == "Floe") { float k = R(1.5f, 5f); t.localScale = new Vector3(k, 1, k * R(0.7f, 1.2f)); }
            if (th.Lilies)
                for (int i = 0; i < 26; i++) Models.Kit("lily", parent, new Vector3(R(-9, -1.5f), 0.05f, R(-48, 48)), R(0, 360), R(0.8f, 1.5f));
            // Barrels, crates and lamp posts along the quay.
            for (float z = -44; z <= 44; z += 11)
            {
                Models.Kit("barrel", parent, new Vector3(8.2f, y, z + 1.2f), 0, 1.2f);
                if (((int)z & 1) == 0) Models.Kit("crate", parent, new Vector3(8.1f, y, z - 0.4f), 20, 1.3f);
            }
            Models.Kit("signpost", parent, new Vector3(9.5f, y, -11), 200, 2.4f);
            Models.Kit("log_stack", parent, new Vector3(12, y, 28), 80, 3.4f);
            Models.Kit("shed", parent, new Vector3(20, y, -34), 90, 12f).GetComponent<MeshRenderer>().sharedMaterial = Materials.KitTint(Lift);
            Models.Kit("tower", parent, new Vector3(4, y, 60), 0, 13f);
        }
    }

    /// <summary>A walker drawn as a few frozen strides, swapped as it goes.</summary>
    public sealed class Strider : MonoBehaviour
    {
        public GameObject Idle; public readonly GameObject[] Steps = new GameObject[4];
        public void Show(bool walking, float phase)
        {
            int k = walking ? Mathf.FloorToInt(Mathf.Repeat(phase, 1f) * 4) % 4 : -1;
            if (Idle != null && Idle.activeSelf != (k < 0)) Idle.SetActive(k < 0);
            for (int i = 0; i < 4; i++) if (Steps[i] != null && Steps[i].activeSelf != (i == k)) Steps[i].SetActive(i == k);
        }
    }

    /// <summary>Something afloat, rocking gently.</summary>
    public sealed class Bobber : MonoBehaviour
    {
        float _seed;
        void Start() { _seed = transform.position.x * 0.37f + transform.position.z * 0.61f; }
        void Update()
        {
            var p = transform.position; p.y = Mathf.Sin(Time.time * 1.4f + _seed) * 0.1f - 0.05f; transform.position = p;
            transform.rotation = Quaternion.Euler(Mathf.Sin(Time.time * 1.1f + _seed) * 5, transform.eulerAngles.y, Mathf.Sin(Time.time * 0.8f + _seed * 2) * 5);
        }
    }
}

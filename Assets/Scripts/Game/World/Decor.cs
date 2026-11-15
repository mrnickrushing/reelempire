using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>
    /// What the player has added to the harbour: bunting along the quay (paint), the decorations collections leave
    /// (flower boxes, a night camp, canoes, a fish statue, pennants, barrels) and what the town builds as orders are
    /// filled (a café, benches, a bandstand, more stalls, a fountain). Free kit models where one fits, simple shapes
    /// otherwise.
    /// </summary>
    public static class Decor
    {
        static readonly Color Lift = new Color(1.55f, 1.5f, 1.45f);

        /// <summary>Triangular flags strung between the quay's lamp posts in a colour and white.</summary>
        public static void Bunting(Transform parent, Color colour)
        {
            Models.Built("Bunting", parent, mb =>
            {
                mb.Matte();
                float y = Layout.QuayY + 4.1f, x = 8.6f;
                var white = Palette.White;
                for (float z0 = -40; z0 < 40; z0 += 16)
                {
                    float z1 = z0 + 16; int flags = 9;
                    for (int i = 0; i < flags; i++)
                    {
                        float t0 = (i + 0.15f) / flags, t1 = (i + 0.85f) / flags, tm = (i + 0.5f) / flags;
                        float sag0 = Mathf.Sin(t0 * Mathf.PI) * 1.1f, sag1 = Mathf.Sin(t1 * Mathf.PI) * 1.1f, sagm = Mathf.Sin(tm * Mathf.PI) * 1.1f;
                        var a = new Vector3(x, y - sag0, Mathf.Lerp(z0, z1, t0));
                        var b = new Vector3(x, y - sag1, Mathf.Lerp(z0, z1, t1));
                        var c = new Vector3(x, y - sagm - 0.8f, Mathf.Lerp(z0, z1, tm));
                        var col = i % 2 == 0 ? colour : white;
                        mb.Tri(a, b, c, col); mb.Tri(b, a, c, col);
                    }
                    for (int i = 0; i < 12; i++)      // the line itself
                    {
                        float t0 = i / 12f, t1 = (i + 1) / 12f;
                        mb.Cylinder(new Vector3(x, y - Mathf.Sin(t0 * Mathf.PI) * 1.1f, Mathf.Lerp(z0, z1, t0)), new Vector3(x, y - Mathf.Sin(t1 * Mathf.PI) * 1.1f, Mathf.Lerp(z0, z1, t1)), 0.025f, 4, Palette.Hex(0x2B3A4A), false);
                    }
                }
            });
        }

        /// <summary>Everything in <paramref name="what"/> (Sim.Decor ids), where it belongs.</summary>
        public static void Build(Transform parent, List<string> what, int berths)
        {
            float y = Layout.QuayY;
            foreach (var id in what)
            {
                switch (id)
                {
                    case "flowers":
                    {
                        string[] kinds = { "flowers", "flowers_red", "flowers_purple" };
                        float[] zs = { -32, -16, 16, 32 };
                        for (int i = 0; i < zs.Length; i++)
                        {
                            Models.Built("Flower box", parent, mb => { mb.Matte(); mb.Box(new Vector3(9.7f, y + 0.25f, zs[i]), new Vector3(1.1f, 0.5f, 3.2f), Palette.Hex(0x8A5A3A)); });
                            for (int k = -1; k <= 1; k++) Models.Kit(kinds[(i + k + 3) % 3], parent, new Vector3(9.7f, y + 0.5f, zs[i] + k * 1f), i * 40 + k * 70, 1.1f);
                        }
                        break;
                    }
                    case "camp":
                        Models.Kit("tent", parent, new Vector3(14, y, 52), 200, 4.2f);
                        Models.Kit("campfire", parent, new Vector3(10.5f, y, 49), 0, 1.6f);
                        Models.Kit("log_stack", parent, new Vector3(9, y, 51.5f), 30, 1.8f);
                        break;
                    case "canoes":
                        for (int i = 0; i < 3; i++) Models.Kit("canoe", parent, new Vector3(6.5f, y + 0.05f + i * 0.02f, -54 - i * 2.2f), 90 + i * 4, 4.6f);
                        break;
                    case "statue":
                        Models.Built("Plinth", parent, mb =>
                        {
                            mb.Surf = SurfaceKind.Brick; mb.Box(new Vector3(5.5f, y + 0.9f, -34), new Vector3(2.4f, 1.8f, 2.4f), Palette.Hex(0xC9C1B0)); mb.Surf = SurfaceKind.Auto;
                            mb.Box(new Vector3(5.5f, y + 1.86f, -34), new Vector3(2.7f, 0.14f, 2.7f), Palette.Hex(0xB0A794));
                        });
                        Models.Kit("fish", parent, new Vector3(5.5f, y + 3.3f, -34), 60, 3.4f, new Color(1.5f, 1.2f, 0.55f));
                        break;
                    case "flags":
                        for (int b = 0; b < berths; b++) Models.Kit("flag", parent, new Vector3(-14.2f, y, Layout.BerthZ(b) + 1.1f), 0, 3.6f);
                        break;
                    case "barrels":
                        for (int i = 0; i < 5; i++) Models.Kit("barrel", parent, new Vector3(12.5f + (i % 3) * 1.1f, y + (i >= 3 ? 1.05f : 0), -11 + (i >= 3 ? 0.55f : 0)), i * 37, 1.1f);
                        break;
                    case "cafe":
                        Models.Built("Café", parent, mb =>
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                var c = new Vector3(42 + i * 4.5f, y, 7.5f);
                                mb.Matte();
                                mb.Cylinder(c, c + Vector3.up * 0.75f, 0.08f, 6, Palette.Hex(0x3A3A3A));
                                mb.Cylinder(c + Vector3.up * 0.75f, c + Vector3.up * 0.82f, 0.65f, 12, Palette.White);
                                foreach (var o in new[] { new Vector3(1, 0, 0), new Vector3(-1, 0, 0) }) mb.Box(c + o + Vector3.up * 0.25f, new Vector3(0.45f, 0.5f, 0.45f), Palette.Hex(0x2F6FA8));
                                mb.Cylinder(c + Vector3.up * 0.82f, c + Vector3.up * 2.6f, 0.04f, 5, Palette.Hex(0xEEEEEE));
                                mb.Cylinder(c + Vector3.up * 2.2f, c + Vector3.up * 2.75f, 1.5f, 0.05f, 10, i % 2 == 0 ? Palette.Hex(0xD9503E) : Palette.Hex(0xF2E6C8));
                            }
                        });
                        break;
                    case "benches":
                        Models.Built("Benches", parent, mb =>
                        {
                            foreach (float x in new[] { 40f, 52f })
                            {
                                var c = new Vector3(x, y, -12.5f);
                                mb.Matte();
                                mb.Box(c + new Vector3(0, 0.45f, 0), new Vector3(2.4f, 0.1f, 0.6f), Palette.Wood);
                                mb.Box(c + new Vector3(0, 0.85f, -0.28f), new Vector3(2.4f, 0.5f, 0.08f), Palette.Wood);
                                foreach (float dx in new[] { -1f, 1f }) mb.Box(c + new Vector3(dx, 0.22f, 0), new Vector3(0.1f, 0.45f, 0.55f), Palette.Hex(0x2B3A4A));
                            }
                        });
                        Models.Kit("flowers_red", parent, new Vector3(46, y, -13), 0, 1.4f);
                        Models.Kit("flowers_purple", parent, new Vector3(44, y, -13.5f), 50, 1.3f);
                        Models.Kit("bush", parent, new Vector3(56, y, -13), 10, 1.6f);
                        break;
                    case "bandstand":
                        Models.Built("Bandstand", parent, mb =>
                        {
                            var c = new Vector3(40, y, 22);
                            mb.Matte();
                            mb.Cylinder(c, c + Vector3.up * 0.6f, 3.6f, 8, Palette.Hex(0xE8DCC0));
                            for (int i = 0; i < 8; i++)
                            {
                                float a = i * Mathf.PI / 4; var p = c + new Vector3(Mathf.Cos(a) * 3.2f, 0.6f, Mathf.Sin(a) * 3.2f);
                                mb.Cylinder(p, p + Vector3.up * 3f, 0.1f, 6, Palette.White);
                            }
                            mb.Cylinder(c + Vector3.up * 3.6f, c + Vector3.up * 5.4f, 4.1f, 0.15f, 8, Palette.Hex(0x2F7A8A));
                            mb.Cylinder(c + Vector3.up * 3.5f, c + Vector3.up * 3.65f, 4.1f, 8, Palette.Hex(0xD9503E));
                        });
                        break;
                    case "stalls":
                        Models.Kit("market_stand_b", parent, new Vector3(57, y, 11), 180, 4.4f, Lift);
                        Models.Kit("market_stand", parent, new Vector3(61, y, 11.5f), 170, 4.2f, Lift);
                        Models.Kit("crate", parent, new Vector3(59, y, 9), 20, 1.1f);
                        break;
                    case "fountain":
                        Models.Built("Fountain", parent, mb =>
                        {
                            var c = new Vector3(46, y, -20);
                            mb.Surf = SurfaceKind.Brick;
                            mb.Cylinder(c, c + Vector3.up * 0.7f, 3f, 14, Palette.Hex(0xC9C1B0));
                            mb.Surf = SurfaceKind.Auto;
                            mb.Mat(0.9f, 0.1f);
                            mb.Cylinder(c + Vector3.up * 0.5f, c + Vector3.up * 0.62f, 2.7f, 14, Palette.Hex(0x4FA3C8));
                            mb.Matte();
                            mb.Cylinder(c + Vector3.up * 0.6f, c + Vector3.up * 2.2f, 0.35f, 10, Palette.Hex(0xB0A794));
                            mb.Cylinder(c + Vector3.up * 2.2f, c + Vector3.up * 2.4f, 1.1f, 12, Palette.Hex(0xC9C1B0));
                        });
                        break;
                }
            }
        }
    }
}

using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The working buildings, drawn so the production chain reads from the buildings themselves: a fish hall with a
    /// belt from the quay and, outside it, the equipment of every line the hall runs (ice tables, a cleaning trough, a
    /// fillet shed, a smokehouse with its chimneys, the cannery's tanks); a fish auction with boxes on the floor,
    /// buyers and an auctioneer's stand; and on the quay and the piers, the gear each milestone brings.
    /// </summary>
    public static class Facilities
    {
        static readonly Color Lift = new Color(1.55f, 1.5f, 1.45f);
        static Color Steel => Palette.Hex(0xB9C2CA);
        static Color Ice => Palette.Hex(0xEAF6FC);
        static Color FishBox => Palette.Hex(0x2F7FC0);

        /// <summary>The walls' paint for this water: the kit's barn red turned by the theme's hue.</summary>
        public static Color Walls(float hueTurn)
        {
            float h = Mathf.Repeat(0.012f + hueTurn, 1f);
            return Color.HSVToRGB(h, 0.5f, 0.82f);
        }

        // ---------------------------------------------------------------- the hall

        /// <summary>The fish hall, its lines and what its milestones have added. Returns the belt, run by the view.</summary>
        public static Belt Hall(Transform parent, HarbourState h, Theme th)
        {
            float y = Layout.QuayY;
            int m = Content.MilestonesAt(h.hall);
            bool Owns(int line) => (h.goodsOwned & (1 << line)) != 0;
            var wall = th.Wall ?? Walls(th.Paint);
            Color roof = th.Roof, trim = Palette.White, dark = Palette.Hex(0x23282F);
            const float x0 = 14, x1 = 29, z0 = -11, z1 = 11, wallH = 5.2f, roofH = 3.2f;
            float cx = (x0 + x1) / 2;

            Models.Built("Fish hall", parent, mb =>
            {
                // The shed: siding walls, a tin roof with its ridge along the quay.
                mb.Matte(); mb.Surf = th.WallSurf;
                mb.Box(new Vector3(cx, y + wallH / 2, 0), new Vector3(x1 - x0, wallH, z1 - z0), wall);
                mb.Surf = th.RoofSurf;
                mb.Push().Translate(cx, y + wallH, 0).RotateY(90);
                mb.GableRoof(Vector3.zero, new Vector3(z1 - z0, roofH, x1 - x0), roof, wall, 0.7f);
                mb.Pop();
                mb.Surf = SurfaceKind.Auto;
                // A big fish painted on the slope that faces the water, so the hall says what it is from far off.
                {
                    float run = (x1 - x0) / 2 + 0.7f, rise = roofH * run / ((x1 - x0) / 2 + 0.7f), ang = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
                    float u = 0.5f;                                      // halfway up the slope
                    var at = new Vector3(x0 - 0.7f + run * u, y + wallH + rise * u + 0.04f, 0);
                    mb.Push().Translate(at).Rotate(Quaternion.Euler(0, 0, ang));
                    mb.Matte();
                    var ink = roof.grayscale > 0.8f ? Palette.Hex(0x2F6FA8) : trim;     // on a snowy roof the fish is painted blue
                    mb.Sphere(new Vector3(0, 0, 1.2f), new Vector3(1.55f, 0.02f, 3.6f), 16, ink);
                    var t0 = new Vector3(0, 0.01f, -2.1f);
                    mb.Tri(t0, new Vector3(1.5f, 0.01f, -4f), new Vector3(-1.5f, 0.01f, -4f), ink);
                    mb.Tri(t0, new Vector3(-1.5f, 0.01f, -4f), new Vector3(1.5f, 0.01f, -4f), ink);
                    mb.Sphere(new Vector3(0.1f, 0.03f, 3.3f), new Vector3(0.3f, 0.02f, 0.3f), 8, ink == trim ? Palette.Hex(0x2F6FA8) : trim);
                    mb.Pop();
                }
                // A white band under the eaves and white corners.
                mb.Box(new Vector3(x0 - 0.06f, y + wallH - 0.25f, 0), new Vector3(0.14f, 0.5f, z1 - z0 + 0.2f), trim);
                foreach (float z in new[] { z0, z1 }) foreach (float x in new[] { x0, x1 })
                    mb.Box(new Vector3(x, y + wallH / 2, z), new Vector3(0.35f, wallH, 0.35f), trim);
                // Three roller doors onto the quay: the middle one open on the dark floor inside, with the belt going in.
                foreach (float z in new[] { -6.5f, 0f, 6.5f })
                {
                    bool open = z == 0 || m >= 2 && z < 0;
                    mb.Box(new Vector3(x0 - 0.05f, y + 1.9f, z), new Vector3(0.12f, 3.8f, 4f), open ? dark : Palette.Hex(0xC9CED3));
                    if (!open) for (int i = 1; i < 8; i++) mb.Box(new Vector3(x0 - 0.12f, y + i * 0.47f, z), new Vector3(0.04f, 0.05f, 3.9f), Palette.Hex(0x9AA2AA));
                    mb.Box(new Vector3(x0 - 0.2f, y + 4.05f, z), new Vector3(0.4f, 0.4f, 4.4f), open ? Palette.Hex(0xC9CED3) : trim);   // the rolled shutter
                }
                // Windows down the long side, lit at night.
                mb.Lit();
                for (float x = x0 + 2; x < x1 - 1; x += 3)
                    foreach (float side in new[] { -1f, 1f })
                        mb.Box(new Vector3(x, y + 3.4f, side * (z1 + 0.04f)), new Vector3(1.6f, 1f, 0.08f), new Color(0.75f, 0.86f, 0.92f, 0));
                mb.Matte();
                // The sign board over the doors: a white board with a blue fish.
                mb.Box(new Vector3(x0 - 0.25f, y + wallH + 0.9f, 0), new Vector3(0.2f, 1.5f, 6.4f), trim);
                mb.Box(new Vector3(x0 - 0.38f, y + wallH + 0.9f, 0), new Vector3(0.06f, 1.2f, 6f), Palette.Hex(0x2F6FA8));
                mb.Sphere(new Vector3(x0 - 0.45f, y + wallH + 0.9f, 0.3f), new Vector3(0.08f, 0.38f, 1.5f), 10, trim);
                var tail = new[] { new Vector3(x0 - 0.45f, y + wallH + 0.9f, -1.1f), new Vector3(x0 - 0.45f, y + wallH + 1.35f, -1.9f), new Vector3(x0 - 0.45f, y + wallH + 0.45f, -1.9f) };
                mb.Tri(tail[0], tail[1], tail[2], trim); mb.Tri(tail[0], tail[2], tail[1], trim);
            });

            // The belt from the quay in through the open door: fish boxes ride it while there is fish to work.
            var belt = BuildBelt(parent, new Vector3(8.6f, y + 0.95f, 0.6f), new Vector3(x0 + 2.5f, y + 0.95f, 0.6f));

            // Fresh: the ice tables by the north door, fish laid on ice, with ice bins beside them.
            Models.Built("Ice tables", parent, mb =>
            {
                for (int i = 0; i < 2; i++)
                {
                    float z = 6 + i * 2.6f, x = 12.6f;
                    mb.Shiny(); mb.Box(new Vector3(x, y + 0.9f, z), new Vector3(1.3f, 0.12f, 2.2f), Steel);
                    foreach (float dz in new[] { -0.95f, 0.95f }) foreach (float dx in new[] { -0.5f, 0.5f })
                        mb.Cylinder(new Vector3(x + dx, y, z + dz), new Vector3(x + dx, y + 0.86f, z + dz), 0.05f, 5, Steel);
                    mb.Matte(); mb.Box(new Vector3(x, y + 1.02f, z), new Vector3(1.15f, 0.14f, 2.05f), Ice);
                    for (int k = 0; k < 4; k++) mb.Sphere(new Vector3(x - 0.25f + (k % 2) * 0.5f, y + 1.13f, z - 0.6f + k * 0.4f), new Vector3(0.14f, 0.07f, 0.42f), 8, Palette.Hex(0xA9B8C4));
                }
                for (int i = 0; i < 3; i++)
                {
                    var c = new Vector3(10.6f, y + 0.4f, 6.2f + i * 1.4f);
                    mb.Paint(); mb.Box(c, new Vector3(1f, 0.8f, 1.1f), FishBox); mb.Matte();
                    mb.Box(c + new Vector3(0, 0.38f, 0), new Vector3(0.86f, 0.06f, 0.96f), Ice);
                }
            });
            // Cleaned: a steel trough with taps and hoses by the south door.
            if (Owns(1))
                Models.Built("Cleaning trough", parent, mb =>
                {
                    float x = 12.8f;
                    mb.Shiny();
                    mb.Box(new Vector3(x, y + 0.95f, -7.5f), new Vector3(1.2f, 0.5f, 5f), Steel, Palette.Hex(0x7FA6BC));
                    for (float z = -9.6f; z <= -5.4f; z += 1.4f) mb.Cylinder(new Vector3(x - 0.5f, y, z), new Vector3(x - 0.5f, y + 0.72f, z), 0.06f, 5, Steel);
                    for (float z = -9; z <= -6; z += 1.5f)
                    {
                        mb.Cylinder(new Vector3(x + 0.55f, y + 1.2f, z), new Vector3(x + 0.55f, y + 2f, z), 0.04f, 5, Steel);
                        mb.Cylinder(new Vector3(x + 0.55f, y + 2f, z), new Vector3(x + 0.15f, y + 1.85f, z), 0.04f, 5, Steel);
                    }
                    mb.Matte();
                    mb.Cylinder(new Vector3(x + 0.62f, y + 0.3f, -4.6f), new Vector3(x + 0.62f, y + 0.31f, -4.6f), 0.45f, 10, Palette.Hex(0x2E9E5A));    // a coiled green hose
                });
            // Fillets: a long, low shed on the hall's south side with a run of windows over the line inside.
            if (Owns(2))
                Models.Built("Fillet shed", parent, mb =>
                {
                    float sx0 = 16, sx1 = 28, sz0 = -16.5f, sz1 = z0;
                    mb.Surf = th.WallSurf; mb.Box(new Vector3((sx0 + sx1) / 2, y + 1.7f, (sz0 + sz1) / 2), new Vector3(sx1 - sx0, 3.4f, sz1 - sz0), MeshBuilder.Lighten(wall, 0.12f));
                    mb.Surf = th.RoofSurf; mb.Box(new Vector3((sx0 + sx1) / 2, y + 3.5f, (sz0 + sz1) / 2 - 0.2f), new Vector3(sx1 - sx0 + 0.6f, 0.2f, sz1 - sz0 + 0.8f), MeshBuilder.Lighten(roof, 0.1f));
                    mb.Surf = SurfaceKind.Auto; mb.Lit();
                    for (float x = sx0 + 1.5f; x < sx1 - 1; x += 2.2f) mb.Box(new Vector3(x, y + 2.2f, sz0 - 0.04f), new Vector3(1.6f, 0.8f, 0.08f), new Color(0.75f, 0.86f, 0.92f, 0));
                    mb.Matte();
                    // Ventilator caps along its roof.
                    for (float x = sx0 + 2; x < sx1; x += 4) { mb.Cylinder(new Vector3(x, y + 3.6f, (sz0 + sz1) / 2), new Vector3(x, y + 4.3f, (sz0 + sz1) / 2), 0.3f, 8, Steel); mb.Sphere(new Vector3(x, y + 4.35f, (sz0 + sz1) / 2), new Vector3(0.42f, 0.25f, 0.42f), 8, Steel); }
                });
            // Smoked: a brick smokehouse on the north side, two chimneys smoking.
            var smoke = new List<Vector3>();
            if (Owns(3))
            {
                Models.Built("Smokehouse", parent, mb =>
                {
                    float sx = 19.5f, sz = 15f;
                    mb.Surf = SurfaceKind.Brick; mb.Box(new Vector3(sx, y + 2.3f, sz), new Vector3(7, 4.6f, 7), Palette.Hex(0x9A5A44));
                    mb.Surf = th.Ice ? SurfaceKind.None : SurfaceKind.RoofTiles; mb.GableRoof(new Vector3(sx, y + 4.6f, sz), new Vector3(7, 2.2f, 7), th.Ice ? th.Roof : Palette.Hex(0x4A3A34), Palette.Hex(0x9A5A44), 0.4f);
                    mb.Surf = SurfaceKind.Brick;
                    foreach (float dx in new[] { -2f, 2f }) mb.Box(new Vector3(sx + dx, y + 6.6f, sz + 1.5f), new Vector3(1.1f, 4.2f, 1.1f), Palette.Hex(0x8A4A3A));
                    mb.Surf = SurfaceKind.Auto;
                    mb.Box(new Vector3(sx - 3.55f, y + 1.3f, sz), new Vector3(0.1f, 2.6f, 1.8f), Palette.Hex(0x3A2A24));    // its door
                    // Racks of fish hanging to smoke under a lean-to on its side.
                    mb.Box(new Vector3(sx + 4.6f, y + 2.6f, sz), new Vector3(2.2f, 0.14f, 6f), Palette.Wood);
                    for (float z = sz - 2.6f; z <= sz + 2.6f; z += 1.3f) mb.Cylinder(new Vector3(sx + 5.5f, y, z), new Vector3(sx + 5.5f, y + 2.6f, z), 0.07f, 5, Palette.Wood);
                    for (float z = sz - 2.4f; z <= sz + 2.4f; z += 0.6f) mb.Sphere(new Vector3(sx + 4.6f, y + 1.9f, z), new Vector3(0.1f, 0.42f, 0.16f), 6, Palette.Hex(0xC27A3A));
                });
                smoke.Add(new Vector3(17.5f, y + 8.9f, 16.5f)); smoke.Add(new Vector3(21.5f, y + 8.9f, 16.5f));
            }
            else smoke.Add(new Vector3(cx + 4, y + wallH + roofH + 1.2f, 6));       // the hall's stove pipe
            if (!Owns(3))
                Models.Built("Stove pipe", parent, mb => { mb.Matte(); mb.Cylinder(new Vector3(cx + 4, y + wallH + 1, 6), new Vector3(cx + 4, y + wallH + roofH + 1.2f, 6), 0.25f, 8, Palette.SteelDark); });
            foreach (var p in smoke) Life.Smoke(parent, p);
            // Canned: the cannery's tanks and a pipe into the hall, a stack of tins on pallets.
            if (Owns(4))
            {
                Models.Kit("tank", parent, new Vector3(26f, y, 15.5f), 0, 6.5f, Lift);
                Models.Kit("tank", parent, new Vector3(28.6f, y, 18.5f), 40, 5.2f, Lift);
                Models.Built("Cannery", parent, mb =>
                {
                    mb.Shiny();
                    mb.Cylinder(new Vector3(26f, y + 4.2f, 13.6f), new Vector3(26f, y + 4.2f, z1 + 0.1f), 0.22f, 8, Steel);
                    mb.Cylinder(new Vector3(26f, y + 4.2f, 13.6f), new Vector3(26f, y + 5.4f, 14.4f), 0.22f, 8, Steel);
                    mb.Matte();
                    for (int i = 0; i < 2; i++)
                    {
                        var p = new Vector3(23f + i * 0.0f, y, 13.2f + i * 2.4f);
                        mb.Box(p + new Vector3(0, 0.12f, 0), new Vector3(2f, 0.24f, 2f), Palette.WoodPale);
                        for (int k = 0; k < 9; k++)
                        {
                            float tx = (k % 3 - 1) * 0.6f, tz = (k / 3 - 1) * 0.6f;
                            for (int l = 0; l < 3 - (k % 2); l++)
                                mb.Cylinder(p + new Vector3(tx, 0.24f + l * 0.42f, tz), p + new Vector3(tx, 0.62f + l * 0.42f, tz), 0.26f, 10, l % 2 == 0 ? Palette.Hex(0xD9503E) : Palette.Hex(0xE8E2D0));
                        }
                    }
                });
            }

            // Milestones on the hall.
            Models.Built("Hall milestones", parent, mb =>
            {
                if (m >= 1)          // 10: striped awnings over the doors, so the work goes on in the rain
                {
                    foreach (float dz in new[] { -6.5f, 0f, 6.5f })
                        for (int i = 0; i < 6; i++)
                        {
                            float za = dz - 2.4f + i * 0.8f, zb = za + 0.8f;
                            Color c = i % 2 == 0 ? Palette.Hex(0x2F6FA8) : trim;
                            mb.Quad(new Vector3(x0, y + 4.5f, za), new Vector3(x0, y + 4.5f, zb), new Vector3(x0 - 2.2f, y + 3.7f, zb), new Vector3(x0 - 2.2f, y + 3.7f, za), c);
                            mb.Quad(new Vector3(x0 - 2.2f, y + 3.7f, za), new Vector3(x0 - 2.2f, y + 3.7f, zb), new Vector3(x0, y + 4.5f, zb), new Vector3(x0, y + 4.5f, za), MeshBuilder.Darken(c, 0.8f));
                            mb.Quad(new Vector3(x0 - 2.2f, y + 3.7f, za), new Vector3(x0 - 2.2f, y + 3.7f, zb), new Vector3(x0 - 2.2f, y + 3.3f, zb), new Vector3(x0 - 2.2f, y + 3.3f, za), c);
                            mb.Quad(new Vector3(x0 - 2.2f, y + 3.3f, za), new Vector3(x0 - 2.2f, y + 3.3f, zb), new Vector3(x0 - 2.2f, y + 3.7f, zb), new Vector3(x0 - 2.2f, y + 3.7f, za), c);
                        }
                }
                if (m >= 2)          // 25: turbine vents along the ridge
                    for (float z = -7.5f; z <= 7.5f; z += 5)
                    {
                        var p = new Vector3(cx, y + wallH + roofH - 0.1f, z);
                        mb.Shiny(); mb.Cylinder(p, p + Vector3.up * 0.7f, 0.3f, 8, Steel); mb.Sphere(p + Vector3.up * 0.85f, new Vector3(0.5f, 0.35f, 0.5f), 10, Steel); mb.Matte();
                    }
                if (m >= 3)          // 50: a hoist over the open door, a box of fish on its hook
                {
                    mb.Paint();
                    var top = new Vector3(x0 - 0.2f, y + wallH + 0.2f, 3.6f);
                    mb.Box(top + new Vector3(-1.6f, 0, 0), new Vector3(3.6f, 0.3f, 0.3f), Palette.PaintYellow);
                    mb.Cylinder(top + new Vector3(0.1f, -0.15f, 0), top + new Vector3(-1.4f, -1.4f, 0), 0.08f, 6, Palette.PaintYellow);
                    mb.Cylinder(top + new Vector3(-3.1f, -0.1f, 0), top + new Vector3(-3.1f, -2.4f, 0), 0.025f, 4, Palette.Black);
                    mb.Box(top + new Vector3(-3.1f, -2.85f, 0), new Vector3(0.9f, 0.7f, 0.9f), FishBox);
                    mb.Matte();
                }
                if (m >= 4)          // 100: a gilded fish on the ridge
                {
                    var p = new Vector3(cx, y + wallH + roofH, 0);
                    mb.Cylinder(p, p + Vector3.up * 1.5f, 0.1f, 6, Palette.SteelDark);
                    mb.Shiny();
                    mb.Sphere(p + Vector3.up * 2.4f, new Vector3(0.5f, 1.1f, 3.2f), 12, Palette.Hex(0xE8B23A));
                    var t0 = p + new Vector3(0, 2.4f, -2.8f);
                    mb.Tri(t0, t0 + new Vector3(0, 1.3f, -1.6f), t0 + new Vector3(0, -1.3f, -1.6f), Palette.Hex(0xE8B23A));
                    mb.Tri(t0, t0 + new Vector3(0, -1.3f, -1.6f), t0 + new Vector3(0, 1.3f, -1.6f), Palette.Hex(0xE8B23A));
                    mb.Matte();
                }
                if (m >= 6)          // 200: pennants down the ridge
                    for (float z = z0 + 1; z <= z1 - 1; z += 2.5f)
                    {
                        var p = new Vector3(cx, y + wallH + roofH + 1.6f, z);
                        mb.Cylinder(p + Vector3.down * 1.6f, p + Vector3.up * 0.6f, 0.04f, 4, Palette.SteelDark);
                        Color c = ((int)((z - z0) / 2.5f)) % 2 == 0 ? Palette.Hex(0xD9503E) : Palette.White;
                        mb.Tri(p + Vector3.up * 0.5f, p + new Vector3(0, 0.05f, 1.1f), p, c); mb.Tri(p + Vector3.up * 0.5f, p, p + new Vector3(0, 0.05f, 1.1f), c);
                    }
            });

            // A hand at the ice tables and, with the cleaning line, one at the trough.
            Figure(parent, 0, new Vector3(11.6f, y, 7.4f), 90);
            if (Owns(1)) Figure(parent, 1, new Vector3(11.7f, y, -8.2f), 90);
            if (m >= 3) Figure(parent, 2, new Vector3(10.6f, y, 4.6f), 60);
            return belt;
        }

        /// <summary>A dock hand standing still (the mini characters' idle pose).</summary>
        static void Figure(Transform parent, int who, Vector3 at, float yaw)
        {
            Models.Kit("hand_" + who % 3 + "_idle", parent, at, yaw, 1.9f);
        }

        static Belt BuildBelt(Transform parent, Vector3 from, Vector3 to)
        {
            var go = Models.Built("Belt", parent, mb =>
            {
                var d = to - from; float len = d.magnitude; var mid = (from + to) / 2;
                mb.Matte();
                mb.Box(mid, new Vector3(len, 0.12f, 1.1f), Palette.Hex(0x2B2F36), Palette.Hex(0x3A3F46));
                mb.Shiny();
                foreach (float side in new[] { -0.62f, 0.62f }) mb.Box(mid + new Vector3(0, 0.08f, side), new Vector3(len + 0.2f, 0.22f, 0.12f), Steel);
                for (float t = 0.4f; t < len; t += 2.2f)
                    foreach (float side in new[] { -0.5f, 0.5f })
                        mb.Cylinder(from + new Vector3(t, -0.08f, side) + Vector3.down * 0.0f, new Vector3(from.x + t, Layout.QuayY, from.z + side), 0.06f, 5, Steel);
                mb.Matte();
            });
            var belt = go.AddComponent<Belt>();
            belt.From = from + Vector3.up * 0.32f; belt.To = to + Vector3.up * 0.32f;
            for (int i = 0; i < 4; i++)
            {
                var box = Models.Built("Belt box", go.transform, mb =>
                {
                    mb.Paint(); mb.Box(Vector3.zero, new Vector3(0.75f, 0.45f, 0.8f), FishBox); mb.Matte();
                    mb.Box(new Vector3(0, 0.2f, 0), new Vector3(0.66f, 0.06f, 0.7f), Ice);
                    mb.Sphere(new Vector3(0.08f, 0.26f, 0), new Vector3(0.12f, 0.07f, 0.32f), 8, Palette.Hex(0xA9B8C4));
                });
                belt.Boxes.Add(box.transform);
            }
            return belt;
        }

        // ---------------------------------------------------------------- the market

        /// <summary>The fish auction: an open-fronted shed, boxes of fish laid out in front of it, buyers looking them
        /// over and the auctioneer on his stand; and the stalls along the road.</summary>
        public static void Market(Transform parent, HarbourState h, Theme th, Color? stallLook, float stallHue)
        {
            float y = Layout.QuayY;
            int m = Content.MilestonesAt(h.market);
            var wall = th.Wall.HasValue ? MeshBuilder.Lighten(th.Wall.Value, 0.08f) : Walls(th.Paint + 0.04f);
            Color roof = th.MarketRoof, trim = Palette.White;
            const float x0 = 69, x1 = 83, z0 = -1, z1 = 15;
            float cz = (z0 + z1) / 2;
            Models.Built("Auction", parent, mb =>
            {
                // The floor: a concrete slab running out in front of the shed.
                mb.Matte(); mb.Surf = SurfaceKind.Concrete;
                mb.Box(new Vector3((63.5f + x1) / 2, y + 0.06f, cz), new Vector3(x1 - 63.5f, 0.12f, z1 - z0 + 1), Palette.Hex(0xC9C4B8));
                // The shed: a closed back, an open front on posts, a long roof.
                mb.Surf = th.WallSurf;
                mb.Box(new Vector3(x1 - 0.2f, y + 2.6f, cz), new Vector3(0.4f, 5.2f, z1 - z0), wall);
                foreach (float z in new[] { z0, z1 }) mb.Box(new Vector3((x0 + x1) / 2 + 2, y + 2.6f, z), new Vector3(x1 - x0 - 4, 5.2f, 0.4f), wall);
                mb.Surf = SurfaceKind.Auto; mb.Paint();
                for (float z = z0; z <= z1 + 0.01f; z += (z1 - z0) / 4) mb.Cylinder(new Vector3(x0, y, z), new Vector3(x0, y + 5.2f, z), 0.16f, 8, trim);
                mb.Matte(); mb.Surf = th.RoofSurf == SurfaceKind.Tin ? SurfaceKind.RoofTiles : th.RoofSurf;
                mb.Push().Translate((x0 + x1) / 2, y + 5.2f, cz).RotateY(90);
                mb.GableRoof(Vector3.zero, new Vector3(z1 - z0, 2.6f, x1 - x0), roof, wall, 0.8f);
                mb.Pop();
                mb.Surf = SurfaceKind.Auto;
                // The auctioneer's stand at the front corner: a raised box with a rail and a brass bell.
                var st = new Vector3(x0 + 1.4f, y, z1 - 2);
                mb.Box(st + new Vector3(0, 0.7f, 0), new Vector3(1.8f, 1.4f, 1.8f), Palette.Wood, Palette.WoodPale);
                mb.Box(st + new Vector3(-0.85f, 1.95f, 0), new Vector3(0.1f, 1.1f, 1.8f), Palette.Wood);
                mb.Shiny(); mb.Cylinder(st + new Vector3(-0.9f, 2.5f, 0.7f), st + new Vector3(-0.9f, 2.9f, 0.7f), 0.22f, 0.08f, 10, Palette.Hex(0xD6A23A)); mb.Matte();
                // The price board on the back wall: a black board ruled into columns of chalk.
                var b = new Vector3(x1 - 0.45f, y + 3.2f, cz);
                mb.Box(b, new Vector3(0.12f, 2.2f, 7.5f), Palette.Hex(0x22302A));
                for (int r = 0; r < 5; r++) for (int c = 0; c < 3; c++) mb.Box(b + new Vector3(-0.08f, 0.75f - r * 0.38f, -2.6f + c * 2.6f), new Vector3(0.02f, 0.08f, 1.6f - (r + c) % 3 * 0.35f), Palette.Hex(0xE8E4D8));
                // Boxes of fish on the floor, in rows, some on ice and some still wet from the boats.
                Color[] catches = { Palette.Hex(0xA9B8C4), Palette.Hex(0xC9864A), Palette.Hex(0x8A9A6A), Palette.Hex(0xD9A0A0) };
                int rows = m >= 1 ? 4 : 3;
                for (int r = 0; r < rows; r++)
                    for (int k = 0; k < 5; k++)
                    {
                        var c = new Vector3(64.6f + r * 1.5f + (r >= 3 ? 1.6f : 0), y + 0.4f, 1.2f + k * 2.4f + (r % 2) * 0.6f);
                        mb.Paint(); mb.Box(c, new Vector3(1.1f, 0.5f, 1.4f), k % 3 == 2 ? Palette.Hex(0xE07A2A) : FishBox); mb.Matte();
                        mb.Box(c + new Vector3(0, 0.22f, 0), new Vector3(0.98f, 0.06f, 1.28f), Ice);
                        var col = catches[(r + k) % catches.Length];
                        for (int f = 0; f < 3; f++) mb.Sphere(c + new Vector3(-0.3f + f * 0.3f, 0.3f, 0), new Vector3(0.12f, 0.07f, 0.48f), 8, col);
                    }
                // A string of lights across the front (25).
                if (m >= 2)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        float t = i / 15f; float z = Mathf.Lerp(z0, z1, t), sag = Mathf.Sin(t * Mathf.PI) * 0.7f;
                        mb.Lit(); mb.Sphere(new Vector3(x0 - 0.1f, y + 4.6f - sag, z), Vector3.one * 0.13f, 6, new Color(1f, 0.85f, 0.5f, 0)); mb.Matte();
                    }
                }
                // A clock over the front (100).
                if (m >= 4)
                {
                    var c = new Vector3(x0 - 0.5f, y + 6.6f, cz);
                    mb.Cylinder(c + new Vector3(0.3f, 0, 0), c - new Vector3(0.1f, 0, 0), 0.95f, 16, trim);
                    mb.Cylinder(c - new Vector3(0.1f, 0, 0), c - new Vector3(0.14f, 0, 0), 0.8f, 16, Palette.Hex(0xF6F0DC));
                    mb.Box(c - new Vector3(0.18f, -0.25f, 0), new Vector3(0.04f, 0.5f, 0.06f), Palette.Black);
                    mb.Box(c - new Vector3(0.18f, 0, -0.2f), new Vector3(0.04f, 0.06f, 0.4f), Palette.Black);
                }
                // Bunting along the eaves (200).
                if (m >= 6)
                    for (int i = 0; i < 14; i++)
                    {
                        float za = Mathf.Lerp(z0, z1, i / 14f), zb = Mathf.Lerp(z0, z1, (i + 0.8f) / 14f);
                        Color c = i % 2 == 0 ? Palette.Hex(0xE2B13C) : Palette.Hex(0x2F6FA8);
                        var a = new Vector3(x0 - 0.3f, y + 5.1f, za); var bb = new Vector3(x0 - 0.3f, y + 5.1f, zb); var tip = new Vector3(x0 - 0.3f, y + 4.4f, (za + zb) / 2);
                        mb.Tri(a, bb, tip, c); mb.Tri(bb, a, tip, c);
                    }
            });
            // Buyers looking the boxes over, more as the market grows; the auctioneer on his stand.
            Figure(parent, 2, new Vector3(x0 + 1.4f, y + 1.4f, z1 - 2.2f), -90);
            var spots = new[] { new Vector3(63.4f, 0, 3.4f), new Vector3(63.6f, 0, 8.6f), new Vector3(66.6f, 0, 6.2f), new Vector3(66.4f, 0, 11.4f), new Vector3(63.2f, 0, 13.4f), new Vector3(68.2f, 0, 2.6f), new Vector3(68.4f, 0, 8.8f) };
            int buyers = 3 + (m >= 1 ? 2 : 0) + (m >= 3 ? 2 : 0);
            for (int i = 0; i < buyers && i < spots.Length; i++) Figure(parent, i, spots[i] + Vector3.up * y, 70 + i * 47 % 140);
            // The refrigerated van at the back door (50).
            if (m >= 3) Models.Kit("van", parent, new Vector3(x1 + 3.5f, y, z0 + 3), 0, 5.2f, new Color(1.9f, 1.9f, 1.95f));
            // The stalls along the road, painted as the market's paint says.
            Color lift = stallLook ?? Lift; float hue = stallLook != null ? stallHue : 0;
            Models.Kit("market_stalls", parent, new Vector3(70, y, 24), 0, 12f, lift, hue);
            Models.Kit("market_stalls_b", parent, new Vector3(68, y, -14), 0, 11f, lift, hue);
            Models.Kit("market_stand", parent, new Vector3(61, y, -0.5f), 90, 4.2f, lift, hue);
            Models.Kit("bucket_fish", parent, new Vector3(60.5f, y, 3), 30, 1.6f);
            Models.Kit("flag", parent, new Vector3(62, y, 18), 0, 8f);
        }

        // ---------------------------------------------------------------- the trophy hall's front

        /// <summary>The trophy hall's front in the harbour: a little museum with columns and a gilded fish over the door.
        /// Tap it to go in.</summary>
        public static void Museum(Transform parent, Theme th)
        {
            var c = Layout.TrophyHall;
            Models.Built("Trophy hall", parent, mb =>
            {
                Color wall = Palette.Hex(0xE8E0CC), trim = Palette.White, roof = th.Ice ? th.Roof : Palette.Hex(0x2F5F8A);
                mb.Surf = SurfaceKind.Stone; mb.Box(c + new Vector3(0, 0.25f, 0), new Vector3(10, 0.5f, 8), Palette.Hex(0xC9C1B0));
                mb.Box(c + new Vector3(0, 0.15f, -4.4f), new Vector3(6, 0.3f, 1), Palette.Hex(0xC9C1B0));      // the steps
                mb.Surf = SurfaceKind.Brick; mb.Box(c + new Vector3(0, 3, 0.8f), new Vector3(9, 5, 6), wall);
                mb.Surf = SurfaceKind.RoofTiles; mb.GableRoof(c + new Vector3(0, 5.5f, 0.2f), new Vector3(9.4f, 2.2f, 7.6f), roof, trim, 0.3f);
                mb.Surf = SurfaceKind.Auto; mb.Paint();
                for (int i = 0; i < 4; i++) mb.Cylinder(c + new Vector3(-3.3f + i * 2.2f, 0.5f, -3.4f), c + new Vector3(-3.3f + i * 2.2f, 5.5f, -3.4f), 0.32f, 10, trim);
                mb.Box(c + new Vector3(0, 5.6f, -3.4f), new Vector3(9.4f, 0.4f, 0.8f), trim);
                mb.Matte();
                mb.Box(c + new Vector3(0, 1.8f, -2.22f), new Vector3(1.8f, 2.6f, 0.1f), Palette.Hex(0x3A2A20));      // the door
                mb.Shiny();
                mb.Sphere(c + new Vector3(0, 6.7f, -3.85f), new Vector3(1.4f, 0.55f, 0.12f), 12, Palette.Hex(0xE8B23A));
                var t0 = c + new Vector3(-1.3f, 6.7f, -3.85f);
                mb.Tri(t0, t0 + new Vector3(-0.8f, 0.6f, 0), t0 + new Vector3(-0.8f, -0.6f, 0), Palette.Hex(0xE8B23A));
                mb.Tri(t0, t0 + new Vector3(-0.8f, -0.6f, 0), t0 + new Vector3(-0.8f, 0.6f, 0), Palette.Hex(0xE8B23A));
                mb.Matte();
            });
        }

        // ---------------------------------------------------------------- the quay and the piers

        /// <summary>The porters' gear on the quay by the hall door, more of it at each dock milestone.</summary>
        public static void Quay(Transform parent, HarbourState h)
        {
            float y = Layout.QuayY;
            int m = Content.MilestonesAt(h.dock);
            if (m >= 1)          // 10: hand trucks
                Models.Built("Hand trucks", parent, mb =>
                {
                    for (int i = 0; i < 2; i++)
                    {
                        var p = new Vector3(9.6f, y, -4.6f - i * 1.4f);
                        mb.Paint();
                        mb.Cylinder(p + new Vector3(0, 0.15f, 0), p + new Vector3(-0.6f, 1.3f, 0), 0.04f, 5, Palette.Hex(0xD9503E));
                        mb.Box(p + new Vector3(0.1f, 0.06f, 0), new Vector3(0.5f, 0.06f, 0.55f), Palette.SteelDark);
                        mb.Matte();
                        foreach (float s in new[] { -0.3f, 0.3f }) mb.Cylinder(p + new Vector3(-0.05f, 0.15f, s), p + new Vector3(-0.05f, 0.15f, s + 0.06f * Mathf.Sign(s)), 0.15f, 8, Palette.Tyre);
                        mb.Paint(); mb.Box(p + new Vector3(0.05f, 0.4f, 0), new Vector3(0.6f, 0.55f, 0.6f), FishBox); mb.Matte();
                    }
                });
            if (m >= 2)          // 25: pallets of empty boxes, stacked ready for the boats
                for (int i = 0; i < 2; i++) Models.Kit("pallet", parent, new Vector3(10f, y, 11.5f + i * 2.4f), 0, 2.1f);
            if (m >= 2)
                Models.Built("Box stacks", parent, mb =>
                {
                    for (int i = 0; i < 2; i++) for (int l = 0; l < 3; l++)
                    { mb.Paint(); mb.Box(new Vector3(10f, y + 0.4f + l * 0.42f, 11.5f + i * 2.4f), new Vector3(0.9f, 0.4f, 1.1f), l % 2 == 0 ? FishBox : Palette.Hex(0xE07A2A)); }
                    mb.Matte();
                });
            if (m >= 3)          // 50: a quayside crane swinging boxes up from the boats
                Models.Built("Quay crane", parent, mb =>
                {
                    var b = new Vector3(5.5f, y, -11);
                    mb.Paint();
                    mb.Cylinder(b, b + Vector3.up * 0.5f, 1f, 1.1f, 10, Palette.SteelDark);
                    mb.Box(b + new Vector3(0, 3.2f, 0), new Vector3(0.5f, 5.4f, 0.5f), Palette.PaintYellow);
                    mb.Box(b + new Vector3(-2.2f, 5.9f, 0), new Vector3(6f, 0.4f, 0.4f), Palette.PaintYellow);
                    mb.Box(b + new Vector3(0.9f, 5.6f, 0), new Vector3(1f, 1f, 1f), Palette.SteelDark);   // its counterweight
                    mb.Cylinder(b + new Vector3(0, 5.9f, 0), b + new Vector3(-2.4f, 7.4f, 0), 0.06f, 4, Palette.SteelDark);
                    mb.Cylinder(b + new Vector3(-4.8f, 5.75f, 0), b + new Vector3(-4.8f, 3f, 0), 0.03f, 4, Palette.Black);
                    mb.Box(b + new Vector3(-4.8f, 2.6f, 0), new Vector3(0.9f, 0.6f, 0.9f), Palette.Hex(0xE07A2A));
                    mb.Matte();
                });
            if (m >= 4)          // 100: a weigh station: a platform scale with a dial
                Models.Built("Weigh station", parent, mb =>
                {
                    var p = new Vector3(6.6f, y, 9);
                    mb.Shiny(); mb.Box(p + new Vector3(0, 0.08f, 0), new Vector3(1.8f, 0.16f, 1.8f), Steel);
                    mb.Cylinder(p + new Vector3(0.8f, 0, -0.8f), p + new Vector3(0.8f, 1.4f, -0.8f), 0.06f, 6, Steel); mb.Matte();
                    mb.Cylinder(p + new Vector3(0.8f, 1.6f, -0.8f) + new Vector3(0.05f, 0, 0), p + new Vector3(0.8f, 1.6f, -0.8f) - new Vector3(0.05f, 0, 0), 0.35f, 14, Palette.White);
                });
        }

        /// <summary>Gear on a berth's pier as its boat reaches milestones: boxes, a net rack, a davit, a lamp, a flag.</summary>
        public static void Pier(Transform parent, int berth, int level)
        {
            int m = Content.MilestonesAt(level);
            if (m <= 0) return;
            float y = Layout.QuayY, z = Layout.BerthZ(berth), side = z + (berth % 2 == 0 ? -1.6f : 1.6f);
            Models.Built("Pier gear " + berth, parent, mb =>
            {
                // 10: a stack of fish boxes at the pier's foot.
                for (int l = 0; l < 2; l++) { mb.Paint(); mb.Box(new Vector3(-1.2f, y + 0.25f + l * 0.42f, z - 0.7f), new Vector3(0.9f, 0.4f, 1f), l == 0 ? FishBox : Palette.Hex(0xE07A2A)); }
                mb.Matte();
                if (m >= 2)      // 25: a rack of nets drying
                {
                    float x = -6.5f;
                    foreach (float dx in new[] { -1.4f, 1.4f }) mb.Cylinder(new Vector3(x + dx, y, side), new Vector3(x + dx, y + 2.2f, side), 0.07f, 5, Palette.Wood);
                    mb.Cylinder(new Vector3(x - 1.5f, y + 2.15f, side), new Vector3(x + 1.5f, y + 2.15f, side), 0.06f, 5, Palette.Wood);
                    var net = Palette.Hex(0x3E8A6A);
                    mb.Quad(new Vector3(x - 1.4f, y + 2.1f, side), new Vector3(x + 1.4f, y + 2.1f, side), new Vector3(x + 1.2f, y + 0.6f, side + 0.15f), new Vector3(x - 1.2f, y + 0.5f, side + 0.15f), net);
                    mb.Quad(new Vector3(x - 1.2f, y + 0.5f, side + 0.15f), new Vector3(x + 1.2f, y + 0.6f, side + 0.15f), new Vector3(x + 1.4f, y + 2.1f, side), new Vector3(x - 1.4f, y + 2.1f, side), net);
                    for (int k = 0; k < 6; k++) mb.Sphere(new Vector3(x - 1.2f + k * 0.48f, y + 0.55f + (k % 2) * 0.05f, side + 0.16f), Vector3.one * 0.08f, 6, Palette.Hex(0xE8A22A));
                }
                if (m >= 3)      // 50: a davit at the pier's end to swing the catch ashore
                {
                    var b = new Vector3(-12.5f, y, side);
                    mb.Paint();
                    mb.Cylinder(b, b + Vector3.up * 3f, 0.12f, 8, Palette.White);
                    mb.Cylinder(b + Vector3.up * 3f, b + new Vector3(0, 3.4f, (berth % 2 == 0 ? -1 : 1) * 1.6f), 0.1f, 6, Palette.White);
                    mb.Cylinder(b + new Vector3(0, 3.4f, (berth % 2 == 0 ? -1 : 1) * 1.6f), b + new Vector3(0, 2f, (berth % 2 == 0 ? -1 : 1) * 1.6f), 0.02f, 4, Palette.Black);
                    mb.Matte();
                }
                if (m >= 4)      // 100: a lamp and a life ring
                {
                    var p = new Vector3(-9.5f, y, z + (berth % 2 == 0 ? 1.2f : -1.2f));
                    mb.Cylinder(p, p + Vector3.up * 3.2f, 0.07f, 6, Palette.Hex(0x2B3A4A));
                    mb.Lit(); mb.Sphere(p + Vector3.up * 3.3f, Vector3.one * 0.22f, 8, new Color(1f, 0.9f, 0.62f, 0)); mb.Matte();
                    mb.Push().Translate(p + new Vector3(0, 1.2f, 0)).Rotate(Quaternion.Euler(0, 0, 90));
                    mb.Cylinder(new Vector3(0, -0.05f, 0), new Vector3(0, 0.05f, 0), 0.38f, 12, Palette.Hex(0xE8582A));
                    mb.Pop();
                }
                if (m >= 6)      // 200: a pennant at the pier head
                {
                    var p = new Vector3(-14.3f, y, z);
                    mb.Cylinder(p, p + Vector3.up * 3.6f, 0.05f, 5, Palette.White);
                    mb.Tri(p + Vector3.up * 3.6f, p + new Vector3(0, 3.3f, 1.4f), p + Vector3.up * 3f, Palette.Hex(0xE2B13C));
                    mb.Tri(p + Vector3.up * 3.6f, p + Vector3.up * 3f, p + new Vector3(0, 3.3f, 1.4f), Palette.Hex(0xE2B13C));
                }
            });
        }
    }

    /// <summary>Fish boxes riding a belt from one end to the other while it runs.</summary>
    public sealed class Belt : MonoBehaviour
    {
        public Vector3 From, To; public readonly List<Transform> Boxes = new List<Transform>();
        public bool Running = true;
        float _t;
        void Update()
        {
            if (Running) _t += Time.deltaTime * 0.9f / Mathf.Max(1, (To - From).magnitude);
            for (int i = 0; i < Boxes.Count; i++)
            {
                float p = Mathf.Repeat(_t + i / (float)Boxes.Count, 1f);
                var b = Boxes[i];
                bool show = Running || p > 0.05f && p < 0.95f;
                if (b.gameObject.activeSelf != show) b.gameObject.SetActive(show);
                b.position = Vector3.Lerp(From, To, p);
                // Boxes appear at the quay end and go in at the door: scale them in and out at the ends.
                float k = Mathf.Clamp01(Mathf.Min(p, 1 - p) * 12);
                b.localScale = Vector3.one * Mathf.Max(0.001f, k);
            }
        }
    }
}

using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>Where things stand: the sea on the left, the quay along x = 0, the hall, the road and the market on the right.</summary>
    public static class Layout
    {
        public const float QuayY = 1.1f, PierLength = 15f, QuayWidth = 9f;
        public static readonly Vector3 HallCentre = new Vector3(21, QuayY, 0), HallSize = new Vector3(17, 9, 24);
        public static readonly Vector3 HallDoor = new Vector3(11.2f, QuayY, 0);
        public static readonly Vector3 TruckBay = new Vector3(33, QuayY, -5), MarketGate = new Vector3(62, QuayY, -5), MarketCentre = new Vector3(70, QuayY, 3);

        static readonly float[] Rows = { -5, 6, -16, 17, -27, 28, -38, 39 };
        public static float BerthZ(int berth) { return Rows[Mathf.Clamp(berth, 0, Rows.Length - 1)]; }
        /// <summary>The foot of a berth's pier on the quay, where its crate stands.</summary>
        public static Vector3 Crate(int berth) { return new Vector3(2.2f, QuayY, BerthZ(berth) - 2.4f); }
        public static Vector3 Moored(int berth) { return new Vector3(-PierLength * 0.55f, 0, BerthZ(berth) + 2.6f + BoatLength(berth) * 0.08f); }
        public static Vector3 Grounds(int berth) { return new Vector3(-34 - berth % 3 * 7 - berth * 1.5f, 0, BerthZ(berth) + (berth % 2 == 0 ? 3 : -2)); }
        public static float BoatLength(int berth) { return 5.6f + berth * 1.1f; }
    }

    /// <summary>What makes each water look like itself: its ground, its sea and what grows there.</summary>
    public sealed class Theme
    {
        public Color Grass, Sand, Deep, Shallow;
        public string[] Trees;
        public bool Lilies, Ice;
        public float Fog = 1;

        public static Theme For(string harbour)
        {
            switch (harbour)
            {
                case "river": return new Theme { Grass = Palette.Hex(0x6FBF4A), Sand = Palette.Hex(0xD9C89A), Deep = Palette.Hex(0x1E6E7A), Shallow = Palette.Hex(0x4FC2A8), Trees = new[] { "tree_pine", "tree_pine_b", "tree_c", "tree_pine", "tree_d" }, Lilies = true };
                case "coast": return new Theme { Grass = Palette.Hex(0xB7D46A), Sand = Palette.Hex(0xF6E2A6), Deep = Palette.Hex(0x1466A8), Shallow = Palette.Hex(0x3FC6D8), Trees = new[] { "palm_a", "palm_b", "tree_a", "palm_a", "bush" } };
                case "reef": return new Theme { Grass = Palette.Hex(0xF2DFA4), Sand = Palette.Hex(0xFFF0C2), Deep = Palette.Hex(0x0E86C4), Shallow = Palette.Hex(0x58E6D8), Trees = new[] { "palm_a", "palm_b", "palm_a", "palm_b", "bush_b" } };
                case "deep": return new Theme { Grass = Palette.Hex(0x7FA08A), Sand = Palette.Hex(0xB9B4A6), Deep = Palette.Hex(0x0B2F6A), Shallow = Palette.Hex(0x1F6FB0), Trees = new[] { "rock_c", "tree_pine_b", "rock_b", "bush", "tree_pine" } };
                case "arctic": return new Theme { Grass = Palette.Hex(0xF4F8FC), Sand = Palette.Hex(0xDDEAF4), Deep = Palette.Hex(0x1C4E7A), Shallow = Palette.Hex(0x8FD8EC), Trees = new[] { "tree_pine", "tree_pine_b", "rock_c", "tree_pine_b", "rock_b" }, Ice = true };
                default: return new Theme { Grass = Palette.Hex(0x8FD05A), Sand = Palette.Hex(0xF2DFA4), Deep = Palette.Hex(0x126B9E), Shallow = Palette.Hex(0x40CCC7), Trees = new[] { "tree_a", "tree_b", "tree_c", "tree_d", "tree_pine", "tree_pine_b", "tree_b", "tree_d" }, Lilies = true };
            }
        }
    }
}

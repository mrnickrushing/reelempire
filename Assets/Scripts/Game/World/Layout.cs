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

        /// <summary>Where each of the harbour's works stands (the plot is there before it is built).</summary>
        public static Vector3 WorkPlot(string id)
        {
            switch (id)
            {
                case "netloft": return new Vector3(20, QuayY, -34);
                case "shipyard": return new Vector3(36, QuayY, 17);
                case "icehouse": return new Vector3(52, QuayY, -53);
                case "auction": return MarketCentre + new Vector3(15, 0, -5);
                case "lamp": return new Vector3(3, QuayY, 62);
            }
            return ChartRoom;
        }
        /// <summary>The chart room: a watch tower behind the hall, where research is done.</summary>
        public static readonly Vector3 ChartRoom = new Vector3(37, QuayY, -17);
        /// <summary>Where each water's landmark stands: on the rise north of the hall, and in the shallows past the quay's north end.</summary>
        public static readonly Vector3 Landmark = new Vector3(27, QuayY, 39), WaterMark = new Vector3(-17, 0, 58);
        public const float LandmarkRadius = 14, WaterMarkRadius = 10;
        /// <summary>The trophy hall's front door in the harbour (tap it to go in).</summary>
        public static readonly Vector3 TrophyHall = new Vector3(51, QuayY, 33);
    }

    /// <summary>What makes each water look like itself: its ground, its sea and what grows there.</summary>
    public sealed class Theme
    {
        public Color Grass, Sand, Deep, Shallow;
        public string[] Trees;
        public bool Lilies, Ice;
        /// <summary>Barn paint, as a turn of the hue from the kit's red: each water has its own.</summary>
        public float Paint;
        public float Fog = 1;
        /// <summary>How this water builds: wall paint and finish (null wall: the barn red turned by <see cref="Paint"/>),
        /// roof colour and finish, the market's roof, the quay's surface, and whether the roofs carry snow.</summary>
        public Color? Wall; public SurfaceKind WallSurf = SurfaceKind.Siding;
        public Color Roof = Palette.Hex(0x8C98A2), MarketRoof = Palette.Hex(0x2F7A8A); public SurfaceKind RoofSurf = SurfaceKind.Tin;
        public Color Quay = Palette.Hex(0xD9A066); public SurfaceKind QuaySurf = SurfaceKind.Planks;

        public static Theme For(string harbour)
        {
            switch (harbour)
            {
                case "river": return new Theme { Paint = 0.62f, Grass = Palette.Hex(0x6FBF4A), Sand = Palette.Hex(0xD9C89A), Deep = Palette.Hex(0x1E6E7A), Shallow = Palette.Hex(0x4FC2A8), Trees = new[] { "tree_pine", "tree_pine_b", "tree_c", "tree_pine", "tree_d" }, Lilies = true,
                    Wall = Palette.Hex(0x9A6A44), WallSurf = SurfaceKind.WoodGrain, Roof = Palette.Hex(0x4A7356), MarketRoof = Palette.Hex(0x4A7356), RoofSurf = SurfaceKind.Shingles };
                case "coast": return new Theme { Paint = 0.1f, Grass = Palette.Hex(0xB7D46A), Sand = Palette.Hex(0xF6E2A6), Deep = Palette.Hex(0x1466A8), Shallow = Palette.Hex(0x3FC6D8), Trees = new[] { "palm_a", "palm_b", "tree_a", "palm_a", "bush" },
                    Wall = Palette.Hex(0xF0EBE0), WallSurf = SurfaceKind.Brick, Roof = Palette.Hex(0xC2643E), MarketRoof = Palette.Hex(0xC2643E), RoofSurf = SurfaceKind.RoofTiles, Quay = Palette.Hex(0xCFC6B4), QuaySurf = SurfaceKind.Stone };
                case "reef": return new Theme { Paint = 0.5f, Grass = Palette.Hex(0xF2DFA4), Sand = Palette.Hex(0xFFF0C2), Deep = Palette.Hex(0x0E86C4), Shallow = Palette.Hex(0x58E6D8), Trees = new[] { "palm_a", "palm_b", "palm_a", "palm_b", "bush_b" },
                    Wall = Palette.Hex(0xEAD6AE), WallSurf = SurfaceKind.WoodGrain, Roof = Palette.Hex(0xC9AE78), MarketRoof = Palette.Hex(0x3FA6A0), RoofSurf = SurfaceKind.Shingles, Quay = Palette.Hex(0xE6C48C) };
                case "deep": return new Theme { Paint = 0.72f, Grass = Palette.Hex(0x7FA08A), Sand = Palette.Hex(0xB9B4A6), Deep = Palette.Hex(0x0B2F6A), Shallow = Palette.Hex(0x1F6FB0), Trees = new[] { "rock_c", "tree_pine_b", "rock_b", "bush", "tree_pine" },
                    Wall = Palette.Hex(0x45698C), WallSurf = SurfaceKind.PaintedMetal, Roof = Palette.Hex(0xA4AEB6), MarketRoof = Palette.Hex(0xD9503E), Quay = Palette.Hex(0xB8B4AA), QuaySurf = SurfaceKind.Concrete };
                case "arctic": return new Theme { Grass = Palette.Hex(0xF4F8FC), Sand = Palette.Hex(0xDDEAF4), Deep = Palette.Hex(0x1C4E7A), Shallow = Palette.Hex(0x8FD8EC), Trees = new[] { "tree_pine", "tree_pine_b", "rock_c", "tree_pine_b", "rock_b" }, Ice = true,
                    Wall = Palette.Hex(0xC98A32), Roof = Palette.Hex(0xF2F6FA), MarketRoof = Palette.Hex(0xF2F6FA), RoofSurf = SurfaceKind.None };
                default: return new Theme { Grass = Palette.Hex(0x8FD05A), Sand = Palette.Hex(0xF2DFA4), Deep = Palette.Hex(0x126B9E), Shallow = Palette.Hex(0x40CCC7), Trees = new[] { "tree_a", "tree_b", "tree_c", "tree_d", "tree_pine", "tree_pine_b", "tree_b", "tree_d" }, Lilies = true };
            }
        }
    }
}

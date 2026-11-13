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
        public static float BoatLength(int berth) { return 4.2f + berth * 1.15f; }
    }
}

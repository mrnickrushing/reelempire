using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>
    /// Draws the harbour on screen from the simulation: the sea and the land, a pier and a boat per berth, the haulers
    /// on the quay, the hall with its piles, the trucks and the market. Rebuilt when a berth or a crew member is added.
    /// </summary>
    public sealed class HarbourView : MonoBehaviour
    {
        public static HarbourView Instance { get; private set; }

        Sim S { get { return GameRoot.Sim; } }
        HarbourState H { get { return S.H; } }

        Transform _static, _moving;
        readonly List<Transform> _boats = new List<Transform>(), _crates = new List<Transform>(), _haulers = new List<Transform>(), _trucks = new List<Transform>();
        readonly List<Transform> _haulLoads = new List<Transform>(), _truckLoads = new List<Transform>();
        readonly List<int> _haulTo = new List<int>();
        Transform _rawPile, _donePile, _smoke;
        int _berths = -1, _nHaul = -1, _nTruck = -1; string _harbour;

        public static HarbourView Create(Transform parent)
        {
            var go = new GameObject("Harbour");
            go.transform.SetParent(parent, false);
            Instance = go.AddComponent<HarbourView>();
            Instance.Rebuild();
            return Instance;
        }

        void Rebuild()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            _boats.Clear(); _crates.Clear(); _haulers.Clear(); _trucks.Clear(); _haulLoads.Clear(); _truckLoads.Clear(); _haulTo.Clear();
            _static = new GameObject("Static").transform; _static.SetParent(transform, false);
            _moving = new GameObject("Moving").transform; _moving.SetParent(transform, false);
            var h = H;
            _berths = h.berths.Count; _nHaul = h.haulers.Count; _nTruck = h.trucks.Count; _harbour = h.id;
            BuildSea();
            BuildLand();
            for (int i = 0; i < Content.Berths.Length; i++) BuildPier(i, i < h.berths.Count);
            for (int i = 0; i < h.berths.Count; i++)
            {
                _boats.Add(Scenery.Boat(i, _moving).transform);
                var crate = Scenery.Crate(_moving); crate.transform.position = Layout.Crate(i); _crates.Add(crate.transform);
            }
            for (int i = 0; i < h.haulers.Count; i++)
            {
                var t = Scenery.Hauler(_moving, i).transform; _haulers.Add(t); _haulTo.Add(0);
                var load = Scenery.Crate(t); load.transform.localPosition = new Vector3(0, 0.95f, 0.55f); load.transform.localScale *= 0.6f; _haulLoads.Add(load.transform);
            }
            for (int i = 0; i < h.trucks.Count; i++)
            {
                var t = Scenery.Truck(_moving).transform; _trucks.Add(t);
                var load = t.Find("Load"); _truckLoads.Add(load);
            }
            Scenery.Hall(_static, h);
            _rawPile = Scenery.Pile(_static, new Color(0.62f, 0.7f, 0.76f)).transform; _rawPile.position = Layout.HallDoor + new Vector3(-0.5f, 0, 5.5f);
            _donePile = Scenery.Pile(_static, new Color(0.86f, 0.62f, 0.3f)).transform; _donePile.position = Layout.TruckBay + new Vector3(-3.5f, 0, 5.5f);
            Scenery.Market(_static);
            Scenery.Surroundings(_static, h.id);
        }

        void BuildSea()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Destroy(go.GetComponent<Collider>());
            go.name = "Sea"; go.transform.SetParent(_static, false);
            go.transform.position = new Vector3(-200, 0, 0); go.transform.localScale = new Vector3(120, 1, 160);
            var sh = Resources.Load<Shader>("Shaders/Water") ?? Shader.Find("ReelEmpire/Water");
            var m = new Material(sh);
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void BuildLand()
        {
            Models.Built("Land", _static, mb =>
            {
                var grass = Palette.Hex(0x8FD05A); var sand = Palette.Hex(0xF2DFA4); var stone = Palette.Hex(0xB8AE9C); var planks = Palette.Hex(0xD9A066);
                mb.Mat(0.02f);
                mb.Surf = SurfaceKind.None;
                mb.Box(new Vector3(310, -2.5f + Layout.QuayY / 2 - 0.02f, 0), new Vector3(600, 5 + Layout.QuayY, 900), grass, grass);       // the meadow
                mb.Box(new Vector3(5.5f, -2.5f + Layout.QuayY / 2, 0), new Vector3(11, 5 + Layout.QuayY, 900), stone, sand);                  // the shore: a strip of sand
                // The beach slopes into the water beyond the ends of the quay.
                foreach (float side in new[] { -1f, 1f })
                    mb.Quad(new Vector3(0, Layout.QuayY, side * 49), new Vector3(0, Layout.QuayY, side * 450), new Vector3(-14, -0.4f, side * 450), new Vector3(-14, -0.4f, side * 49), sand);
                mb.Surf = SurfaceKind.Planks;
                mb.Box(new Vector3(Layout.QuayWidth / 2, Layout.QuayY + 0.06f, 0), new Vector3(Layout.QuayWidth, 0.14f, 98), planks);         // the quay's boards
                mb.Box(new Vector3(-0.2f, Layout.QuayY - 0.6f, 0), new Vector3(0.5f, 1.5f, 98), Palette.Hex(0x8A5A3C));                       // its face
                mb.Surf = SurfaceKind.None;
                mb.Box(new Vector3(47, Layout.QuayY + 0.04f, -5), new Vector3(44, 0.08f, 7.5f), Palette.Hex(0xE6CF9A));                       // the sandy road to market
                mb.Box(new Vector3(35, Layout.QuayY + 0.04f, 0), new Vector3(10, 0.08f, 24), Palette.Hex(0xDCC48C));                          // the loading yard
                mb.Surf = SurfaceKind.Auto;
            });
        }

        void BuildPier(int berth, bool open)
        {
            float z = Layout.BerthZ(berth);
            Models.Built("Pier " + berth, _static, mb =>
            {
                var planks = open ? Palette.Hex(0xE0A86A) : Palette.Hex(0xB9B0A0); var post = Palette.Hex(0x7A5234);
                mb.Mat(0.02f);
                float len = open ? Layout.PierLength : 5f;
                mb.Surf = SurfaceKind.Planks;
                mb.Box(new Vector3(-len / 2, Layout.QuayY - 0.1f, z), new Vector3(len, 0.22f, 2.8f), planks);
                mb.Surf = SurfaceKind.Bark;
                for (float x = -1.5f; x > -len; x -= 3.4f)
                    foreach (float side in new[] { -1.25f, 1.25f })
                        mb.Cylinder(new Vector3(x, -2, z + side), new Vector3(x, Layout.QuayY + 0.5f, z + side), 0.2f, 8, post);
                mb.Surf = SurfaceKind.Auto;
            });
        }

        void Update()
        {
            var s = S; if (s == null) return;
            var h = H;
            if (h.id != _harbour || h.berths.Count != _berths || h.haulers.Count != _nHaul || h.trucks.Count != _nTruck) Rebuild();
            float time = Time.time;

            for (int i = 0; i < _boats.Count; i++)
            {
                var b = h.berths[i]; var t = _boats[i];
                Vector3 home = Layout.Moored(i), far = Layout.Grounds(i);
                Vector3 pos = home; float yaw = -90;
                if (b.sailing)
                {
                    float p = 1 - Mathf.Clamp01(b.trip / s.TripOf(h, i));
                    if (p < 0.35f) { pos = Vector3.Lerp(home, far, Smooth(p / 0.35f)); yaw = Heading(far - home); }
                    else if (p < 0.65f) { pos = far; yaw = Heading(far - home) + Mathf.Sin(time * 0.6f + i) * 25; }
                    else { pos = Vector3.Lerp(far, home, Smooth((p - 0.65f) / 0.35f)); yaw = Heading(home - far); }
                }
                pos.y = Mathf.Sin(time * 1.3f + i * 1.7f) * 0.12f;
                t.position = pos;
                t.rotation = Quaternion.Slerp(t.rotation, Quaternion.Euler(Mathf.Sin(time * 1.1f + i) * 2.5f, yaw, Mathf.Sin(time * 0.9f + i * 2) * 3f), 1 - Mathf.Exp(-6 * Time.deltaTime));
                float fill = (float)System.Math.Min(1, b.crate / s.DeckOf(h, i));
                _crates[i].localScale = Vector3.one * (fill <= 0 ? 0.001f : Mathf.Lerp(0.6f, 1.5f, fill));
            }

            for (int i = 0; i < _haulers.Count; i++)
            {
                var c = h.haulers[i]; var t = _haulers[i];
                Vector3 door = Layout.HallDoor + new Vector3(-1.5f, 0, -2 + i * 1.3f);
                if (!c.busy) { _haulTo[i] = Fullest(h); Walk(t, door, 90, false); _haulLoads[i].gameObject.SetActive(false); continue; }
                float p = c.t / Content.HaulRound;
                Vector3 crate = Layout.Crate(Mathf.Min(_haulTo[i], h.berths.Count - 1)) + new Vector3(1.6f, 0, 0);
                if (p < 0.5f) Walk(t, Vector3.Lerp(door, crate, p * 2), Heading(crate - door), true);
                else Walk(t, Vector3.Lerp(crate, door, p * 2 - 1), Heading(door - crate), true);
                _haulLoads[i].gameObject.SetActive(c.turned && c.load > 0);
            }

            for (int i = 0; i < _trucks.Count; i++)
            {
                var c = h.trucks[i]; var t = _trucks[i];
                Vector3 bay = Layout.TruckBay + new Vector3(0, 0, 5 + i * 3.4f), road = Layout.TruckBay + new Vector3(4, 0, i % 2 == 0 ? -1.4f : 1.4f), gate = Layout.MarketGate + new Vector3(0, 0, i % 2 == 0 ? -1.4f : 1.4f);
                if (!c.busy) { t.position = bay; t.rotation = Quaternion.Euler(0, 90, 0); if (_truckLoads[i] != null) _truckLoads[i].gameObject.SetActive(false); continue; }
                float p = c.t / Content.TruckRound;
                Vector3 pos; float yaw;
                if (p < 0.08f) { pos = Vector3.Lerp(bay, road, p / 0.08f); yaw = Heading(road - bay); }
                else if (p < 0.5f) { pos = Vector3.Lerp(road, gate, Smooth((p - 0.08f) / 0.42f)); yaw = 90; }
                else if (p < 0.92f) { pos = Vector3.Lerp(gate, road, Smooth((p - 0.5f) / 0.42f)); yaw = -90; }
                else { pos = Vector3.Lerp(road, bay, (p - 0.92f) / 0.08f); yaw = Heading(bay - road); }
                t.position = pos;
                t.rotation = Quaternion.Slerp(t.rotation, Quaternion.Euler(0, yaw, 0), 1 - Mathf.Exp(-8 * Time.deltaTime));
                if (_truckLoads[i] != null) _truckLoads[i].gameObject.SetActive(!c.turned);
            }

            double cap = s.PileCap(h);
            _rawPile.localScale = Vector3.one * (h.raw <= 0 ? 0.001f : Mathf.Lerp(0.5f, 2.2f, (float)System.Math.Min(1, h.raw / cap)));
            _donePile.localScale = Vector3.one * (h.done <= 0 ? 0.001f : Mathf.Lerp(0.5f, 2.2f, (float)System.Math.Min(1, h.done / cap)));
        }

        static int Fullest(HarbourState h) { int best = 0; for (int i = 1; i < h.berths.Count; i++) if (h.berths[i].crate > h.berths[best].crate) best = i; return best; }
        static float Smooth(float t) { t = Mathf.Clamp01(t); return t * t * (3 - 2 * t); }
        static float Heading(Vector3 d) { return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg; }

        static void Walk(Transform t, Vector3 at, float yaw, bool moving)
        {
            var st = t.GetComponent<Strider>();
            if (st != null) st.Show(moving, Time.time * 2.2f + t.GetSiblingIndex() * 0.3f);
            else at.y += moving ? Mathf.Abs(Mathf.Sin(Time.time * 9)) * 0.08f : 0;
            t.position = at;
            t.rotation = Quaternion.Slerp(t.rotation, Quaternion.Euler(0, yaw, 0), 1 - Mathf.Exp(-10 * Time.deltaTime));
        }

        // ---- what the interface points at ----

        public Vector3 BoatAt(int berth) { return berth < _boats.Count ? _boats[berth].position + Vector3.up * 2.5f : Layout.Moored(berth); }
        public Vector3 BerthSign(int berth) { return new Vector3(-7.5f, Layout.QuayY + 0.4f, Layout.BerthZ(berth) - 1.6f); }
        public Vector3 DockSign { get { return new Vector3(5.5f, Layout.QuayY + 3.4f, 0.5f); } }
        public Vector3 HallSign { get { return Layout.HallCentre + new Vector3(0, Layout.HallSize.y + 2.5f, 0); } }
        public Vector3 MarketSign { get { return Layout.MarketCentre + new Vector3(-2, 9, 0); } }
        public Vector3 HaulerAt(int i) { return i < _haulers.Count ? _haulers[i].position + Vector3.up * 2.4f : Layout.HallDoor; }
        public Vector3 TruckAt(int i) { return i < _trucks.Count ? _trucks[i].position + Vector3.up * 3.2f : Layout.TruckBay; }
    }
}

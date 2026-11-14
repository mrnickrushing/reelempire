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
        Transform _rawPile, _donePile, _smoke, _bonus, _beam;
        readonly List<Light> _lamps = new List<Light>();
        Material _glow;
        int _berths = -1, _nHaul = -1, _nTruck = -1; string _harbour;

        public static HarbourView Create(Transform parent)
        {
            var go = new GameObject("Harbour");
            go.transform.SetParent(parent, false);
            Instance = go.AddComponent<HarbourView>();
            Instance.Rebuild();
            return Instance;
        }

        int _worksKey;
        /// <summary>Changes when a work is built or the plots appear, so the harbour is drawn again.</summary>
        int WorksKey(HarbourState h) { int k = S.Unlocked("works") ? 1 : 0; foreach (int l in h.works) k = k * 4 + l; return k; }

        void Rebuild()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            _boats.Clear(); _crates.Clear(); _haulers.Clear(); _trucks.Clear(); _haulLoads.Clear(); _truckLoads.Clear(); _haulTo.Clear();
            _static = new GameObject("Static").transform; _static.SetParent(transform, false);
            _moving = new GameObject("Moving").transform; _moving.SetParent(transform, false);
            var h = H;
            _berths = h.berths.Count; _nHaul = h.haulers.Count; _nTruck = h.trucks.Count; _harbour = h.id;
            _worksKey = WorksKey(h);
            Scenery.Paint = Theme.For(h.id).Paint;
            BuildSea();
            BuildLand();
            for (int i = 0; i < Content.Berths.Length; i++) BuildPier(i, i < h.berths.Count);
            for (int i = 0; i < h.berths.Count; i++)
            {
                _boats.Add(Scenery.Boat(i, _moving).transform);
                Life.Wake(_boats[i], Layout.BoatLength(i));
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
            _bonus = (Models.Kit("chest", _moving, Vector3.zero, 0, 2.6f) ?? Scenery.Crate(_moving)).transform; _bonus.gameObject.SetActive(false);
            Scenery.Market(_static);
            Scenery.Surroundings(_static, h.id);
            BuildLamps();
            // Life: smoke from the smokehouse, gulls over the water, the lighthouse's beam.
            Life.Smoke(_static, new Vector3(Layout.HallCentre.x + 3, Layout.QuayY + 11.5f, Layout.HallCentre.z + 14));
            if (!Theme.For(h.id).Ice) Life.Gulls(_moving, 5, h.id.GetHashCode());
            if (S.WorkLevel(h, "lamp") > 0) Life.LighthouseBeam(_static, new Vector3(3, Layout.QuayY + 15.2f, 62));      // the lamp is one of the works
        }

        /// <summary>Lamps along the quay and by the market, lit at dusk, and the lighthouse's turning beam.</summary>
        void BuildLamps()
        {
            _lamps.Clear();
            var at = new List<Vector3>();
            for (float z = -40; z <= 40; z += 16) at.Add(new Vector3(8.6f, Layout.QuayY, z));
            at.Add(Layout.MarketCentre + new Vector3(-9, 0, -8)); at.Add(Layout.MarketCentre + new Vector3(-9, 0, 9)); at.Add(Layout.TruckBay + new Vector3(8, 0, 6));
            Models.Built("Lamp posts", _static, mb =>
            {
                foreach (var p in at)
                {
                    mb.Matte();
                    mb.Cylinder(p, p + Vector3.up * 4.4f, 0.09f, 6, Palette.Hex(0x2B3A4A));
                    mb.Box(p + new Vector3(0, 4.5f, 0), new Vector3(0.5f, 0.12f, 0.5f), Palette.Hex(0x2B3A4A));
                    mb.Lit();
                    mb.Sphere(p + new Vector3(0, 4.2f, 0), Vector3.one * 0.26f, 8, new Color(1f, 0.9f, 0.62f, 0));
                }
                mb.Matte();
            });
            // Only a few real lights reach a surface on a phone, so the light itself is painted: a warm pool on the
            // ground under each lamp and a halo round its head, both faded in with the dusk.
            var soft = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float d = Mathf.Clamp01(1 - new Vector2(x - 31.5f, y - 31.5f).magnitude / 31.5f);
                soft.SetPixel(x, y, new Color(1, 1, 1, d * d));
            }
            soft.Apply();
            var basis = Resources.Load<Material>("Materials/Weather");
            _glow = basis != null ? new Material(basis) : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            _glow.SetTexture("_BaseMap", soft); _glow.mainTexture = soft;
            _glow.SetFloat("_Surface", 1); _glow.SetInt("_SrcBlend", 5); _glow.SetInt("_DstBlend", 1); _glow.SetInt("_ZWrite", 0);
            _glow.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); _glow.renderQueue = 3100;
            _glow.SetColor("_BaseColor", Color.clear);
            var pools = new GameObject("Lamp light"); pools.transform.SetParent(_static, false);
            var mesh = new Mesh(); var vs = new List<Vector3>(); var uv = new List<Vector2>(); var tr = new List<int>();
            foreach (var p in at)
            {
                const float R = 7.5f; int b = vs.Count; var c = p + Vector3.up * 0.06f;
                vs.Add(c + new Vector3(-R, 0, -R)); vs.Add(c + new Vector3(-R, 0, R)); vs.Add(c + new Vector3(R, 0, R)); vs.Add(c + new Vector3(R, 0, -R));
                uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(0, 1)); uv.Add(new Vector2(1, 1)); uv.Add(new Vector2(1, 0));
                tr.Add(b); tr.Add(b + 1); tr.Add(b + 2); tr.Add(b); tr.Add(b + 2); tr.Add(b + 3);
            }
            mesh.SetVertices(vs); mesh.SetUVs(0, uv); mesh.SetTriangles(tr, 0); mesh.RecalculateBounds();
            pools.AddComponent<MeshFilter>().sharedMesh = mesh;
            var pr = pools.AddComponent<MeshRenderer>(); pr.sharedMaterial = _glow; pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var halo = new GameObject("Lamp halos"); halo.transform.SetParent(_static, false);
            var ps = halo.AddComponent<ParticleSystem>(); ps.Stop();
            var hm = ps.main; hm.loop = false; hm.playOnAwake = false; hm.maxParticles = 64; hm.simulationSpace = ParticleSystemSimulationSpace.World; hm.startSpeed = 0;
            var he = ps.emission; he.enabled = false; var hs = ps.shape; hs.enabled = false;
            halo.GetComponent<ParticleSystemRenderer>().sharedMaterial = _glow;
            foreach (var p in at) ps.Emit(new ParticleSystem.EmitParams { position = p + Vector3.up * 4.2f, startSize = 3.4f, startLifetime = 1e9f, startColor = Color.white, velocity = Vector3.zero }, 1);
            ps.Pause();
            for (int i = at.Count - 3; i < at.Count; i++)      // the market's lamps also throw real light on the stalls and trucks
            {
                var p = at[i];
                var l = new GameObject("Lamp").AddComponent<Light>(); l.transform.SetParent(_static, false); l.transform.position = p + Vector3.up * 4f;
                l.type = LightType.Point; l.color = new Color(1f, 0.82f, 0.5f); l.range = 17; l.intensity = 0; l.shadows = LightShadows.None;
                _lamps.Add(l);
            }
            var beam = new GameObject("Lighthouse beam").AddComponent<Light>(); beam.transform.SetParent(_static, false); beam.transform.position = new Vector3(3, Layout.QuayY + 15.5f, 62);
            beam.type = LightType.Spot; beam.color = new Color(1f, 0.95f, 0.75f); beam.range = 190; beam.spotAngle = 26; beam.innerSpotAngle = 12; beam.intensity = 0; beam.shadows = LightShadows.None;
            _beam = beam.transform; _lamps.Add(beam);
        }

        void BuildSea()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Destroy(go.GetComponent<Collider>());
            go.name = "Sea"; go.transform.SetParent(_static, false);
            go.transform.position = new Vector3(-200, 0, 0); go.transform.localScale = new Vector3(120, 1, 160);
            var sh = Resources.Load<Shader>("Shaders/Water") ?? Shader.Find("ReelEmpire/Water");
            var m = new Material(sh);
            var th = Theme.For(H.id); m.SetColor("_Deep", th.Deep); m.SetColor("_Shallow", th.Shallow);
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
            go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void BuildLand()
        {
            Models.Built("Land", _static, mb =>
            {
                var th = Theme.For(H.id); var grass = th.Grass; var sand = th.Sand; var stone = Palette.Hex(0xB8AE9C); var planks = Palette.Hex(0xD9A066);
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
            if (Resources.Load<Mesh>("Models/dock") != null)
            {
                // Kit dock pieces end to end (one for a berth not opened yet); each is one unit long, 0.43 wide, 0.38 tall.
                const float tile = 5f;
                int n = open ? 3 : 1;
                for (int i = 0; i < n; i++) Models.Kit("dock", _static, new Vector3(-tile * (i + 0.5f) + 0.3f, Layout.QuayY - tile * 0.38f + 0.06f, z), 90, tile, new Color(1.5f, 1.4f, 1.25f));
                return;
            }
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
            if (h.id != _harbour || h.berths.Count != _berths || h.haulers.Count != _nHaul || h.trucks.Count != _nTruck || WorksKey(h) != _worksKey) Rebuild();
            float time = Time.time;
            // Dusk: the lamps come on one after another, and the lighthouse starts to turn.
            float night = s.Night;
            for (int i = 0; i < _lamps.Count; i++)
            {
                bool isBeam = _lamps[i].type == LightType.Spot;
                float want = Mathf.Clamp01(night * 1.6f - i * 0.05f) * (isBeam ? 260 : 26);
                _lamps[i].intensity = Mathf.MoveTowards(_lamps[i].intensity, want, Time.deltaTime * (isBeam ? 200 : 20));
                if (_lamps[i].enabled != want > 0 || _lamps[i].intensity > 0 != _lamps[i].enabled) _lamps[i].enabled = _lamps[i].intensity > 0.01f || want > 0;
            }
            if (_glow != null) _glow.SetColor("_BaseColor", new Color(1f, 0.78f, 0.42f, Mathf.Clamp01(night * 1.4f) * 0.6f));
            if (_beam != null) _beam.rotation = Quaternion.Euler(14, time * 26, 0);

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

            // The crate drifts along the shore between the piers, a different stretch each time.
            bool afloat = s.BonusAfloat;
            if (_bonus.gameObject.activeSelf != afloat) _bonus.gameObject.SetActive(afloat);
            if (afloat)
            {
                float z0 = -30 + s.State.bonusSeed % 60, drift = ((float)Sim.BonusStays - s.BonusLeft) * 0.35f;
                _bonus.position = new Vector3(-20 - s.State.bonusSeed % 9, Mathf.Sin(time * 1.6f) * 0.15f - 0.25f, z0 + drift);
                _bonus.rotation = Quaternion.Euler(Mathf.Sin(time * 1.2f) * 8, time * 14, Mathf.Sin(time * 0.9f) * 8);
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

        public Vector3 BonusAt { get { return _bonus != null ? _bonus.position : Vector3.zero; } }
        public Vector3 BoatAt(int berth) { return berth < _boats.Count ? _boats[berth].position + Vector3.up * 2.5f : Layout.Moored(berth); }
        public Vector3 BerthSign(int berth) { return new Vector3(0.8f, Layout.QuayY + 0.2f, Layout.BerthZ(berth) + 3.4f); }
        public Vector3 DockSign { get { return Layout.HallDoor + new Vector3(-1.2f, 5.2f, -0.5f); } }
        /// <summary>Where the price of the next berth hangs: on the quay at the foot of its stub of pier.</summary>
        public Vector3 NextBerthSign(int berth) { return new Vector3(-4f, Layout.QuayY + 0.2f, Layout.BerthZ(berth) + 2.5f); }
        public Vector3 HallSign { get { return Layout.HallCentre + new Vector3(0, Layout.HallSize.y + 2.5f, 0); } }
        public Vector3 MarketSign { get { return Layout.MarketCentre + new Vector3(-2, 9, 0); } }
        public Vector3 HaulerAt(int i) { return i < _haulers.Count ? _haulers[i].position + Vector3.up * 2.4f : Layout.HallDoor; }
        public Vector3 TruckAt(int i) { return i < _trucks.Count ? _trucks[i].position + Vector3.up * 3.2f : Layout.TruckBay; }
    }
}

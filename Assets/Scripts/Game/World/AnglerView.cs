using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>
    /// Fishing by hand, out in the harbour: the angler at the south end of the quay, a rod that bends as the line draws
    /// tight, a line that sags when it's slack, the float on the real water, the dark shapes of fish beneath it (the big
    /// ones out in the gold water), and the catch swung up out of the water and held up. The rules and the one button
    /// stay in <c>Hud.Angling</c>; it tells this what to show.
    /// </summary>
    public sealed class AnglerView : MonoBehaviour
    {
        public static AnglerView Instance { get; private set; }
        public static bool Open => Instance != null;

        static readonly Vector3 Stand = new Vector3(0.75f, Layout.QuayY, -45f);
        static readonly Vector3 Out = new Vector3(-1f, 0, 0.1f).normalized;          // the way the line goes out
        static readonly Vector3 Side = Vector3.Cross(Vector3.up, Out).normalized;
        /// <summary>Where a cast can land: from <see cref="Near"/> to <see cref="Far"/> metres out, the gold in the middle.</summary>
        public const float Near = 6f, Far = 22f;
        const float RodLength = 3.4f, Water = 0.04f;

        enum Mode { Aim, Flying, Float, Bite, Fight, Landing, Done }
        Mode _mode = Mode.Aim;

        Vector3 _camFocus; float _camDistance; Vector2 _limX, _limZ, _limD;
        LineRenderer _rod, _line;
        Transform _float, _strip, _marker, _ripple, _catch, _glow;
        Material _rippleMat;
        ParticleSystem _splash;
        readonly List<Swimmer> _swimmers = new List<Swimmer>();
        Swimmer _taker;
        Vector3 _floatAt, _from, _to, _fishAt;
        float _t, _aim, _bend, _tension = 0.2f, _progress, _cast, _rippleT, _whip;
        bool _running, _warning;

        sealed class Swimmer { public Transform T; public SpriteRenderer R; public Vector3 Pos, Goal; public float Speed, Length, Rest, Heading; public bool Big; }

        static Vector3 Hands => Stand + Out * 0.45f + new Vector3(0, 1.05f, 0) + Side * 0.1f;
        static Vector3 OnWater(float metres) { var p = Stand + Out * metres; p.y = Water; return p; }

        // ---- What the HUD calls ------------------------------------------------------------------------------------

        public static void Show()
        {
            if (Instance != null) return;
            Instance = new GameObject("Angler").AddComponent<AnglerView>();
            Instance.Build();
        }

        public static void Hide()
        {
            if (Instance == null) return;
            var me = Instance; var cam = CameraRig.Instance;
            cam.LimX = me._limX; cam.LimZ = me._limZ; cam.LimD = me._limD;
            cam.Focus = me._camFocus; cam.Distance = me._camDistance; cam.Cut();
            Destroy(me.gameObject); Instance = null;
        }

        /// <summary>Aiming: the mark runs along the water; <paramref name="p"/> is 0 near, 1 far, 0.5 the gold.</summary>
        public void Aim(float p)
        {
            if (_mode != Mode.Aim) Reset();
            _aim = p;
            _marker.position = OnWater(Mathf.Lerp(Near, Far, p)) + Vector3.up * 0.02f;
        }

        /// <summary>The cast: the rod whips and the float flies out to where the mark stopped.</summary>
        public void Cast(float p)
        {
            _mode = Mode.Flying; _t = 0; _whip = 0;
            _cast = Mathf.Lerp(Near, Far, p);
            _from = Tip(); _to = OnWater(_cast);
            _strip.gameObject.SetActive(false);
        }

        /// <summary>A fish takes: the nearest shape darts to the float and it goes under.</summary>
        public void Bite()
        {
            _mode = Mode.Bite; _t = 0;
            _taker = null; float best = float.MaxValue;
            foreach (var s in _swimmers) { float d = (s.Pos - _floatAt).sqrMagnitude; if (d < best) { best = d; _taker = s; } }
            if (_taker != null) { _taker.Goal = _floatAt; _taker.Speed = 5; }
            Ripple(1.6f);
        }

        /// <summary>The strike set the hook: the shape under the float becomes the fish that's on.</summary>
        public void Hooked(SpeciesDef sp)
        {
            _mode = Mode.Fight; _progress = 0; _fishAt = _floatAt;
            if (_taker == null) _taker = AddSwimmer(_floatAt, false);
            Dress(_taker, sp, true);
            Splash(_floatAt, 18, 1f);
        }

        /// <summary>Each frame of the fight: how tight the line is, how near the fish is, and whether it's running.</summary>
        public void Fight(float tension, float progress, bool running, bool warning)
        {
            _tension = tension; _progress = progress; _running = running; _warning = warning;
        }

        /// <summary>Landed: the fish comes up out of the water in a splash and swings in to the angler's hand.</summary>
        public void Land(SpeciesDef sp, Color glow)
        {
            _mode = Mode.Landing; _t = 0;
            if (_taker != null) _taker.T.gameObject.SetActive(false);
            Splash(_floatAt, 30, 1.4f);
            _float.gameObject.SetActive(false);
            var pic = Hud.SpeciesPicture(sp);
            _catch = new GameObject("Catch").transform; _catch.SetParent(transform, false);
            var sr = _catch.gameObject.AddComponent<SpriteRenderer>(); sr.sprite = pic; sr.sortingOrder = 2;
            float length = 2.2f + (int)sp.Rarity * 0.35f;
            if (pic != null) _catch.localScale = Vector3.one * (length / Mathf.Max(0.01f, pic.bounds.size.x));
            _glow = GameObject.CreatePrimitive(PrimitiveType.Quad).transform; Destroy(_glow.GetComponent<Collider>());
            _glow.SetParent(_catch, false); _glow.localPosition = new Vector3(0, 0, 0.05f);
            _glow.localScale = Vector3.one * (pic != null ? pic.bounds.size.x * 1.3f : 3);
            var gm = Life.Puff(true); gm.SetColor("_BaseColor", new Color(glow.r, glow.g, glow.b, 0.75f));
            _glow.GetComponent<MeshRenderer>().sharedMaterial = gm;
            _from = _floatAt;
        }

        /// <summary>The fish is gone (snapped, slipped, missed): the shape swims off and the line comes back slack.</summary>
        public void Lost(bool snapped)
        {
            _mode = Mode.Done; _t = 0;
            if (_taker != null) { _taker.Goal = _taker.Pos + Out * 8 + Side * Random.Range(-6f, 6f); _taker.Speed = 6; _taker.Rest = 0; }
            if (snapped) _float.gameObject.SetActive(false);
            Ripple(1f);
        }

        /// <summary>Ready to cast again: the float back on the rod tip, the strip on the water.</summary>
        public void Reset()
        {
            _mode = Mode.Aim; _t = 0; _tension = 0.2f; _progress = 0; _running = _warning = false;
            if (_catch != null) { Destroy(_catch.gameObject); _catch = null; }
            if (_taker != null)
            {
                // The one just landed comes back as another fish, out in the water.
                if (!_taker.T.gameObject.activeSelf) { _taker.T.gameObject.SetActive(true); _taker.Pos = _taker.Goal = OnWater(Far + 3) + Side * Random.Range(-5f, 5f); }
                Dress(_taker, null, false); _taker.Speed = 1; _taker = null;
            }
            _float.gameObject.SetActive(true);
            _strip.gameObject.SetActive(true);
        }

        // ---- Building -------------------------------------------------------------------------------------------------

        void Build()
        {
            var cam = CameraRig.Instance;
            _camFocus = cam.Focus; _camDistance = cam.Distance; _limX = cam.LimX; _limZ = cam.LimZ; _limD = cam.LimD;
            // The camera over the quay's end: the angler on the right, the water and the float to the left.
            var focus = Stand + Out * 6.5f + new Vector3(0, 0, 0.6f); focus.y = 0;
            cam.LimX = new Vector2(focus.x, focus.x); cam.LimZ = new Vector2(focus.z, focus.z); cam.LimD = new Vector2(26, 44);
            cam.Focus = focus; cam.Distance = 32; cam.Cut();

            float yaw = Mathf.Atan2(Out.x, Out.z) * Mathf.Rad2Deg;
            Models.Kit("hand_1_idle", transform, Stand, yaw, 1.9f);
            Models.Kit("bucket_fish", transform, Stand + Side * 1.1f - Out * 0.9f, 30, 0.75f);
            Models.Kit("box", transform, Stand - Side * 1.2f - Out * 1.1f, 12, 0.8f);

            _rod = Line("Rod", Palette.Hex(0x2A1A10), 0.2f, 0.08f, 14);
            // A dark cork grip, a pale cane rod with a red whipping near the tip.
            var rg = new Gradient();
            rg.SetKeys(new[] { new GradientColorKey(Palette.Hex(0x3A2414), 0), new GradientColorKey(Palette.Hex(0x3A2414), 0.16f), new GradientColorKey(Palette.Hex(0xE8C27A), 0.2f),
                               new GradientColorKey(Palette.Hex(0xE0B868), 0.86f), new GradientColorKey(Palette.Hex(0xD8402E), 0.9f), new GradientColorKey(Palette.Hex(0xE0B868), 0.94f) },
                       new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
            _rod.colorGradient = rg;
            _line = Line("Line", Palette.Hex(0xF4F1E6), 0.05f, 0.05f, 20);
            _float = Models.Built("Float", transform, mb =>
            {
                mb.Paint();
                mb.Sphere(new Vector3(0, 0.02f, 0), new Vector3(0.2f, 0.16f, 0.2f), 10, Palette.White);
                mb.Sphere(new Vector3(0, 0.15f, 0), new Vector3(0.16f, 0.12f, 0.16f), 10, Palette.Hex(0xE8402E));
                mb.Cylinder(new Vector3(0, 0.2f, 0), new Vector3(0, 0.6f, 0), 0.035f, 6, Palette.Hex(0xE8402E));
                mb.Matte();
            }).transform;
            _float.localScale = Vector3.one * 1.4f;

            // The ripple round the float: a soft ring that spreads when it lands and when a fish takes.
            _ripple = GameObject.CreatePrimitive(PrimitiveType.Quad).transform; Destroy(_ripple.GetComponent<Collider>());
            _ripple.SetParent(transform, false); _ripple.rotation = Quaternion.Euler(90, 0, 0);
            _rippleMat = Life.Puff(false, false); _rippleMat.SetTexture("_BaseMap", RingTexture); _rippleMat.mainTexture = RingTexture;
            _ripple.GetComponent<MeshRenderer>().sharedMaterial = _rippleMat;
            _ripple.gameObject.SetActive(false);

            // Where to cast: a pale strip on the water, the good water green, the best gold; the mark runs along it.
            _strip = new GameObject("Strip").transform; _strip.SetParent(transform, false);
            float mid = (Near + Far) / 2, len = Far - Near;
            Patch(_strip, mid, len, 1.5f, new Color(1, 1, 1, 0.28f));
            Patch(_strip, mid, len * 0.34f, 1.5f, new Color(0.3f, 0.85f, 0.45f, 0.5f));
            Patch(_strip, mid, len * 0.1f, 1.5f, new Color(1f, 0.8f, 0.2f, 0.85f));
            _marker = GameObject.CreatePrimitive(PrimitiveType.Quad).transform; Destroy(_marker.GetComponent<Collider>());
            _marker.SetParent(_strip, false); _marker.rotation = Quaternion.LookRotation(Vector3.down, Out); _marker.localScale = Vector3.one * 2.4f;
            var mm = Life.Puff(false, false); mm.SetTexture("_BaseMap", RingTexture); mm.mainTexture = RingTexture; mm.SetColor("_BaseColor", Color.white);
            _marker.GetComponent<MeshRenderer>().sharedMaterial = mm;

            // Spray.
            var sgo = new GameObject("Splash"); sgo.transform.SetParent(transform, false);
            _splash = sgo.AddComponent<ParticleSystem>(); _splash.Stop();
            var main = _splash.main; main.loop = false; main.playOnAwake = false; main.maxParticles = 200; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1.6f; main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            var em = _splash.emission; em.enabled = false; var sh = _splash.shape; sh.enabled = false;
            var col = _splash.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0.9f, 0), new GradientAlphaKey(0, 1) });
            col.color = g;
            var pr = sgo.GetComponent<ParticleSystemRenderer>(); pr.sharedMaterial = Life.Puff(false); pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // The fish in the water: small ones near the quay, the big ones out in the gold.
            var h = GameRoot.Sim.H; var pool = Book.In(h.id);
            for (int i = 0; i < 7; i++)
            {
                bool big = i < 2;
                var s = AddSwimmer(OnWater(big ? Random.Range(12f, 16f) : Random.Range(Near, Far)) + Side * Random.Range(-4f, 4f), big);
                Dress(s, pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null, false);
            }
            Reset();
            Aim(0);
            _float.position = Tip();
        }

        LineRenderer Line(string name, Color c, float w0, float w1, int points)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var l = go.AddComponent<LineRenderer>();
            l.useWorldSpace = true; l.positionCount = points; l.startWidth = w0; l.endWidth = w1;
            l.startColor = l.endColor = c; l.numCapVertices = 2; l.generateLightingData = true;
            l.sharedMaterial = Materials.Kit; l.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            return l;
        }

        void Patch(Transform parent, float at, float length, float width, Color c)
        {
            var q = GameObject.CreatePrimitive(PrimitiveType.Quad).transform; Destroy(q.GetComponent<Collider>());
            q.SetParent(parent, false);
            q.position = OnWater(at) + Vector3.up * (0.01f + c.a * 0.01f);
            q.rotation = Quaternion.LookRotation(Vector3.down, Out);
            q.localScale = new Vector3(width, length, 1);
            var m = Life.Puff(false, false); m.SetColor("_BaseColor", c); m.renderQueue = 3000 + Mathf.RoundToInt(c.a * 10);
            q.GetComponent<MeshRenderer>().sharedMaterial = m;
        }

        Swimmer AddSwimmer(Vector3 at, bool big)
        {
            var go = new GameObject("Shape"); go.transform.SetParent(transform, false);
            var s = new Swimmer { T = go.transform, R = go.AddComponent<SpriteRenderer>(), Pos = at, Goal = at, Big = big, Speed = 1, Heading = Random.Range(0, 360f) };
            s.R.color = new Color(0.02f, 0.09f, 0.14f, 0.5f); s.R.sortingOrder = 1;
            _swimmers.Add(s);
            return s;
        }

        /// <summary>Gives a shape a species' outline (dark, under the water) and its size.</summary>
        void Dress(Swimmer s, SpeciesDef sp, bool hooked)
        {
            if (sp != null) s.R.sprite = Hud.SpeciesPicture(sp);
            float rank = sp != null ? (int)sp.Rarity : s.Big ? 3 : 1;
            s.Length = (s.Big && !hooked ? 2.4f : 1.2f) + rank * 0.25f;
            if (s.R.sprite != null) s.T.localScale = Vector3.one * (s.Length / Mathf.Max(0.01f, s.R.sprite.bounds.size.x));
            s.R.color = new Color(0.02f, 0.09f, 0.14f, hooked ? 0.62f : 0.45f);
        }

        static Texture2D _ring;
        static Texture2D RingTexture
        {
            get
            {
                if (_ring != null) return _ring;
                _ring = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                {
                    float r = new Vector2(x - 31.5f, y - 31.5f).magnitude / 31.5f;
                    float a = Mathf.Clamp01(1 - Mathf.Abs(r - 0.8f) / 0.14f);
                    _ring.SetPixel(x, y, new Color(1, 1, 1, a * a));
                }
                _ring.Apply();
                return _ring;
            }
        }

        // ---- Each frame -------------------------------------------------------------------------------------------

        Vector3 Tip()
        {
            // The rod held up and out over the water, whipped back for a cast, bowed toward the float under load.
            float lift = Mathf.Lerp(55, 38, Mathf.Clamp01(_bend));
            if (_whip > 0) lift += Mathf.Sin(Mathf.Clamp01(_whip / 0.35f) * Mathf.PI) * 40;
            var dir = Quaternion.AngleAxis(-lift, Side) * Out;
            return Hands + dir * RodLength;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime, time = Time.unscaledTime;
            _t += dt;
            if (_whip >= 0 && _mode == Mode.Flying) _whip += dt;

            // The float.
            switch (_mode)
            {
                case Mode.Aim:
                    _float.position = Tip() + Vector3.down * 0.9f + Side * Mathf.Sin(time * 2) * 0.08f;
                    break;
                case Mode.Flying:
                {
                    float k = Mathf.Clamp01((_t - 0.2f) / 0.55f);
                    var p = Vector3.Lerp(_from, _to, k); p.y += Mathf.Sin(k * Mathf.PI) * (2.5f + _cast * 0.12f);
                    _float.position = k <= 0 ? Tip() + Vector3.down * 0.9f : p;
                    if (k >= 1) { _floatAt = _to; _mode = Mode.Float; Splash(_floatAt, 10, 0.6f); Ripple(1.2f); }
                    break;
                }
                case Mode.Float:
                    _float.position = _floatAt + Vector3.up * Mathf.Sin(time * 3) * 0.04f;
                    break;
                case Mode.Bite:
                    _float.position = _floatAt + Vector3.down * (0.12f + Mathf.Abs(Mathf.Sin(time * 30)) * 0.12f);
                    if ((_rippleT -= dt) <= 0) Ripple(0.8f);
                    break;
                case Mode.Fight:
                {
                    // The fish pulls out and to the side on a run; it comes in toward the quay as it tires.
                    float reach = Mathf.Lerp(_cast, 2.2f, _progress);
                    float swing = (_running ? Mathf.Sin(time * 1.7f) * 4.5f : Mathf.Sin(time * 0.9f) * 1.6f);
                    var want = OnWater(reach + (_running ? 1.2f : 0)) + Side * swing;
                    _fishAt = Vector3.Lerp(_fishAt, want, 1 - Mathf.Exp(-(_running ? 3 : 1.6f) * dt));
                    if (_warning) _fishAt += Side * Mathf.Sin(time * 50) * 0.03f;
                    _floatAt = _fishAt;
                    _float.position = _fishAt + Vector3.down * (0.08f + (_running ? 0.1f : 0.02f) * (1 + Mathf.Sin(time * 18)));
                    if (_running && (_rippleT -= dt) <= 0) { Ripple(0.9f); Splash(_fishAt, 4, 0.5f); }
                    break;
                }
                case Mode.Landing:
                {
                    // Up out of the water, over the edge of the quay, held up by the angler.
                    float k = Mathf.Clamp01(_t / 0.9f), e = 1 - (1 - k) * (1 - k);
                    var hold = Hands + new Vector3(0, 1.7f, 0) - Out * 0.3f;
                    var p = Vector3.Lerp(_from, hold, e); p.y += Mathf.Sin(k * Mathf.PI) * 2.2f;
                    _catch.position = p;
                    var cam = CameraRig.Instance.Cam.transform;
                    _catch.rotation = Quaternion.LookRotation(cam.forward, Vector3.up) * Quaternion.Euler(0, 0, Mathf.Sin(time * (k < 1 ? 14 : 5)) * (k < 1 ? 18 : 5));
                    if (_glow != null) _glow.localScale = Vector3.one * Mathf.Lerp(0.2f, 1, e) * (_catch.GetComponent<SpriteRenderer>().sprite != null ? _catch.GetComponent<SpriteRenderer>().sprite.bounds.size.x * 1.4f : 3);
                    _floatAt = p;
                    break;
                }
                case Mode.Done:
                    if (_float.gameObject.activeSelf) _float.position = Vector3.Lerp(_float.position, OnWater(Mathf.Max(2, _cast * 0.6f)), dt * 0.8f);
                    break;
            }

            // The rod: how far it bows follows the line's pull.
            float pull = _mode == Mode.Fight ? Mathf.Clamp01(_tension) * (_running ? 1.15f : 1) : _mode == Mode.Bite ? 0.25f : _mode == Mode.Landing ? 0.7f * (1 - Mathf.Clamp01(_t / 0.9f)) + 0.15f : 0.05f;
            _bend = Mathf.Lerp(_bend, pull, 1 - Mathf.Exp(-10 * dt));
            DrawRod(time);
            DrawLine(time);

            // The ripple fades as it spreads.
            if (_ripple.gameObject.activeSelf)
            {
                float a = _ripple.localScale.x;
                _ripple.localScale = Vector3.one * (a + dt * 2.2f);
                var c = _rippleMat.GetColor("_BaseColor"); c.a = Mathf.Max(0, c.a - dt * 0.9f); _rippleMat.SetColor("_BaseColor", c);
                if (c.a <= 0) _ripple.gameObject.SetActive(false);
            }

            Swim(dt, time);
        }

        void DrawRod(float time)
        {
            var b = Hands; var tip = Tip();
            var toward = (_floatAt - tip); toward.y = Mathf.Min(toward.y, 0); toward = toward.sqrMagnitude > 0.01f ? toward.normalized : Out;
            var bow = (toward * 0.9f + Vector3.down * 0.7f) * _bend * 1.2f;
            if (_mode == Mode.Fight && _running) bow += Side * Mathf.Sin(time * 25) * 0.05f;
            var c = b + (tip - b) * 0.55f;
            int n = _rod.positionCount;
            for (int i = 0; i < n; i++)
            {
                float t = i / (n - 1f);
                var p = (1 - t) * (1 - t) * b + 2 * (1 - t) * t * c + t * t * (tip + bow);
                _rod.SetPosition(i, p);
            }
        }

        Vector3 RodTip => _rod.GetPosition(_rod.positionCount - 1);

        void DrawLine(float time)
        {
            var a = RodTip;
            var end = _mode == Mode.Landing && _catch != null ? _catch.position + Vector3.up * 0.3f : _float.position + Vector3.up * 0.15f;
            bool show = _mode == Mode.Landing || _float.gameObject.activeSelf;
            _line.enabled = show; if (!show) return;
            // Slack line hangs down toward the water; a tight one runs straight.
            float slack = _mode == Mode.Fight ? Mathf.Clamp01(1 - _tension * 1.3f) : _mode == Mode.Landing ? 0 : _mode == Mode.Aim ? 0 : 0.6f;
            float span = (end - a).magnitude;
            int n = _line.positionCount;
            for (int i = 0; i < n; i++)
            {
                float t = i / (n - 1f);
                var p = Vector3.Lerp(a, end, t);
                p.y -= Mathf.Sin(t * Mathf.PI) * slack * span * 0.16f;
                if (p.y < Water) p.y = Water;
                _line.SetPosition(i, p);
            }
            var tight = Color.Lerp(Palette.Hex(0xF4F1E6), Palette.Hex(0xFF5A3C), Mathf.InverseLerp(0.8f, 1f, _mode == Mode.Fight ? _tension : 0));
            _line.startColor = _line.endColor = tight;
        }

        void Swim(float dt, float time)
        {
            foreach (var s in _swimmers)
            {
                if (!s.T.gameObject.activeSelf) continue;
                if (s == _taker && _mode == Mode.Fight) { s.Goal = _fishAt; s.Pos = Vector3.Lerp(s.Pos, _fishAt - Out * 0.2f, 1 - Mathf.Exp(-8 * dt)); }
                else
                {
                    var d = s.Goal - s.Pos; d.y = 0;
                    if (d.magnitude < 0.3f)
                    {
                        if ((s.Rest -= dt) <= 0 && s != _taker)
                        {
                            float r = s.Big ? Random.Range(11f, 17f) : Random.Range(Near - 1, Far);
                            s.Goal = OnWater(r) + Side * Random.Range(-5f, 5f); s.Rest = Random.Range(0.5f, 2.5f); s.Speed = Random.Range(0.6f, 1.4f);
                        }
                    }
                    else s.Pos += d.normalized * Mathf.Min(d.magnitude, s.Speed * dt);
                }
                var move = s.Goal - s.Pos; move.y = 0;
                if (move.sqrMagnitude > 0.01f) s.Heading = Mathf.LerpAngle(s.Heading, Mathf.Atan2(move.z, move.x) * Mathf.Rad2Deg, 1 - Mathf.Exp(-4 * dt));
                float wiggle = Mathf.Sin(time * (s.Speed * 6 + 2) + s.Length) * 6;
                s.T.position = new Vector3(s.Pos.x, Water + 0.01f, s.Pos.z);
                s.T.rotation = Quaternion.Euler(0, -(s.Heading + wiggle), 0) * Quaternion.Euler(90, 0, 0);
            }
        }

        void Splash(Vector3 at, int count, float strength)
        {
            for (int i = 0; i < count; i++)
            {
                var v = new Vector3(Random.Range(-1f, 1f), Random.Range(2.5f, 4.5f), Random.Range(-1f, 1f)) * strength;
                _splash.Emit(new ParticleSystem.EmitParams { position = at + Vector3.up * 0.1f, velocity = v, startSize = Random.Range(0.2f, 0.45f) * strength, startColor = Color.white, startLifetime = Random.Range(0.5f, 0.9f) }, 1);
            }
        }

        void Ripple(float size)
        {
            _ripple.gameObject.SetActive(true);
            _ripple.position = _floatAt + Vector3.up * 0.03f;
            _ripple.localScale = Vector3.one * size;
            _rippleMat.SetColor("_BaseColor", new Color(1, 1, 1, 0.8f));
            _rippleT = 0.7f;
        }
    }
}

using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The small things that make the harbour look alive: foam behind a boat under way, smoke from the smokehouse,
    /// gulls wheeling over the water, and the lighthouse's beam sweeping round after dark.
    /// </summary>
    public static class Life
    {
        static Texture2D _soft;
        /// <summary>A round soft-edged dot, the basis of every puff and glow.</summary>
        public static Texture2D Soft
        {
            get
            {
                if (_soft != null) return _soft;
                _soft = new Texture2D(64, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                {
                    float d = Mathf.Clamp01(1 - new Vector2(x - 31.5f, y - 31.5f).magnitude / 31.5f);
                    _soft.SetPixel(x, y, new Color(1, 1, 1, d * d));
                }
                _soft.Apply();
                return _soft;
            }
        }

        /// <summary>A see-through material for puffs (blended) or light (added), tinted by each particle or vertex.</summary>
        public static Material Puff(bool additive, bool textured = true)
        {
            var basis = Resources.Load<Material>("Materials/Weather");
            var m = basis != null ? new Material(basis) : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            if (textured) { m.SetTexture("_BaseMap", Soft); m.mainTexture = Soft; }
            m.SetFloat("_Surface", 1); m.SetInt("_SrcBlend", 5); m.SetInt("_DstBlend", additive ? 1 : 10); m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.renderQueue = additive ? 3100 : 3000;
            m.SetColor("_BaseColor", Color.white);
            return m;
        }

        static Material _foam, _smokeMat;

        /// <summary>Foam left on the water behind a boat: laid down by distance, so a moored boat leaves none.</summary>
        public static void Wake(Transform boat, float length)
        {
            var go = new GameObject("Wake"); go.transform.SetParent(boat, false); go.transform.localPosition = new Vector3(0, 0.12f, -length * 0.42f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main; main.simulationSpace = ParticleSystemSimulationSpace.World; main.startSpeed = 0; main.startLifetime = 2.4f; main.maxParticles = 200;
            main.startSize = new ParticleSystem.MinMaxCurve(length * 0.28f, length * 0.4f); main.startColor = new Color(1, 1, 1, 0.75f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var em = ps.emission; em.rateOverTime = 0; em.rateOverDistance = 3.2f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(length * 0.22f, 0, 0.2f);
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 1.9f)));
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.12f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            r.sharedMaterial = _foam != null ? _foam : (_foam = Puff(false)); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        /// <summary>Smoke drifting up from a chimney, thinning as it climbs.</summary>
        public static ParticleSystem Smoke(Transform parent, Vector3 at)
        {
            var go = new GameObject("Smoke"); go.transform.SetParent(parent, false); go.transform.position = at; go.transform.rotation = Quaternion.Euler(-90, 0, 0);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main; main.simulationSpace = ParticleSystemSimulationSpace.World; main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 2.4f); main.startLifetime = new ParticleSystem.MinMaxCurve(4.5f, 6.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 1.9f); main.startColor = new Color(0.96f, 0.96f, 0.98f, 0.55f); main.maxParticles = 60;
            main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            var em = ps.emission; em.rateOverTime = 4;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.angle = 7; shape.radius = 0.25f;
            var force = ps.forceOverLifetime; force.enabled = true; force.space = ParticleSystemSimulationSpace.World; force.x = 0.7f; force.z = 0.35f;     // the breeze off the water
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 3.2f)));
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.15f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>(); r.sharedMaterial = _smokeMat != null ? _smokeMat : (_smokeMat = Puff(false)); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return ps;
        }

        /// <summary>The lighthouse's beam: two faint wedges of light turning round the lantern, and the lantern's own glow.</summary>
        public static Beam LighthouseBeam(Transform parent, Vector3 lantern)
        {
            var go = new GameObject("Beam"); go.transform.SetParent(parent, false); go.transform.position = lantern;
            var mesh = new Mesh(); var v = new List<Vector3>(); var c = new List<Color>(); var t = new List<int>();
            const float Len = 95, Half = 13, Drop = 16;
            foreach (float dir in new[] { 1f, -1f })
            {
                int b = v.Count;
                v.Add(Vector3.zero); v.Add(new Vector3(-Half, -Drop, Len * dir)); v.Add(new Vector3(Half, -Drop, Len * dir)); v.Add(new Vector3(0, -Drop * 0.45f, Len * 0.5f * dir));
                c.Add(new Color(1, 0.96f, 0.8f, 0.5f)); c.Add(new Color(1, 0.96f, 0.8f, 0)); c.Add(new Color(1, 0.96f, 0.8f, 0)); c.Add(new Color(1, 0.96f, 0.8f, 0.2f));
                t.AddRange(new[] { b, b + 1, b + 3, b, b + 3, b + 2, b + 1, b + 2, b + 3, b, b + 3, b + 1, b, b + 2, b + 3 });      // both faces
            }
            mesh.SetVertices(v); mesh.SetColors(c); mesh.SetTriangles(t, 0); mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>(); var mat = Puff(true, false); r.sharedMaterial = mat; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var beam = go.AddComponent<Beam>(); beam.Mat = mat;
            return beam;
        }

        /// <summary>A handful of gulls circling over the water off the quay.</summary>
        public static void Gulls(Transform parent, int count, int seed)
        {
            var rnd = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                var g = new GameObject("Gull").AddComponent<Gull>(); g.transform.SetParent(parent, false);
                g.Centre = new Vector3(-18 - (float)rnd.NextDouble() * 30, 9 + (float)rnd.NextDouble() * 6, -40 + (float)rnd.NextDouble() * 80);
                g.Radius = 9 + (float)rnd.NextDouble() * 12; g.Speed = (0.35f + (float)rnd.NextDouble() * 0.25f) * (i % 2 == 0 ? 1 : -1); g.Phase = (float)rnd.NextDouble() * 10;
            }
        }
    }

    public sealed class Beam : MonoBehaviour
    {
        public Material Mat;
        void Update()
        {
            var s = GameRoot.Sim; if (s == null) return;
            float night = Mathf.Clamp01(s.Night * 1.5f - 0.2f);
            if (Mat != null) Mat.SetColor("_BaseColor", new Color(1, 1, 1, night * 0.55f));
            transform.rotation = Quaternion.Euler(0, Time.time * 26, 0);
        }
    }

    /// <summary>A gull: a white body and two wings that beat, gliding round a circle and banking into the turn.</summary>
    public sealed class Gull : MonoBehaviour
    {
        public Vector3 Centre; public float Radius, Speed, Phase;
        Transform _left, _right;
        static Mesh _body, _wing; static Material _mat;

        void Start()
        {
            if (_body == null)
            {
                var mb = new MeshBuilder(); mb.Mat(0.05f);
                mb.Sphere(Vector3.zero, new Vector3(0.22f, 0.2f, 0.62f), 6, Palette.Hex(0xFAFAF6), false);
                mb.Sphere(new Vector3(0, 0.06f, 0.6f), new Vector3(0.15f, 0.14f, 0.17f), 5, Palette.Hex(0xFFFFFF), false);
                mb.Box(new Vector3(0, 0.04f, 0.82f), new Vector3(0.06f, 0.05f, 0.2f), Palette.Hex(0xF2A93B));
                mb.Box(new Vector3(0, 0.02f, -0.72f), new Vector3(0.3f, 0.03f, 0.3f), Palette.Hex(0xD8DCE0));
                _body = mb.ToMesh("Gull");
                var wb = new MeshBuilder(); wb.Mat(0.05f);
                wb.Box(new Vector3(0.55f, 0, 0.05f), new Vector3(1.1f, 0.04f, 0.5f), Palette.Hex(0xF4F6F8));
                wb.Box(new Vector3(1.35f, 0, -0.05f), new Vector3(0.6f, 0.035f, 0.34f), Palette.Hex(0x5A6068));       // dark tips
                _wing = wb.ToMesh("Gull wing");
                _mat = Materials.World;
            }
            gameObject.AddComponent<MeshFilter>().sharedMesh = _body; gameObject.AddComponent<MeshRenderer>().sharedMaterial = _mat;
            _left = Part(1); _right = Part(-1);
            transform.localScale = Vector3.one * 1.5f;
        }

        Transform Part(float side)
        {
            var go = new GameObject("Wing"); go.transform.SetParent(transform, false); go.transform.localScale = new Vector3(side, 1, 1);
            go.AddComponent<MeshFilter>().sharedMesh = _wing; go.AddComponent<MeshRenderer>().sharedMaterial = _mat;
            return go.transform;
        }

        void Update()
        {
            float a = Time.time * Speed + Phase;
            var p = Centre + new Vector3(Mathf.Cos(a) * Radius, Mathf.Sin(a * 1.7f) * 1.2f, Mathf.Sin(a) * Radius * 1.4f);
            var ahead = Centre + new Vector3(Mathf.Cos(a + 0.05f * Mathf.Sign(Speed)) * Radius, 0, Mathf.Sin(a + 0.05f * Mathf.Sign(Speed)) * Radius * 1.4f);
            transform.position = p;
            var dir = ahead - p; dir.y = 0;
            if (dir.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 0, -18 * Mathf.Sign(Speed));
            // A few beats, then a glide.
            float cycle = Mathf.Repeat(Time.time * 0.35f + Phase, 1);
            float flap = cycle < 0.55f ? Mathf.Sin(cycle / 0.55f * Mathf.PI * 6) * 32 : 6;
            if (_left != null) { _left.localRotation = Quaternion.Euler(0, 0, flap); _right.localRotation = Quaternion.Euler(0, 0, -flap); }
        }
    }
}

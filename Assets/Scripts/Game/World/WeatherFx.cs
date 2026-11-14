using ReelEmpire.Core;
using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>Weather you can see: rain that follows the camera (snow in the Arctic), and lightning in a storm.</summary>
    public sealed class WeatherFx : MonoBehaviour
    {
        ParticleSystem _ps;
        ParticleSystemRenderer _r;
        int _shown = -1;
        float _nextFlash;

        public static WeatherFx Create(Transform parent)
        {
            var go = new GameObject("Weather");
            go.transform.SetParent(parent, false);
            var w = go.AddComponent<WeatherFx>();
            w._ps = go.AddComponent<ParticleSystem>();
            w._r = go.GetComponent<ParticleSystemRenderer>();
            var basis = Resources.Load<Material>("Materials/Weather");
            var mat = basis != null ? new Material(basis) : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            mat.SetFloat("_Surface", 1); mat.SetColor("_BaseColor", new Color(1, 1, 1, 0.55f));
            mat.renderQueue = 3000; mat.SetInt("_SrcBlend", 5); mat.SetInt("_DstBlend", 10); mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            w._r.sharedMaterial = mat;
            var main = w._ps.main; main.loop = true; main.maxParticles = 2500; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var shape = w._ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(240, 1, 200);
            var em = w._ps.emission; em.rateOverTime = 0;
            return w;
        }

        void LateUpdate()
        {
            var cam = CameraRig.Instance; var s = GameRoot.Sim;
            if (cam == null || s == null) return;
            transform.position = cam.Focus + Vector3.up * 60;
            var w = s.Weather; bool snow = s.H.id == "arctic";
            // Lightning: a flash every few seconds of a storm, sometimes two together.
            if (w == Weather.Storm && Time.time > _nextFlash)
            {
                Look.Flash = 1;
                _nextFlash = Time.time + (Random.value < 0.3f ? 0.18f : Random.Range(4f, 11f));
            }
            int key = (int)w * 2 + (snow ? 1 : 0);
            if (key == _shown) return;
            _shown = key;
            var main = _ps.main; var em = _ps.emission;
            bool wet = w == Weather.Rain || w == Weather.Storm;
            em.rateOverTime = !wet ? 0 : snow ? 450 : w == Weather.Storm ? 2200 : 1100;
            main.startSpeed = snow ? 9 : 70;
            main.startLifetime = snow ? 7 : 1.0f;
            main.startSize = snow ? new ParticleSystem.MinMaxCurve(0.4f, 0.9f) : 0.09f;
            main.startColor = snow ? new Color(1, 1, 1, 0.9f) : new Color(0.85f, 0.92f, 1f, 0.38f);
            _r.renderMode = snow ? ParticleSystemRenderMode.Billboard : ParticleSystemRenderMode.Stretch;
            _r.lengthScale = 34; _r.velocityScale = 0;
            transform.rotation = Quaternion.Euler(snow ? 90 : 78, 20, 0);     // emits along +Z: point it down (rain slants)
            if (wet) _ps.Play(); else _ps.Stop();
        }
    }
}

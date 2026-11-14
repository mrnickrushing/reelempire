using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The picture: a filmic tone curve, a touch of bloom on glass and lights, a little more contrast and colour, a
    /// faint vignette, and shadows that reach across the whole harbour. The weather tints and dims it.
    /// </summary>
    public sealed class Look : MonoBehaviour
    {
        public static Look Instance { get; private set; }
        /// <summary>Lightning: set to 1 and it fades in a blink.</summary>
        public static float Flash;
        ColorAdjustments _grade;
        Bloom _bloom;
        Light _sun;
        float _gloom;

        public static Look Create(Camera cam, Light sun)
        {
            var go = new GameObject("Look");
            var look = go.AddComponent<Look>();
            Instance = look;
            look._sun = sun;
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null) { urp.shadowDistance = 420; urp.shadowCascadeCount = 2; }

            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            var p = ScriptableObject.CreateInstance<VolumeProfile>();
            vol.profile = p;
            var tone = p.Add<Tonemapping>(); tone.mode.Override(TonemappingMode.ACES);
            var bloom = p.Add<Bloom>(); look._bloom = bloom; bloom.intensity.Override(0.18f); bloom.threshold.Override(1.5f); bloom.scatter.Override(0.6f);
            look._grade = p.Add<ColorAdjustments>();
            look._grade.postExposure.Override(0.1f); look._grade.contrast.Override(8f); look._grade.saturation.Override(22f);
            var vig = p.Add<Vignette>(); vig.intensity.Override(0.2f); vig.smoothness.Override(0.5f);
            return look;
        }

        void Update()
        {
            var sim = GameRoot.Sim;
            if (sim == null) return;
            Flash = Mathf.MoveTowards(Flash, 0, Time.unscaledDeltaTime * 7);
            var w = sim.Weather;
            const bool inside = false;
            float want = inside || w == Core.Weather.Clear ? 0 : w == Core.Weather.Rain ? 0.45f : w == Core.Weather.Fog ? 0.6f : 0.9f;
            _gloom = inside ? 0 : Mathf.MoveTowards(_gloom, want, Time.unscaledDeltaTime * 0.4f);
            _grade.postExposure.Override(Mathf.Lerp(0.1f, Mathf.Lerp(-0.3f, 0.05f, sim.Night), _gloom));     // bad weather darkens the day, not the night
            _grade.saturation.Override(Mathf.Lerp(22f, -25f, _gloom));
            _grade.colorFilter.Override(Color.Lerp(Color.white, new Color(0.78f, 0.84f, 0.95f), _gloom));
            // The day: the sun swings round and sinks, the sky dims, and everything that glows comes on at dusk.
            float night = sim.Night * 0.72f, phase = sim.DayPhase;      // a bright moonlit night: the harbour stays readable
            Shader.SetGlobalFloat("_NightGlow", night);
            // Clouds drift over on a fair day and leave soft shadows; under a grey sky there is nothing to cast them.
            Shader.SetGlobalFloat("_CloudShade", 0.3f * (1 - _gloom) * (1 - sim.Night * 0.7f));
            Shader.SetGlobalColor("_GlowColor", new Color(2.6f, 2.2f, 1.5f));
            if (_sun != null)
            {
                float day = 1 - night;
                float elev = Mathf.Lerp(8, 62, Mathf.Sin(Mathf.Clamp01(phase / 0.56f) * Mathf.PI));
                if (inside) { elev = 58; phase = 0.3f; }
                _sun.transform.rotation = Quaternion.Euler(night > 0.99f ? 50 : elev, -150 + phase * 140, 0);
                _sun.color = Color.Lerp(new Color(0.55f, 0.65f, 1f), Color.Lerp(new Color(1f, 0.74f, 0.5f), new Color(1f, 0.96f, 0.88f), Mathf.Clamp01((elev - 8) / 25f)), day);
                _sun.intensity = Mathf.Lerp(0.95f, Mathf.Lerp(1.35f, 0.5f, _gloom), day);
                _sun.intensity += Flash * 3f;
                _sun.shadowStrength = Mathf.Lerp(0.5f, Mathf.Lerp(0.85f, 0.3f, _gloom), day);
            }
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(0.62f, 0.72f, 0.86f), new Color(0.36f, 0.44f, 0.68f), night);
            RenderSettings.ambientEquatorColor = Color.Lerp(new Color(0.55f, 0.58f, 0.56f), new Color(0.3f, 0.36f, 0.52f), night);
            RenderSettings.ambientGroundColor = Color.Lerp(new Color(0.3f, 0.32f, 0.26f), new Color(0.08f, 0.1f, 0.15f), night);
            RenderSettings.reflectionIntensity = Mathf.Lerp(0.15f, 0.1f, night);    // the sky's sun would glare off the flat ground
            float fog = inside ? 0 : w == Core.Weather.Fog ? 1 : w == Core.Weather.Storm ? 0.5f : w == Core.Weather.Rain ? 0.3f : 0;
            // Night fog is lit from below by the harbour: a grey glow, never a black wall.
            RenderSettings.fogColor = Color.Lerp(new Color(0.74f, 0.83f, 0.92f), Color.Lerp(new Color(0.05f, 0.07f, 0.13f), new Color(0.17f, 0.19f, 0.25f), fog), night);
            if (RenderSettings.skybox != null) RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(1.05f, 0.16f, night));
            _bloom.intensity.Override(Mathf.Lerp(0.18f, 0.9f, night));
            _bloom.threshold.Override(Mathf.Lerp(1.5f, 0.95f, night));
            // The camera stands far back (a long lens), so the fog is measured from what it looks at, not from the lens:
            // on a clear day it starts well behind the harbour; in fog it begins just in front and the far side fades out.
            float reach = CameraRig.Instance != null ? CameraRig.Instance.Distance : 300;
            RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, reach + Mathf.Lerp(250, -60, fog), Time.unscaledDeltaTime * 3);
            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, reach + Mathf.Lerp(900, 300, fog), Time.unscaledDeltaTime * 3);
        }
    }
}

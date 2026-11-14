using UnityEngine;

namespace ReelEmpire.Game
{
    /// <summary>The view over the harbour: a three-quarter angle, dragged to pan and pinched (or scrolled) to zoom.</summary>
    public sealed class CameraRig : MonoBehaviour
    {
        public static CameraRig Instance { get; private set; }
        public Camera Cam { get; private set; }

        public const float Pitch = 38f, Yaw = 18f;
        public Vector3 Focus = new Vector3(18, 0, -2);
        public float Distance = 150f;
        public const float MinDistance = 70f, MaxDistance = 260f;
        float _shown;

        public static CameraRig Create()
        {
            var go = new GameObject("Camera");
            var rig = go.AddComponent<CameraRig>();
            rig.Cam = go.AddComponent<Camera>();
            rig.Cam.fieldOfView = 22;
            rig.Cam.nearClipPlane = 1f; rig.Cam.farClipPlane = 2200f;
            rig.Cam.clearFlags = CameraClearFlags.Skybox;
            rig.Cam.backgroundColor = new Color(0.72f, 0.82f, 0.9f);
            go.tag = "MainCamera";
            go.AddComponent<AudioListener>();
            // A phone is wider than a tablet: on a narrow screen stand further back so the whole chain is in view.
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 1.78f;
            rig.Distance = Mathf.Clamp(325f / aspect, 150f, 250f);      // the same width of harbour on every screen: jetty to market
            if (aspect < 1.6f) rig.Focus.x += 8;      // a tall screen sees further down the shore, where the plots run off to the right
            rig._shown = rig.Distance;
            rig.Place();
            Instance = rig;
            return rig;
        }

        /// <summary>Drag by screen pixels: the ground moves with the finger.</summary>
        public void Pan(Vector2 pixels)
        {
            float perPixel = 2f * Distance * Mathf.Tan(Cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
            var right = Quaternion.Euler(0, Yaw, 0) * Vector3.right;
            var fwd = Quaternion.Euler(0, Yaw, 0) * Vector3.forward;
            Focus -= right * pixels.x * perPixel + fwd * pixels.y * perPixel / Mathf.Sin(Pitch * Mathf.Deg2Rad);
            Focus.x = Mathf.Clamp(Focus.x, -45, 60);
            Focus.z = Mathf.Clamp(Focus.z, -40, 40);
        }

        /// <summary>Zoom by a factor (over 1 = closer).</summary>
        public void Zoom(float factor) { Distance = Mathf.Clamp(Distance / Mathf.Max(0.01f, factor), MinDistance, MaxDistance); }

        void LateUpdate()
        {
            _shown = Mathf.Lerp(_shown, Distance, 1 - Mathf.Exp(-12 * Time.unscaledDeltaTime));
            Place();
        }

        void Place()
        {
            var rot = Quaternion.Euler(Pitch, Yaw, 0);
            transform.rotation = rot;
            transform.position = Focus - rot * Vector3.forward * _shown;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The trophy hall, a room the player can go into: every mounted fish on a board on the wall with a brass plate
    /// (its name, its weight and the day it went up), the chosen one in a glass case in the middle, walls in the
    /// colour the player picks. Built well away from the harbour; the camera cuts to it and back.
    /// </summary>
    public sealed class TrophyHall : MonoBehaviour
    {
        public static TrophyHall Instance { get; private set; }
        public static bool Open => Instance != null;
        public static readonly Vector3 Origin = new Vector3(0, 0, -1600);
        /// <summary>The hall is turned to the camera's yaw so the back wall faces the player square on.</summary>
        static readonly Quaternion Turn = Quaternion.Euler(0, CameraRig.Yaw, 0);

        static readonly Color[] WallColours = { Palette.Hex(0x2F5F8A), Palette.Hex(0x2F6A4A), Palette.Hex(0x7A2E2E), Palette.Hex(0xEDE6D6) };
        static readonly Color[] Frames = { Palette.Hex(0x8A8A8A), Palette.Hex(0x3E8A4A), Palette.Hex(0x2F7FC0), Palette.Hex(0x7A4FC8), Palette.Hex(0xE8B23A) };
        const float Spacing = 5.4f, Depth = 18, Height = 7.6f;

        readonly Dictionary<Collider, string> _boards = new Dictionary<Collider, string>();
        Vector3 _focus; float _distance; Vector2 _limX, _limZ, _limD;
        float _width; string _highlight;

        Sim S => GameRoot.Sim;

        public static void Show(string highlight = null)
        {
            if (Instance != null) { Instance._highlight = highlight; Instance.Rebuild(); return; }
            var go = new GameObject("Trophy hall");
            go.transform.SetPositionAndRotation(Origin, Turn);
            Instance = go.AddComponent<TrophyHall>();
            Instance._highlight = highlight;
            var cam = CameraRig.Instance;
            Instance._focus = cam.Focus; Instance._distance = cam.Distance; Instance._limX = cam.LimX; Instance._limZ = cam.LimZ; Instance._limD = cam.LimD;
            Instance.Rebuild();
        }

        public static void Hide()
        {
            if (Instance == null) return;
            var cam = CameraRig.Instance; var me = Instance;
            cam.LimX = me._limX; cam.LimZ = me._limZ; cam.LimD = me._limD;
            cam.Focus = me._focus; cam.Distance = me._distance; cam.Cut();
            Destroy(me.gameObject); Instance = null;
        }

        /// <summary>The mounted fish under a point on the screen, or null.</summary>
        public string Pick(Vector2 screen)
        {
            var ray = CameraRig.Instance.Cam.ScreenPointToRay(screen);
            return Physics.Raycast(ray, out var hit, 3000) && _boards.TryGetValue(hit.collider, out var id) ? id : null;
        }

        public void Rebuild()
        {
            for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
            _boards.Clear();
            var s = S;
            var mounted = s.State.trophies.Select(Book.Get).Where(x => x != null)
                .OrderByDescending(x => x.Rarity).ThenByDescending(x => s.Entry(x.Id)?.best ?? 0).ToList();
            int cols = Mathf.Max(4, Mathf.CeilToInt(mounted.Count / 2f));
            _width = cols * Spacing + 6;
            BuildRoom(s, cols);
            var canvas = WallCanvas();
            for (int i = 0; i < mounted.Count; i++)
            {
                int row = i / cols, col = i % cols;
                float x = -(cols - 1) * Spacing / 2 + col * Spacing, y = row == 0 ? 5.55f : 2.15f;
                Board(mounted[i], new Vector3(x, y, Depth / 2 - 0.3f), canvas, s);
            }
            if (mounted.Count == 0)
            {
                var t = Ui.Label("Empty", canvas, "<b>Your trophy hall</b>\n<size=30>Mount a rare, epic or legendary fish from its BOOK page or a record card. Each one adds to your income everywhere, and hangs here.</size>", 44, Color.white, TextAnchor.MiddleCenter, false);
                t.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(1600, 300)); t.Stroke(2f);
            }
            Centrepiece(s.HallStar, s);
            // The camera: the whole back wall in view, a little room to pan along a long hall.
            var cam = CameraRig.Instance;
            float fit = (_width + 4) / 0.84f;
            float pan = Mathf.Max(0, _width / 2 - 14), along = pan * Mathf.Cos(CameraRig.Yaw * Mathf.Deg2Rad), drift = pan * Mathf.Sin(CameraRig.Yaw * Mathf.Deg2Rad);
            cam.LimX = new Vector2(Origin.x - along - 4, Origin.x + along + 4);
            cam.LimZ = new Vector2(Origin.z - drift - 4, Origin.z + drift + 8);
            cam.LimD = new Vector2(22, Mathf.Max(60, fit * 1.2f));
            cam.Focus = At(new Vector3(0, 0, FocusZ)); cam.Distance = Mathf.Clamp(fit, 34, 70);
            if (_highlight != null)
            {
                int k = mounted.FindIndex(m => m.Id == _highlight);
                if (k >= 0) { float x = Mathf.Clamp(-(cols - 1) * Spacing / 2 + (k % cols) * Spacing, -pan, pan); cam.Focus = At(new Vector3(x, 0, FocusZ + (k < cols ? 3.5f : 2))); cam.Distance = Mathf.Min(cam.Distance, 40); }
            }
            cam.Cut();
        }

        const float FocusZ = 3.5f;
        static Vector3 At(Vector3 local) => Origin + Turn * local;
        Transform Put(string name) { var t = new GameObject(name).transform; t.SetParent(transform, false); return t; }

        /// <summary>Sizes a species picture by its width: the pictures are square with the fish across most of
        /// the width and empty above and below, so fitting the square to the height would leave the fish tiny.</summary>
        static void Fit(Transform go, Sprite pic, Vector3 centre, float w)
        {
            var b = pic.bounds; float k = w / Mathf.Max(0.01f, b.size.x);
            go.localScale = Vector3.one * k;
            go.localPosition = centre - new Vector3(b.center.x, b.center.y, 0) * k;
        }

        void BuildRoom(Sim s, int cols)
        {
            var wall = WallColours[Mathf.Clamp(s.State.hallWall, 0, WallColours.Length - 1)];
            float w = _width, hx = w / 2, hz = Depth / 2;
            Models.Built("Room", Put("Room"), mb =>
            {
                // The square outside, so the hall doesn't float in the sky.
                mb.Matte(); mb.Surf = SurfaceKind.Stone;
                mb.Box(new Vector3(0, -0.6f, 0), new Vector3(w + 120, 1, Depth + 120), Palette.Hex(0xB9B2A2));
                // Floor: boards, a long red rug down the middle.
                mb.Surf = SurfaceKind.Planks;
                mb.Box(new Vector3(0, -0.05f, 0), new Vector3(w, 0.3f, Depth), Palette.Hex(0xB07A4A));
                mb.Surf = SurfaceKind.Auto;
                mb.Box(new Vector3(0, 0.12f, -1), new Vector3(Mathf.Min(w - 6, 18), 0.04f, 7), Palette.Hex(0x8A2E2E));
                mb.Box(new Vector3(0, 0.13f, -1), new Vector3(Mathf.Min(w - 6, 18) - 0.8f, 0.04f, 6.2f), Palette.Hex(0xA33A36));
                // The walls: back and sides, a dark wood dado, a white cornice; the front is open to the view.
                mb.Surf = SurfaceKind.None;
                mb.Box(new Vector3(0, Height / 2, hz), new Vector3(w + 0.8f, Height, 0.4f), wall);
                foreach (float side in new[] { -1f, 1f }) mb.Box(new Vector3(side * (hx + 0.2f), Height / 2, 0), new Vector3(0.4f, Height, Depth), wall);
                mb.Surf = SurfaceKind.WoodGrain;
                mb.Box(new Vector3(0, 0.7f, hz - 0.25f), new Vector3(w, 1.4f, 0.12f), Palette.Hex(0x5A3A24));
                foreach (float side in new[] { -1f, 1f }) mb.Box(new Vector3(side * (hx - 0.05f), 0.7f, 0), new Vector3(0.12f, 1.4f, Depth), Palette.Hex(0x5A3A24));
                mb.Surf = SurfaceKind.Auto;
                mb.Box(new Vector3(0, Height + 0.1f, hz), new Vector3(w + 1.2f, 0.3f, 0.7f), Palette.White);
                foreach (float side in new[] { -1f, 1f }) mb.Box(new Vector3(side * (hx + 0.2f), Height + 0.1f, 0), new Vector3(0.7f, 0.3f, Depth + 0.4f), Palette.White);
                // A low front wall with a gap in the middle for the door.
                foreach (float side in new[] { -1f, 1f }) mb.Box(new Vector3(side * (hx + 3) / 2, 0.5f, -hz), new Vector3(hx - 3, 1, 0.4f), wall);
                // Brass lamps between the boards, lit in the evening.
                for (int c = 0; c < cols - 1; c++)
                {
                    float x = -(cols - 1) * Spacing / 2 + (c + 0.5f) * Spacing;
                    mb.Shiny(); mb.Box(new Vector3(x, 3.95f, hz - 0.3f), new Vector3(0.12f, 0.6f, 0.2f), Palette.Hex(0xC9A03A));
                    mb.Lit(); mb.Sphere(new Vector3(x, 4.35f, hz - 0.45f), Vector3.one * 0.2f, 8, new Color(1f, 0.85f, 0.55f, 0)); mb.Matte();
                }
                // On the side walls: a ship's wheel and a pair of crossed oars.
                var wheel = new Vector3(-hx + 0.1f, 4.6f, 1);
                mb.Push().Translate(wheel).Rotate(Quaternion.Euler(0, 0, 90));
                for (int i = 0; i < 8; i++) { float a = i * Mathf.PI / 4; var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)); mb.Cylinder(Vector3.zero, d * 1.5f, 0.07f, 5, Palette.Wood); mb.Sphere(d * 1.6f, Vector3.one * 0.13f, 6, Palette.Wood); }
                for (int i = 0; i < 16; i++) { float a = i * Mathf.PI / 8, b = (i + 1) * Mathf.PI / 8; mb.Cylinder(new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * 1.2f, new Vector3(Mathf.Cos(b), 0, Mathf.Sin(b)) * 1.2f, 0.1f, 5, Palette.Hex(0x7A5234)); }
                mb.Pop();
                foreach (float t in new[] { -1f, 1f })
                {
                    var c = new Vector3(hx - 0.1f, 4.4f, 1);
                    mb.Cylinder(c + new Vector3(0, -1.8f, -1.8f * t), c + new Vector3(0, 1.8f, 1.8f * t), 0.07f, 5, Palette.WoodPale);
                    mb.Box(c + new Vector3(0, 1.9f, 1.9f * t), new Vector3(0.08f, 0.9f, 0.35f), Palette.WoodPale);
                }
                // Benches either side of the case.
                foreach (float side in new[] { -1f, 1f })
                {
                    var b = new Vector3(side * 6.5f, 0, -3);
                    mb.Box(b + new Vector3(0, 0.5f, 0), new Vector3(3, 0.12f, 0.8f), Palette.Wood);
                    foreach (float dx in new[] { -1.3f, 1.3f }) mb.Box(b + new Vector3(dx, 0.25f, 0), new Vector3(0.12f, 0.5f, 0.7f), Palette.Hex(0x3A2A20));
                }
            });
            foreach (float side in new[] { -1f, 1f }) Models.Kit("bush", Put("Plant"), new Vector3(side * (hx - 1.4f), 0, hz - 1.4f), 0, 1.6f);
        }

        Transform WallCanvas()
        {
            var go = new GameObject("Plates", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.WorldSpace; c.worldCamera = CameraRig.Instance.Cam;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(_width * 100, Height * 100);
            rt.localScale = Vector3.one * 0.01f;
            rt.localPosition = new Vector3(0, Height / 2, Depth / 2 - 0.62f);
            return rt;
        }

        void Board(SpeciesDef sp, Vector3 at, Transform canvas, Sim s)
        {
            var e = s.Entry(sp.Id); var note = s.NoteFor(sp.Id);
            var t = Put("Board " + sp.Id); t.localPosition = at;
            Models.Built("Frame", t, mb =>
            {
                mb.Paint(); mb.Box(Vector3.zero, new Vector3(4.6f, 3.3f, 0.2f), Frames[(int)sp.Rarity]);
                // A pale painted board so dark fish read against it, a brass plate along the bottom.
                mb.Matte(); mb.Box(new Vector3(0, 0, -0.06f), new Vector3(4.2f, 2.9f, 0.2f), Palette.Hex(0xE8DFC9));
                mb.Shiny(); mb.Box(new Vector3(0, -1.08f, -0.18f), new Vector3(4.0f, 0.62f, 0.04f), Palette.Hex(0x9A7428)); mb.Matte();
                if (sp.Id == _highlight)
                {
                    // The fish just mounted: its frame glows.
                    mb.Paint(); var glow = Palette.Hex(0xFFC93A);
                    foreach (float y in new[] { -1.6f, 1.6f }) mb.Box(new Vector3(0, y, -0.2f), new Vector3(4.8f, 0.22f, 0.08f), glow);
                    foreach (float x in new[] { -2.3f, 2.3f }) mb.Box(new Vector3(x, 0, -0.2f), new Vector3(0.22f, 3.42f, 0.08f), glow);
                    mb.Matte();
                }
            });
            var col = t.gameObject.AddComponent<BoxCollider>(); col.size = new Vector3(4.6f, 3.3f, 0.6f);
            _boards[col] = sp.Id;
            var pic = Hud.SpeciesPicture(sp);
            if (pic != null)
            {
                var go = new GameObject("Fish"); go.transform.SetParent(t, false);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = pic;
                Fit(go.transform, pic, new Vector3(0, 0.32f, -0.25f), 3.6f);
            }
            string when = note != null ? System.DateTimeOffset.FromUnixTimeSeconds((long)note.at).ToLocalTime().ToString("d MMM yyyy") : "an old catch";
            float kg = e != null ? e.best : note != null ? note.kg : 0;
            var l = Ui.Label("Plate", canvas, "<b>" + sp.Name + "</b>  " + kg.ToString(kg < 10 ? "0.00" : "0.0") + " kg\n<size=24>" + when + "</size>", 32, Palette.Hex(0xFFF1CC), TextAnchor.MiddleCenter, false);
            l.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(at.x * 100, (at.y - Height / 2 - 1.08f) * 100), new Vector2(390, 60));
            l.lineSpacing = 0.85f; l.resizeTextForBestFit = true; l.resizeTextMinSize = 16; l.resizeTextMaxSize = 32;
            if (sp.Id == _highlight)
            {
                var tag = Ui.Label("New", canvas, "NEW", 40, Palette.Hex(0xFFC93A), TextAnchor.MiddleLeft);
                tag.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(at.x * 100 - 115, (at.y - Height / 2 + 1.2f) * 100), new Vector2(180, 56));
                tag.Stroke(3f);
            }
        }

        void Centrepiece(SpeciesDef star, Sim s)
        {
            var t = Put("Case"); t.localPosition = new Vector3(0, 0, -0.5f);
            Models.Built("Plinth", t, mb =>
            {
                mb.Surf = SurfaceKind.Stone; mb.Box(new Vector3(0, 0.6f, 0), new Vector3(5.2f, 1.2f, 1.8f), Palette.Hex(0xD9D2C2)); mb.Surf = SurfaceKind.Auto;
                mb.Box(new Vector3(0, 1.24f, 0), new Vector3(5.5f, 0.1f, 2f), Palette.Hex(0xB8B0A0));
                // The glass case: a fine brass frame round where the glass would be.
                mb.Shiny();
                foreach (float x in new[] { -2.6f, 2.6f }) foreach (float z in new[] { -0.9f, 0.9f }) mb.Cylinder(new Vector3(x, 1.25f, z), new Vector3(x, 4.1f, z), 0.04f, 5, Palette.Hex(0xC9A03A));
                foreach (float z in new[] { -0.9f, 0.9f }) mb.Cylinder(new Vector3(-2.6f, 4.1f, z), new Vector3(2.6f, 4.1f, z), 0.04f, 5, Palette.Hex(0xC9A03A));
                foreach (float x in new[] { -2.6f, 2.6f }) mb.Cylinder(new Vector3(x, 4.1f, -0.9f), new Vector3(x, 4.1f, 0.9f), 0.04f, 5, Palette.Hex(0xC9A03A));
                mb.Box(new Vector3(0, 0.75f, -0.92f), new Vector3(2.4f, 0.45f, 0.04f), Palette.Hex(0x9A7428));
                mb.Matte();
                // Velvet rope on brass posts.
                foreach (float x in new[] { -3.6f, 3.6f }) foreach (float z in new[] { -2.2f, 1.6f }) mb.Cylinder(new Vector3(x, 0, z), new Vector3(x, 1.1f, z), 0.06f, 6, Palette.Hex(0xC9A03A));
                mb.Cylinder(new Vector3(-3.6f, 1, -2.2f), new Vector3(3.6f, 1, -2.2f), 0.05f, 5, Palette.Hex(0x8A1E2E));
            });
            if (star == null) return;
            var pic = Hud.SpeciesPicture(star);
            if (pic != null)
            {
                var go = new GameObject("Star"); go.transform.SetParent(t, false);
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = pic;
                Fit(go.transform, pic, new Vector3(0, 2.6f, 0), 4.4f);
                go.AddComponent<Hover>();
            }
            var cgo = new GameObject("Case plate", typeof(RectTransform)); cgo.transform.SetParent(t, false);
            var c = cgo.AddComponent<Canvas>(); c.renderMode = RenderMode.WorldSpace; c.worldCamera = CameraRig.Instance.Cam;
            var rt = (RectTransform)cgo.transform; rt.sizeDelta = new Vector2(240, 45); rt.localScale = Vector3.one * 0.01f; rt.localPosition = new Vector3(0, 0.75f, -0.95f);
            var e = s.Entry(star.Id); float kg = e != null ? e.best : 0;
            var l = Ui.Label("P", rt, "<b>" + star.Name + "</b>  " + kg.ToString(kg < 10 ? "0.00" : "0.0") + " kg", 30, Palette.Hex(0xFFF1CC)); l.rectTransform.Fill();
            l.resizeTextForBestFit = true; l.resizeTextMinSize = 14; l.resizeTextMaxSize = 30;
        }
    }

    /// <summary>Turns slowly to and fro, a showpiece in its case.</summary>
    public sealed class Hover : MonoBehaviour
    {
        Vector3 _base; void Start() { _base = transform.localPosition; }
        void Update() { transform.localPosition = _base + Vector3.up * Mathf.Sin(Time.time * 1.3f) * 0.08f; transform.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 0.6f) * 12, 0); }
    }
}

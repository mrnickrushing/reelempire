using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The interface: cash and pace top-left, the chain bar top-centre (the weakest stage glows), gems top-right, the
    /// guide card, signs floating over each stage, and the upgrade sheet down the right side.
    /// </summary>
    public sealed partial class Hud : MonoBehaviour
    {
        public static Hud Instance { get; private set; }
        Sim S { get { return GameRoot.Sim; } }
        HarbourState H { get { return S.H; } }

        RectTransform _root, _signs, _fx, _modal;
        Text _cash, _rate, _gems, _harbourName, _conditions;
        readonly Text[] _chainRate = new Text[4];
        readonly Image[] _chainBox = new Image[4];
        double _shownCash;

        public static Hud Create()
        {
            var canvas = Ui.Canvas("HUD", null);
            var hud = canvas.gameObject.AddComponent<Hud>();
            Instance = hud;
            hud.Build(canvas);
            return hud;
        }

        void Build(RectTransform canvas)
        {
            var pad = Ui.Img("Pad", canvas, new Color(0, 0, 0, 0)); pad.raycastTarget = true; pad.rectTransform.Fill();
            pad.gameObject.AddComponent<DragPad>();
            _signs = Ui.Rect("Signs", canvas); _signs.Fill();
            _fx = Ui.Rect("Fx", canvas); _fx.Fill();
            _root = Ui.Rect("Safe", canvas); _root.Fill(); _root.gameObject.AddComponent<SafeArea>();

            // Cash and pace.
            var money = Ui.ArtImg("Money", _root, "plate", new Color(0.1f, 0.14f, 0.2f, 0.92f), 44);
            money.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -20), new Vector2(430, 116));
            var coin = Ui.ArtImg("Coin", money.transform, "coin", Color.white, 0); coin.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(84, 84));
            _cash = Ui.Label("Cash", money.transform, "0", 60, Color.white, TextAnchor.MiddleLeft); _cash.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(112, -6), new Vector2(310, 66));
            _rate = Ui.Label("Rate", money.transform, "", 32, Ui.Green, TextAnchor.MiddleLeft); _rate.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(114, 10), new Vector2(310, 40));

            // Gems.
            var gems = Ui.ArtImg("Gems", _root, "plate", new Color(0.1f, 0.14f, 0.2f, 0.92f), 44);
            gems.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -20), new Vector2(250, 84));
            var gem = Ui.ArtImg("Gem", gems.transform, "gem", Color.white, 0); gem.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, 0), new Vector2(60, 60));
            _gems = Ui.Label("N", gems.transform, "0", 46, Color.white, TextAnchor.MiddleRight); _gems.rectTransform.Fill(); _gems.rectTransform.offsetMax = new Vector2(-28, 0);

            // The chain: fleet, dock, hall, market.
            var chain = Ui.ArtImg("Chain", _root, "plate", new Color(0.1f, 0.14f, 0.2f, 0.92f), 44);
            chain.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(820, 132));
            _harbourName = Ui.Label("Name", chain.transform, "", 30, Ui.Gold, TextAnchor.MiddleLeft); _harbourName.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(28, -6), new Vector2(420, 36));
            _conditions = Ui.Label("Cond", chain.transform, "", 28, Ui.Dim, TextAnchor.MiddleRight, false); _conditions.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-28, -6), new Vector2(380, 36));
            string[] names = { "BOATS", "DOCK", "HALL", "MARKET" };
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                var box = Ui.Button("Stage" + i, chain.transform, null, new Color(0.2f, 0.27f, 0.36f, 1), () => OpenSheet((Stage)k, k == 0 ? Mathf.Max(0, _sheetBerth) : 0), 30);
                var rt = (RectTransform)box.transform; rt.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(20 + i * 197, 12), new Vector2(188, 76));
                _chainBox[i] = box.GetComponent<Image>();
                var n = Ui.Label("N", box.transform, names[i], 20, Ui.Dim, TextAnchor.UpperCenter); n.rectTransform.Fill(); n.rectTransform.offsetMax = new Vector2(0, -6);
                _chainRate[i] = Ui.Label("R", box.transform, "", 30, Color.white, TextAnchor.LowerCenter); _chainRate[i].rectTransform.Fill(); _chainRate[i].rectTransform.offsetMin = new Vector2(0, 12);
            }

            BuildGuide();
            BuildSheet();
            _modal = Ui.Rect("Modal", _root); _modal.Fill();
            S.Changed += OnChanged; S.Caught += OnCaught; S.Sold += OnSold; S.MilestoneReached += OnMilestone;
            _shownCash = H.cash;
            OnChanged();
            if (GameRoot.Instance.AwaySeconds > 0) OfferAway();
        }

        void OnDestroy()
        {
            if (S == null) return;
            S.Changed -= OnChanged; S.Caught -= OnCaught; S.Sold -= OnSold; S.MilestoneReached -= OnMilestone;
        }

        bool _dirty;
        void OnChanged() { _dirty = true; }

        void Update()
        {
            var s = S; if (s == null) return;
            var h = H;
            // The counter rolls toward the real figure rather than jumping.
            _shownCash = Math.Abs(h.cash - _shownCash) < Math.Max(1, h.cash * 0.001) ? h.cash : _shownCash + (h.cash - _shownCash) * (1 - Math.Exp(-10 * Time.unscaledDeltaTime));
            _cash.text = Format.Money(_shownCash);
            double idle = s.IdleRate(h);
            _rate.text = idle > 0 ? "+" + Format.Money(idle) + "/s" : "Tap to work";
            _rate.color = idle > 0 ? Ui.Green : Ui.Gold;
            _gems.text = s.State.gems.ToString("N0");
            _harbourName.text = s.Def.Name.ToUpperInvariant();
            _conditions.text = (s.IsNight ? "Night" : "Day") + "  ·  " + WeatherName(s.Weather) + "  ·  " + Sim.SeasonNames[s.Season];

            double fleet = s.FleetRate(h, true), dock = s.DockRate(h), hall = s.HallRate(h), market = s.MarketRate(h);
            _chainRate[0].text = Format.Money(fleet) + "/s"; _chainRate[1].text = Format.Money(dock) + "/s";
            _chainRate[2].text = h.goods == 0 ? "fresh" : Format.Money(hall) + "/s"; _chainRate[3].text = Format.Money(market) + "/s";
            var neck = s.Bottleneck(h);
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5);
            for (int i = 0; i < 4; i++)
            {
                bool weak = (Stage)i == neck && i != 0 || i == 2 && h.goods > 0 && hall < s.Flow(h, true) * 0.98;
                _chainBox[i].color = weak ? Color.Lerp(new Color(0.55f, 0.2f, 0.16f), new Color(0.85f, 0.3f, 0.22f), pulse) : new Color(0.2f, 0.27f, 0.36f, 1);
            }

            UpdateSigns();
            UpdateGuide();
            UpdateSheet();
            UpdateFx();
            UpdateRate();
            if (_dirty) { _dirty = false; RefreshSheet(); }
        }

        public static string WeatherName(Weather w) { return w == Weather.Clear ? "Clear" : w == Weather.Rain ? "Rain" : w == Weather.Fog ? "Fog" : "Storm"; }

        /// <summary>Where a point in the world is on the canvas (false: behind the camera).</summary>
        bool Project(Vector3 world, out Vector2 canvasPos)
        {
            var cam = CameraRig.Instance.Cam;
            var sp = cam.WorldToScreenPoint(world);
            canvasPos = Vector2.zero;
            if (sp.z <= 0) return false;
            var c = (RectTransform)transform;
            canvasPos = new Vector2(sp.x / Screen.width * c.rect.width, sp.y / Screen.height * c.rect.height);
            return true;
        }

        /// <summary>A tap on the open view: the nearest boat, hauler or truck does its work; a sign opens its sheet.</summary>
        public void TapWorld(Vector2 screen)
        {
            var view = HarbourView.Instance; if (view == null) return;
            var cam = CameraRig.Instance.Cam; var h = H;
            float best = 95f * Screen.height / 940f; best *= best;
            Action act = null;
            Action<Vector3, Action> consider = (world, a) =>
            {
                var sp = cam.WorldToScreenPoint(world);
                if (sp.z <= 0) return;
                float d = ((Vector2)sp - screen).sqrMagnitude;
                if (d < best) { best = d; act = a; }
            };
            for (int i = 0; i < h.berths.Count; i++) { int k = i; consider(view.BoatAt(i) - Vector3.up * 1.5f, () => TapBerth(k)); }
            for (int i = 0; i < h.haulers.Count; i++) consider(view.HaulerAt(i) - Vector3.up * 1.2f, TapDock);
            for (int i = 0; i < h.trucks.Count; i++) consider(view.TruckAt(i) - Vector3.up * 1.6f, TapMarket);
            if (act != null) act(); else CloseSheet();
        }

        void TapBerth(int berth)
        {
            if (H.berths[berth].skipper) { OpenSheet(Stage.Berth, berth); return; }
            if (S.TapBerth(berth)) Sfx.Play("dispatch", 0.5f); else OpenSheet(Stage.Berth, berth);
        }
        void TapDock() { if (H.dockSkipper) { OpenSheet(Stage.Dock, 0); return; } if (S.TapDock()) Sfx.Play("dispatch", 0.5f, 1.2f); else OpenSheet(Stage.Dock, 0); }
        void TapMarket() { if (H.marketSkipper) { OpenSheet(Stage.Market, 0); return; } if (S.TapMarket()) Sfx.Play("dispatch", 0.5f, 0.85f); else OpenSheet(Stage.Market, 0); }

        // ---- cards over everything ----

        /// <summary>A card in the middle of the screen over a dimmed game. Returns the card to fill.</summary>
        RectTransform Card(float width, float height, string title)
        {
            Ui.Clear(_modal);
            var dim = Ui.Img("Dim", _modal, new Color(0, 0, 0, 0.6f)); dim.raycastTarget = true; dim.rectTransform.Fill();
            var card = Ui.ArtImg("Card", _modal, "panel", Color.white, 60);
            card.raycastTarget = true;
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));
            var t = Ui.Label("Title", card.transform, title, 58, Ui.Gold, TextAnchor.UpperCenter); t.rectTransform.Band(true, -34, 70, 40);
            Sfx.Play("open", 0.5f);
            return card.rectTransform;
        }

        void CloseCard() { Ui.Clear(_modal); }
        public bool ModalOpen { get { return _modal != null && _modal.childCount > 0; } }

        /// <summary>The welcome-back card: what the harbour made while the app was closed.</summary>
        public void OfferAway()
        {
            double secs = GameRoot.Instance.AwaySeconds;
            double got = S.AwayEarnings(secs);
            if (got <= 0) { S.CollectAway(secs); GameRoot.Instance.ClearAway(); return; }
            var card = Card(820, 560, "WELCOME BACK");
            var a = Ui.Label("A", card, "Your crews kept working for " + Format.Time(Math.Min(secs, S.AwayCap)) + (secs > S.AwayCap ? " (the docks were full after that)" : ""), 34, Ui.Dim, TextAnchor.UpperCenter, false);
            a.rectTransform.Band(true, -120, 90, 60);
            var coin = Ui.ArtImg("Coin", card, "coin", Color.white, 0); coin.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-190, 10), new Vector2(110, 110));
            var n = Ui.Label("N", card, Format.Money(got), 110, Color.white, TextAnchor.MiddleLeft); n.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(-110, 10), new Vector2(500, 130));
            var b = Ui.Button("Collect", card, "COLLECT", Ui.Green, () =>
            {
                S.CollectAway(secs); GameRoot.Instance.ClearAway(); CloseCard(); Sfx.Play("coin", 0.8f); GameRoot.Instance.Save();
            }, 52);
            ((RectTransform)b.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(420, 110));
        }

        public void OfferCloud() { }
    }
}

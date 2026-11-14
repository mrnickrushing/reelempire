using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The interface, in cut paper and stickers: the cash and gem pills in the top corners, the harbour's name on a
    /// ribbon between them, the guide card on the left, the chain as four round badges along the bottom (the weakest
    /// one turns coral and wobbles), signs floating over each stage, and the upgrade sheet rising from the bottom.
    /// </summary>
    public sealed partial class Hud : MonoBehaviour
    {
        public static Hud Instance { get; private set; }
        Sim S { get { return GameRoot.Sim; } }
        HarbourState H { get { return S.H; } }

        RectTransform _root, _signs, _fx, _modal, _chain, _slowFlag;
        Text _cash, _rate, _gems, _harbourName, _conditions;
        readonly Text[] _chainRate = new Text[4];
        readonly Image[] _chainBox = new Image[4];
        double _shownCash;
        Image _boostImg; Text _boostText; RectTransform _boostRoot;

        void UseBoost()
        {
            if (S.UseBoost()) { Sfx.Play("star", 0.7f); Toast("Tide Boost! Double income for an hour"); GameRoot.Instance.Save(); }
            else if (S.AdOffered(AdReward.Boost) && Ads.Ready)
            {
                // Out of charges: an advert is another way to one, if the player wants it.
                Confirm("TIDE BOOST", "Your boosts are spent: the next is ready in " + Format.Time(S.BoostNextIn) + ".\n\nWatch a short advert for an hour of double income now? (" + S.AdsLeft + " left today)", "WATCH", Ui.Green, () =>
                    Ads.Show(ok =>
                    {
                        if (ok && S.BoostByAd()) { Sfx.Play("star", 0.7f); Haptics.Success(); Toast("Tide Boost! Double income for an hour"); GameRoot.Instance.Save(); }
                        else if (!ok) Toast("The advert wasn't finished, so there's no boost.");
                    }), "NOT NOW");
            }
            else { Sfx.Play("error", 0.5f); Toast(S.BoostReady <= 0 ? "The next boost is ready in " + Format.Time(S.BoostNextIn) : "The boost is full: 8 hours at most"); }
        }

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

            // Cash: a paper pill with the coin sitting on its left end, and the pace on a green tab hanging below.
            var tab = Ui.ArtImg("Pace", _root, "tag", Ui.Green, 30);
            tab.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(112, -98), new Vector2(250, 62));
            _rate = Ui.Label("Rate", tab.transform, "", 30, Color.white); _rate.rectTransform.Fill(); _rate.rectTransform.offsetMin = new Vector2(0, 4); _rate.Stroke(2f);
            var money = Ui.ArtImg("Money", _root, "pill", Color.white, 62, 1.5f);
            money.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(64, -18), new Vector2(360, 92));
            _cash = Ui.Label("Cash", money.transform, "0", 54, Ui.Ink, TextAnchor.MiddleCenter); _cash.rectTransform.Fill(); _cash.rectTransform.offsetMin = new Vector2(62, 8); _cash.rectTransform.offsetMax = new Vector2(-18, 0);
            _cash.resizeTextForBestFit = true; _cash.resizeTextMinSize = 30; _cash.resizeTextMaxSize = 54;
            _cashPill = money.rectTransform;
            var coin = Ui.ArtImg("Coin", _root, "coin", Color.white, 0); coin.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -8), new Vector2(112, 116));

            // Gems.
            var gems = Ui.ArtImg("Gems", _root, "pill", Color.white, 62, 1.5f);
            gems.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -18), new Vector2(230, 92));
            _gems = Ui.Label("N", gems.transform, "0", 50, Ui.Ink, TextAnchor.MiddleCenter); _gems.rectTransform.Fill(); _gems.rectTransform.offsetMin = new Vector2(60, 8); _gems.rectTransform.offsetMax = new Vector2(-16, 0);
            var gem = Ui.ArtImg("Gem", _root, "gem", Color.white, 0); gem.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-190, -10), new Vector2(96, 100));

            // The Tide Boost: a round button under the gems with its charges or the time left.
            _boostRoot = Ui.Rect("BoostRoot", _root); _boostRoot.Fill();
            var boost = Ui.Button("Boost", _boostRoot, null, new Color(0, 0, 0, 0), UseBoost, 30);
            var brt = (RectTransform)boost.transform; brt.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -122), new Vector2(112, 120));
            _boostImg = boost.GetComponent<Image>(); _boostImg.sprite = Ui.Art("badge"); _boostImg.type = Image.Type.Simple; _boostImg.color = Ui.Blue;
            var bx = Ui.Label("X", brt, "x2", 46, Color.white); bx.rectTransform.Fill(); bx.rectTransform.offsetMin = new Vector2(0, 12); bx.Stroke(2.5f);
            var btag = Ui.ArtImg("Tag", _boostRoot, "tag", Color.white, 30); btag.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-150, -150), new Vector2(190, 58));
            _boostText = Ui.Label("T", btag.transform, "", 26, Ui.Ink); _boostText.rectTransform.Fill(); _boostText.rectTransform.offsetMin = new Vector2(0, 4);

            // The harbour's name on a ribbon, the conditions on a tag under it.
            var cond = Ui.ArtImg("Cond", _root, "tag", Color.white, 30);
            cond.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -88), new Vector2(430, 58));
            _conditions = Ui.Label("Cond", cond.transform, "", 26, Ui.Ink, TextAnchor.MiddleCenter, false); _conditions.rectTransform.Fill(); _conditions.rectTransform.offsetMin = new Vector2(0, 4);
            var ribbon = Ui.Ribbon("Ribbon", _root, Ui.Coral);
            ribbon.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -8), new Vector2(580, 104));
            ribbon.raycastTarget = true; ribbon.gameObject.AddComponent<Button>().onClick.AddListener(() => { if (S.Unlocked("harbours")) Open(Page.Harbours); else Toast("Open seven docks here and this is where you renovate and sail on"); });
            _harbourName = Ui.Label("Name", ribbon.transform, "", 44, Color.white); _harbourName.rectTransform.Fill(); _harbourName.rectTransform.offsetMin = new Vector2(40, 22); _harbourName.rectTransform.offsetMax = new Vector2(-40, 0); _harbourName.Stroke(2.5f);

            // The chain along the bottom: boats, dock, hall, market, joined by arrows.
            _chain = Ui.Rect("Chain", _root); _chain.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(760, 180));
            string[] names = { "BOATS", "PORTERS", "HALL", "MARKET" };
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                if (i > 0)
                {
                    var arrow = Ui.ArtImg("Arrow", _chain, "up", Color.white, 0); arrow.rectTransform.Place(new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(i * 190 + 0, 104), new Vector2(40, 40));
                    arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, -90);
                    var ao = arrow.gameObject.AddComponent<Outline>(); ao.effectColor = Ui.Ink; ao.effectDistance = new Vector2(3, -3);
                }
                var box = Ui.Button("Stage" + i, _chain, null, new Color(0, 0, 0, 0), () => OpenSheet((Stage)k, k == 0 ? Mathf.Max(0, _sheetBerth) : 0), 30);
                var rt = (RectTransform)box.transform; rt.Place(new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(95 + i * 190, 44), new Vector2(120, 128));
                var img = box.GetComponent<Image>(); img.sprite = Ui.Art("badge"); img.type = Image.Type.Simple; img.color = Color.white;
                _chainBox[i] = img;
                _chainRate[i] = Ui.Label("R", box.transform, "", 30, Ui.Ink); _chainRate[i].rectTransform.Fill(); _chainRate[i].rectTransform.offsetMin = new Vector2(10, 14); _chainRate[i].rectTransform.offsetMax = new Vector2(-10, -4);
                _chainRate[i].resizeTextForBestFit = true; _chainRate[i].resizeTextMinSize = 16; _chainRate[i].resizeTextMaxSize = 30; _chainRate[i].horizontalOverflow = HorizontalWrapMode.Wrap;
                var n = Ui.Label("N", _chain, names[i], 26, Color.white); n.rectTransform.Place(new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(95 + i * 190, 6), new Vector2(180, 34)); n.Stroke(2f);
            }

            // Which one to fix: a flag over the slowest part.
            var flag = Ui.ArtImg("Slow", _chain, "tag", Ui.Coral, 30); _slowFlag = flag.rectTransform; _slowFlag.Place(new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(95, 168), new Vector2(170, 44));
            var fl = Ui.Label("T", flag.transform, "TOO SLOW", 24, Color.white); fl.rectTransform.Fill(); fl.rectTransform.offsetMin = new Vector2(0, 4); fl.Stroke(2f);

            BuildSkills();
            BuildGuide();
            BuildNav();
            BuildSheet();
            _modal = Ui.Rect("Modal", _root); _modal.Fill();
            S.Changed += OnChanged; S.Caught += OnCaught; S.Sold += OnSold; S.MilestoneReached += OnMilestone; S.BonusAppeared += OnBonus;
            _shownCash = H.cash;
            OnChanged();
            if (GameRoot.Instance.AwaySeconds > 0) OfferAway();
            BuildTutorial();
            OfferCloud();
        }

        void OnDestroy()
        {
            if (S == null) return;
            S.Changed -= OnChanged; S.Caught -= OnCaught; S.Sold -= OnSold; S.MilestoneReached -= OnMilestone; S.BonusAppeared -= OnBonus;
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
            _rate.text = idle > 0 ? "+" + Format.Money(idle) + "/s" : "TAP TO WORK";
            ((Image)_rate.transform.parent.GetComponent<Image>()).color = idle > 0 ? Ui.Green : Ui.Gold;
            _gems.text = s.State.gems.ToString("N0");
            _harbourName.text = s.Def.Name.ToUpperInvariant() + (h.renovations > 0 ? "  x" + Format.Money(Math.Pow(2, h.renovations)) : "");
            _conditions.text = (s.IsNight ? "Night" : "Day") + "  •  " + WeatherName(s.Weather) + "  •  " + Sim.SeasonNames[s.Season];

            int ready = s.BoostReady;
            _boostText.text = s.Boosted ? Format.Time(s.BoostLeft) : ready > 0 ? ready + " ready" : Format.Time(s.BoostNextIn);
            _boostImg.color = ready > 0 ? Ui.Blue : s.Boosted ? Ui.Green : Ui.Off;
            _boostImg.rectTransform.localScale = Vector3.one * (ready > 0 && !s.Boosted ? 1 + 0.05f * Mathf.Sin(Time.unscaledTime * 5) : 1);

            double fleet = s.FleetRate(h, true), dock = s.DockRate(h), hall = s.HallRate(h), market = s.MarketRate(h);
            _chainRate[0].text = Format.Money(fleet) + "\n/s"; _chainRate[1].text = Format.Money(dock) + "\n/s";
            _chainRate[2].text = h.goods == 0 ? "fresh" : Format.Money(hall) + "\n/s"; _chainRate[3].text = Format.Money(market) + "\n/s";
            var neck = s.Bottleneck(h);
            for (int i = 0; i < 4; i++)
            {
                // Not until the guide has got as far as the chain: before that it would only pull the eye from the step in hand.
                bool weak = s.State.goalIndex >= 7 && ((Stage)i == neck && i != 0 || i == 2 && h.goods > 0 && hall < s.Flow(h, true) * 0.98);
                _chainBox[i].color = weak ? Ui.Coral : Color.white;
                _chainRate[i].color = weak ? Color.white : Ui.Ink;
                _chainBox[i].rectTransform.localRotation = Quaternion.Euler(0, 0, weak ? Mathf.Sin(Time.unscaledTime * 9) * 5 : 0);
            }
            int slow = -1;
            for (int i = 1; i < 4; i++) if (_chainBox[i].color == Ui.Coral) { slow = i; break; }
            if (_slowFlag.gameObject.activeSelf != slow >= 0) _slowFlag.gameObject.SetActive(slow >= 0);
            if (slow >= 0) _slowFlag.anchoredPosition = new Vector2(95 + slow * 190, 172 + Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 6);
            // The sheet takes the bottom of the screen: the chain steps aside while it is up.
            _chain.anchoredPosition = new Vector2(0, Mathf.Lerp(14, -220, 1 - Mathf.Clamp01(_sheetY / SheetHide)));

            UpdateSigns();
            UpdateSkills();
            UpdateAngling();
            UpdateGuide();
            UpdateSheet();
            UpdateFx();
            UpdateFlyers();
            UpdateBonus();
            UpdateRate();
            UpdateNav();
            UpdateTutorial();
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
            if (S.BonusAfloat) consider(view.BonusAt, TakeBonus);
            if (act != null) act(); else CloseSheet();
        }

        void TapBerth(int berth)
        {
            Haptics.Light();
            if (H.berths[berth].skipper) { OpenSheet(Stage.Berth, berth); return; }
            if (S.TapBerth(berth)) Sfx.Play("dispatch", 0.5f); else OpenSheet(Stage.Berth, berth);
        }
        void TapDock() { if (H.dockSkipper) { OpenSheet(Stage.Dock, 0); return; } if (S.TapDock()) Sfx.Play("dispatch", 0.5f, 1.2f); else OpenSheet(Stage.Dock, 0); }
        void TapMarket() { if (H.marketSkipper) { OpenSheet(Stage.Market, 0); return; } if (S.TapMarket()) Sfx.Play("dispatch", 0.5f, 0.85f); else OpenSheet(Stage.Market, 0); }

        // ---- cards over everything ----

        /// <summary>A card in the middle of the screen over a dimmed game. Returns the card to fill.</summary>
        RectTransform Card(float width, float height, string title)
        {
            // News still on screen steps aside for the card and comes back when it has gone.
            if (_toast != null) { if (_toastAge < 1.6f) _toastHeld = _toastText; Destroy(_toast.gameObject); _toast = null; }
            Ui.Clear(_modal);
            var dim = Ui.Img("Dim", _modal, new Color(0.05f, 0.15f, 0.25f, 0.6f)); dim.raycastTarget = true; dim.rectTransform.Fill();
            var card = Ui.ArtImg("Card", _modal, "panel", Color.white, 60);
            card.raycastTarget = true;
            card.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));
            var ribbon = Ui.Ribbon("Ribbon", card.transform, Ui.Coral);
            ribbon.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -8), new Vector2(Mathf.Min(width + 40, 640), 116));
            var t = Ui.Label("Title", ribbon.transform, title, 50, Color.white); t.rectTransform.Fill(); t.rectTransform.offsetMin = new Vector2(40, 24); t.rectTransform.offsetMax = new Vector2(-40, 0); t.Stroke(2.5f);
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
int missed = S.WelcomeHome(secs);
            var card = Card(860, 600, missed > 0 ? "WE MISSED YOU!" : "WELCOME BACK");
            var a = Ui.Label("A", card, "Your crews kept working for " + Format.Time(Math.Min(secs, S.AwayCap)) + (secs > S.AwayCap ? " (the docks were full after that)" : "") + (missed > 0 ? ".\nHere are " + missed + " gems and three fresh Tide Boosts to get going again." : ""), 30, Ui.Dim, TextAnchor.UpperCenter, false);
            a.rectTransform.Band(true, -90, 110, 60);
            var rays = Ui.ArtImg("Rays", card, "rays", new Color(1f, 0.77f, 0.2f, 0.5f), 0); rays.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220, 20), new Vector2(300, 300));
            var coin = Ui.ArtImg("Coin", card, "coin", Color.white, 0); coin.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220, 20), new Vector2(130, 134));
            var n = Ui.Label("N", card, Format.Money(got), 110, Ui.Ink, TextAnchor.MiddleLeft); n.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0.5f), new Vector2(-130, 20), new Vector2(520, 130));
            string buys = S.Affords(S.H.cash + got);
            if (buys != null) { var af = Ui.Label("Af", card, buys, 30, new Color(0.12f, 0.62f, 0.29f)); af.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 178), new Vector2(780, 40)); }
            var b = Ui.Button("Collect", card, "COLLECT", Ui.Blue, () =>
            {
                S.CollectAway(secs); GameRoot.Instance.ClearAway(); CloseCard(); Sfx.Play("coin", 0.8f); CoinBurst(8); GameRoot.Instance.Save();
                Reminders.AskOnce();        // the first time the harbour has earned by itself is the moment a reminder makes sense
            }, 44);
            ((RectTransform)b.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-205, 44), new Vector2(380, 124));
            bool free = S.DoubleFree;
            bool byAd = !free && S.AdOffered(AdReward.Double) && Ads.Ready;       // after the free one: an advert instead of gems
            var d = Ui.Button("Double", card, null, Ui.Green, () =>
            {
                if (byAd)
                {
                    Ads.Show(ok =>
                    {
                        if (!ok) { Toast("The advert wasn't finished: collect, or try again."); return; }
                        if (!S.CollectAwayByAd(secs)) return;
                        GameRoot.Instance.ClearAway(); CloseCard(); Sfx.Play("star", 0.9f); Haptics.Success(); CoinBurst(16); Toast("Doubled: +" + Format.Money(got * 2)); GameRoot.Instance.Save();
                        Reminders.AskOnce();
                    });
                    return;
                }
                if (!S.CollectAwayDoubled(secs)) { Sfx.Play("error", 0.5f); Toast("Not enough gems for another double today"); return; }
                GameRoot.Instance.ClearAway(); CloseCard(); Sfx.Play("star", 0.9f); Haptics.Success(); CoinBurst(16); Toast("Doubled: +" + Format.Money(got * 2)); GameRoot.Instance.Save();
                Reminders.AskOnce();
            }, 44);
            ((RectTransform)d.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(205, 44), new Vector2(380, 124));
            var dl = Ui.Label("L", d.transform, "DOUBLE IT", 42, Color.white, TextAnchor.UpperCenter); dl.rectTransform.Fill(); dl.rectTransform.offsetMax = new Vector2(0, -14); dl.Stroke(2.5f);
            var dc = Ui.Label("C", d.transform, free ? "FREE TODAY" : byAd ? "WATCH AN ADVERT" : DoubleCost(), 28, Color.white, TextAnchor.LowerCenter); dc.rectTransform.Fill(); dc.rectTransform.offsetMin = new Vector2(0, 26); dc.Stroke(2f);
        }

        static string DoubleCost() { return Sim.DoubleGems + " GEMS"; }

        // ---- coins flying to the counter ----

        sealed class Flyer { public RectTransform Rt; public Vector2 From, Mid, To; public float Age, Life, Delay; }
        readonly List<Flyer> _flyers = new List<Flyer>();
        float _cashPunch;
        RectTransform _cashPill;

        /// <summary>Coins arc from a point on the canvas (the market, by default) to the cash counter, which jumps as they land.</summary>
        void CoinBurst(int count, Vector2? from = null)
        {
            var c = (RectTransform)transform;
            Vector2 start = from ?? new Vector2(c.rect.width * 0.5f, c.rect.height * 0.5f);
            Vector2 to = new Vector2(80, c.rect.height - 70);
            for (int i = 0; i < count && _flyers.Count < 40; i++)
            {
                var img = Ui.ArtImg("Coin", _fx, "coin", Color.white, 0);
                var rt = img.rectTransform; rt.anchorMin = rt.anchorMax = Vector2.zero; rt.sizeDelta = new Vector2(54, 56); rt.anchoredPosition = start; rt.localScale = Vector3.zero;
                var spread = new Vector2(UnityEngine.Random.Range(-90f, 90f), UnityEngine.Random.Range(-60f, 90f));
                _flyers.Add(new Flyer { Rt = rt, From = start, Mid = start + spread + new Vector2(0, 120), To = to, Life = UnityEngine.Random.Range(0.55f, 0.8f), Delay = i * 0.035f });
            }
        }

        void UpdateFlyers()
        {
            for (int i = _flyers.Count - 1; i >= 0; i--)
            {
                var f = _flyers[i];
                if (f.Delay > 0) { f.Delay -= Time.unscaledDeltaTime; continue; }
                f.Age += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(f.Age / f.Life), e = t * t * (3 - 2 * t);
                f.Rt.anchoredPosition = Vector2.Lerp(Vector2.Lerp(f.From, f.Mid, e), Vector2.Lerp(f.Mid, f.To, e), e);
                f.Rt.localScale = Vector3.one * (t < 0.15f ? t / 0.15f : Mathf.Lerp(1, 0.6f, (t - 0.15f) / 0.85f));
                if (t >= 1) { Destroy(f.Rt.gameObject); _flyers.RemoveAt(i); _cashPunch = 1; }
            }
            if (_cashPill != null)
            {
                _cashPunch = Mathf.MoveTowards(_cashPunch, 0, Time.unscaledDeltaTime * 5);
                _cashPill.localScale = Vector3.one * (1 + 0.07f * _cashPunch);
            }
        }

        RectTransform _cloud;

        /// <summary>iCloud holds a newer save from another device: ask which one to keep (never replaced unasked).</summary>
        public void OfferCloud()
        {
            var cloud = GameRoot.Instance.PendingCloud; var meta = GameRoot.Instance.PendingMeta;
            if (cloud == null || _cloud != null) return;
            _cloud = Ui.Img("Cloud", _root, new Color(0.05f, 0.15f, 0.25f, 0.7f)).rectTransform;
            _cloud.Fill(); _cloud.GetComponent<Image>().raycastTarget = true;
            var box = Ui.ArtImg("Box", _cloud, "panel", Color.white, 60).rectTransform;
            box.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(940, 640));
            Ui.Label("T", box, "CONTINUE FROM YOUR OTHER DEVICE?", 48, Ui.Ink).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(860, 80));
            int theirs = meta != null ? meta.Stars : new Sim(cloud).Stars;
            Ui.Label("B", box, "iCloud has a newer save from another device: <b>" + theirs + (theirs == 1 ? " star" : " stars") + "</b>, saved " + Format.Time(Math.Max(0, SaveSystem.Now - cloud.savedAt)) + " ago.\n\n"
                + "This device has <b>" + S.Stars + (S.Stars == 1 ? " star" : " stars") + "</b>.\n\nThe one you don't pick is replaced.", 34, Ui.Ink, TextAnchor.UpperCenter, false)
                .rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(820, 330));
            var yes = Ui.Button("Yes", box, "USE ICLOUD", Ui.Blue, () => { Destroy(_cloud.gameObject); _cloud = null; GameRoot.Instance.ResolveCloud(true); }, 42);
            ((RectTransform)yes.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(215, 46), new Vector2(400, 116));
            var no = Ui.Button("No", box, "KEEP THIS ONE", Ui.Off, () => { Destroy(_cloud.gameObject); _cloud = null; GameRoot.Instance.ResolveCloud(false); }, 42);
            ((RectTransform)no.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-215, 46), new Vector2(400, 116));
        }
    }
}

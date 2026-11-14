using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>What floats over the harbour: a level sign per stage, TAP bubbles over idle hands, the next berth's
    /// price, pay chips, and the guide card with its pointer.</summary>
    public sealed partial class Hud
    {
        sealed class Sign { public RectTransform Rt; public Text Label; public Image Back; public Image Up; }
        readonly List<Sign> _berthSigns = new List<Sign>();
        readonly List<RectTransform> _tapBubbles = new List<RectTransform>();
        Sign _dockSign, _hallSign, _marketSign, _nextSign;
        int _signBerths = -1;

        Sign MakeSign(Action tap, float width = 132)
        {
            var b = Ui.Button("Sign", _signs, null, new Color(0, 0, 0, 0), tap, 30);
            var rt = (RectTransform)b.transform; rt.sizeDelta = new Vector2(width, 64); rt.anchorMin = rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0);
            var img = b.GetComponent<Image>(); img.sprite = Ui.Art("tag", 30); img.type = Image.Type.Sliced; img.color = Color.white;
            var l = Ui.Label("L", rt, "", 34, Ui.Ink); l.rectTransform.Fill(); l.rectTransform.offsetMin = new Vector2(10, 8); l.rectTransform.offsetMax = new Vector2(-46, 0);
            var up = Ui.ArtImg("Up", rt, "badge", Ui.Green, 0); up.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-22, 6), new Vector2(54, 58));
            var arrow = Ui.ArtImg("A", up.transform, "up", Color.white, 0); arrow.rectTransform.Fill(14); arrow.rectTransform.offsetMin = new Vector2(14, 18);
            return new Sign { Rt = rt, Label = l, Back = img, Up = up };
        }

        RectTransform MakeBubble()
        {
            var img = Ui.ArtImg("Tap", _signs, "bubble", Color.white, 0);
            var rt = img.rectTransform; rt.sizeDelta = new Vector2(68, 76); img.raycastTarget = false; rt.anchorMin = rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0);
            var hand = Ui.ArtImg("Hand", rt, "hand", Color.white, 0); hand.rectTransform.Fill(14); hand.rectTransform.offsetMin = new Vector2(14, 26); hand.raycastTarget = false;
            return rt;
        }

        void RebuildSigns()
        {
            Ui.Clear(_signs); _berthSigns.Clear(); _tapBubbles.Clear();
            var h = H; _signBerths = h.berths.Count;
            for (int i = 0; i < h.berths.Count; i++) { int k = i; _berthSigns.Add(MakeSign(() => OpenSheet(Stage.Berth, k))); }
            _dockSign = MakeSign(() => OpenSheet(Stage.Dock, 0)); _hallSign = MakeSign(() => OpenSheet(Stage.Hall, 0)); _marketSign = MakeSign(() => OpenSheet(Stage.Market, 0));
            _nextSign = h.berths.Count < Content.Berths.Length ? MakeSign(UnlockBerth, 400) : null;
            if (_nextSign != null) _nextSign.Up.gameObject.SetActive(false);
            for (int i = 0; i < h.berths.Count + 2; i++) _tapBubbles.Add(MakeBubble());
        }

        void UnlockBerth()
        {
            if (S.UnlockBerth()) { Sfx.Play("build", 0.8f); Toast(Content.Berths[H.berths.Count - 1].Name + " is open!"); GameRoot.Instance.Save(); }
            else { Sfx.Play("error", 0.5f); Toast("Not enough cash yet"); }
        }

        void Put(RectTransform rt, Vector3 world, bool show = true)
        {
            Vector2 p;
            bool on = show && Project(world, out p) ? true : false;
            if (on) { Project(world, out p); rt.anchoredPosition = p; }
            if (rt.gameObject.activeSelf != on) rt.gameObject.SetActive(on);
        }

        void SetSign(Sign s, Vector3 world, string text, bool canBuy)
        {
            Put(s.Rt, world);
            s.Label.text = text;
            s.Up.color = canBuy ? Ui.Green : Ui.Off;
        }

        void UpdateSigns()
        {
            var s = S; var h = H; var view = HarbourView.Instance;
            if (view == null) return;
            if (_signBerths != h.berths.Count) RebuildSigns();
            for (int i = 0; i < h.berths.Count; i++)
                SetSign(_berthSigns[i], view.BerthSign(i), h.berths[i].level.ToString(), s.Affordable(h, Stage.Berth, i) > 0);
            SetSign(_dockSign, view.DockSign, h.dock.ToString(), s.Affordable(h, Stage.Dock) > 0);
            SetSign(_hallSign, view.HallSign, h.hall.ToString(), s.Affordable(h, Stage.Hall) > 0);
            SetSign(_marketSign, view.MarketSign, h.market.ToString(), s.Affordable(h, Stage.Market) > 0);
            if (_nextSign != null)
            {
                double cost = s.BerthUnlockCost(h); bool can = h.cash >= cost;
                Put(_nextSign.Rt, view.NextBerthSign(h.berths.Count));
                _nextSign.Label.text = can ? "NEW DOCK  " + Format.Money(cost) : "NEW DOCK  " + Format.Money(cost) + "  <size=22>" + Mathf.FloorToInt((float)(h.cash / cost * 100)) + "%</size>";
                _nextSign.Label.rectTransform.offsetMax = new Vector2(-8, 0);
                _nextSign.Back.color = can ? Ui.Gold : Color.white;
            }
            // TAP bubbles over what stands idle without a skipper.
            float bob = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 10;
            int n = 0;
            for (int i = 0; i < h.berths.Count; i++)
            {
                var b = h.berths[i];
                bool idle = !b.skipper && !b.sailing && b.crate < s.DeckOf(h, i);
                Put(_tapBubbles[n], view.BoatAt(i) + Vector3.up * 4.2f, idle);
                if (idle) _tapBubbles[n].anchoredPosition += new Vector2(0, bob);
                n++;
            }
            bool dockIdle = !h.dockSkipper && h.haulers.Exists(c => !c.busy) && CratesWaiting(h) && s.PileRoom(h) > 0;
            Put(_tapBubbles[n], view.HaulerAt(h.haulers.FindIndex(c => !c.busy) < 0 ? 0 : h.haulers.FindIndex(c => !c.busy)) + Vector3.up * 2.6f, dockIdle);
            if (dockIdle) _tapBubbles[n].anchoredPosition += new Vector2(0, bob);
            n++;
            int idleTruck = h.trucks.FindIndex(c => !c.busy);
            bool marketIdle = !h.marketSkipper && idleTruck >= 0 && (h.raw > 0 || h.done > 0);
            Put(_tapBubbles[n], view.TruckAt(Mathf.Max(0, idleTruck)) + Vector3.up * 3f, marketIdle);
            if (marketIdle) _tapBubbles[n].anchoredPosition += new Vector2(0, bob);
        }

        static bool CratesWaiting(HarbourState h) { foreach (var b in h.berths) if (b.crate > 0) return true; return false; }

        // ---- chips and toasts ----

        sealed class Chip { public RectTransform Rt; public Text Label; public float Age, Life; public Vector2 From; }
        readonly List<Chip> _chips = new List<Chip>();
        float _lastSoldSound;

        void Pop(Vector3 world, string text, Color color, int size = 40, float life = 1.3f, Sprite picture = null)
        {
            Vector2 p;
            if (!Project(world, out p) || _chips.Count > 24) return;
            var l = Ui.Label("Chip", _fx, text, size, color);
            l.rectTransform.anchorMin = l.rectTransform.anchorMax = Vector2.zero; l.rectTransform.sizeDelta = new Vector2(420, 60); l.rectTransform.anchoredPosition = p;
            l.Stroke(2.5f);
            if (picture != null) { var pic = Ui.Img("Pic", l.transform, Color.white); pic.sprite = picture; pic.preserveAspect = true; pic.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0), new Vector2(0, -6), new Vector2(110, 110)); }
            _chips.Add(new Chip { Rt = l.rectTransform, Label = l, Life = life, From = p });
        }

        void UpdateFx()
        {
            for (int i = _chips.Count - 1; i >= 0; i--)
            {
                var c = _chips[i];
                c.Age += Time.unscaledDeltaTime;
                float t = c.Age / c.Life;
                if (t >= 1) { Destroy(c.Rt.gameObject); _chips.RemoveAt(i); continue; }
                c.Rt.anchoredPosition = c.From + new Vector2(0, 90 * (1 - (1 - t) * (1 - t)));
                c.Rt.localScale = Vector3.one * (t < 0.12f ? Mathf.Lerp(0.5f, 1.15f, t / 0.12f) : Mathf.Lerp(1.15f, 1f, Mathf.Clamp01((t - 0.12f) / 0.15f)));
                var col = c.Label.color; col.a = t > 0.7f ? 1 - (t - 0.7f) / 0.3f : 1; c.Label.color = col;
            }
            if (_toastHeld != null && !ModalOpen) { var held = _toastHeld; _toastHeld = null; Toast(held); }
            if (_toast != null)
            {
                _toastAge += Time.unscaledDeltaTime;
                var cg = _toast.GetComponent<CanvasGroup>();
                cg.alpha = _toastAge < 0.2f ? _toastAge / 0.2f : _toastAge > 2.4f ? 1 - (_toastAge - 2.4f) / 0.4f : 1;
                if (_toastAge > 2.8f) { Destroy(_toast.gameObject); _toast = null; }
            }
        }

        static readonly Color[] RarityColors = { Color.white, new Color(0.56f, 0.93f, 0.56f), new Color(0.5f, 0.85f, 1f), new Color(0.85f, 0.68f, 1f), new Color(1f, 0.82f, 0.3f) };
        public static Color RarityColor(Rarity r) { return RarityColors[(int)r]; }

        void OnCaught(int berth, SpeciesDef sp, float kg, double units, bool first)
        {
            var view = HarbourView.Instance; if (view == null) return;
            if (first && sp.Rarity >= Rarity.Rare || sp.Rarity >= Rarity.Epic) { BigCatch(sp, kg, first); return; }
            if (first)
            {
                Toast("New species: " + sp.Name + "  +" + Content.RarityGems[(int)sp.Rarity] + " gems");
                Sfx.Play("star", 0.6f);
            }
            if (sp.Rarity >= Rarity.Rare || first) Pop(view.BoatAt(berth) + Vector3.up * 2, sp.Name + "  " + kg.ToString(kg < 10 ? "0.0" : "0") + " kg", RarityColor(sp.Rarity), 34, 2f, SpeciesPicture(sp));
        }

        void OnSold(double cash)
        {
            var view = HarbourView.Instance; if (view == null) return;
            Pop(Layout.MarketGate + Vector3.up * 3, "+" + Format.Money(cash), Ui.Gold, 44);
            Vector2 from; if (!SheetOpen && !ScreenOpen && Project(Layout.MarketGate + Vector3.up * 2, out from) && _flyers.Count < 12) CoinBurst(3, from);
            if (Time.unscaledTime - _lastSoldSound > 0.35f) { _lastSoldSound = Time.unscaledTime; Sfx.Play("coin", 0.35f); }
        }

        void OnMilestone(Stage stage, int berth, int level)
        {
            var view = HarbourView.Instance; if (view == null) return;
            Vector3 at = stage == Stage.Berth ? view.BerthSign(berth) : stage == Stage.Dock ? view.DockSign : stage == Stage.Hall ? view.HallSign : view.MarketSign;
            bool speed = stage == Stage.Berth && Array.IndexOf(Content.SpeedMilestones, level) >= 0;
            Pop(at + Vector3.up * 2, "LEVEL " + level + "!  " + (speed ? "x2 SPEED" : "x2"), Ui.Gold, 52, 2.2f);
            Sfx.Play("star", 0.8f);
            Haptics.Success();
            RateSoon();
        }

/// <summary>A rare fish comes aboard: the whole screen says so (only when nothing else is up).</summary>
        void BigCatch(SpeciesDef sp, float kg, bool first)
        {
            Sfx.Play("star", 1f); Haptics.Success();
            if (ModalOpen || ScreenOpen) { Toast((first ? "New species: " : "") + sp.Name + "  " + kg.ToString(kg < 10 ? "0.0" : "0") + " kg"); return; }
            var card = Card(760, 640, sp.Rarity == Rarity.Legendary ? "A LEGEND!" : sp.Rarity == Rarity.Epic ? "EPIC CATCH!" : "RARE CATCH!");
            var rays = Ui.ArtImg("Rays", card, "rays", RarityInk[(int)sp.Rarity], 0); rays.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(520, 520));
            rays.gameObject.AddComponent<Spinner>();
            var pic = Ui.Img("Pic", card, Color.white); pic.sprite = SpeciesPicture(sp); pic.preserveAspect = true; pic.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(300, 300));
            var n = Ui.Label("N", card, sp.Name, 52, Ui.Ink); n.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 196), new Vector2(700, 64));
            string extra = first ? "New in the book  ·  +" + Content.RarityGems[(int)sp.Rarity] + " gems" : "x" + Content.RarityValue[(int)sp.Rarity] + " for this trip";
            var w = Ui.Label("W", card, kg.ToString(kg < 10 ? "0.0" : "0") + " kg  ·  " + extra, 30, Ui.Dim); w.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 152), new Vector2(700, 40));
            var ok = Ui.Button("Ok", card, S.CanMount(sp) ? "MOUNT IT  +" + Mathf.RoundToInt((float)(Sim.TrophyBonus(sp.Rarity) * 100)) + "%" : "WHAT A FISH!", Ui.Green, () => { if (S.Mount(sp.Id)) Toast("Mounted in the trophy hall"); CloseCard(); GameRoot.Instance.Save(); }, 40);
            ((RectTransform)ok.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(460, 108));
        }

        // ---- the drifting crate ----

        RectTransform _bonusBubble;

        void OnBonus() { Sfx.Play("chime", 0.5f); }

        void UpdateBonus()
        {
            var view = HarbourView.Instance; if (view == null) return;
            bool on = S.BonusAfloat && !ScreenOpen;
            if (on && _bonusBubble == null)
            {
                var img = Ui.ArtImg("Bonus", _signs, "bubble", Ui.Gold, 0);
                _bonusBubble = img.rectTransform; _bonusBubble.sizeDelta = new Vector2(104, 118); _bonusBubble.anchorMin = _bonusBubble.anchorMax = Vector2.zero; _bonusBubble.pivot = new Vector2(0.5f, 0);
                var star = Ui.ArtImg("S", _bonusBubble, "star", Color.white, 0); star.rectTransform.Fill(18); star.rectTransform.offsetMin = new Vector2(18, 36);
                var btn = img.gameObject.AddComponent<Button>(); img.raycastTarget = true; btn.onClick.AddListener(TakeBonus);
            }
            if (_bonusBubble == null) return;
            if (_bonusBubble.gameObject.activeSelf != on) _bonusBubble.gameObject.SetActive(on);
            if (!on) return;
            Vector2 p;
            if (Project(view.BonusAt + Vector3.up * 2.2f, out p)) _bonusBubble.anchoredPosition = p + new Vector2(0, Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 12);
            // It blinks when it is about to drift away.
            _bonusBubble.GetComponent<Image>().color = S.BonusLeft < 8 && Mathf.Sin(Time.unscaledTime * 14) > 0 ? Color.white : Ui.Gold;
        }

        void TakeBonus()
        {
            var view = HarbourView.Instance; var at = view != null ? view.BonusAt : Vector3.zero;
            var p = S.ClaimBonus(); if (p == null) return;
            Sfx.Play("claim", 0.9f); Haptics.Medium();
            Vector2 from; bool onScreen = Project(at + Vector3.up * 2, out from);
            switch (p.Value.Kind)
            {
                case BonusKind.Cash: Pop(at + Vector3.up * 3, "+" + Format.Money(p.Value.Cash), Ui.Gold, 52, 1.8f); if (onScreen) CoinBurst(10, from); break;
                case BonusKind.Gems: Pop(at + Vector3.up * 3, "+" + p.Value.Gems + " gems", new Color(0.8f, 0.7f, 1f), 52, 1.8f); break;
                default: Pop(at + Vector3.up * 3, "x2 for 10 minutes!", new Color(0.5f, 0.95f, 1f), 48, 2f); break;
            }
            GameRoot.Instance.Save();
        }

        RectTransform _toast; float _toastAge;
        string _toastHeld;

        public void Toast(string text)
        {
            if (text == null) return;
            if (ModalOpen) { _toastHeld = text; return; }      // a card is up: say it when the card has gone
            if (_toast != null) Destroy(_toast.gameObject);
            var img = Ui.ArtImg("Toast", _root, "plate", Ui.Ink, 36);
            img.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -156), new Vector2(Mathf.Min(1100, 130 + text.Length * 19), 76));
            var l = Ui.Label("T", img.transform, text, 32, Color.white, TextAnchor.MiddleCenter, false); l.rectTransform.Fill(8); l.rectTransform.offsetMin = new Vector2(8, 12);
            img.gameObject.AddComponent<CanvasGroup>().alpha = 0;
            _toast = img.rectTransform; _toastAge = 0;
        }

        // ---- the guide ----

        RectTransform _guide, _pointer; Text _guideText, _guideCount; Button _guideClaim; Image _guideBar;

        void BuildGuide()
        {
            var g = Ui.ArtImg("Guide", _root, "plate", Color.white, 36);
            _guide = g.rectTransform; _guide.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -178), new Vector2(410, 170));
            var tab = Ui.ArtImg("Tab", _guide, "tag", Ui.Gold, 30); tab.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(20, -4), new Vector2(120, 52));
            var t = Ui.Label("H", tab.transform, "GOAL", 28, Color.white); t.rectTransform.Fill(); t.rectTransform.offsetMin = new Vector2(0, 4); t.Stroke(2f);
            _guideCount = Ui.Label("C", _guide, "", 26, Ui.Dim, TextAnchor.UpperRight); _guideCount.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -10), new Vector2(200, 30));
            _guideText = Ui.Label("T", _guide, "", 28, Ui.Ink, TextAnchor.UpperLeft, false); _guideText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -40), new Vector2(370, 90));
            _guideText.resizeTextForBestFit = true; _guideText.resizeTextMinSize = 18; _guideText.resizeTextMaxSize = 28; _guideText.verticalOverflow = VerticalWrapMode.Truncate;
            var back = Ui.Img("BarBack", _guide, Ui.Faint, true); back.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(22, 22), new Vector2(366, 16));
            _guideBar = Ui.Img("Bar", back.transform, Ui.Green, true); _guideBar.rectTransform.anchorMin = Vector2.zero; _guideBar.rectTransform.anchorMax = new Vector2(0, 1); _guideBar.rectTransform.offsetMin = _guideBar.rectTransform.offsetMax = Vector2.zero;
            _guideClaim = Ui.Button("Claim", _guide, "CLAIM", Ui.Green, ClaimGoal, 34);
            ((RectTransform)_guideClaim.transform).Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-14, 14), new Vector2(170, 72));
            var hand = Ui.ArtImg("Pointer", _fx.parent, "hand", Color.white, 0);
            _pointer = hand.rectTransform; _pointer.anchorMin = _pointer.anchorMax = Vector2.zero; _pointer.pivot = new Vector2(0.3f, 1); _pointer.sizeDelta = new Vector2(90, 90);
        }

        void ClaimGoal()
        {
            var g = S.Goal; if (g == null) return;
            string reward = (g.CashSeconds > 0 || g.Floor > 0 ? "+" + Format.Money(S.GoalCash(g)) : "") + (g.Gems > 0 ? "  +" + g.Gems + " gems" : "");
            if (!S.ClaimGoal()) return;
            Sfx.Play("claim", 0.8f); Haptics.Medium();
            Toast("Done!  " + reward.Trim());
            CoinBurst(8, new Vector2(230, ((RectTransform)transform).rect.height - 260));
            GameRoot.Instance.Save();
        }

        void UpdateGuide()
        {
            var s = S; var g = s.Goal;
            _guide.gameObject.SetActive(g != null);
            bool point = false;
            if (g != null)
            {
                bool done = s.GoalDone;
                _guideText.text = g.Text;
                _guideCount.text = g.Need > 1 ? Format.Money(s.GoalHave) + " / " + Format.Money(g.Need) : "";
                _guideBar.rectTransform.anchorMax = new Vector2((float)(s.GoalHave / g.Need), 1);
                _guideBar.transform.parent.gameObject.SetActive(!done);
                _guideClaim.gameObject.SetActive(done);
                _guideText.rectTransform.sizeDelta = new Vector2(done ? 190 : 370, done ? 112 : 90);
                // The hand points at what the step is about, for the first steps and while nothing else is open.
                var view = HarbourView.Instance;
                if (!done && view != null && s.State.goalIndex < 12 && !SheetOpen && !ModalOpen && !ScreenOpen && (g.Point != Stage.Berth || g.PointBerth >= 0))
                {
                    var h = H;
                    Vector3 at = g.Point == Stage.Dock ? (h.dockSkipper || g.Id == "dock5" || g.Id == "dockskipper" ? view.DockSign : view.HaulerAt(0))
                        : g.Point == Stage.Market ? (g.Id == "marketskipper" ? view.MarketSign : view.TruckAt(0))
                        : g.Point == Stage.Hall ? view.HallSign
                        : g.PointBerth >= h.berths.Count ? view.NextBerthSign(h.berths.Count) : g.Id == "cast" ? view.BoatAt(g.PointBerth) : view.BerthSign(g.PointBerth);
                    Vector2 p;
                    if (g.Id != "species5" && Project(at, out p)) { point = true; _pointer.anchoredPosition = p + new Vector2(0, -6 - Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 16); }
                }
            }
            if (_pointer.gameObject.activeSelf != point) _pointer.gameObject.SetActive(point);
        }
    }
}

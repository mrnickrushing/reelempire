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

        Sign MakeSign(Action tap, float width = 150)
        {
            var b = Ui.Button("Sign", _signs, null, new Color(0.12f, 0.17f, 0.24f, 0.95f), tap, 30);
            var rt = (RectTransform)b.transform; rt.sizeDelta = new Vector2(width, 56); rt.anchorMin = rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0);
            var l = Ui.Label("L", rt, "", 30, Color.white); l.rectTransform.Fill(); l.rectTransform.offsetMin = new Vector2(8, 8); l.rectTransform.offsetMax = new Vector2(-40, 0);
            var up = Ui.Img("Up", rt, Ui.Green); up.sprite = Ui.Circle; up.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-8, 3), new Vector2(34, 34));
            var arrow = Ui.Label("A", up.transform, "▲", 20, Color.white); arrow.rectTransform.Fill();
            return new Sign { Rt = rt, Label = l, Back = b.GetComponent<Image>(), Up = up };
        }

        RectTransform MakeBubble()
        {
            var img = Ui.ArtImg("Tap", _signs, "bubble", Color.white, 0);
            var rt = img.rectTransform; rt.sizeDelta = new Vector2(92, 92); rt.anchorMin = rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0);
            var hand = Ui.ArtImg("Hand", rt, "hand", Color.white, 0); hand.rectTransform.Fill(16);
            return rt;
        }

        void RebuildSigns()
        {
            Ui.Clear(_signs); _berthSigns.Clear(); _tapBubbles.Clear();
            var h = H; _signBerths = h.berths.Count;
            for (int i = 0; i < h.berths.Count; i++) { int k = i; _berthSigns.Add(MakeSign(() => OpenSheet(Stage.Berth, k))); }
            _dockSign = MakeSign(() => OpenSheet(Stage.Dock, 0)); _hallSign = MakeSign(() => OpenSheet(Stage.Hall, 0)); _marketSign = MakeSign(() => OpenSheet(Stage.Market, 0));
            _nextSign = h.berths.Count < Content.Berths.Length ? MakeSign(UnlockBerth, 210) : null;
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
            s.Up.color = canBuy ? Ui.Green : new Color(0.3f, 0.35f, 0.42f);
        }

        void UpdateSigns()
        {
            var s = S; var h = H; var view = HarbourView.Instance;
            if (view == null) return;
            if (_signBerths != h.berths.Count) RebuildSigns();
            for (int i = 0; i < h.berths.Count; i++)
                SetSign(_berthSigns[i], view.BerthSign(i), "Lv " + h.berths[i].level, s.Affordable(h, Stage.Berth, i) > 0);
            SetSign(_dockSign, view.DockSign, "Lv " + h.dock, s.Affordable(h, Stage.Dock) > 0);
            SetSign(_hallSign, view.HallSign, "Lv " + h.hall, s.Affordable(h, Stage.Hall) > 0);
            SetSign(_marketSign, view.MarketSign, "Lv " + h.market, s.Affordable(h, Stage.Market) > 0);
            if (_nextSign != null)
            {
                double cost = s.BerthUnlockCost(h); bool can = h.cash >= cost;
                Put(_nextSign.Rt, view.BerthSign(h.berths.Count));
                _nextSign.Label.text = "OPEN  " + Format.Money(cost);
                _nextSign.Label.rectTransform.offsetMax = new Vector2(-8, 0);
                _nextSign.Back.color = can ? Ui.Green : new Color(0.12f, 0.17f, 0.24f, 0.95f);
            }
            // TAP bubbles over what stands idle without a skipper.
            float bob = Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 10;
            int n = 0;
            for (int i = 0; i < h.berths.Count; i++)
            {
                var b = h.berths[i];
                bool idle = !b.skipper && !b.sailing && b.crate < s.DeckOf(h, i);
                Put(_tapBubbles[n], view.BoatAt(i) + Vector3.up * 3.5f, idle);
                if (idle) _tapBubbles[n].anchoredPosition += new Vector2(0, bob);
                n++;
            }
            bool dockIdle = !h.dockSkipper && h.haulers.Exists(c => !c.busy) && CratesWaiting(h) && s.PileRoom(h) > 0;
            Put(_tapBubbles[n], view.HaulerAt(h.haulers.FindIndex(c => !c.busy) < 0 ? 0 : h.haulers.FindIndex(c => !c.busy)) + Vector3.up, dockIdle);
            if (dockIdle) _tapBubbles[n].anchoredPosition += new Vector2(0, bob);
            n++;
            int idleTruck = h.trucks.FindIndex(c => !c.busy);
            bool marketIdle = !h.marketSkipper && idleTruck >= 0 && (h.raw > 0 || h.done > 0);
            Put(_tapBubbles[n], view.TruckAt(Mathf.Max(0, idleTruck)) + Vector3.up, marketIdle);
            if (marketIdle) _tapBubbles[n].anchoredPosition += new Vector2(0, bob);
        }

        static bool CratesWaiting(HarbourState h) { foreach (var b in h.berths) if (b.crate > 0) return true; return false; }

        // ---- chips and toasts ----

        sealed class Chip { public RectTransform Rt; public Text Label; public float Age, Life; public Vector2 From; }
        readonly List<Chip> _chips = new List<Chip>();
        float _lastSoldSound;

        void Pop(Vector3 world, string text, Color color, int size = 40, float life = 1.3f)
        {
            Vector2 p;
            if (!Project(world, out p) || _chips.Count > 24) return;
            var l = Ui.Label("Chip", _fx, text, size, color);
            l.rectTransform.anchorMin = l.rectTransform.anchorMax = Vector2.zero; l.rectTransform.sizeDelta = new Vector2(420, 60); l.rectTransform.anchoredPosition = p;
            var sh = l.gameObject.AddComponent<Outline>(); sh.effectColor = new Color(0, 0, 0, 0.7f); sh.effectDistance = new Vector2(2, -2);
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
            if (_toast != null)
            {
                _toastAge += Time.unscaledDeltaTime;
                var cg = _toast.GetComponent<CanvasGroup>();
                cg.alpha = _toastAge < 0.2f ? _toastAge / 0.2f : _toastAge > 2.4f ? 1 - (_toastAge - 2.4f) / 0.4f : 1;
                if (_toastAge > 2.8f) { Destroy(_toast.gameObject); _toast = null; }
            }
        }

        static readonly Color[] RarityColors = { Color.white, new Color(0.45f, 0.85f, 0.45f), new Color(0.4f, 0.7f, 1f), new Color(0.8f, 0.5f, 1f), new Color(1f, 0.8f, 0.25f) };
        public static Color RarityColor(Rarity r) { return RarityColors[(int)r]; }

        void OnCaught(int berth, SpeciesDef sp, float kg, double units, bool first)
        {
            var view = HarbourView.Instance; if (view == null) return;
            if (first)
            {
                Toast("New species: " + sp.Name + "  +" + Content.RarityGems[(int)sp.Rarity] + " gems");
                Sfx.Play("star", 0.6f);
            }
            if (sp.Rarity >= Rarity.Rare || first) Pop(view.BoatAt(berth), sp.Name + "  " + kg.ToString(kg < 10 ? "0.0" : "0") + " kg", RarityColor(sp.Rarity), 36, 1.8f);
        }

        void OnSold(double cash)
        {
            var view = HarbourView.Instance; if (view == null) return;
            Pop(Layout.MarketGate + Vector3.up * 3, "+" + Format.Money(cash), Ui.Gold, 44);
            if (Time.unscaledTime - _lastSoldSound > 0.35f) { _lastSoldSound = Time.unscaledTime; Sfx.Play("coin", 0.35f); }
        }

        void OnMilestone(Stage stage, int berth, int level)
        {
            var view = HarbourView.Instance; if (view == null) return;
            Vector3 at = stage == Stage.Berth ? view.BerthSign(berth) : stage == Stage.Dock ? view.DockSign : stage == Stage.Hall ? view.HallSign : view.MarketSign;
            bool speed = stage == Stage.Berth && Array.IndexOf(Content.SpeedMilestones, level) >= 0;
            Pop(at + Vector3.up * 2, "LEVEL " + level + "!  " + (speed ? "x2 SPEED" : "x2"), Ui.Gold, 52, 2.2f);
            Sfx.Play("star", 0.8f);
            RateSoon();
        }

        RectTransform _toast; float _toastAge;
        public void Toast(string text)
        {
            if (_toast != null) Destroy(_toast.gameObject);
            var img = Ui.ArtImg("Toast", _root, "plate", new Color(0.08f, 0.11f, 0.16f, 0.95f), 44);
            img.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(Mathf.Min(1100, 120 + text.Length * 19), 70));
            var l = Ui.Label("T", img.transform, text, 34, Color.white); l.rectTransform.Fill(8);
            img.gameObject.AddComponent<CanvasGroup>().alpha = 0;
            _toast = img.rectTransform; _toastAge = 0;
        }

        // ---- the guide ----

        RectTransform _guide, _pointer; Text _guideText, _guideCount; Button _guideClaim; Image _guideBar;

        void BuildGuide()
        {
            var g = Ui.ArtImg("Guide", _root, "plate", new Color(0.1f, 0.14f, 0.2f, 0.92f), 44);
            _guide = g.rectTransform; _guide.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -146), new Vector2(430, 150));
            var t = Ui.Label("H", _guide, "NEXT", 24, Ui.Gold, TextAnchor.UpperLeft); t.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -10), new Vector2(200, 30));
            _guideCount = Ui.Label("C", _guide, "", 24, Ui.Dim, TextAnchor.UpperRight); _guideCount.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -10), new Vector2(200, 30));
            _guideText = Ui.Label("T", _guide, "", 30, Color.white, TextAnchor.UpperLeft, false); _guideText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -42), new Vector2(390, 80));
            var back = Ui.Img("BarBack", _guide, new Color(0, 0, 0, 0.4f), true); back.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(22, 14), new Vector2(386, 12));
            _guideBar = Ui.Img("Bar", back.transform, Ui.Green, true); _guideBar.rectTransform.anchorMin = Vector2.zero; _guideBar.rectTransform.anchorMax = new Vector2(0, 1); _guideBar.rectTransform.offsetMin = _guideBar.rectTransform.offsetMax = Vector2.zero;
            _guideClaim = Ui.Button("Claim", _guide, "CLAIM", Ui.Green, ClaimGoal, 36);
            ((RectTransform)_guideClaim.transform).Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-14, 12), new Vector2(170, 64));
            var hand = Ui.ArtImg("Pointer", _fx.parent, "hand", Color.white, 0);
            _pointer = hand.rectTransform; _pointer.anchorMin = _pointer.anchorMax = Vector2.zero; _pointer.pivot = new Vector2(0.3f, 1); _pointer.sizeDelta = new Vector2(90, 90);
        }

        void ClaimGoal()
        {
            var g = S.Goal; if (g == null) return;
            string reward = (g.CashSeconds > 0 || g.Floor > 0 ? "+" + Format.Money(S.GoalCash(g)) : "") + (g.Gems > 0 ? "  +" + g.Gems + " gems" : "");
            if (!S.ClaimGoal()) return;
            Sfx.Play("claim", 0.8f);
            Toast("Done!  " + reward.Trim());
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
                _guideText.rectTransform.sizeDelta = new Vector2(done ? 220 : 390, 80);
                // The hand points at what the step is about, for the first steps and while nothing else is open.
                var view = HarbourView.Instance;
                if (!done && view != null && s.State.goalIndex < 12 && !SheetOpen && !ModalOpen && (g.Point != Stage.Berth || g.PointBerth >= 0))
                {
                    var h = H;
                    Vector3 at = g.Point == Stage.Dock ? (h.dockSkipper || g.Id == "dock5" || g.Id == "dockskipper" ? view.DockSign : view.HaulerAt(0))
                        : g.Point == Stage.Market ? (g.Id == "marketskipper" ? view.MarketSign : view.TruckAt(0))
                        : g.Point == Stage.Hall ? view.HallSign
                        : g.PointBerth >= h.berths.Count ? view.BerthSign(h.berths.Count) : g.Id == "cast" ? view.BoatAt(g.PointBerth) : view.BerthSign(g.PointBerth);
                    Vector2 p;
                    if (g.Id != "species5" && Project(at, out p)) { point = true; _pointer.anchoredPosition = p + new Vector2(0, -6 - Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 16); }
                }
            }
            if (_pointer.gameObject.activeSelf != point) _pointer.gameObject.SetActive(point);
        }
    }
}

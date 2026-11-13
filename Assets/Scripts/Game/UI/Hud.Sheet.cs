using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>The upgrade sheet rising from the bottom of the screen: on the left a stage's name and level and the way
    /// to its next milestone, in the middle what it does now and after the purchase (the hall: its lines), on the right
    /// its skipper, how many levels to buy and the big button.</summary>
    public sealed partial class Hud
    {
        RectTransform _sheet, _sheetBody;
        Stage _sheetStage; int _sheetBerth = -1; bool _sheetOpen; float _sheetY = SheetHide;
        int _buyMode;       // 0: x1, 1: x10, 2: to the next milestone, 3: all the cash buys
        static readonly string[] BuyNames = { "x1", "x10", "NEXT", "MAX" };
        Button _upgrade; Text _upgradeLabel, _upgradeCost;
        readonly List<Action> _live = new List<Action>();

        public bool SheetOpen { get { return _sheetOpen; } }
        const float SheetWidth = 1420, SheetHeight = 356, SheetHide = 420;

        void BuildSheet()
        {
            var img = Ui.ArtImg("Sheet", _root, "panel", Color.white, 60);
            img.raycastTarget = true;
            _sheet = img.rectTransform;
            _sheet.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, -SheetHide), new Vector2(SheetWidth, SheetHeight));
            _sheetBody = Ui.Rect("Body", _sheet); _sheetBody.Fill(); _sheetBody.offsetMin = new Vector2(30, 34); _sheetBody.offsetMax = new Vector2(-30, -24);
        }

        public void OpenSheet(Stage stage, int berth)
        {
            if (stage == Stage.Berth) berth = Mathf.Clamp(berth, 0, H.berths.Count - 1);
            if (_sheetOpen && _sheetStage == stage && _sheetBerth == berth) { CloseSheet(); return; }
            _sheetStage = stage; _sheetBerth = berth; _sheetOpen = true;
            Sfx.Play("open", 0.4f);
            RefreshSheet();
        }

        public void CloseSheet() { if (_sheetOpen) Sfx.Play("close", 0.4f); _sheetOpen = false; }

        void UpdateSheet()
        {
            _sheetY = Mathf.Lerp(_sheetY, _sheetOpen ? 0 : SheetHide, 1 - Mathf.Exp(-16 * Time.unscaledDeltaTime));
            _sheet.anchoredPosition = new Vector2(0, 8 - _sheetY);
            if (!_sheetOpen) return;
            foreach (var a in _live) a();
        }

        int BuyCount()
        {
            var h = H; int lv = S.LevelOf(h, _sheetStage, _sheetBerth);
            int left = Content.MaxLevel - lv;
            switch (_buyMode)
            {
                case 0: return Math.Min(1, left);
                case 1: return Math.Min(10, left);
                case 2: return Math.Min(Content.NextMilestone(lv) - lv, left);
                default: return Math.Max(1, Math.Min(left, S.Affordable(h, _sheetStage, _sheetBerth)));
            }
        }

        /// <summary>A stat row in the middle column: its name, the figure now and (in green) after the purchase.</summary>
        void Row(RectTransform col, float y, string name, string now, string next)
        {
            var back = Ui.ArtImg("Row", col, "card", Ui.Card, 26); back.rectTransform.Band(true, y, 68);
            var n = Ui.Label("N", back.transform, name, 26, Ui.Dim, TextAnchor.MiddleLeft, false); n.rectTransform.Fill(); n.rectTransform.offsetMin = new Vector2(20, 0);
            var v = Ui.Label("V", back.transform, now + (next != null && next != now ? "  <color=#1E9E4A>▸ " + next + "</color>" : ""), 30, Ui.Ink, TextAnchor.MiddleRight); v.rectTransform.Fill(); v.rectTransform.offsetMax = new Vector2(-20, 0);
        }

        void RefreshSheet()
        {
            if (!_sheetOpen || _sheetBody == null) return;
            Ui.Clear(_sheetBody); _live.Clear();
            var s = S; var h = H; var stage = _sheetStage; int berth = stage == Stage.Berth ? Mathf.Min(_sheetBerth, h.berths.Count - 1) : 0;
            int lv = s.LevelOf(h, stage, berth);
            string title = stage == Stage.Berth ? Content.Berths[berth].Name : stage == Stage.Dock ? "The Dock" : stage == Stage.Hall ? "Processing Hall" : "Fish Market";
            string sub = stage == Stage.Berth ? Content.Berths[berth].Boat + " · berth " + (berth + 1) : stage == Stage.Dock ? "Haulers carry the catch to the hall" : stage == Stage.Hall ? "Turns fish into what sells for more" : "Trucks sell what the hall puts out";

            var left = Ui.Rect("Left", _sheetBody); left.anchorMin = new Vector2(0, 0); left.anchorMax = new Vector2(0, 1); left.pivot = new Vector2(0, 0.5f); left.offsetMin = Vector2.zero; left.offsetMax = new Vector2(400, 0);
            var mid = Ui.Rect("Mid", _sheetBody); mid.anchorMin = new Vector2(0, 0); mid.anchorMax = new Vector2(0, 1); mid.pivot = new Vector2(0, 0.5f); mid.offsetMin = new Vector2(430, 0); mid.offsetMax = new Vector2(900, 0);
            var right = Ui.Rect("Right", _sheetBody); right.anchorMin = new Vector2(0, 0); right.anchorMax = new Vector2(1, 1); right.offsetMin = new Vector2(930, 0); right.offsetMax = new Vector2(-64, 0);
            var x = Ui.Button("X", _sheetBody, "✕", Ui.Coral, CloseSheet, 30); ((RectTransform)x.transform).Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(8, 6), new Vector2(64, 64));

            // Left: the name, the level on a badge, the way to the next milestone.
            var badge = Ui.ArtImg("Lv", left, "badge", Ui.Blue, 0); badge.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 0), new Vector2(124, 132));
            var lvk = Ui.Label("K", badge.transform, "LEVEL", 20, Color.white); lvk.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(120, 24)); lvk.Stroke(1.5f);
            var lvn = Ui.Label("N", badge.transform, lv.ToString(), 52, Color.white); lvn.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -6), new Vector2(120, 60)); lvn.Stroke(2.5f);
            var t = Ui.Label("Title", left, title, 44, Ui.Ink, TextAnchor.UpperLeft); t.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(138, -6), new Vector2(270, 100));
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 28; t.resizeTextMaxSize = 44;
            var st = Ui.Label("Sub", left, sub, 24, Ui.Dim, TextAnchor.UpperLeft, false); st.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, -142), new Vector2(400, 60));
            int next = Content.NextMilestone(lv), prev = 1; foreach (var m in Content.Milestones) if (m <= lv) prev = m;
            bool speedNext = stage == Stage.Berth && Array.IndexOf(Content.SpeedMilestones, next) >= 0;
            bool crewNext = stage != Stage.Berth && stage != Stage.Hall && Array.IndexOf(Content.CrewMilestones, next) >= 0;
            string prize = lv >= Content.MaxLevel ? "As far as it goes" : "Level " + next + ":  " + (speedNext ? "boat twice as fast" : "x2") + (crewNext ? (stage == Stage.Dock ? " + a hauler" : " + a truck") : "");
            var star = Ui.ArtImg("Star", left, "star", Color.white, 0); star.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(-4, 36), new Vector2(50, 52));
            var ms = Ui.Label("Ms", left, prize, 26, Ui.Ink, TextAnchor.MiddleLeft); ms.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(54, 42), new Vector2(346, 40));
            var back = Ui.ArtImg("BarBack", left, "well", Ui.Faint, 20); back.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(400, 34));
            var bar = Ui.ArtImg("Bar", back.transform, "well", Ui.Gold, 20); bar.rectTransform.anchorMin = Vector2.zero; bar.rectTransform.anchorMax = new Vector2(lv >= Content.MaxLevel ? 1 : Mathf.Max(0.09f, (lv - prev) / (float)Mathf.Max(1, next - prev)), 1); bar.rectTransform.offsetMin = bar.rectTransform.offsetMax = Vector2.zero;

            // Middle: what it does now and after the purchase; the hall shows its lines instead.
            int n = BuyCount();
            Func<Func<string>, string> after = f =>
            {
                if (n <= 0) return null;
                Bump(h, stage, berth, n); string v = f(); Bump(h, stage, berth, -n); return v;
            };
            float y = 0;
            if (stage == Stage.Berth)
            {
                Row(mid, y, "Catch a trip", Format.Money(s.CatchOf(h, berth)), after(() => Format.Money(s.CatchOf(h, berth)))); y -= 76;
                Row(mid, y, "Trip", s.TripOf(h, berth).ToString("0.#") + "s", after(() => s.TripOf(h, berth).ToString("0.#") + "s")); y -= 76;
                Row(mid, y, "Lands", Format.Money(s.BerthRate(h, berth)) + "/s", after(() => Format.Money(s.BerthRate(h, berth)) + "/s"));
            }
            else if (stage == Stage.Dock)
            {
                Row(mid, y, "Carries", Format.Money(s.DockRate(h)) + "/s", after(() => Format.Money(s.DockRate(h)) + "/s")); y -= 76;
                Row(mid, y, "Haulers", h.haulers.Count.ToString(), after(() => Content.CrewAt(h.dock).ToString())); y -= 76;
                Row(mid, y, "The boats land", Format.Money(s.FleetRate(h, true)) + "/s", null);
            }
            else if (stage == Stage.Market)
            {
                Row(mid, y, "Sells", Format.Money(s.MarketRate(h)) + "/s", after(() => Format.Money(s.MarketRate(h)) + "/s")); y -= 76;
                Row(mid, y, "Trucks", h.trucks.Count.ToString(), after(() => Content.CrewAt(h.market).ToString())); y -= 76;
                Row(mid, y, "Today's favourite", Content.Goods[s.GoodsOfTheDay].Name + " +50%", null);
            }
            else
            {
                st.text = "Works " + Format.Money(s.HallRate(h)) + "/s" + (n > 0 ? "  ▸  " + after(() => Format.Money(s.HallRate(h)) + "/s") : "") + "\nComes in " + Format.Money(Math.Min(s.FleetRate(h, true), s.DockRate(h))) + "/s";
                int shown = 0;
                for (int g = 0; g < Content.Goods.Length; g++)
                {
                    int k = g; var def = Content.Goods[g];
                    bool owned = s.OwnsGoods(h, g), running = h.goods == g, canBuy = !owned && s.OwnsGoods(h, g - 1 < 0 ? 0 : g - 1);
                    if (!owned && !canBuy) continue;
                    if (++shown > 5) break;
                    var row = Ui.ArtImg("Line", mid, "card", running ? new Color(0.72f, 0.93f, 0.76f) : Ui.Card, 26); row.rectTransform.Band(true, y, 56);
                    string worth = "x" + s.GoodsValue(h, g).ToString("0.##") + (g == s.GoodsOfTheDay ? " today!" : "") + (def.Speed < 1 ? "  " + Mathf.RoundToInt((float)def.Speed * 100) + "% pace" : "");
                    var nm = Ui.Label("N", row.transform, def.Name + "  <size=22><color=#" + (g == s.GoodsOfTheDay ? "D9631E" : "5A6E82") + ">" + worth + "</color></size>", 28, Ui.Ink, TextAnchor.MiddleLeft); nm.rectTransform.Fill(); nm.rectTransform.offsetMin = new Vector2(18, 0);
                    if (running) { var on = Ui.Label("On", row.transform, "RUNNING", 24, new Color(0.12f, 0.62f, 0.29f), TextAnchor.MiddleRight); on.rectTransform.Fill(); on.rectTransform.offsetMax = new Vector2(-18, 0); }
                    else
                    {
                        double cost = s.GoodsCost(h, g);
                        var b = Ui.Button("B", row.transform, owned ? "RUN" : Format.Money(cost), owned ? Ui.Blue : Ui.Green, () =>
                        {
                            if (owned ? S.SetGoods(k) : S.BuyGoods(k)) { Sfx.Play(owned ? "confirm" : "build", 0.7f); GameRoot.Instance.Save(); }
                            else { Sfx.Play("error", 0.5f); Toast("Not enough cash yet"); }
                        }, 26);
                        ((RectTransform)b.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-4, 0), new Vector2(140, 54));
                        if (!owned) { var bi = b.GetComponent<Image>(); _live.Add(() => bi.color = H.cash >= cost ? Ui.Green : Ui.Off); }
                    }
                    y -= 60;
                }
            }

            // Right: the skipper, how many levels, and the button.
            float top = 0;
            if (stage != Stage.Hall)
            {
                bool has = s.HasSkipper(h, stage, berth);
                var row = Ui.ArtImg("Skipper", right, "card", has ? new Color(0.72f, 0.93f, 0.76f) : Ui.Card, 26); row.rectTransform.Band(true, 0, 84);
                string who = stage == Stage.Berth ? "Skipper" : stage == Stage.Dock ? "Dock foreman" : "Market manager";
                var nm = Ui.Label("N", row.transform, who, 30, Ui.Ink, TextAnchor.UpperLeft); nm.rectTransform.Fill(); nm.rectTransform.offsetMin = new Vector2(20, 0); nm.rectTransform.offsetMax = new Vector2(0, -8);
                var w = Ui.Label("W", row.transform, has ? "On the job: no taps needed" : "Works without tapping", 22, Ui.Dim, TextAnchor.LowerLeft, false); w.rectTransform.Fill(); w.rectTransform.offsetMin = new Vector2(20, 12);
                if (!has)
                {
                    double cost = s.SkipperCost(h, stage, berth);
                    var b = Ui.Button("Hire", row.transform, cost <= 0 ? "FREE" : Format.Money(cost), Ui.Green, () =>
                    {
                        if (S.HireSkipper(stage, berth)) { Sfx.Play("confirm", 0.8f); Toast(who + " hired"); GameRoot.Instance.Save(); }
                        else { Sfx.Play("error", 0.5f); Toast("Not enough cash yet"); }
                    }, 28);
                    ((RectTransform)b.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-8, 0), new Vector2(150, 68));
                    var bi = b.GetComponent<Image>(); _live.Add(() => bi.color = H.cash >= S.SkipperCost(H, stage, berth) ? Ui.Green : Ui.Off);
                }
                top = -92;
            }
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                var b = Ui.Button("Mode" + i, right, BuyNames[i], _buyMode == i ? Ui.Blue : Ui.Off, () => { _buyMode = k; RefreshSheet(); }, 26);
                var rt = (RectTransform)b.transform; rt.anchorMin = new Vector2(i / 4f, 1); rt.anchorMax = new Vector2((i + 1) / 4f, 1); rt.pivot = new Vector2(0.5f, 1); rt.offsetMin = new Vector2(2, 0); rt.offsetMax = new Vector2(-2, 0); rt.anchoredPosition = new Vector2(0, top); rt.sizeDelta = new Vector2(rt.sizeDelta.x, 60);
            }
            _upgrade = Ui.Button("Upgrade", right, null, Ui.Green, DoUpgrade, 44);
            ((RectTransform)_upgrade.transform).Band(false, 0, stage == Stage.Hall ? 190 : 124);
            _upgradeLabel = Ui.Label("L", _upgrade.transform, "", 40, Color.white, TextAnchor.UpperCenter); _upgradeLabel.rectTransform.Fill(); _upgradeLabel.rectTransform.offsetMax = new Vector2(0, -14); _upgradeLabel.Stroke(2.5f);
            _upgradeCost = Ui.Label("C", _upgrade.transform, "", 36, Color.white, TextAnchor.LowerCenter); _upgradeCost.rectTransform.Fill(); _upgradeCost.rectTransform.offsetMin = new Vector2(0, 26); _upgradeCost.Stroke(2f);
            var upImg = _upgrade.GetComponent<Image>();
            int shownCount = n;
            _live.Add(() =>
            {
                var hh = H; int count = BuyCount();
                if (_buyMode == 3 && count != shownCount) { _dirty = true; return; }
                if (S.LevelOf(hh, stage, berth) >= Content.MaxLevel) { _upgradeLabel.text = "MAX LEVEL"; _upgradeCost.text = ""; upImg.color = Ui.Off; return; }
                double cost = S.LevelCost(hh, stage, berth, count);
                _upgradeLabel.text = "UPGRADE" + (count > 1 ? " +" + count : "");
                _upgradeCost.text = Format.Money(cost);
                upImg.color = hh.cash >= cost ? Ui.Green : Ui.Off;
            });
        }

        static void Bump(HarbourState h, Stage stage, int berth, int n)
        {
            if (stage == Stage.Berth) h.berths[berth].level += n; else if (stage == Stage.Dock) h.dock += n; else if (stage == Stage.Hall) h.hall += n; else h.market += n;
        }

        void DoUpgrade()
        {
            int n = BuyCount();
            if (n > 0 && S.Upgrade(_sheetStage, _sheetBerth < 0 ? 0 : _sheetBerth, n)) { Sfx.Play("build", 0.6f, 1 + Mathf.Min(0.4f, n * 0.02f)); }
            else { Sfx.Play("error", 0.5f); }
        }
    }
}

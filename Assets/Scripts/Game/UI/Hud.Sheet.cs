using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>The upgrade sheet down the right side: a stage's level, what the next levels give and cost, its skipper,
    /// and for the hall the lines it can run.</summary>
    public sealed partial class Hud
    {
        RectTransform _sheet, _sheetBody;
        Stage _sheetStage; int _sheetBerth = -1; bool _sheetOpen; float _sheetX = 620;
        int _buyMode;       // 0: x1, 1: x10, 2: to the next milestone, 3: all the cash buys
        static readonly string[] BuyNames = { "x1", "x10", "NEXT", "MAX" };
        Button _upgrade; Text _upgradeLabel, _upgradeCost;
        Button _skipperBtn; Text _skipperCost;
        readonly List<Action> _live = new List<Action>();

        public bool SheetOpen { get { return _sheetOpen; } }
        const float SheetWidth = 560;

        void BuildSheet()
        {
            var img = Ui.ArtImg("Sheet", _root, "panel", Color.white, 60);
            img.raycastTarget = true;
            _sheet = img.rectTransform;
            _sheet.anchorMin = new Vector2(1, 0); _sheet.anchorMax = new Vector2(1, 1); _sheet.pivot = new Vector2(1, 0.5f);
            _sheet.offsetMin = new Vector2(-SheetWidth, 16); _sheet.offsetMax = new Vector2(0, -120);
            _sheetBody = Ui.Rect("Body", _sheet); _sheetBody.Fill(26);
            _sheet.anchoredPosition = new Vector2(_sheetX, _sheet.anchoredPosition.y);
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
            float want = _sheetOpen ? 0 : SheetWidth + 60;
            _sheetX = Mathf.Lerp(_sheetX, want, 1 - Mathf.Exp(-16 * Time.unscaledDeltaTime));
            _sheet.anchoredPosition = new Vector2(_sheetX, _sheet.anchoredPosition.y);
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

        Text Row(float y, string name, string now, string next)
        {
            var back = Ui.Img("Row", _sheetBody, new Color(0, 0, 0, 0.22f), true); back.rectTransform.Band(true, y, 62);
            var n = Ui.Label("N", back.transform, name, 28, Ui.Dim, TextAnchor.MiddleLeft, false); n.rectTransform.Fill(); n.rectTransform.offsetMin = new Vector2(18, 0);
            var v = Ui.Label("V", back.transform, now + (next != null && next != now ? "  <color=#5FD27A>→ " + next + "</color>" : ""), 30, Color.white, TextAnchor.MiddleRight); v.rectTransform.Fill(); v.rectTransform.offsetMax = new Vector2(-18, 0);
            return v;
        }

        void RefreshSheet()
        {
            if (!_sheetOpen || _sheetBody == null) return;
            Ui.Clear(_sheetBody); _live.Clear();
            var s = S; var h = H; var stage = _sheetStage; int berth = stage == Stage.Berth ? Mathf.Min(_sheetBerth, h.berths.Count - 1) : 0;
            int lv = s.LevelOf(h, stage, berth);
            string title = stage == Stage.Berth ? Content.Berths[berth].Name : stage == Stage.Dock ? "The Dock" : stage == Stage.Hall ? "Processing Hall" : "Fish Market";
            string sub = stage == Stage.Berth ? Content.Berths[berth].Boat + " · berth " + (berth + 1) : stage == Stage.Dock ? "Haulers carry the catch to the hall" : stage == Stage.Hall ? "Turns fish into what sells for more" : "Trucks sell what the hall puts out";

            var t = Ui.Label("Title", _sheetBody, title.ToUpperInvariant(), 46, Ui.Gold, TextAnchor.UpperLeft); t.rectTransform.Band(true, 0, 54);
            var st = Ui.Label("Sub", _sheetBody, sub, 26, Ui.Dim, TextAnchor.UpperLeft, false); st.rectTransform.Band(true, -54, 34);
            var x = Ui.Button("X", _sheetBody, "✕", new Color(0.3f, 0.34f, 0.4f), CloseSheet, 34); ((RectTransform)x.transform).Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(64, 64));

            // Level and the way to the next milestone.
            int next = Content.NextMilestone(lv), prev = 1; foreach (var m in Content.Milestones) if (m <= lv) prev = m;
            var lvl = Ui.Label("Lv", _sheetBody, "LEVEL " + lv, 38, Color.white, TextAnchor.UpperLeft); lvl.rectTransform.Band(true, -100, 44);
            bool speedNext = stage == Stage.Berth && Array.IndexOf(Content.SpeedMilestones, next) >= 0;
            bool crewNext = stage != Stage.Berth && stage != Stage.Hall && Array.IndexOf(Content.CrewMilestones, next) >= 0;
            string prize = lv >= Content.MaxLevel ? "As far as it goes" : "Level " + next + ": " + (speedNext ? "boat twice as fast" : "x2") + (crewNext ? (stage == Stage.Dock ? " + a hauler" : " + a truck") : "");
            var ms = Ui.Label("Ms", _sheetBody, prize, 26, Ui.Gold, TextAnchor.UpperRight, false); ms.rectTransform.Band(true, -108, 34);
            var back = Ui.Img("BarBack", _sheetBody, new Color(0, 0, 0, 0.45f), true); back.rectTransform.Band(true, -150, 18);
            var bar = Ui.Img("Bar", back.transform, Ui.Gold, true); bar.rectTransform.anchorMin = Vector2.zero; bar.rectTransform.anchorMax = new Vector2(lv >= Content.MaxLevel ? 1 : Mathf.Clamp01((lv - prev) / (float)Mathf.Max(1, next - prev)), 1); bar.rectTransform.offsetMin = bar.rectTransform.offsetMax = Vector2.zero;

            // What it does now and after the purchase.
            int n = BuyCount();
            float y = -186;
            Func<Func<string>, string> after = f =>
            {
                if (n <= 0) return null;
                Bump(h, stage, berth, n); string v = f(); Bump(h, stage, berth, -n); return v;
            };
            if (stage == Stage.Berth)
            {
                Row(y, "Catch a trip", Format.Money(s.CatchOf(h, berth)), after(() => Format.Money(s.CatchOf(h, berth)))); y -= 68;
                Row(y, "Trip", s.TripOf(h, berth).ToString("0.#") + "s", after(() => s.TripOf(h, berth).ToString("0.#") + "s")); y -= 68;
                Row(y, "Lands", Format.Money(s.BerthRate(h, berth)) + "/s", after(() => Format.Money(s.BerthRate(h, berth)) + "/s")); y -= 68;
            }
            else if (stage == Stage.Dock)
            {
                Row(y, "Carries", Format.Money(s.DockRate(h)) + "/s", after(() => Format.Money(s.DockRate(h)) + "/s")); y -= 68;
                Row(y, "Haulers", h.haulers.Count.ToString(), after(() => Content.CrewAt(h.dock).ToString())); y -= 68;
                Row(y, "The boats land", Format.Money(s.FleetRate(h, true)) + "/s", null); y -= 68;
            }
            else if (stage == Stage.Hall)
            {
                Row(y, "Works", Format.Money(s.HallRate(h)) + "/s", after(() => Format.Money(s.HallRate(h)) + "/s")); y -= 68;
                Row(y, "Comes in", Format.Money(Math.Min(s.FleetRate(h, true), s.DockRate(h))) + "/s", null); y -= 68;
            }
            else
            {
                Row(y, "Sells", Format.Money(s.MarketRate(h)) + "/s", after(() => Format.Money(s.MarketRate(h)) + "/s")); y -= 68;
                Row(y, "Trucks", h.trucks.Count.ToString(), after(() => Content.CrewAt(h.market).ToString())); y -= 68;
                Row(y, "Today's favourite", Content.Goods[s.GoodsOfTheDay].Name + " +50%", null); y -= 68;
            }

            // The hall's lines.
            if (stage == Stage.Hall)
            {
                y -= 6;
                for (int g = 0; g < Content.Goods.Length; g++)
                {
                    int k = g; var def = Content.Goods[g];
                    bool owned = s.OwnsGoods(h, g), running = h.goods == g, canBuy = !owned && s.OwnsGoods(h, g - 1 < 0 ? 0 : g - 1);
                    if (!owned && !canBuy) continue;
                    var row = Ui.Img("Line", _sheetBody, running ? new Color(0.16f, 0.4f, 0.26f, 0.9f) : new Color(0, 0, 0, 0.22f), true); row.rectTransform.Band(true, y, 76);
                    string worth = "x" + s.GoodsValue(h, g).ToString("0.##") + (g == s.GoodsOfTheDay ? " today" : "") + (def.Speed < 1 ? " · " + Mathf.RoundToInt((float)def.Speed * 100) + "% pace" : "");
                    var nm = Ui.Label("N", row.transform, def.Name, 30, Color.white, TextAnchor.UpperLeft); nm.rectTransform.Fill(); nm.rectTransform.offsetMin = new Vector2(18, 0); nm.rectTransform.offsetMax = new Vector2(0, -6);
                    var w = Ui.Label("W", row.transform, worth, 24, g == s.GoodsOfTheDay ? Ui.Gold : Ui.Dim, TextAnchor.LowerLeft, false); w.rectTransform.Fill(); w.rectTransform.offsetMin = new Vector2(18, 8);
                    if (running) { var on = Ui.Label("On", row.transform, "RUNNING", 26, Ui.Green, TextAnchor.MiddleRight); on.rectTransform.Fill(); on.rectTransform.offsetMax = new Vector2(-18, 0); }
                    else
                    {
                        double cost = s.GoodsCost(h, g);
                        var b = Ui.Button("B", row.transform, owned ? "RUN" : Format.Money(cost), owned ? Ui.Blue : Ui.Green, () =>
                        {
                            if (owned ? S.SetGoods(k) : S.BuyGoods(k)) { Sfx.Play(owned ? "confirm" : "build", 0.7f); GameRoot.Instance.Save(); }
                            else { Sfx.Play("error", 0.5f); Toast("Not enough cash yet"); }
                        }, 30);
                        ((RectTransform)b.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(170, 60));
                        if (!owned) { var img = b.GetComponent<Image>(); _live.Add(() => img.color = H.cash >= cost ? Ui.Green : Ui.Off); }
                    }
                    y -= 82;
                }
            }

            // The skipper.
            if (stage != Stage.Hall)
            {
                bool has = s.HasSkipper(h, stage, berth);
                var row = Ui.Img("Skipper", _sheetBody, has ? new Color(0.16f, 0.4f, 0.26f, 0.9f) : new Color(0, 0, 0, 0.22f), true); row.rectTransform.Band(true, y - 6, 84);
                string who = stage == Stage.Berth ? "Skipper" : stage == Stage.Dock ? "Dock foreman" : "Market manager";
                var nm = Ui.Label("N", row.transform, who, 30, Color.white, TextAnchor.UpperLeft); nm.rectTransform.Fill(); nm.rectTransform.offsetMin = new Vector2(18, 0); nm.rectTransform.offsetMax = new Vector2(0, -8);
                var w = Ui.Label("W", row.transform, has ? "Works without a tap" : "Hire to work without tapping", 24, Ui.Dim, TextAnchor.LowerLeft, false); w.rectTransform.Fill(); w.rectTransform.offsetMin = new Vector2(18, 10);
                if (!has)
                {
                    double cost = s.SkipperCost(h, stage, berth);
                    var b = Ui.Button("Hire", row.transform, cost <= 0 ? "FREE" : Format.Money(cost), Ui.Green, () =>
                    {
                        if (S.HireSkipper(stage, berth)) { Sfx.Play("confirm", 0.8f); Toast(who + " hired"); GameRoot.Instance.Save(); }
                        else { Sfx.Play("error", 0.5f); Toast("Not enough cash yet"); }
                    }, 30);
                    ((RectTransform)b.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(170, 64));
                    var img = b.GetComponent<Image>(); _live.Add(() => img.color = H.cash >= S.SkipperCost(H, stage, berth) ? Ui.Green : Ui.Off);
                }
                y -= 92;
            }

            // How many levels, and the button.
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                var b = Ui.Button("Mode" + i, _sheetBody, BuyNames[i], _buyMode == i ? Ui.Blue : new Color(0.24f, 0.29f, 0.36f), () => { _buyMode = k; RefreshSheet(); }, 30);
                ((RectTransform)b.transform).Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(i * 129, 132), new Vector2(121, 60));
            }
            _upgrade = Ui.Button("Upgrade", _sheetBody, null, Ui.Green, DoUpgrade, 44);
            ((RectTransform)_upgrade.transform).Band(false, 0, 120);
            _upgradeLabel = Ui.Label("L", _upgrade.transform, "", 40, Color.white, TextAnchor.UpperCenter); _upgradeLabel.rectTransform.Fill(); _upgradeLabel.rectTransform.offsetMax = new Vector2(0, -14);
            _upgradeCost = Ui.Label("C", _upgrade.transform, "", 34, Color.white, TextAnchor.LowerCenter); _upgradeCost.rectTransform.Fill(); _upgradeCost.rectTransform.offsetMin = new Vector2(0, 20);
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

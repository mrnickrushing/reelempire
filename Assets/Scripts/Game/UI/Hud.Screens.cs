using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>The full screens behind the round buttons in the bottom corners: the species book, the awards, the
    /// shop and the settings (sound, reminders, the account, Game Center, starting over).</summary>
    public sealed partial class Hud
    {
        enum Page { None, Book, Awards, Shop, Settings, Harbours, Today, Crew, Season, Derby, Orders, Research, Guide, Collections }
        Page _screen; string _bookHarbour; RectTransform _screenRoot, _list; float _listY;
        readonly List<Image> _navDots = new List<Image>();

        readonly Dictionary<string, RectTransform> _nav = new Dictionary<string, RectTransform>();
        readonly Dictionary<string, float> _navPop = new Dictionary<string, float>();

        void BuildNav()
        {
            _nav["book"] = NavButton(new Vector2(0, 0), new Vector2(84, 16), "BOOK", Resources.Load<Sprite>("UI/Species/lake_golden_koi"), Color.white, () => Open(Page.Book), false);
            _nav["awards"] = NavButton(new Vector2(0, 0), new Vector2(226, 16), "AWARDS", Ui.Art("star"), Color.white, () => Open(Page.Awards), true);
            _nav["today"] = NavButton(new Vector2(0, 0), new Vector2(368, 16), "TODAY", Ui.Ic("calendar"), Ui.Ink, () => Open(Page.Today), true);
            _nav["angling"] = NavButton(new Vector2(0, 0), new Vector2(84, 176), "FISH", Resources.Load<Sprite>("UI/Species/lake_northern_pike"), Color.white, OpenAngling, false);
            _nav["crew"] = NavButton(new Vector2(1, 0), new Vector2(-368, 16), "CREW", Resources.Load<Sprite>("UI/Faces/crew_brannock"), Color.white, () => Open(Page.Crew), true);
            NavButton(new Vector2(1, 0), new Vector2(-226, 16), "SHOP", Ui.Art("gem"), Color.white, () => Open(Page.Shop), false);
            NavButton(new Vector2(1, 0), new Vector2(-84, 16), "MORE", Ui.Ic("gear"), Ui.Ink, () => Open(Page.Settings), false);
            _screenRoot = Ui.Rect("Screen", _root); _screenRoot.Fill();
            // What is already open in this save is not news.
            foreach (var f in Sim.Features) if (S.Unlocked(f) && !S.State.seenFeatures.Contains(f)) S.State.seenFeatures.Add(f);
        }

        RectTransform NavButton(Vector2 anchor, Vector2 pos, string label, Sprite icon, Color tint, Action tap, bool dot)
        {
            var b = Ui.Button(label, _root, null, new Color(0, 0, 0, 0), tap, 30);
            var rt = (RectTransform)b.transform; rt.Place(anchor, new Vector2(0.5f, 0), pos + new Vector2(0, 34), new Vector2(118, 126));
            var img = b.GetComponent<Image>(); img.sprite = Ui.Art("badge"); img.type = Image.Type.Simple; img.color = Color.white;
            var ic = Ui.Img("I", rt, tint); ic.sprite = icon; ic.preserveAspect = true; ic.rectTransform.Fill(24); ic.rectTransform.offsetMin = new Vector2(24, 32);
            var l = Ui.Label("L", rt, label, 26, Color.white); l.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, 4), new Vector2(170, 34)); l.Stroke(2f);
            if (dot)
            {
                var d = Ui.ArtImg("Dot", rt, "badge", Ui.Coral, 0); d.rectTransform.Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-14, -16), new Vector2(40, 42));
                _navDots.Add(d);
            }
            return rt;
        }

        void UpdateNav()
        {
            var s = S;
            // The dots are made in the order of the buttons that carry one: AWARDS, TODAY, CREW.
            if (_navDots.Count > 0) { bool claim = s.AchievementsToClaim > 0 || s.TrophiesWaiting > 0; if (_navDots[0].gameObject.activeSelf != claim) _navDots[0].gameObject.SetActive(claim); }
            if (_navDots.Count > 1) { bool today = s.TodayWaiting; if (_navDots[1].gameObject.activeSelf != today) _navDots[1].gameObject.SetActive(today); }
            if (_navDots.Count > 2) { bool chest = s.ChestFree; if (_navDots[2].gameObject.activeSelf != chest) _navDots[2].gameObject.SetActive(chest); }
            // Each part of the game appears when the harbour has grown into it, with a word about what it is.
            foreach (var kv in _nav)
            {
                bool open = s.Unlocked(kv.Key);
                if (kv.Value.gameObject.activeSelf != open) kv.Value.gameObject.SetActive(open);
                float pop; _navPop.TryGetValue(kv.Key, out pop);
                if (pop > 0) { pop = Mathf.Max(0, pop - Time.unscaledDeltaTime); _navPop[kv.Key] = pop; }
                kv.Value.localScale = Vector3.one * (1 + 0.18f * Mathf.Abs(Mathf.Sin(pop * 6)) * Mathf.Clamp01(pop));
            }
            if (_boostRoot != null && _boostRoot.gameObject.activeSelf != s.Unlocked("boost")) _boostRoot.gameObject.SetActive(s.Unlocked("boost"));
            if (ModalOpen || ScreenOpen || s.State.tutorialStep == 0) return;
            foreach (var f in Sim.Features)
            {
                if (_toast != null) break;       // let the last piece of news be read first
                if (!s.Unlocked(f) || s.State.seenFeatures.Contains(f)) continue;
                var lesson = LessonFor(f);
                if (lesson != null && (SheetOpen || AnglingOpen)) break;      // Marta waits until the panel is put away
                s.State.seenFeatures.Add(f);
                Sfx.Play("star", 0.7f); Haptics.Success();
                if (lesson != null) ShowLesson(lesson, true); else Toast(Sim.FeatureNews(f));
                if (_nav.ContainsKey(f)) _navPop[f] = 4; else if (f == "voyages" || f == "derby" || f == "season") _navPop["today"] = 4;
                break;      // one piece of news at a time
            }
            if (s.StarterOfferDue && !SheetOpen) OfferStarter();
        }

        public bool ScreenOpen { get { return _screen != Page.None; } }

        void Open(Page s)
        {
            CloseSheet();
            _screen = s; _listY = 0;
            if (s == Page.Book && _bookHarbour == null) _bookHarbour = H.id;
            Sfx.Play("open", 0.5f);
            RefreshScreen();
        }

        void CloseScreen() { _screen = Page.None; Ui.Clear(_screenRoot); Sfx.Play("close", 0.4f); }

        /// <summary>The frame every screen shares: a dimmed game, a paper panel, the title on a ribbon, a close button and a
        /// scrolling list. Returns the panel; <see cref="_list"/> is where rows go.</summary>
        static readonly Dictionary<string, string> About = new Dictionary<string, string>
        {
            { "TODAY", "Free things to collect each day: a gift, three tasks, and boats you can send away for cash and gems." },
            { "THE CREW", "A hand on duty speeds up their part of the harbour and has a skill to call on from the main screen." },
            { "WEEKEND DERBY", "Every fish landed this weekend scores points. Reach the marks for gems and beat the other skippers." },
            { "HARBOURS", "Your waters. Open the next one, or renovate this one to earn more from the start." },
            { "SPECIES BOOK", "Every kind of fish you've landed. A new one pays gems; a full page earns 10% more in that water." },
            { "AWARDS", "Prizes for things you've done in your harbours. Tap a green one to take it." },
            { "SHOP", "Gems speed things up. Everything here is optional: the whole game can be played free." },
            { "SETTINGS", "Sound, reminders, your account and help." },
            { "RESEARCH", "The chart room turns what the harbour learns into lasting improvements. Each branch is learned in order." },
            { "HOW IT WORKS", "Marta's notes on every part of the game. Tap READ to hear one again." },
            { "COLLECTIONS", "Small sets of fish to land. Each one done puts something in your harbour for good." },
            { "ORDERS", "The town wants fish. Fill an order for a big payment and gems on top of what the fish sell for." },
        };

        RectTransform Frame(string title, float listTop = 70)
        {
            if (_list != null) _listY = _list.anchoredPosition.y;
            Ui.Clear(_screenRoot);
            var dim = Ui.Img("Dim", _screenRoot, new Color(0.05f, 0.15f, 0.25f, 0.62f)); dim.raycastTarget = true; dim.rectTransform.Fill();
            dim.gameObject.AddComponent<Button>().onClick.AddListener(CloseScreen);
            var panel = Ui.ArtImg("Panel", _screenRoot, "panel", Color.white, 60); panel.raycastTarget = true;
            var p = panel.rectTransform; p.anchorMin = new Vector2(0.5f, 0); p.anchorMax = new Vector2(0.5f, 1); p.pivot = new Vector2(0.5f, 0.5f); p.offsetMin = new Vector2(-760, 24); p.offsetMax = new Vector2(760, -70);
            var ribbon = Ui.Ribbon("Ribbon", p, Ui.Coral); ribbon.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -6), new Vector2(620, 116));
            var t = Ui.Label("Title", ribbon.transform, title, 52, Color.white); t.rectTransform.Fill(); t.rectTransform.offsetMin = new Vector2(40, 24); t.rectTransform.offsetMax = new Vector2(-40, 0); t.Stroke(2.5f);
            var x = Ui.Button("X", p, "✕", Ui.Coral, CloseScreen, 34); ((RectTransform)x.transform).Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-26, -30), new Vector2(84, 84));

            // One plain line under the title saying what the page is for.
            string about; string key = title.StartsWith("SHOP") ? "SHOP" : title;
            if (About.TryGetValue(key, out about))
            {
                var ab = Ui.Label("About", p, about, 28, Ui.Dim, TextAnchor.MiddleCenter, false);
                ab.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -listTop + 8), new Vector2(1380, 40));
                listTop += 44;
            }
            var view = Ui.Rect("View", p); view.Fill(); view.offsetMin = new Vector2(34, 44); view.offsetMax = new Vector2(-34, -listTop);
            view.gameObject.AddComponent<RectMask2D>();
            var catcher = view.gameObject.AddComponent<Image>(); catcher.color = new Color(0, 0, 0, 0); catcher.raycastTarget = true;
            _list = Ui.Rect("List", view); _list.anchorMin = new Vector2(0, 1); _list.anchorMax = new Vector2(1, 1); _list.pivot = new Vector2(0.5f, 1); _list.offsetMin = _list.offsetMax = Vector2.zero;
            var sr = view.gameObject.AddComponent<ScrollRect>(); sr.content = _list; sr.horizontal = false; sr.viewport = view; sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 40;
            return p;
        }

        void EndList(float height) { _list.sizeDelta = new Vector2(0, height); _list.anchoredPosition = new Vector2(0, Mathf.Max(0, _listY)); }

        /// <summary>A row: a picture, a name and what it is, and a button (or a word where the button would be).</summary>
        float Entry(float y, Sprite icon, Color iconTint, string name, string blurb, string button, Color colour, Action tap, string status = null, bool lit = false)
        {
            const float H = 122;
            var row = Ui.ArtImg("Row", _list, "card", lit ? new Color(0.72f, 0.93f, 0.76f) : Ui.Card, 26); row.rectTransform.Band(true, y, H - 10);
            if (icon != null) { var ic = Ui.Img("I", row.transform, iconTint); ic.sprite = icon; ic.preserveAspect = true; ic.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(84, 84)); }
            var n = Ui.Label("N", row.transform, name, 34, Ui.Ink, TextAnchor.UpperLeft); n.rectTransform.Fill(); n.rectTransform.offsetMin = new Vector2(124, 0); float right = button != null ? -330 : status != null ? -200 : -24; n.rectTransform.offsetMax = new Vector2(right, -12);
            var b = Ui.Label("B", row.transform, blurb, 25, Ui.Dim, TextAnchor.LowerLeft, false); b.rectTransform.Fill(); b.rectTransform.offsetMin = new Vector2(124, 12); b.rectTransform.offsetMax = new Vector2(right, -50);
            b.resizeTextForBestFit = true; b.resizeTextMinSize = 17; b.resizeTextMaxSize = 25; b.verticalOverflow = VerticalWrapMode.Truncate;
            if (button != null)
            {
                var btn = Ui.Button("B", row.transform, button, tap == null ? Ui.Off : colour, tap, 32);
                ((RectTransform)btn.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 2), new Vector2(300, 88));
            }
            else if (status != null) { var st = Ui.Label("S", row.transform, status, 30, new Color(0.12f, 0.62f, 0.29f), TextAnchor.MiddleRight); st.rectTransform.Fill(); st.rectTransform.offsetMax = new Vector2(-30, 0); }
            return y - H;
        }

        /// <summary>A second, smaller button on the row just added (left of its main one).</summary>
        void SecondButton(string text, Color colour, Action tap)
        {
            var row = _list.GetChild(_list.childCount - 1);
            var b = row.Find("B"); if (b != null && b.GetComponent<Text>() != null) ((RectTransform)b).offsetMax = new Vector2(-520, -50);
            var n = row.Find("N"); if (n != null) ((RectTransform)n).offsetMax = new Vector2(-520, -12);
            var btn = Ui.Button("B2", row, text, colour, tap, 30);
            ((RectTransform)btn.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-330, 2), new Vector2(180, 88));
        }

        float Heading(float y, string text)
        {
            var l = Ui.Label("H", _list, text, 30, Ui.Dim, TextAnchor.LowerLeft); l.rectTransform.Band(true, y, 50, 8);
            return y - 58;
        }

        void RefreshScreen()
        {
            switch (_screen)
            {
                case Page.Book: ShowBook(); break;
                case Page.Awards: ShowAwards(); break;
                case Page.Shop: ShowShop(); break;
                case Page.Settings: ShowSettings(); break;
                case Page.Harbours: ShowHarbours(); break;
                case Page.Today: ShowToday(); break;
                case Page.Crew: ShowCrew(); break;
                case Page.Season: ShowSeason(); break;
                case Page.Derby: ShowDerby(); break;
                case Page.Orders: ShowOrders(); break;
                case Page.Collections: ShowCollections(); break;
                case Page.Research: ShowResearch(); break;
                case Page.Guide: ShowGuide(); break;
            }
        }

        void ShowToday()
        {
            var s = S;
            Frame("TODAY");
            float y = 0;
            // The gift.
            int gi = s.GiftIndex; var gift = Sim.Gifts[gi];
            y = Entry(y, Ui.Art("star"), Color.white, "Day " + (gi + 1) + " gift: " + gift.Name, s.GiftReady ? "A gift for every day you play: day 7 is 60 gems. Miss a day and nothing is lost." : "Taken. The next one is ready in " + Format.Time(s.GiftIn) + ".",
                s.GiftReady ? "TAKE IT" : null, Ui.Gold, s.GiftReady ? (Action)(() => { var g = S.ClaimGift(); if (g != null) { Sfx.Play("claim", 0.9f); Toast("Today's gift: " + g.Name); GameRoot.Instance.Save(); ShowToday(); } }) : null, s.GiftReady ? null : "TAKEN", s.GiftReady);

            long sHave; int sNeed; s.SeasonProgress(out sHave, out sNeed);
            if (s.Unlocked("season")) y = Entry(y, Ui.Art("star"), Color.white, Sim.SeasonNames[s.Season] + " season: tier " + s.SeasonTier + " of " + Sim.SeasonTiers,
                (sNeed > 0 ? sHave + " / " + sNeed + " points to the next tier. " : "Every tier reached. ") + (s.SeasonWaiting > 0 ? s.SeasonWaiting + " reward" + (s.SeasonWaiting > 1 ? "s" : "") + " to take." : "Ends in " + Format.Time(s.SeasonLeft) + "."),
                "OPEN", Ui.Blue, () => Open(Page.Season), null, s.SeasonWaiting > 0);
            if (s.Unlocked("orders")) y = Entry(y, Ui.Ic("box"), Ui.Ink, "Orders from the town", s.OrdersReady > 0 ? s.OrdersReady + " ready to hand over." : s.State.orders.Count + " on the board. Filling one pays well.",
                "OPEN", Ui.Blue, () => Open(Page.Orders), null, s.OrdersReady > 0);
            if (s.Unlocked("research")) y = Entry(y, Ui.Ic("medal"), Ui.Ink, "Research at the chart room", s.State.notes + (s.State.notes == 1 ? " note" : " notes") + " in hand." + (s.ResearchReady ? " Something can be learned." : ""),
                "OPEN", Ui.Blue, () => Open(Page.Research), null, s.ResearchReady);
            if (s.Unlocked("book")) { var town = s.NextTown; int cw = s.CollectionsWaiting;
                y = Entry(y, Ui.Ic("target"), Ui.Ink, "Collections and the town", (cw > 0 ? cw + " collection" + (cw > 1 ? "s" : "") + " done: claim " + (cw > 1 ? "them" : "it") + " for your harbour. " : s.State.collections.Count + " of " + Sim.Collections.Length + " collections done. ") + (town != null ? "Next in town: " + town.Name.ToLowerInvariant() + " after " + town.Orders + " orders (" + s.State.ordersDone + ")." : "The town is all built."),
                    "OPEN", Ui.Blue, () => Open(Page.Collections), null, cw > 0); }
            bool prize = s.DerbyPrizeWaiting >= 0;
            if (s.Unlocked("derby")) y = Entry(y, Ui.Ic("trophy"), Ui.Ink, "Weekend derby", prize ? "The last derby is over: your prize is waiting." : s.DerbyOn ? "On now: " + s.DerbyScore.ToString("N0") + " points, place " + s.DerbyRankNow + " of 50. Ends in " + Format.Time(s.DerbyLeft) + "." : "Friday to Sunday, every fish landed scores. The next one starts in " + Format.Time(s.DerbyIn) + ".",
                "OPEN", Ui.Blue, () => Open(Page.Derby), null, prize || s.DerbyOn);

            if (s.Unlocked("voyages")) y = Heading(y, "VOYAGES: send a spare boat out (" + s.State.voyages.Count + " of " + s.VoyageSlots + " away)  ·  " + s.State.grounds.Count + " of " + Sim.Grounds.Length + " grounds charted");
            else y = Heading(y, "VOYAGES open with your fourth dock  ·  THE DERBY with your fifth  ·  THE SEASON with your sixth");
            for (int i = 0; i < s.State.voyages.Count; i++)
            {
                var v = s.State.voyages[i]; var d = Sim.Voyages[v.kind]; int k = i; double left = s.VoyageLeft(v);
                y = Entry(y, Resources.Load<Sprite>("UI/Species/coast_mackerel"), Color.white, d.Name, left > 0 ? "Back in " + Format.Time(left) : "Back with a full hold: +" + Format.Money(s.VoyageCash(d)) + " and " + d.Gems + " gems",
                    left > 0 ? null : "UNLOAD", Ui.Green, left > 0 ? null : (Action)(() => { double cash = S.CollectVoyage(k); if (cash > 0) { Sfx.Play("coin", 0.9f); Toast("+" + Format.Money(cash) + "  +" + d.Gems + " gems"); GameRoot.Instance.Save(); ShowToday(); if (S.NewGround != null) ShowGround(S.NewGround); } }), left > 0 ? "AT SEA" : null, left <= 0);
            }
            // The second slot, shown where it would be, for someone who hasn't got it.
            if (s.Unlocked("voyages") && !s.Owns("reelempire.fleet") && Store.Available)
            {
                var fp = Sim.Product("reelempire.fleet");
                y = Entry(y, Ui.Ic("lock"), Ui.Dim, "Second boat slot", s.PurchaseBenefit(fp.Id) ?? fp.Blurb, "SHOP", Ui.Off, () => Open(Page.Shop), null);
            }
            if (s.Unlocked("voyages") && s.State.voyages.Count < s.VoyageSlots)
                for (int i = 0; i < Sim.Voyages.Length; i++)
                {
                    var d = Sim.Voyages[i]; int k = i;
                    y = Entry(y, Ui.Ic("compass"), Ui.Ink, d.Name + "  <size=24><color=#5A6E82>" + s.FoundOn(i) + "/3 grounds" + (s.RouteCharted(i) ? ", charted +25%" : "") + "</color></size>", d.Blurb + ": about +" + Format.Money(s.VoyageCash(d)) + " and " + d.Gems + " gems", "SEND  " + Short(s.VoyageSeconds(d)), Ui.Blue,
                        () => { if (S.StartVoyage(k)) { Sfx.Play("dispatch", 0.7f); Toast(d.Name + ": the boat is away"); Reminders.AskOnce(); GameRoot.Instance.Save(); ShowToday(); } });
                }

            y = Heading(y, "TODAY'S TASKS");
            for (int i = 0; i < Sim.Tasks.Length; i++)
            {
                var t = Sim.Tasks[i]; int k = i; bool done = s.TaskDone(i), claimed = s.TaskClaimed(i);
                y = Entry(y, Ui.Ic("target"), Ui.Ink, t.Text, Format.Money(s.TaskHave(i)) + " / " + Format.Money(t.Need), claimed ? null : "+" + t.Gems + " GEMS", Ui.Green,
                    done && !claimed ? (Action)(() => { if (S.ClaimTask(k)) { Sfx.Play("claim", 0.8f); GameRoot.Instance.Save(); ShowToday(); } }) : null, claimed ? "DONE" : null, done && !claimed);
            }

            if (s.Unlocked("harbours")) y = Heading(y, "SKIPPER'S LOGBOOK: opens as your renown grows (now " + s.State.renown + ")");
            foreach (var p in Sim.LogbookPages)
            {
                if (!s.Unlocked("harbours")) break;
                bool has = s.Logbook(p.Id);
                y = Entry(y, Ui.Ic(has ? "medal" : "lock"), Ui.Ink, p.Name, p.Blurb, null, Ui.Off, null, has ? "YOURS" : "AT " + p.Renown + " RENOWN", has);
            }
            EndList(-y);
        }

        /// <summary>A length of time in one word: 30m, 2h.</summary>
        static string Short(double seconds) { return seconds >= 3600 ? Math.Round(seconds / 3600) + "h" : Math.Round(seconds / 60) + "m"; }

        static readonly string[] StageNames = { "Boats", "Porters", "Hall", "Market" };
        static readonly string[] CrewRarity = { "COMMON", "", "RARE", "EPIC", "LEGENDARY" };

        void ShowCrew()
        {
            var s = S;
            Frame("THE CREW");
            float y = 0;
            // The chest: free on a timer, or for gems; the odds are printed.
            Action<CrewDef> got = card =>
            {
                if (card == null) { Sfx.Play("error", 0.5f); Toast("Not enough gems"); return; }
                var own = S.Owned(card.Id);
                Sfx.Play("star", 0.9f);
                Toast(card.Name + " (" + CrewRarity[(int)card.Rarity].ToLowerInvariant() + ")" + (own.level > 1 || own.copies > 0 ? ": now level " + own.level : " signs on!"));
                GameRoot.Instance.Save(); ShowCrew();
            };
            y = Entry(y, Ui.Art("star"), Color.white, "Crew chest", s.ChestFree ? "One card. Your free chest is ready." : "One card. The next free chest is ready in " + Format.Time(s.ChestIn) + ".",
                s.ChestFree ? "OPEN FREE" : Sim.ChestGems + " GEMS", s.ChestFree ? Ui.Gold : Ui.Grape, () => got(S.OpenChest(!S.ChestFree)), null, s.ChestFree);
            var odds = Ui.Label("Odds", _list, "Odds: Common " + Sim.ChestOdds[0] + "%  ·  Rare " + Sim.ChestOdds[1] + "%  ·  Epic " + Sim.ChestOdds[2] + "%  ·  Legendary " + Sim.ChestOdds[3] + "%.  The 10th chest without an Epic or better always holds one.", 24, Ui.Dim, TextAnchor.MiddleLeft, false);
            odds.rectTransform.Band(true, y, 44, 10); y -= 52;
            y = Entry(y, Ui.Ic("medal"), Ui.Ink, "Recruit tokens: " + s.State.tokens,
                "Every chest brings one; a derby placing brings 1 to 5. Spend them on the card you want: Common " + Sim.RecruitCost(Sim.Crew[0]) + ", Rare 15, Epic 40, Legendary 100.", null, Ui.Off, null, null);

            // Fleet presets: the baits and crew for a purpose, put back in one tap.
            y = Heading(y, "FLEET PRESETS: baits at every dock and the crew on duty");
            for (int i = 0; i < Sim.PresetNames.Length; i++)
            {
                int k = i; bool saved = s.PresetSaved(i);
                y = Entry(y, Ui.Ic("compass"), Ui.Ink, Sim.PresetNames[i], s.PresetText(i), "USE", Ui.Green, saved ? (Action)(() =>
                {
                    int n = S.UsePreset(k);
                    Telemetry.Log("Preset.used", ("Preset.name", Sim.PresetNames[k]), ("Preset.changes", n.ToString()));
                    Sfx.Play("confirm", 0.7f); Toast(Sim.PresetNames[k] + (n > 0 ? ": " + n + (n == 1 ? " change" : " changes") : ": already set")); GameRoot.Instance.Save(); ShowCrew();
                }) : null);
                SecondButton("SAVE", Ui.Blue, () =>
                {
                    S.SavePreset(k); Telemetry.Log("Preset.saved", ("Preset.name", Sim.PresetNames[k]));
                    Sfx.Play("build", 0.6f); Toast(Sim.PresetNames[k] + " saved"); GameRoot.Instance.Save(); ShowCrew();
                });
            }

            for (int st = 0; st < 4; st++)
            {
                var stage = (Stage)st;
                y = Heading(y, StageNames[st].ToUpperInvariant() + (s.OnDuty(stage) == null ? ": nobody on duty" : ""));
                foreach (var d in Sim.Crew)
                {
                    if (d.Stage != stage) continue;
                    var own = s.Owned(d.Id); bool duty = s.OnDuty(stage) == d; string id = d.Id;
                    string rarity = "<color=#" + ColorUtility.ToHtmlStringRGB(RarityInk[(int)d.Rarity]) + ">" + CrewRarity[(int)d.Rarity] + "</color>";
                    int cost = Sim.RecruitCost(d);
                    Action recruit = s.State.tokens >= cost ? (Action)(() =>
                    {
                        if (!S.Recruit(id)) return;
                        Telemetry.Log("Crew.recruited", ("Crew.id", id), ("Crew.cost", cost.ToString()));
                        var o = S.Owned(id); Sfx.Play("star", 0.9f);
                        Toast(d.Name + (o.level > 1 || o.copies > 0 ? ": a copy, " + o.copies + "/" + Sim.CopiesFor(o.level) + " to the next level" : " signs on!"));
                        GameRoot.Instance.Save(); ShowCrew();
                    }) : null;
                    if (own == null)
                        y = Entry(y, Resources.Load<Sprite>("UI/Faces/crew_" + d.Id), new Color(0.086f, 0.196f, 0.31f, 0.3f), "???  " + rarity, d.Role + ": +" + Mathf.RoundToInt((float)Sim.CrewBonus(d, 1) * 100) + "% " + StageNames[st].ToLowerInvariant() + "  ·  Skill: " + Sim.SkillOf(d).Name + ", " + Sim.SkillOf(d).Blurb, "RECRUIT  " + cost, Ui.Grape, recruit);
                    else if (duty && own.level < Sim.CrewMaxLevel)
                    {
                        y = Entry(y, Resources.Load<Sprite>("UI/Faces/crew_" + d.Id), Color.white, d.Name + "  " + rarity + "  <color=#1E9E4A>ON DUTY</color>",
                            "Skill: " + Sim.SkillOf(d).Name + ", " + Sim.SkillOf(d).Blurb + " for " + Format.Time(Sim.SkillSeconds(d, own.level)) + ".\n" + d.Role + "  ·  level " + own.level + "  ·  +" + Mathf.RoundToInt((float)Sim.CrewBonus(d, own.level) * 100) + "% " + StageNames[st].ToLowerInvariant() + "  ·  " + own.copies + "/" + Sim.CopiesFor(own.level) + " copies to level " + (own.level + 1),
                            "A COPY  " + cost, Ui.Grape, recruit, null, true);
                        continue;
                    }
                    else
                        y = Entry(y, Resources.Load<Sprite>("UI/Faces/crew_" + d.Id), Color.white, d.Name + "  " + rarity,
                            "Skill: " + Sim.SkillOf(d).Name + ", " + Sim.SkillOf(d).Blurb + " for " + Format.Time(Sim.SkillSeconds(d, own.level)) + ".\n" + d.Role + "  ·  level " + own.level + "  ·  +" + Mathf.RoundToInt((float)Sim.CrewBonus(d, own.level) * 100) + "% " + StageNames[st].ToLowerInvariant() + (own.level < Sim.CrewMaxLevel ? "  ·  " + own.copies + "/" + Sim.CopiesFor(own.level) + " copies to level " + (own.level + 1) : "  ·  top level"),
                            duty ? null : "PUT ON DUTY", Ui.Blue, duty ? null : (Action)(() => { if (S.Assign(stage, id)) { Sfx.Play("confirm", 0.7f); GameRoot.Instance.Save(); ShowCrew(); } }), duty ? "ON DUTY" : null, duty);
                }
            }
            EndList(-y);
        }

        void ShowSeason()
        {
            var s = S;
            Frame(Sim.SeasonThemes[s.SeasonTheme].ToUpperInvariant());
            float y = 0;
            long have; int need; s.SeasonProgress(out have, out need);
            var prem = Sim.Product("reelempire.season");
            y = Entry(y, Ui.Art("star"), Color.white, Sim.SeasonNames[s.Season] + " season  ·  tier " + s.SeasonTier + " of " + Sim.SeasonTiers + (need > 0 ? "  ·  " + have + " / " + need + " points" : ""),
                "Points come from playing: every level bought, new species, daily tasks, voyages and derby goals. Ends in " + Format.Time(s.SeasonLeft) + ".",
                s.SeasonPremium ? null : "PASS  " + Store.Price(prem), Ui.Green, s.SeasonPremium ? null : (Action)(() => Store.Buy(prem, (ok, msg) => { Toast(msg); if (ok) Sfx.Play("confirm"); if (_screen == Page.Season) ShowSeason(); })), s.SeasonPremium ? "PREMIUM" : null, s.SeasonPremium);
            if (!s.SeasonPremium) { var note = Ui.Label("N", _list, "The Season Pass adds the premium reward on every tier of this season, including the tiers you've already reached.", 24, Ui.Dim, TextAnchor.MiddleLeft, false); note.rectTransform.Band(true, y, 40, 10); y -= 48; }
            int tier = s.SeasonTier;
            for (int t = 0; t < Sim.SeasonTiers; t++)
            {
                int k = t; bool reached = t < tier;
                var free = Sim.SeasonRewardFor(t, false); var paid = Sim.SeasonRewardFor(t, true, s.SeasonTheme);
                bool cf = s.SeasonCanClaim(t, false), cp = s.SeasonCanClaim(t, true);
                const float H = 96;
                var row = Ui.ArtImg("Tier", _list, "card", cf || cp ? new Color(0.72f, 0.93f, 0.76f) : reached ? Ui.Card : new Color(0.93f, 0.9f, 0.84f), 26); row.rectTransform.Band(true, y, H - 8);
                var num = Ui.ArtImg("N", row.transform, "badge", reached ? Ui.Gold : Ui.Off, 0); num.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, 2), new Vector2(66, 70));
                var nl = Ui.Label("L", num.transform, (t + 1).ToString(), 30, Color.white); nl.rectTransform.Fill(); nl.rectTransform.offsetMin = new Vector2(0, 8); nl.Stroke(2f);
                // The free reward on the left half, the premium on the right.
                for (int side = 0; side < 2; side++)
                {
                    bool premium = side == 1; var r = premium ? paid : free; bool can = premium ? cp : cf; bool taken = s.SeasonClaimed(t, premium);
                    float x0 = premium ? 760 : 100;
                    var lab = Ui.Label("R", row.transform, (premium ? "<color=#7B61FF>PREMIUM</color>  " : "<color=#5A6E82>FREE</color>  ") + r.Text, 26, Ui.Ink, TextAnchor.MiddleLeft); lab.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(x0, 0), new Vector2(430, 60));
                    lab.resizeTextForBestFit = true; lab.resizeTextMinSize = 18; lab.resizeTextMaxSize = 26;
                    if (taken) { var d = Ui.Label("D", row.transform, "TAKEN", 22, new Color(0.12f, 0.62f, 0.29f), TextAnchor.MiddleRight); d.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(x0 + 640, 0), new Vector2(190, 50)); }
                    else if (can)
                    {
                        var b = Ui.Button("B", row.transform, "TAKE", Ui.Green, () => { var got = S.ClaimSeason(k, premium); if (got != null) { Sfx.Play("claim", 0.8f); Toast(got.Text); GameRoot.Instance.Save(); ShowSeason(); } }, 26);
                        ((RectTransform)b.transform).Place(new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(x0 + 640, 2), new Vector2(150, 62));
                    }
                    else if (premium && reached && !s.SeasonPremium) { var d = Ui.Label("D", row.transform, "WITH THE PASS", 20, Ui.Dim, TextAnchor.MiddleRight); d.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(x0 + 640, 0), new Vector2(200, 50)); }
                }
                y -= H;
            }
            EndList(-y);
        }

        void ShowDerby()
        {
            var s = S;
            Frame("WEEKEND DERBY");
            float y = 0;
            long waiting = s.DerbyPrizeWaiting;
            if (waiting >= 0)
                y = Entry(y, Ui.Ic("trophy"), Ui.Ink, "The last derby is over", "Take your prize for where you finished among the fifty skippers.", "TAKE PRIZE", Ui.Gold,
                    () => { int rank = S.ClaimDerbyPrize(); if (rank > 0) { Sfx.Play("star", 0.9f); Toast("You finished " + rank + (rank == 1 ? "st" : rank == 2 ? "nd" : rank == 3 ? "rd" : "th") + ": +" + Sim.DerbyPrize(rank) + " gems, +" + Sim.DerbyTokens(rank) + (Sim.DerbyTokens(rank) == 1 ? " recruit token" : " recruit tokens")); GameRoot.Instance.Save(); ShowDerby(); } }, null, true);
            if (!s.DerbyOn)
            {
                var next = Sim.FormatOf(s.DerbyIndex + 1);
                y = Entry(y, Ui.Ic("calendar"), Ui.Ink, "Next: the " + Sim.FormatNames[(int)next] + ", in " + Format.Time(s.DerbyIn), "Friday to Sunday night. " + Sim.FormatRules[(int)next] + " Five goals pay gems, and your place among fifty skippers pays a prize.", null, Ui.Off, null, "FRIDAY");
                y = Heading(y, "PRIZES: 1st 100 gems  ·  top 3: 60  ·  top 10: 30  ·  top 25: 15  ·  everyone: 5, and recruit tokens");
                EndList(-y); return;
            }
            y = Entry(y, Ui.Ic("trophy"), Ui.Ink, s.DerbyScore.ToString("N0") + " points  ·  place " + s.DerbyRankNow + " of 50", "Ends in " + Format.Time(s.DerbyLeft) + ".", null, Ui.Off, null, "ON NOW", true);
            y = Heading(y, Sim.FormatNames[(int)s.DerbyFormatNow].ToUpperInvariant() + ": " + Sim.FormatRules[(int)s.DerbyFormatNow]);
            y = Heading(y, "GOALS");
            for (int i = 0; i < Sim.DerbyGoals.Length; i++)
            {
                int k = i; bool done = s.DerbyGoalDone(i), claimed = s.DerbyGoalClaimed(i);
                long goal = s.DerbyGoal(i);
                y = Entry(y, Ui.Ic("target"), Ui.Ink, goal.ToString("N0") + " points", Math.Min(s.DerbyScore, goal).ToString("N0") + " / " + goal.ToString("N0"), claimed ? null : "+" + Sim.DerbyGoalGems[i] + " GEMS", Ui.Green,
                    done && !claimed ? (Action)(() => { if (S.ClaimDerbyGoal(k)) { Sfx.Play("claim", 0.8f); GameRoot.Instance.Save(); ShowDerby(); } }) : null, claimed ? "DONE" : null, done && !claimed);
            }
            y = Heading(y, "THE TABLE  (1st 100 gems + 5 tokens · top 3: 60 + 5 · top 10: 30 + 3 · top 25: 15 + 1 · everyone: 5 + 1)");
            var fair = Ui.Label("Fair", _list, "The other skippers are part of the game, not real players. For real ones, see the Game Center leaderboard in MORE.", 22, Ui.Dim, TextAnchor.MiddleLeft, false); fair.rectTransform.Band(true, y, 36, 10); y -= 44;
            double frac = 1 - s.DerbyLeft / (3 * 86400.0);
            var table = new List<KeyValuePair<string, long>>();
            for (int i = 0; i < Sim.DerbyRivals; i++) table.Add(new KeyValuePair<string, long>(Sim.RivalNames[i], s.RivalScore(s.DerbyIndex, i, frac)));
            string me = string.IsNullOrEmpty(s.State.playerName) ? "You" : s.State.playerName + " (you)";
            table.Add(new KeyValuePair<string, long>(me, s.DerbyScore));
            table.Sort((a, b) => b.Value != a.Value ? b.Value.CompareTo(a.Value) : (a.Key == me ? 1 : -1));
            for (int i = 0; i < table.Count; i++)
            {
                bool mine = table[i].Key == me;
                var row = Ui.ArtImg("Row", _list, "card", mine ? new Color(0.72f, 0.93f, 0.76f) : Ui.Card, 26); row.rectTransform.Band(true, y, 54);
                var a = Ui.Label("A", row.transform, (i + 1) + ".   " + table[i].Key, 28, Ui.Ink, TextAnchor.MiddleLeft); a.rectTransform.Fill(); a.rectTransform.offsetMin = new Vector2(24, 0);
                var b = Ui.Label("B", row.transform, table[i].Value.ToString("N0"), 28, Ui.Ink, TextAnchor.MiddleRight); b.rectTransform.Fill(); b.rectTransform.offsetMax = new Vector2(-24, 0);
                y -= 60;
            }
            EndList(-y);
        }

        /// <summary>The Deckhand's Pack, shown once, to someone who has come back on another day: never at launch, never
        /// on the first day, never twice.</summary>
        void OfferStarter()
        {
            var st = S.State; var prod = Sim.Product("reelempire.starter");
            if (st.starterOffered || S.Owns(prod.Id) || ModalOpen || !Store.Available) return;
            st.starterOffered = true;
            Telemetry.Log("Starter.offered", ("Starter.variant", S.StarterVariant));
            var card = Card(900, 560, "A HAND ON DECK?");
            var star = Ui.ArtImg("Star", card, "gem", Color.white, 0); star.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(60, 30), new Vector2(170, 176));
            var t = Ui.Label("T", card, "<b>" + prod.Name + "</b>\n250 gems, plus an extra porter and an extra truck in every harbour, for good.\n<color=#1E7A3E>" + S.PurchaseBenefit(prod.Id) + "</color>\n\nOne time only. You can always find it in the SHOP.", 30, Ui.Ink, TextAnchor.UpperLeft, false);
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 20; t.resizeTextMaxSize = 30;
            t.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(270, -100), new Vector2(580, 280));
            var buy = Ui.Button("Buy", card, "BUY  " + Store.Price(prod), Ui.Green, () => { CloseCard(); Store.Buy(prod, (ok, msg) => { Toast(msg); if (ok) { Sfx.Play("confirm"); Haptics.Success(); } }); }, 44);
            ((RectTransform)buy.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(215, 40), new Vector2(400, 116));
            var no = Ui.Button("No", card, "NOT NOW", Ui.Off, CloseCard, 40); ((RectTransform)no.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-215, 40), new Vector2(400, 116));
        }

        void ShowHarbours()
        {
            var s = S; var h = H;
            Frame("HARBOURS");
            float y = 0;
            y = Heading(y, "RENOWN " + s.State.renown + ":  +" + Mathf.RoundToInt((float)(s.State.renown * Sim.RenownEach * 100)) + "% income in every harbour");
            // Renovating the harbour on screen.
            string blocked = s.RenovationBlocked(h);
            if (h.renovations < Sim.MaxRenovations)
            {
                int gain = s.RenownWaiting;
                y = Entry(y, Ui.Art("star"), Color.white, "Renovate " + s.Def.Name + "  (x" + Format.Money(Math.Pow(2, h.renovations)) + " ▸ x" + Format.Money(Math.Pow(2, h.renovations + 1)) + ")",
                    "Start again from the Old Jetty, earning double here for good" + (gain > 0 ? ", and +" + gain + " renown" : "") + (h.renovations == 0 ? ", then pick what it specialises in." : ".") + (blocked != null ? "  " + blocked + "." : ""),
                    Format.Money(s.RenovationCost(h)), Ui.Gold, blocked == null ? (Action)AskRenovate : null);
            }
            else y = Entry(y, Ui.Art("star"), Color.white, s.Def.Name + " is fully renovated", "x32 income here, for good.", null, Ui.Off, null, "DONE", true);
            // A renovated harbour specialises: picked freely the first time, changed once a day after that.
            if (h.renovations > 0)
            {
                string focus = s.FocusOf(h); double wait = string.IsNullOrEmpty(focus) ? 0 : s.FocusChangeIn(h);
                y = Heading(y, "WHAT " + s.Def.Name.ToUpperInvariant() + " SPECIALISES IN" + (string.IsNullOrEmpty(focus) ? ": pick one" : wait > 0 ? "  (can change in " + Format.Time(wait) + ")" : "  (you can change it now)"));
                for (int i = 0; i < Sim.Focuses.Length; i++)
                {
                    string f = Sim.Focuses[i]; bool on = focus == f; int k = i;
                    y = Entry(y, Ui.Ic(i == 0 ? "target" : i == 1 ? "box" : i == 2 ? "compass" : "chart"), Ui.Ink, Sim.FocusNames[i], Sim.FocusBlurbs[i],
                        on ? null : string.IsNullOrEmpty(focus) ? "CHOOSE" : "CHANGE", Ui.Blue,
                        on || wait > 0 ? null : (Action)(() =>
                        {
                            if (!S.SetFocus(f)) return;
                            Telemetry.Log("Focus.chosen", ("Focus.kind", f), ("Focus.renovations", H.renovations.ToString()));
                            Sfx.Play("confirm", 0.8f); Toast(S.Def.Name + " now specialises in " + Sim.FocusNames[k].ToLowerInvariant()); GameRoot.Instance.Save(); ShowHarbours();
                        }), on ? "CHOSEN" : null, on);
                }
            }
            y = Heading(y, "YOUR WATERS");
            for (int i = 0; i < s.State.harbours.Count; i++)
            {
                var hh = s.State.harbours[i]; var d = s.DefOf(hh); int k = i; bool here = i == s.State.at;
                y = Entry(y, Resources.Load<Sprite>("UI/Species/" + Book.In(d.Id)[15].Id.Replace('.', '_')), Color.white, d.Name,
                    d.Water + "  ·  " + hh.berths.Count + " docks  ·  x" + Format.Money(Math.Pow(2, hh.renovations)) + "  ·  " + (s.IdleRate(hh) > 0 ? "+" + Format.Money(s.IdleRate(hh)) + "/s" : "needs skippers to earn by itself"),
                    here ? null : "SAIL THERE", Ui.Blue, here ? null : (Action)(() => { if (S.Travel(k)) { CloseScreen(); Toast("Welcome to " + d.Name); GameRoot.Instance.Save(); } }), here ? "YOU ARE HERE" : null, here);
            }
            var next = s.NextHarbour;
            if (next != null)
            {
                string why = s.NextHarbourBlocked;
                y = Entry(y, Ui.Ic("compass"), Ui.Ink, "Open " + next.Name, next.Blurb + (why != null ? "  " + why + "." : ""), Format.Money(s.NextHarbourCost), Ui.Green,
                    why == null ? (Action)(() => { if (S.OpenNextHarbour()) { CloseScreen(); Sfx.Play("star"); Toast("Welcome to " + next.Name + "!"); GameRoot.Instance.Save(); } }) : null);
            }
            EndList(-y);
        }

        void AskRenovate()
        {
            var s = S; var h = H; int gain = s.RenownWaiting;
            Confirm("RENOVATE?", s.Def.Name + " starts again from one jetty and a rowboat: its levels, docks, lines and cash go.\n\nFrom then on it earns x" + Format.Money(Math.Pow(2, h.renovations + 1)) + " for good" + (gain > 0 ? ", and you gain " + gain + " renown (+" + gain * 2 + "% income everywhere)" : "") + ". Gems, the book and your other harbours are kept.",
                "RENOVATE", Ui.Gold, () => { if (S.Renovate()) { Sfx.Play("star"); GameRoot.Instance.Save(); if (string.IsNullOrEmpty(S.FocusOf(H))) { Toast("Renovated! Now pick what " + S.Def.Name + " specialises in."); ShowHarbours(); } else { CloseScreen(); Toast("Renovated! Everything here now earns double."); } } });
        }

        public static Sprite SpeciesPicture(SpeciesDef sp) { return Resources.Load<Sprite>("UI/Species/" + sp.Id.Replace('.', '_')); }
        static readonly string[] RarityNames = { "COMMON", "UNCOMMON", "RARE", "EPIC", "LEGENDARY" };
        static readonly Color[] RarityInk = { new Color(0.42f, 0.5f, 0.58f), new Color(0.18f, 0.65f, 0.32f), new Color(0.1f, 0.55f, 0.85f), new Color(0.55f, 0.36f, 0.95f), new Color(0.95f, 0.6f, 0.05f) };

        void ShowBook()
        {
            var s = S;
            var p = Frame("SPECIES BOOK", 150);
            // A tab for each harbour: the ones not reached yet are shown but shut.
            for (int i = 0; i < Content.Harbours.Length; i++)
            {
                var d = Content.Harbours[i]; bool open = i < s.State.harbours.Count; string id = d.Id;
                var tab = Ui.Button("Tab", p, open ? d.Water.ToUpperInvariant() : "?", id == _bookHarbour ? Ui.Blue : Ui.Off, open ? (Action)(() => { _bookHarbour = id; _listY = 0; if (_list != null) _list.anchoredPosition = Vector2.zero; ShowBook(); }) : null, 26);
                ((RectTransform)tab.transform).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(40 + i * 204, -72), new Vector2(196, 66));
            }
            var all = Book.In(_bookHarbour);
            int trophyPct = Mathf.RoundToInt((float)((s.TrophyMult - 1) * 100));
            var count = Ui.Label("C", p, s.CaughtIn(_bookHarbour) + " / " + all.Count + (s.PageComplete(_bookHarbour) ? "  +10% here" : "") + (trophyPct > 0 ? "   +" + trophyPct + "%" : ""), 28, Ui.Ink, TextAnchor.MiddleRight);
            // Tiles, six across.
            const int cols = 6; const float w = 236, hgt = 250;
            for (int i = 0; i < all.Count; i++)
            {
                var sp = all[i]; var e = s.Entry(sp.Id); bool got = e != null;
                var tile = Ui.ArtImg("Tile", _list, "card", got ? Color.white : new Color(0.93f, 0.88f, 0.78f), 26);
                { var tsp = sp; tile.raycastTarget = true; tile.gameObject.AddComponent<Button>().onClick.AddListener(() => ShowSpecies(tsp)); }
                if (s.State.pinned == sp.Id) { var pin = Ui.ArtImg("Pin", tile.transform, "tag", Ui.Coral, 30); pin.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(6, 10), new Vector2(110, 40)); var pl = Ui.Label("P", pin.transform, "TARGET", 18, Color.white); pl.rectTransform.Fill(); pl.Stroke(1.5f); }
                tile.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(4 + i % cols * (w + 6), -(i / cols) * (hgt + 6)), new Vector2(w, hgt));
                var band = Ui.ArtImg("Band", tile.transform, "well", RarityInk[(int)sp.Rarity], 20); band.rectTransform.Band(true, -10, 34, 12);
                var rl = Ui.Label("R", band.transform, RarityNames[(int)sp.Rarity], 20, Color.white); rl.rectTransform.Fill();
                var pic = Ui.Img("Pic", tile.transform, got ? Color.white : new Color(0.086f, 0.196f, 0.31f, 0.35f)); pic.sprite = SpeciesPicture(sp); pic.preserveAspect = true;
                pic.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(124, 124));
                var n = Ui.Label("N", tile.transform, got ? sp.Name : "???", 26, Ui.Ink); n.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 46), new Vector2(w - 16, 34));
                n.resizeTextForBestFit = true; n.resizeTextMinSize = 16; n.resizeTextMaxSize = 26;
                string line = got ? "Best " + e.best.ToString(e.best < 10 ? "0.0" : "0") + " kg  ·  x" + e.count : Sim.BiteText(sp.When);
                if (got && s.CanMount(sp))
                {
                    string sid = sp.Id; int pct = Mathf.RoundToInt((float)(Sim.TrophyBonus(sp.Rarity) * 100));
                    var mb = Ui.Button("Mount", tile.transform, "MOUNT  +" + pct + "%", Ui.Gold, () => { if (S.Mount(sid)) { Sfx.Play("star", 0.8f); Toast("Mounted in the trophy hall: +" + pct + "% income everywhere"); GameRoot.Instance.Save(); ShowBook(); } }, 22);
                    ((RectTransform)mb.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(w - 20, 50));
                    line = "";
                }
                else if (got && s.Mounted(sp.Id)) line = "Trophy  ·  " + line;
                var b = Ui.Label("B", tile.transform, line, 20, got ? Ui.Dim : (s.Biting(sp) ? new Color(0.12f, 0.62f, 0.29f) : Ui.Dim), TextAnchor.MiddleCenter, false); b.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(w - 12, 28));
            }
            EndList(((all.Count + cols - 1) / cols) * (hgt + 6));
        }

        void ShowAwards()
        {
            var s = S;
            Frame("AWARDS");
            float y = 0;
            y = Heading(y, "Every award you take adds +1% income for good  (now +" + s.State.achieved.Count + "%)");
            var order = new List<AchievementDef>(Sim.Achievements);
            order.Sort((a, b) => Rank(s, a).CompareTo(Rank(s, b)));
            foreach (var a in order)
            {
                bool done = s.AchievementDone(a), claimed = s.AchievementClaimed(a); string id = a.Id;
                string progress = done ? "" : "  (" + Format.Money(Math.Min(a.Need, a.Have(s))) + " / " + Format.Money(a.Need) + ")";
                y = Entry(y, Ui.Art("star"), claimed || done ? Color.white : new Color(1, 1, 1, 0.35f), a.Name, a.Blurb + progress,
                    claimed ? null : "+" + a.Gems + " GEMS", Ui.Green, done && !claimed ? (Action)(() => { if (S.ClaimAchievement(id)) { Sfx.Play("claim", 0.8f); GameRoot.Instance.Save(); ShowAwards(); } }) : null, claimed ? "CLAIMED" : null, done && !claimed);
            }
            EndList(-y);
        }
        static int Rank(Sim s, AchievementDef a) { return s.AchievementClaimed(a) ? 2 : s.AchievementDone(a) ? 0 : 1; }

        void ShowShop()
        {
            var s = S;
            Frame("SHOP" + (Store.Testing ? " (TEST)" : ""));
            float y = 0;
            y = Heading(y, "TIME WARPS: your harbour's earnings, right now");
            for (int i = 0; i < Sim.Warps.Length; i++)
            {
                var w = Sim.Warps[i]; int k = i; double cash = s.WarpCash(w);
                y = Entry(y, Ui.Art("coin"), Color.white, w.Name, cash > 0 ? "+" + Format.Money(cash) + ": " + (w.Hours < 24 ? w.Hours + " hour" + (w.Hours > 1 ? "s" : "") : "a whole day") + " of work at once" : "Hire skippers for every stage first",
                    w.Gems + " GEMS", Ui.Grape, cash > 0 && s.State.gems >= w.Gems ? (Action)(() => { if (S.BuyWarp(k)) { Sfx.Play("coin", 0.9f); Toast("+" + Format.Money(cash)); GameRoot.Instance.Save(); ShowShop(); } }) : null);
            }
            y = Heading(y, "FOR GOOD: bought once, kept for ever");
            foreach (var prod in Sim.Products)
            {
                if (!prod.Once) continue;
                var pr = prod; bool owned = s.Owns(prod.Id);
                string benefit = s.PurchaseBenefit(prod.Id);
                y = Entry(y, Ui.Art(prod.Id.EndsWith("ledger") ? "coin" : "star"), Color.white, prod.Name, prod.Blurb + (benefit != null && !owned ? "\n<color=#1E7A3E>" + benefit + "</color>" : ""), owned ? null : Store.Price(prod), Ui.Green,
                    owned ? null : (Action)(() => Store.Buy(pr, (ok, msg) => { Toast(msg); if (ok) Sfx.Play("confirm"); if (_screen == Page.Shop) ShowShop(); })), owned ? "OWNED" : null, owned);
            }
            y = PaintRows(y);
            y = Heading(y, "GEMS");
            foreach (var prod in Sim.Products)
            {
                if (prod.Once || prod.Season) continue;
                var pr = prod;
                y = Entry(y, Ui.Art("gem"), Color.white, prod.Name, prod.Blurb, Store.Price(prod), Ui.Green,
                    () => Store.Buy(pr, (ok, msg) => { Toast(msg); if (ok) Sfx.Play("confirm"); if (_screen == Page.Shop) ShowShop(); }));
            }
            y = Entry(y, Ui.Ic("cart"), Ui.Ink, "Restore purchases", "Bring back what this Apple Account has bought once.", "RESTORE", Ui.Blue, () => Store.Restore(msg => { Toast(msg); if (_screen == Page.Shop) ShowShop(); }));
            var note = Ui.Label("Note", _list, "No forced ads: one only plays when you tap for it (a Tide Boost, a second DOUBLE, a wrench). Everything can be earned by playing.", 26, Ui.Dim, TextAnchor.MiddleCenter, false); note.rectTransform.Band(true, y, 60);
            EndList(-y + 70);
        }

        const string PrivacyUrl = "https://mrnickrushing.github.io/reelempire/privacy.html", SupportUrl = "https://mrnickrushing.github.io/reelempire/support.html";

        void ShowSettings()
        {
            var s = S;
            Frame("SETTINGS");
            Action again = () => { if (_screen == Page.Settings) ShowSettings(); };
            float y = 0;
            y = Heading(y, "THE GAME");
            y = Entry(y, Ui.Ic("sound"), Ui.Ink, "Sound", Sfx.Muted ? "Off" : "On", Sfx.Muted ? "TURN ON" : "TURN OFF", Ui.Blue, () => { Sfx.Muted = !Sfx.Muted; again(); });
            y = Entry(y, Ui.Ic("sound"), Ui.Ink, "Music", Sfx.Music ? "On" : "Off", Sfx.Music ? "TURN OFF" : "TURN ON", Ui.Blue, () => { Sfx.Music = !Sfx.Music; again(); });
            y = Entry(y, Ui.Ic("bolt"), Ui.Ink, "Battery saver", FrameBudget.BatterySaver ? "On: 30 frames a second, a cooler phone" : "Off: 60 frames a second", FrameBudget.BatterySaver ? "TURN OFF" : "TURN ON", Ui.Blue, () => { FrameBudget.BatterySaver = !FrameBudget.BatterySaver; again(); });
            y = Entry(y, Ui.Ic("bolt"), Ui.Ink, "Vibration", Haptics.On ? "On: a tap you can feel on upgrades and catches" : "Off", Haptics.On ? "TURN OFF" : "TURN ON", Ui.Blue, () => { Haptics.On = !Haptics.On; again(); });
            y = Entry(y, Ui.Ic("compass"), Ui.Ink, "Labels over the harbour", CompactSigns ? "Fewer: the slowest part, what you have open, and the best upgrade for its price" : "All: a level sign over every dock and building", CompactSigns ? "SHOW ALL" : "FEWER", Ui.Blue, () => { CompactSigns = !CompactSigns; Telemetry.Log("Labels.changed", ("Labels.compact", CompactSigns ? "true" : "false")); again(); });
            y = Entry(y, Ui.Ic("bell"), Ui.Ink, "Reminders", Reminders.On ? "On: a notification when the docks are full" : "Off", Reminders.On ? "TURN OFF" : "TURN ON", Ui.Blue, () => { Reminders.On = !Reminders.On; again(); });
            y = Entry(y, Ui.Ic("chart"), Ui.Ink, "Anonymous play statistics", (Telemetry.Sharing ? "On" : "Off") + ": where players get stuck. Nothing about you, no tracking.", Telemetry.Sharing ? "TURN OFF" : "TURN ON", Ui.Blue, () => { Telemetry.Sharing = !Telemetry.Sharing; again(); });

            y = Heading(y, "YOUR ACCOUNT");
            if (!AppleAccount.SignedIn)
                y = Entry(y, Ui.Ic("user"), Ui.Ink, "Sign in with Apple", "Ties your purchases to your account so they follow you to a new phone.", AppleAccount.Supported ? "SIGN IN" : "ON IPHONE", Ui.Ink,
                    AppleAccount.Supported ? (Action)(() => AppleAccount.SignIn((ok, msg) => { Toast(ok ? "Signed in" : msg ?? "Not signed in"); if (ok) GameRoot.Instance.Save(); again(); })) : null);
            else
                y = Entry(y, Ui.Ic("user"), Ui.Ink, "Signed in with Apple", "Your purchases are tied to your account.", "SIGN OUT", Ui.Off, () => { AppleAccount.SignOut(); Toast("Signed out"); again(); });
            y = Entry(y, Ui.Ic("user"), Ui.Ink, "Harbour master: " + (string.IsNullOrEmpty(s.State.playerName) ? "(no name yet)" : s.State.playerName), "The name on your harbour.", "RENAME", Ui.Blue, AskName);
            y = Entry(y, Ui.Ic("globe"), Ui.Ink, "iCloud save", CloudSave.Available ? "On: your harbours are kept in your own iCloud and follow you to a new phone." : "Off: sign in to iCloud in the Settings app to keep your harbours safe.", null, Ui.Off, null, CloudSave.Available ? "ON" : "OFF");
            y = Entry(y, Ui.Ic("trophy"), Ui.Ink, "Leaderboards", "Earnings, species, stars and the heaviest fish, under your Game Center name.", "OPEN", Ui.Blue, GameCenter.Blocked == null ? (Action)GameCenter.ShowBoards : () => Toast(GameCenter.Blocked));
            y = Entry(y, Ui.Ic("medal"), Ui.Ink, "Game Center achievements", "Every award you earn here is kept on your Game Center profile.", "OPEN", Ui.Blue, GameCenter.Blocked == null ? (Action)GameCenter.ShowAchievements : () => Toast(GameCenter.Blocked));

            y = Heading(y, "MORE");
            y = Entry(y, Ui.Ic("compass"), Ui.Ink, "How it works", "Every part of the game explained, and what opens next.", "READ", Ui.Blue, () => Open(Page.Guide));
            y = Entry(y, Ui.Art("star"), Color.white, "Enjoying Reel Empire?", "Rate it on the App Store.", "RATE", Ui.Gold, OpenStoreReview);
            y = Entry(y, Ui.Ic("lock"), Ui.Ink, "Privacy policy", "What the game keeps and what it never does.", "READ", Ui.Blue, () => Application.OpenURL(PrivacyUrl));
            y = Entry(y, Ui.Ic("bell"), Ui.Ink, "Help and support", "Questions, problems, a purchase that went wrong.", "OPEN", Ui.Blue, () => Application.OpenURL(SupportUrl));
            y = Entry(y, Ui.Ic("medal"), Ui.Ink, "Credits", "The free models, fonts, icons and sounds this game is built with.", "SHOW", Ui.Blue, ShowCredits);
            y = Entry(y, Ui.Ic("trash"), Ui.Ink, "Delete account and start over", "Erases every harbour on this device and in your iCloud, and signs you out. Purchases made once can be restored.", "DELETE", Ui.Coral, AskDelete);
            var v = Ui.Label("V", _list, "Reel Empire: Idle Tycoon  " + Application.version + "  ·  Rushing Technologies", 24, Ui.Dim, TextAnchor.MiddleCenter, false); v.rectTransform.Band(true, y, 50);
            EndList(-y + 60);
        }

        /// <summary>A card with two buttons asking for a yes or a no.</summary>
        void Confirm(string title, string body, string yes, Color yesColour, Action onYes, string no = "CANCEL")
        {
            var card = Card(880, 500, title);
            var b = Ui.Label("B", card, body, 32, Ui.Ink, TextAnchor.UpperCenter, false); b.rectTransform.Band(true, -96, 220, 50);
            var y = Ui.Button("Yes", card, yes, yesColour, () => { CloseCard(); onYes(); }, 40); ((RectTransform)y.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(205, 44), new Vector2(380, 110));
            var n = Ui.Button("No", card, no, Ui.Off, CloseCard, 40); ((RectTransform)n.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-205, 44), new Vector2(380, 110));
        }

        void AskDelete()
        {
            Confirm("DELETE EVERYTHING?", "Every harbour, the species book and your gems are erased from this device and from your iCloud, and you are signed out. This can't be undone.\n\nThings bought once (not gems) can be restored from the shop.",
                "DELETE", Ui.Coral, () =>
                {
                    AppleAccount.SignOut(); Telemetry.Forget(); GameCenter.Forget();
                    _screen = Page.None;
                    GameRoot.Instance.Restart();
                });
        }

        void AskName()
        {
            var card = Card(880, 440, "HARBOUR MASTER");
            var back = Ui.ArtImg("Field", card, "card", Color.white, 26); back.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(700, 100));
            var text = Ui.Label("T", back.transform, "", 44, Ui.Ink); text.rectTransform.Fill(12); text.supportRichText = false;
            var input = back.gameObject.AddComponent<InputField>(); back.raycastTarget = true;
            input.textComponent = text; input.characterLimit = 16; input.text = S.State.playerName ?? "";
            var ok = Ui.Button("Ok", card, "SAVE", Ui.Green, () => { S.Rename(input.text); GameRoot.Instance.Save(); CloseCard(); if (_screen == Page.Settings) ShowSettings(); }, 40);
            ((RectTransform)ok.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(205, 44), new Vector2(380, 110));
            var no = Ui.Button("No", card, "CANCEL", Ui.Off, CloseCard, 40); ((RectTransform)no.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-205, 44), new Vector2(380, 110));
            input.Select(); input.ActivateInputField();
        }

        const string CreditsText =
            "Reel Empire: Idle Tycoon is made by Rushing Technologies.\n\n" +
            "<b>Models</b>\nBoats, the truck, trees, rocks, props and the porters: Kenney (kenney.nl), CC0.\n" +
            "Barns, market stalls, docks, fish and sea creatures: Quaternius (quaternius.com), CC0. Lighthouse: MaverickFX, CC0.\n" +
            "Octopus, Lobster and Eel: Poly by Google, CC BY 3.0. Shrimp: jeremy, CC BY 3.0. (poly.pizza)\n\n" +
            "<b>Fonts</b>\nLilita One by Juan Montoreano and Fredoka by Milena Brandão and Hafontia, SIL Open Font License.\n\n" +
            "<b>Sound</b>\nInterface sounds: Kenney, CC0.\n\n<b>Icons</b>\nFont Awesome Free by Fonticons, Inc. (fontawesome.com), CC BY 4.0.\n\n<b>Sky</b>\nPoly Haven, CC0.\n\nThank you for playing.";

        void ShowCredits()
        {
            var card = Card(1400, 860, "CREDITS");
            var b = Ui.Label("B", card, CreditsText, 28, Ui.Ink, TextAnchor.UpperLeft, false); b.rectTransform.Fill(); b.rectTransform.offsetMin = new Vector2(56, 150); b.rectTransform.offsetMax = new Vector2(-56, -90);
            b.resizeTextForBestFit = true; b.resizeTextMinSize = 16; b.resizeTextMaxSize = 28;      // it all fits above CLOSE, whatever the screen
            var ok = Ui.Button("Ok", card, "CLOSE", Ui.Blue, CloseCard, 40); ((RectTransform)ok.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(360, 104));
        }
    }

    /// <summary>Turns slowly (the rays behind a prize).</summary>
    public sealed class Spinner : MonoBehaviour
    {
        void Update() { transform.Rotate(0, 0, 22 * Time.unscaledDeltaTime); }
    }
}

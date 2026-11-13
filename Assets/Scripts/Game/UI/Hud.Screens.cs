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
        enum Page { None, Book, Awards, Shop, Settings, Harbours }
        Page _screen; string _bookHarbour; RectTransform _screenRoot, _list; float _listY;
        readonly List<Image> _navDots = new List<Image>();

        void BuildNav()
        {
            NavButton(new Vector2(0, 0), new Vector2(84, 16), "BOOK", Resources.Load<Sprite>("UI/Species/lake_golden_koi"), Color.white, () => Open(Page.Book), false);
            NavButton(new Vector2(0, 0), new Vector2(226, 16), "AWARDS", Ui.Art("star"), Color.white, () => Open(Page.Awards), true);
            NavButton(new Vector2(1, 0), new Vector2(-226, 16), "SHOP", Ui.Art("gem"), Color.white, () => Open(Page.Shop), false);
            NavButton(new Vector2(1, 0), new Vector2(-84, 16), "MORE", Ui.Ic("gear"), Ui.Ink, () => Open(Page.Settings), false);
            _screenRoot = Ui.Rect("Screen", _root); _screenRoot.Fill();
        }

        void NavButton(Vector2 anchor, Vector2 pos, string label, Sprite icon, Color tint, Action tap, bool dot)
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
        }

        void UpdateNav()
        {
            bool claim = S.AchievementsToClaim > 0;
            foreach (var d in _navDots) if (d.gameObject.activeSelf != claim) d.gameObject.SetActive(claim);
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
            var n = Ui.Label("N", row.transform, name, 34, Ui.Ink, TextAnchor.UpperLeft); n.rectTransform.Fill(); n.rectTransform.offsetMin = new Vector2(124, 0); n.rectTransform.offsetMax = new Vector2(-330, -12);
            var b = Ui.Label("B", row.transform, blurb, 25, Ui.Dim, TextAnchor.LowerLeft, false); b.rectTransform.Fill(); b.rectTransform.offsetMin = new Vector2(124, 12); b.rectTransform.offsetMax = new Vector2(-330, -50);
            b.resizeTextForBestFit = true; b.resizeTextMinSize = 17; b.resizeTextMaxSize = 25; b.verticalOverflow = VerticalWrapMode.Truncate;
            if (button != null)
            {
                var btn = Ui.Button("B", row.transform, button, tap == null ? Ui.Off : colour, tap, 32);
                ((RectTransform)btn.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 2), new Vector2(300, 88));
            }
            else if (status != null) { var st = Ui.Label("S", row.transform, status, 30, new Color(0.12f, 0.62f, 0.29f), TextAnchor.MiddleRight); st.rectTransform.Fill(); st.rectTransform.offsetMax = new Vector2(-30, 0); }
            return y - H;
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
            }
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
                    "Start again from the Old Jetty, earning double here for good" + (gain > 0 ? ", and +" + gain + " renown." : ".") + (blocked != null ? "  " + blocked + "." : ""),
                    Format.Money(s.RenovationCost(h)), Ui.Gold, blocked == null ? (Action)AskRenovate : null);
            }
            else y = Entry(y, Ui.Art("star"), Color.white, s.Def.Name + " is fully renovated", "x32 income here, for good.", null, Ui.Off, null, "DONE", true);
            y = Heading(y, "YOUR WATERS");
            for (int i = 0; i < s.State.harbours.Count; i++)
            {
                var hh = s.State.harbours[i]; var d = s.DefOf(hh); int k = i; bool here = i == s.State.at;
                y = Entry(y, Resources.Load<Sprite>("UI/Species/" + Book.In(d.Id)[15].Id.Replace('.', '_')), Color.white, d.Name,
                    d.Water + "  ·  " + hh.berths.Count + " berths  ·  x" + Format.Money(Math.Pow(2, hh.renovations)) + "  ·  " + (s.IdleRate(hh) > 0 ? "+" + Format.Money(s.IdleRate(hh)) + "/s" : "needs skippers to earn by itself"),
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
            Confirm("RENOVATE?", s.Def.Name + " starts again from one jetty and a rowboat: its levels, berths, lines and cash go.\n\nFrom then on it earns x" + Format.Money(Math.Pow(2, h.renovations + 1)) + " for good" + (gain > 0 ? ", and you gain " + gain + " renown (+" + gain * 2 + "% income everywhere)" : "") + ". Gems, the book and your other harbours are kept.",
                "RENOVATE", Ui.Gold, () => { if (S.Renovate()) { CloseScreen(); Sfx.Play("star"); Toast("Renovated! Everything here now earns double."); GameRoot.Instance.Save(); } });
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
            var count = Ui.Label("C", p, s.CaughtIn(_bookHarbour) + " / " + all.Count, 34, Ui.Ink, TextAnchor.MiddleRight); count.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -78), new Vector2(200, 50));
            // Tiles, six across.
            const int cols = 6; const float w = 236, hgt = 250;
            for (int i = 0; i < all.Count; i++)
            {
                var sp = all[i]; var e = s.Entry(sp.Id); bool got = e != null;
                var tile = Ui.ArtImg("Tile", _list, "card", got ? Color.white : new Color(0.93f, 0.88f, 0.78f), 26);
                tile.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(4 + i % cols * (w + 6), -(i / cols) * (hgt + 6)), new Vector2(w, hgt));
                var band = Ui.ArtImg("Band", tile.transform, "well", RarityInk[(int)sp.Rarity], 20); band.rectTransform.Band(true, -10, 34, 12);
                var rl = Ui.Label("R", band.transform, RarityNames[(int)sp.Rarity], 20, Color.white); rl.rectTransform.Fill();
                var pic = Ui.Img("Pic", tile.transform, got ? Color.white : new Color(0.086f, 0.196f, 0.31f, 0.35f)); pic.sprite = SpeciesPicture(sp); pic.preserveAspect = true;
                pic.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(124, 124));
                var n = Ui.Label("N", tile.transform, got ? sp.Name : "???", 26, Ui.Ink); n.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 46), new Vector2(w - 16, 34));
                n.resizeTextForBestFit = true; n.resizeTextMinSize = 16; n.resizeTextMaxSize = 26;
                string line = got ? "Best " + e.best.ToString(e.best < 10 ? "0.0" : "0") + " kg  ·  x" + e.count : Sim.BiteText(sp.When);
                var b = Ui.Label("B", tile.transform, line, 20, got ? Ui.Dim : (s.Biting(sp) ? new Color(0.12f, 0.62f, 0.29f) : Ui.Dim), TextAnchor.MiddleCenter, false); b.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(w - 12, 28));
            }
            EndList(((all.Count + cols - 1) / cols) * (hgt + 6));
        }

        void ShowAwards()
        {
            var s = S;
            Frame("AWARDS");
            float y = 0;
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
                y = Entry(y, Ui.Art(prod.Id.EndsWith("ledger") ? "coin" : "star"), Color.white, prod.Name, prod.Blurb, owned ? null : Store.Price(prod), Ui.Green,
                    owned ? null : (Action)(() => Store.Buy(pr, (ok, msg) => { Toast(msg); if (ok) Sfx.Play("confirm"); if (_screen == Page.Shop) ShowShop(); })), owned ? "OWNED" : null, owned);
            }
            y = Heading(y, "GEMS");
            foreach (var prod in Sim.Products)
            {
                if (prod.Once) continue;
                var pr = prod;
                y = Entry(y, Ui.Art("gem"), Color.white, prod.Name, prod.Blurb, Store.Price(prod), Ui.Green,
                    () => Store.Buy(pr, (ok, msg) => { Toast(msg); if (ok) Sfx.Play("confirm"); if (_screen == Page.Shop) ShowShop(); }));
            }
            y = Entry(y, Ui.Ic("cart"), Ui.Ink, "Restore purchases", "Bring back what this Apple Account has bought once.", "RESTORE", Ui.Blue, () => Store.Restore(msg => { Toast(msg); if (_screen == Page.Shop) ShowShop(); }));
            var note = Ui.Label("Note", _list, "No ads here, ever. Everything in the game can be earned by playing.", 26, Ui.Dim, TextAnchor.MiddleCenter, false); note.rectTransform.Band(true, y, 60);
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
            y = Entry(y, Ui.Art("star"), Color.white, "Enjoying Reel Empire?", "Rate it on the App Store.", "RATE", Ui.Gold, OpenStoreReview);
            y = Entry(y, Ui.Ic("lock"), Ui.Ink, "Privacy policy", "What the game keeps and what it never does.", "READ", Ui.Blue, () => Application.OpenURL(PrivacyUrl));
            y = Entry(y, Ui.Ic("bell"), Ui.Ink, "Help and support", "Questions, problems, a purchase that went wrong.", "OPEN", Ui.Blue, () => Application.OpenURL(SupportUrl));
            y = Entry(y, Ui.Ic("medal"), Ui.Ink, "Credits", "The free models, fonts and sounds this game is built with.", "SHOW", Ui.Blue, ShowCredits);
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
            "<b>Models</b>\nBoats, the truck, trees, rocks, props and the dock hands: Kenney (kenney.nl), CC0.\n" +
            "Barns, market stalls, docks, fish and sea creatures: Quaternius (quaternius.com), CC0. Lighthouse: MaverickFX, CC0.\n" +
            "Octopus, Lobster and Eel: Poly by Google, CC BY 3.0. Shrimp: jeremy, CC BY 3.0. (poly.pizza)\n\n" +
            "<b>Fonts</b>\nLilita One by Juan Montoreano and Fredoka by Milena Brandão and Hafontia, SIL Open Font License.\n\n" +
            "<b>Sound</b>\nInterface sounds: Kenney, CC0.\n\n<b>Sky</b>\nPoly Haven, CC0.\n\nThank you for playing.";

        void ShowCredits()
        {
            var card = Card(1100, 760, "CREDITS");
            var b = Ui.Label("B", card, CreditsText, 28, Ui.Ink, TextAnchor.UpperLeft, false); b.rectTransform.Fill(); b.rectTransform.offsetMin = new Vector2(50, 150); b.rectTransform.offsetMax = new Vector2(-50, -90);
            var ok = Ui.Button("Ok", card, "CLOSE", Ui.Blue, CloseCard, 40); ((RectTransform)ok.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(360, 104));
        }
    }
}

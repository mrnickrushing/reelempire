using System;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The meta layer's screens: paint in the shop with SEE IT (tried on in the harbour itself), collections, the town's
    /// progress, the record-catch card (with SHARE and MOUNT IT), and a voyage's newly found ground.
    /// </summary>
    public sealed partial class Hud
    {
        // ---- paint ----

        /// <summary>The shop's paint rows: each part's paint, SEE IT to try it on, then buy or wear.</summary>
        float PaintRows(float y)
        {
            var s = S;
            y = Heading(y, "HARBOUR PAINT: paint only, it never changes earnings. SEE IT tries it on your harbour.");
            foreach (var d in Sim.Cosmetics)
            {
                bool owned = s.OwnsCosmetic(d.Id);
                if (d.Gems <= 0 && !owned) continue;        // season paint shows once earned
                var worn = s.Wearing(d.Part); bool on = worn != null && worn.Id == d.Id; var def = d;
                string part = Sim.PartNames[Array.IndexOf(Sim.Parts, d.Part)];
                string blurb = part + (d.Season >= 0 ? "  ·  from the " + Sim.SeasonThemes[d.Season] + " season" : "") + (on ? "  ·  on now" : "");
                string button = on ? "TAKE OFF" : owned ? "PUT ON" : d.Gems + " GEMS";
                Action tap = on ? () => { S.UseCosmetic(null, def.Part); Sfx.Play("close", 0.5f); GameRoot.Instance.Save(); ShowShop(); }
                    : owned ? (Action)(() => { S.UseCosmetic(def.Id); Telemetry.Log("Paint.worn", ("Paint.id", def.Id)); Sfx.Play("confirm", 0.7f); GameRoot.Instance.Save(); ShowShop(); })
                    : s.State.gems >= d.Gems ? (Action)(() => BuyPaint(def)) : null;
                y = Entry(y, Ui.Ic("star"), Swatch(d), d.Name, blurb, button, on ? Ui.Off : owned ? Ui.Blue : Ui.Grape, tap, null, on);
                SecondButton("SEE IT", Ui.Blue, () => TryPaint(def));
            }
            return y;
        }

        /// <summary>The paint's own colour for its row: the hue it turns a part, else its tint brought into range.</summary>
        static Color Swatch(CosmeticDef d)
        {
            if (d.Hue > 0) return Color.HSVToRGB(d.Hue, 0.62f, 0.86f);
            float top = Mathf.Max(d.R, Mathf.Max(d.G, d.B)), k = top > 1 ? 0.92f / top : 0.92f;
            var c = new Color(d.R * k, d.G * k, d.B * k);
            return d.Id.EndsWith("_gilt") ? new Color(0.93f, 0.72f, 0.2f) : c;
        }

        void BuyPaint(CosmeticDef d)
        {
            if (!S.BuyCosmetic(d.Id)) { Sfx.Play("error", 0.5f); Toast("Not enough gems"); return; }
            Telemetry.Log("Paint.bought", ("Paint.id", d.Id), ("Paint.gems", d.Gems.ToString()));
            Sfx.Play("build", 0.8f); Haptics.Success(); Toast(d.Name + ": yours, and on now");
            GameRoot.Instance.Save();
        }

        RectTransform _tryBar; Vector3 _tryFocus; float _tryDistance;
        readonly System.Collections.Generic.List<CanvasGroup> _tryHidden = new System.Collections.Generic.List<CanvasGroup>();

        /// <summary>While paint is tried on the harbour is the whole picture: the rest of the HUD and the signs fade out
        /// (a CanvasGroup, so the HUD's own refresh switching things on doesn't bring them back).</summary>
        void TryClear(bool clear)
        {
            if (clear)
            {
                if (_tryHidden.Count > 0) return;
                var hide = new System.Collections.Generic.List<Transform> { _signs };
                foreach (Transform c in _root) if (c != _tryBar && c != _modal && c != _screenRoot) hide.Add(c);
                foreach (var t in hide)
                {
                    var g = t.GetComponent<CanvasGroup>(); if (g == null) g = t.gameObject.AddComponent<CanvasGroup>();
                    g.alpha = 0; g.blocksRaycasts = false; _tryHidden.Add(g);
                }
            }
            else { foreach (var g in _tryHidden) if (g != null) { g.alpha = 1; g.blocksRaycasts = true; } _tryHidden.Clear(); }
        }

        /// <summary>Closes the shop and shows the paint on the harbour, with the camera on that part, until BACK.</summary>
        void TryPaint(CosmeticDef d)
        {
            Telemetry.Log("Paint.previewed", ("Paint.id", d.Id));
            CloseScreen();
            var cam = CameraRig.Instance;
            if (_tryBar == null) { _tryFocus = cam.Focus; _tryDistance = cam.Distance; }
            HarbourView.PreviewId = d.Id;
            switch (d.Part)
            {
                case "boats": case "docks": cam.Focus = new Vector3(-6, 0, 0); cam.Distance = 70; break;
                case "market": cam.Focus = new Vector3(64, 0, 0); cam.Distance = 70; break;
                case "lighthouse": cam.Focus = new Vector3(4, 0, 54); cam.Distance = 60; break;
                default: cam.Focus = new Vector3(6, 0, 0); cam.Distance = 60; break;
            }
            if (_tryBar != null) Destroy(_tryBar.gameObject);
            var bar = Ui.ArtImg("Try", _root, "panel", Color.white, 40); bar.raycastTarget = true;
            _tryBar = bar.rectTransform; _tryBar.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(1180, 150));
            bool owned = S.OwnsCosmetic(d.Id);
            var l = Ui.Label("L", _tryBar, "Trying on: <b>" + d.Name + "</b>\n<size=24><color=#5A6E82>Paint only: it never changes earnings.</color></size>", 32, Ui.Ink, TextAnchor.MiddleLeft, false);
            l.rectTransform.Fill(); l.rectTransform.offsetMin = new Vector2(40, 0); l.rectTransform.offsetMax = new Vector2(-560, 0);
            var buy = Ui.Button("Buy", _tryBar, owned ? "PUT ON" : d.Gems + " GEMS", owned ? Ui.Blue : Ui.Grape, () =>
            {
                if (owned) { S.UseCosmetic(d.Id); GameRoot.Instance.Save(); Toast(d.Name + " is on"); }
                else BuyPaint(d);
                if (S.OwnsCosmetic(d.Id)) EndTry(false);
            }, 34);
            ((RectTransform)buy.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-270, 0), new Vector2(260, 96));
            if (!owned && S.State.gems < d.Gems) buy.GetComponent<Image>().color = Ui.Off;
            var back = Ui.Button("Back", _tryBar, "BACK", Ui.Coral, () => EndTry(true), 34);
            ((RectTransform)back.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(220, 96));
            TryClear(true);
        }

        /// <summary>Ends a try-on: the camera goes back, and with <paramref name="toShop"/> the shop opens again.</summary>
        void EndTry(bool toShop)
        {
            HarbourView.PreviewId = null;
            TryClear(false);
            if (_tryBar != null) { Destroy(_tryBar.gameObject); _tryBar = null; var cam = CameraRig.Instance; cam.Focus = _tryFocus; cam.Distance = _tryDistance; }
            if (toShop) Open(Page.Shop);
        }

        // ---- collections and the town ----

        void ShowCollections()
        {
            var s = S;
            Frame("COLLECTIONS");
            float y = 0;
            foreach (var c in Sim.Collections)
            {
                bool claimed = s.CollectionClaimed(c.Id); int have = claimed ? c.Need : Math.Min(c.Need, c.Have(s)); bool done = have >= c.Need; string id = c.Id; var def = c;
                y = Entry(y, Ui.Ic("target"), Ui.Ink, c.Name + "  <size=24><color=#5A6E82>" + have + " / " + c.Need + "</color></size>",
                    c.Blurb + ".  Reward: " + c.DecorName.ToLowerInvariant() + " and " + c.Gems + " gems.",
                    claimed ? null : "CLAIM", Ui.Gold, done && !claimed ? (Action)(() =>
                    {
                        if (!S.ClaimCollection(id)) return;
                        Telemetry.Log("Collection.claimed", ("Collection.id", id));
                        Sfx.Play("star", 0.9f); Haptics.Success(); CoinBurst(10);
                        Toast(def.DecorName + " now stands in your harbour. +" + def.Gems + " gems");
                        GameRoot.Instance.Save(); ShowCollections();
                    }) : null, claimed ? "IN THE HARBOUR" : null, done && !claimed);
            }
            y = Heading(y, "THE TOWN: it builds as you fill orders (" + s.State.ordersDone + " so far)");
            foreach (var t in Sim.Town)
            {
                bool built = s.State.ordersDone >= t.Orders;
                y = Entry(y, Ui.Ic("hammer"), Ui.Ink, t.Name, t.Blurb + (built ? "." : ".  After " + t.Orders + " orders (" + Math.Min(s.State.ordersDone, t.Orders) + " / " + t.Orders + ")."), null, Ui.Off, null, built ? "BUILT" : null, built);
            }
            EndList(-y);
        }

        // ---- record catches ----

        /// <summary>A hand-caught fish heavier than any before it: beside the old record, what it means, SHARE and MOUNT IT.</summary>
        void ShowRecord(HandCatch c)
        {
            var sp = c.Species; var s = S;
            Telemetry.Log("Record.caught", ("Record.species", sp.Id), ("Record.rarity", sp.Rarity.ToString()));
            var card = Card(1180, 700, "NEW RECORD!");
            var rays = Ui.ArtImg("Rays", card, "rays", RarityInk[(int)sp.Rarity], 0); rays.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, 30), new Vector2(520, 520));
            var pic = Ui.Img("Pic", card, Color.white); pic.sprite = SpeciesPicture(sp); pic.preserveAspect = true; pic.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, 30), new Vector2(460, 300));
            string Kg(float kg) { return kg.ToString(kg < 10 ? "0.00" : "0.0"); }
            float diff = c.Kg - c.Before;
            string body = "<size=46><b>" + sp.Name + "</b></size>\n"
                + "<size=60><b>" + Kg(c.Kg) + " kg</b></size>\n"
                + (c.Before > 0 ? "<color=#5A6E82>Old record " + Kg(c.Before) + " kg</color>   <color=#1E9E4A><b>+" + Kg(diff) + " kg</b></color>" : "<color=#5A6E82>The first one in your book</color>") + "\n\n"
                + s.RecordNote(sp);
            var bl = Ui.Label("B", card, body, 32, Ui.Ink, TextAnchor.UpperLeft, false); bl.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(580, -120), new Vector2(560, 400));
            bl.lineSpacing = 1.05f;
            var brand = Ui.Label("Brand", card, "Reel Empire  ·  " + s.Def.Name, 22, Ui.Dim); brand.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(1000, 30));
            var row = Ui.Rect("Buttons", card); row.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1100, 100));
            float x = s.CanMount(sp) ? -380 : -250;
            var share = Ui.Button("Share", row, "SHARE", Ui.Blue, () =>
            {
                Telemetry.Log("Record.shared", ("Record.species", sp.Id));
                row.gameObject.SetActive(false);        // the picture is the card without its buttons
                StartCoroutine(Share.Screen("My " + sp.Name + ": " + Kg(c.Kg) + " kg, a new record in Reel Empire!", path =>
                {
                    if (row != null) row.gameObject.SetActive(true);
                    if (path != null) Toast("Saved the picture: " + path);
                }));
            }, 32);
            ((RectTransform)share.transform).Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, 0), new Vector2(280, 96));
            if (s.CanMount(sp))
            {
                int pct = Mathf.RoundToInt((float)(Sim.TrophyBonus(sp.Rarity) * 100));
                var mount = Ui.Button("Mount", row, "MOUNT IT  +" + pct + "%", Ui.Gold, () =>
                {
                    if (S.Mount(sp.Id)) { Telemetry.Log("Record.mounted", ("Record.species", sp.Id)); Sfx.Play("star", 0.8f); Toast("Mounted in the trophy hall: +" + pct + "% income everywhere"); GameRoot.Instance.Save(); CloseCard(); }
                }, 30);
                ((RectTransform)mount.transform).Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-40, 0), new Vector2(360, 96));
            }
            var close = Ui.Button("Close", row, "LOVELY", Ui.Green, CloseCard, 32);
            ((RectTransform)close.transform).Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(s.CanMount(sp) ? 320 : 250, 0), new Vector2(260, 96));
            Sfx.Play("star", 1f); Haptics.Success(); CoinBurst(12);
        }

        // ---- voyages ----

        /// <summary>A voyage home with a ground nobody had charted: its name, the story, and the route's progress.</summary>
        void ShowGround(GroundDef g)
        {
            var s = S; s.NewGround = null;
            Telemetry.Log("Voyage.found", ("Voyage.ground", g.Id));
            int found = s.FoundOn(g.Voyage);
            var card = Card(1080, 560, "NEW GROUND");
            var name = Ui.Label("N", card, g.Name, 46, Ui.Ink); name.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -96), new Vector2(980, 60));
            var route = Ui.Label("R", card, Sim.Voyages[g.Voyage].Name + ":  " + found + " of 3 grounds charted", 26, Ui.Dim); route.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -156), new Vector2(980, 36));
            var story = Ui.Label("S", card, "“" + g.Story + "”", 32, Ui.Ink, TextAnchor.MiddleCenter, false); story.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -200), new Vector2(900, 170));
            string more = found >= 3 ? "The whole route is charted: voyages along it bring home 25% more, for good." : "Chart all three grounds on this route and its voyages bring home 25% more.";
            var m = Ui.Label("M", card, more, 26, found >= 3 ? new Color(0.12f, 0.62f, 0.29f) : Ui.Dim, TextAnchor.MiddleCenter, false); m.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(900, 60));
            var ok = Ui.Button("OK", card, "INTO THE LOG", Ui.Green, CloseCard, 32); ((RectTransform)ok.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(360, 96));
        }
    }
}

using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>The repair card: a boat's hull, the wrenches in hand, and the ways to more of them.</summary>
    public sealed partial class Hud
    {
        void ShowRepair(int berth)
        {
            var s = S; var h = H; if (berth >= h.berths.Count) return;
            float wear = s.WearOf(h, berth); int pct = Mathf.RoundToInt((1 - wear) * 100);
            var card = Card(980, 640, "THE HULL");
            var close = Ui.Button("X", card, "✕", Ui.Coral, CloseCard, 30); ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-20, -24), new Vector2(72, 72));
            var name = Ui.Label("N", card, Content.Berths[berth].Boat + "  ·  dock " + (berth + 1), 34, Ui.Ink); name.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -92), new Vector2(880, 44));
            var back = Ui.ArtImg("BarBack", card, "well", Ui.Faint, 20); back.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -146), new Vector2(760, 44));
            var bar = Ui.ArtImg("Bar", back.transform, "well", wear > Sim.WornAt ? Ui.Coral : wear > Sim.WornAt * 0.5f ? Ui.Gold : Ui.Green, 20);
            bar.rectTransform.anchorMin = Vector2.zero; bar.rectTransform.anchorMax = new Vector2(Mathf.Max(0.06f, 1 - wear), 1); bar.rectTransform.offsetMin = bar.rectTransform.offsetMax = Vector2.zero;
            var pl = Ui.Label("P", back.transform, pct + "%", 28, Color.white); pl.rectTransform.Fill(); pl.rectTransform.offsetMin = new Vector2(0, 4); pl.Stroke(2f);
            int lost = Mathf.RoundToInt((float)(1 - Sim.WearFactor(wear)) * 100);
            string says = wear > Sim.WornAt ? "Worn: she lands <b>" + lost + "% less</b> until she's mended."
                : wear > 0.02f ? "Sound for now. Past half worn a boat starts to land less."
                : "Good as new.";
            says += "\n<size=24><color=#5A6E82>Hulls wear as boats sail (never while you're away). The Boat Shed makes them last longer.</color></size>";
            var t = Ui.Label("T", card, says, 30, Ui.Ink, TextAnchor.UpperCenter, false); t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -208), new Vector2(860, 130));

            // The wrenches in hand, and the ways to more.
            var wi = Ui.Img("W", card, Ui.Ink); wi.sprite = Ui.Ic("wrench"); wi.preserveAspect = true; wi.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(60, 190), new Vector2(54, 54));
            var wn = Ui.Label("WN", card, "x " + s.State.wrenches, 40, Ui.Ink, TextAnchor.MiddleLeft); wn.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(124, 190), new Vector2(160, 54));
            bool ad = s.AdOffered(AdReward.Wrench) && Ads.Ready;
            var watch = Ui.Button("Ad", card, ad ? "FREE: WATCH AN ADVERT" : "FROM GIFTS AND ORDERS", ad ? Ui.Green : Ui.Off, () =>
            {
                if (!ad) { Toast("Wrenches come with the day's gift and with every order you fill."); return; }
                Ads.Show(ok =>
                {
                    if (ok && S.WrenchByAd()) { Sfx.Play("star", 0.7f); Haptics.Success(); Toast("+1 wrench"); GameRoot.Instance.Save(); }
                    else if (!ok) Toast("The advert wasn't finished, so there's no wrench.");
                    ShowRepair(berth);
                });
            }, 26);
            ((RectTransform)watch.transform).Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(290, 180), new Vector2(380, 74));
            var buy = Ui.Button("Buy", card, "+1  " + Sim.WrenchGems + " GEMS", s.State.gems >= Sim.WrenchGems ? Ui.Grape : Ui.Off, () =>
            {
                if (S.BuyWrench()) { Sfx.Play("confirm", 0.7f); GameRoot.Instance.Save(); ShowRepair(berth); }
                else { Sfx.Play("error", 0.5f); Toast("Not enough gems"); }
            }, 26);
            ((RectTransform)buy.transform).Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(686, 180), new Vector2(236, 74));

            bool can = wear > 0.02f && s.State.wrenches > 0;
            var fix = Ui.Button("Fix", card, wear <= 0.02f ? "NOTHING TO MEND" : "MEND  (1 WRENCH)", can ? Ui.Green : Ui.Off, () =>
            {
                if (S.Repair(berth)) { Sfx.Play("build", 0.9f); Haptics.Success(); Toast(Content.Berths[berth].Boat + " is good as new"); GameRoot.Instance.Save(); CloseCard(); if (SheetOpen) RefreshSheet(); }
                else { Sfx.Play("error", 0.5f); Toast(S.State.wrenches <= 0 ? "No wrench in hand" : "Nothing to mend"); }
            }, 40);
            ((RectTransform)fix.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-150, 40), new Vector2(500, 112));
            int worn = s.WornBoats;
            var all = Ui.Button("All", card, "MEND ALL WORN (" + worn + ")", worn > 0 && s.State.wrenches > 0 ? Ui.Blue : Ui.Off, () =>
            {
                int n = S.RepairAll();
                if (n > 0) { Sfx.Play("build", 0.9f); Haptics.Success(); Toast(n + (n == 1 ? " boat mended" : " boats mended")); GameRoot.Instance.Save(); CloseCard(); if (SheetOpen) RefreshSheet(); }
                else { Sfx.Play("error", 0.5f); Toast(S.State.wrenches <= 0 ? "No wrench in hand" : "No boat is worn enough to need it"); }
            }, 28);
            ((RectTransform)all.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(270, 46), new Vector2(320, 100));
        }
    }
}

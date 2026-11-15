using System;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>The order board (what the town wants) and the bait box (what each dock fishes with).</summary>
    public sealed partial class Hud
    {
        void ShowOrders()
        {
            var s = S;
            Frame("ORDERS");
            float y = 0;
            for (int i = 0; i < s.State.orders.Count; i++)
            {
                var o = s.State.orders[i]; int k = i; bool ready = o.have >= o.need;
                var hd = Content.Harbour(o.harbour);
                var sp = o.kind == (int)OrderKind.Species ? Book.Get(o.species) : null;
                Sprite icon = sp != null ? SpeciesPicture(sp) : o.kind == (int)OrderKind.Product ? Ui.Ic("box") : Ui.Art("star");
                string where = o.harbour != s.H.id && hd != null ? "  (at " + hd.Name + ")" : "";
                string pay = "+" + Format.Money(s.OrderCash(o)) + (o.gems > 0 ? " and " + o.gems + (o.gems == 1 ? " gem" : " gems") : "");
                int tie = s.TieLevel(o.who);
                string known = (tie > 0 ? "  <color=#1E9E4A>" + Sim.TieNames[tie] + " +" + Mathf.RoundToInt((float)(Sim.TieBonus[tie] * 100)) + "%</color>" : "") + (o.favourite ? "  <color=#C47A00>their favourite +" + Mathf.RoundToInt((float)(Sim.FavouriteBonus * 100)) + "%</color>" : "");
                string blurb = ready ? "Ready to hand over: " + pay + "." : Format.Money(o.have) + " / " + Format.Money(o.need) + "  ·  pays " + pay + ".  " + s.OrderHint(o);
                y = Entry(y, icon, sp != null || o.kind != (int)OrderKind.Product ? Color.white : Ui.Ink, o.who + " wants " + s.OrderText(o) + where + known, blurb,
                    ready ? "DELIVER" : "TURN DOWN", ready ? Ui.Gold : Ui.Blue,
                    ready ? (Action)(() =>
                    {
                        double cash = S.ClaimOrder(k);
                        if (cash > 0) { Sfx.Play("claim", 0.9f); Haptics.Success(); CoinBurst(10); Toast("Order filled: +" + Format.Money(cash)); TieNewsCard(); GameRoot.Instance.Save(); ShowOrders(); }
                    })
                    : () => Confirm("TURN IT DOWN?", o.who + " will ask someone else. A new order takes its place in about ten minutes.", "TURN DOWN", Ui.Coral,
                        () => { S.DismissOrder(k); GameRoot.Instance.Save(); if (_screen == Page.Orders) ShowOrders(); }, "KEEP IT"),
                    null, ready);
            }
            int empty = s.OrderSlotCount - s.State.orders.Count;
            if (empty > 0) y = Heading(y, (empty == 1 ? "One more order" : "The next order") + " comes in " + Format.Time(s.OrderIn) + ".");
            y = Heading(y, "Regular customers pay up to 35% more, and 25% more for their favourite line.");
            y = Heading(y, "Orders never run out of time. Filling one pays on top of the ordinary sale.  (Filled so far: " + s.State.ordersDone + ")");
            EndList(-y);
        }

        /// <summary>A customer who came to know the harbour better says so.</summary>
        void TieNewsCard()
        {
            var s = S; string news = s.TieNews; if (string.IsNullOrEmpty(news)) return;
            s.TieNews = null;
            Telemetry.Log("Customer.closer", ("Customer.line", news.Length > 40 ? news.Substring(0, 40) : news));
            Toast(news);
        }

        /// <summary>Choosing a dock's bait: what each one does, and the level that unlocks it.</summary>
        void ShowBaits(int berth)
        {
            var s = S; var h = H;
            if (berth < 0 || berth >= h.berths.Count) return;
            var card = Card(1100, 760, "BAIT");
            var close = Ui.Button("X", card, "✕", Ui.Coral, CloseCard, 30); ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-20, -24), new Vector2(72, 72));
            var head = Ui.Label("H", card, Content.Berths[berth].Name + ": free to change whenever you like.", 28, Ui.Dim, TextAnchor.MiddleCenter, false);
            head.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -78), new Vector2(1000, 40));
            var now = s.BaitOf(h, berth);
            for (int i = 0; i < Sim.Baits.Length; i++)
            {
                var b = Sim.Baits[i]; int k = i; bool open = s.BaitUnlocked(h, berth, i), inUse = b == now;
                var row = Ui.ArtImg("Row", card, "card", inUse ? new Color(0.72f, 0.93f, 0.76f) : open ? Ui.Card : Ui.Faint, 26);
                row.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -128 - i * 118), new Vector2(1010, 108));
                string worth = "a trip is worth x" + s.TripMean(h, b).ToString("0.00").TrimEnd('0').TrimEnd('.');
                var n = Ui.Label("N", row.transform, b.Name + "  <size=24><color=#5A6E82>" + worth + "</color></size>", 34, open ? Ui.Ink : Ui.Dim, TextAnchor.UpperLeft); n.rectTransform.Fill(); n.rectTransform.offsetMin = new Vector2(26, 0); n.rectTransform.offsetMax = new Vector2(-290, -10);
                var d = Ui.Label("D", row.transform, b.Blurb, 24, Ui.Dim, TextAnchor.LowerLeft, false); d.rectTransform.Fill(); d.rectTransform.offsetMin = new Vector2(26, 12); d.rectTransform.offsetMax = new Vector2(-290, -46);
                d.resizeTextForBestFit = true; d.resizeTextMinSize = 17; d.resizeTextMaxSize = 24;
                if (inUse) { var on = Ui.Label("On", row.transform, "IN USE", 28, new Color(0.12f, 0.62f, 0.29f), TextAnchor.MiddleRight); on.rectTransform.Fill(); on.rectTransform.offsetMax = new Vector2(-34, 0); }
                else
                {
                    var use = Ui.Button("Use", row.transform, open ? "USE" : "LEVEL " + s.BaitLevel(b), open ? Ui.Green : Ui.Off, open ? (Action)(() =>
                    {
                        if (S.SetBait(berth, k)) { Sfx.Play("confirm", 0.7f); GameRoot.Instance.Save(); CloseCard(); if (SheetOpen) RefreshSheet(); }
                    }) : null, 30);
                    ((RectTransform)use.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 2), new Vector2(250, 80));
                }
            }
        }
    }
}

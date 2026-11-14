using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>The harbour's works (a sign over every plot, a card to build or raise one) and the chart room's research.</summary>
    public sealed partial class Hud
    {
        readonly List<Sign> _workSigns = new List<Sign>();
        Sign _chartSign;
        static readonly float[] WorkSignUp = { 7.5f, 8f, 2f, 4.5f, 20f };

        void BuildWorkSigns()
        {
            _workSigns.Clear();
            for (int i = 0; i < Sim.Works.Length; i++) { int k = i; var sg = MakeSign(() => ShowWork(k), 262); sg.Label.fontSize = 26; _workSigns.Add(sg); }
            _chartSign = MakeSign(() => Open(Page.Research), 270); _chartSign.Up.gameObject.SetActive(false); _chartSign.Label.fontSize = 26; _chartSign.Label.rectTransform.offsetMax = new Vector2(-8, 0);
        }

        void UpdateWorkSigns()
        {
            var s = S; var h = H;
            bool on = s.Unlocked("works") && s.State.tutorialStep != 1;
            for (int i = 0; i < _workSigns.Count; i++)
            {
                var w = Sim.Works[i]; var sg = _workSigns[i];
                Put(sg.Rt, Layout.WorkPlot(w.Id) + Vector3.up * WorkSignUp[i], on);
                if (!on) continue;
                int lv = s.WorkLevel(h, w.Id); double cost = s.WorkCost(h, w.Id); bool can = h.cash >= cost;
                bool done = lv >= Sim.WorkLevels;
                sg.Label.text = lv == 0 ? "+ " + w.Name.ToUpperInvariant() : w.Name.ToUpperInvariant() + "  " + (done ? "★" : lv.ToString());
                sg.Up.gameObject.SetActive(!done);
                sg.Up.color = can ? Ui.Green : Ui.Off;
                sg.Back.color = lv == 0 && can ? Ui.Gold : Color.white;
            }
            bool chart = s.Unlocked("research") && s.State.tutorialStep != 1;
            Put(_chartSign.Rt, Layout.ChartRoom + Vector3.up * 10.5f, chart);
            if (chart)
            {
                _chartSign.Label.text = "RESEARCH  " + s.State.notes + (s.State.notes == 1 ? " note" : " notes");
                _chartSign.Back.color = s.ResearchReady ? Ui.Gold : Color.white;
            }
        }

        /// <summary>A work's card: what it does at this level and the next, and the button to build it.</summary>
        void ShowWork(int index)
        {
            var s = S; var h = H; var w = Sim.Works[index];
            int lv = s.WorkLevel(h, w.Id); bool done = lv >= Sim.WorkLevels; double cost = s.WorkCost(h, w.Id);
            var card = Card(940, 600, w.Name.ToUpperInvariant());
            var close = Ui.Button("X", card, "✕", Ui.Coral, CloseCard, 30); ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-20, -24), new Vector2(72, 72));
            var does = Ui.Label("D", card, w.Does + ".", 32, Ui.Ink, TextAnchor.MiddleCenter, false); does.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -92), new Vector2(840, 50));
            string text = (lv > 0 ? "<b>Level " + lv + ":</b>  " + w.Says(lv) : "<b>Not built yet.</b>")
                + (done ? "\n\nAs fine as it can be made." : "\n<color=#1E9E4A><b>Level " + (lv + 1) + ":</b>  " + w.Says(lv + 1) + "</color>")
                + "\n\n<size=24><color=#5A6E82>Works belong to this harbour: a renovation starts them again with everything else.</color></size>";
            var t = Ui.Label("T", card, text, 30, Ui.Ink, TextAnchor.UpperCenter, false); t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(840, 260));
            if (!done)
            {
                bool can = h.cash >= cost;
                var b = Ui.Button("Build", card, (lv == 0 ? "BUILD  " : "UPGRADE  ") + Format.Money(cost), can ? Ui.Green : Ui.Off, () =>
                {
                    if (S.BuildWork(w.Id)) { Sfx.Play("build", 0.9f); Haptics.Success(); Toast(w.Name + (lv == 0 ? " is built: " : " level " + (lv + 1) + ": ") + w.Says(lv + 1)); GameRoot.Instance.Save(); CloseCard(); }
                    else { Sfx.Play("error", 0.5f); Toast("Not enough cash yet: " + Format.Money(cost) + " needed"); }
                }, 44);
                ((RectTransform)b.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(520, 116));
            }
        }

        void ShowResearch()
        {
            var s = S;
            Frame("RESEARCH");
            float y = 0;
            y = Heading(y, "NOTES: " + s.State.notes + "   ·   a note for every filled order (two for rare fish) and every new species");
            string branch = null;
            foreach (var r in Sim.Research)
            {
                if (r.Branch != branch) { branch = r.Branch; y = Heading(y, branch); }
                var def = r; bool known = s.Researched(r.Id); string blocked = s.ResearchBlocked(r);
                y = Entry(y, Ui.Ic(known ? "medal" : blocked != null && blocked.StartsWith("After") ? "lock" : "target"), Ui.Ink, r.Name, r.Blurb + ".",
                    known ? null : r.Notes + (r.Notes == 1 ? " NOTE" : " NOTES"), Ui.Green,
                    known || blocked != null ? (Action)null : () =>
                    {
                        if (S.DoResearch(def.Id)) { Sfx.Play("star", 0.9f); Haptics.Success(); Toast("Learned: " + def.Name + ". " + def.Blurb + "."); GameRoot.Instance.Save(); ShowResearch(); }
                    },
                    known ? "KNOWN" : null, known || blocked == null);
            }
            y = Heading(y, "What is learned here is kept for good, in every harbour and through every renovation.");
            EndList(-y);
        }
    }
}

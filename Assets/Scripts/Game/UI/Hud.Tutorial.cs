using System;
using System.Collections.Generic;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The first run, in Marta's voice: a welcome card, then a speech bubble by her portrait that says what the
    /// guide's current step is about (the hand points at it), until the third berth is open. It can be skipped, it
    /// keeps its place in <see cref="GameState.tutorialStep"/> (0 new, 1 under way, 2 done), and a save already past
    /// those steps never sees it.
    /// </summary>
    public sealed partial class Hud
    {
        static readonly Dictionary<string, string> Coach = new Dictionary<string, string>
        {
            { "cast", "That's your rowboat. <b>Tap it</b> and off it goes fishing!" },
            { "haul", "Fish on the quay! <b>Tap the porter</b> to carry the crate up to the hall." },
            { "sell", "Now <b>tap the truck</b>. It takes the fish to market, and that's money." },
            { "level5", "Tap the <b>number over the jetty</b>, then UPGRADE. Every level catches more." },
            { "skipper", "Tired of tapping? In the same panel, <b>hire a skipper</b>. The first one's on me." },
            { "level10", "Keep going to <b>level 10</b>: the catch doubles. Milestones at 10, 25, 50 and 100 are worth chasing." },
            { "berth2", "Save up and tap <b>+ DOCK</b> out on the next pier. A new dock lands far more than the old one." },
            { "dock5", "The four circles at the bottom are your harbour: boats, porters, hall, market. The <b>red one is too slow</b>. Tap it and upgrade the porters." },
            { "dockskipper", "A <b>head porter</b> keeps them moving without a tap. Hire one in the porters' panel." },
            { "clean", "Cleaned fish sell for half as much again. Buy the <b>Cleaning Line</b> in the hall." },
            { "marketskipper", "One more: a <b>market manager</b> for the trucks. Then the harbour earns while you're away." },
            { "berth3", "You're running a real harbour now. Open the <b>third dock</b> and I'll leave you to it." },
        };
        const string LastCoached = "berth3";

        RectTransform _coach; Text _coachText; string _coachFor;

        void BuildTutorial()
        {
            var st = S.State;
            int last = Sim.Goals.FindIndex(g => g.Id == LastCoached);
            if (st.tutorialStep < 2 && st.goalIndex > last) st.tutorialStep = 2;
            if (st.tutorialStep == 0) Welcome();
        }

        Image Portrait(Transform parent, float size)
        {
            var disc = Ui.ArtImg("Portrait", parent, "badge", Ui.Blue, 0); disc.rectTransform.sizeDelta = new Vector2(size, size * 1.06f);
            var mask = Ui.Img("Mask", disc.transform, Color.white); mask.sprite = Ui.Circle; mask.rectTransform.Fill(size * 0.09f); mask.rectTransform.offsetMin = new Vector2(size * 0.09f, size * 0.15f);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var face = Ui.Img("Face", mask.transform, Color.white); face.sprite = Resources.Load<Sprite>("UI/Faces/marta"); face.preserveAspect = true;
            face.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -size * 0.2f), new Vector2(size * 1.55f, size * 1.55f));
            return disc;
        }

        void Welcome()
        {
            var card = Card(940, 560, "WELCOME, SKIPPER");
            var p = Portrait(card, 220); p.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(50, 20), p.rectTransform.sizeDelta);
            var t = Ui.Label("T", card, "I'm <b>Marta</b>. This old jetty on Stillwater Lake is yours now, and so is the rowboat.\n\nCatch fish, sell them, and we'll build this place into a fishing empire.", 34, Ui.Ink, TextAnchor.UpperLeft, false);
            t.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, -96), new Vector2(590, 280));
            var go = Ui.Button("Go", card, "LET'S FISH!", Ui.Green, () => { S.State.tutorialStep = 1; CloseCard(); GameRoot.Instance.Save(); }, 46);
            ((RectTransform)go.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(120, 40), new Vector2(420, 116));
            var skip = Ui.Button("Skip", card, "SKIP", Ui.Off, () => { S.State.tutorialStep = 2; CloseCard(); GameRoot.Instance.Save(); }, 30);
            ((RectTransform)skip.transform).Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(50, 50), new Vector2(170, 80));
        }

        void ShowCoach(string text)
        {
            if (_coach == null)
            {
                _coach = Ui.Rect("Coach", _root); _coach.Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-16, 196), new Vector2(700, 170));      // bottom right: clear of the boats, the quay and the goal card
                var bubble = Ui.ArtImg("Bubble", _coach, "plate", Color.white, 36); bubble.rectTransform.Fill(); bubble.rectTransform.offsetMin = new Vector2(110, 0);
                _coachText = Ui.Label("T", bubble.transform, "", 29, Ui.Ink, TextAnchor.MiddleLeft, false); _coachText.rectTransform.Fill(); _coachText.rectTransform.offsetMin = new Vector2(66, 16); _coachText.rectTransform.offsetMax = new Vector2(-62, -8);
                var p = Portrait(_coach, 164); p.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0), p.rectTransform.sizeDelta);
                var x = Ui.Button("X", bubble.transform, "✕", Ui.Off, () => { S.State.tutorialStep = 2; GameRoot.Instance.Save(); }, 24);
                ((RectTransform)x.transform).Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-8, -6), new Vector2(54, 54));
            }
            _coachText.text = text;
            _coach.localScale = Vector3.one * 0.8f;      // pops in
            Sfx.Play("open", 0.35f);
        }

        void UpdateTutorial()
        {
            var st = S.State;
            bool on = st.tutorialStep == 1 && !ModalOpen && !ScreenOpen;
            var g = S.Goal;
            if (st.tutorialStep == 1 && (g == null || !Coach.ContainsKey(g.Id)))
            {
                // Past the last coached step: a goodbye, and that is the tutorial done.
                st.tutorialStep = 2; on = false;
                if (!ModalOpen)
                {
                    var card = Card(880, 470, "YOU'VE GOT IT!");
                    var p = Portrait(card, 190); p.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 10), p.rectTransform.sizeDelta);
                    var t = Ui.Label("T", card, "Three docks, crews on every stage and money coming in by itself.\n\nThe GOAL card will always tell you what's next. Fair winds, skipper!", 32, Ui.Ink, TextAnchor.UpperLeft, false);
                    t.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(266, -90), new Vector2(570, 230));
                    var ok = Ui.Button("Ok", card, "AYE AYE!", Ui.Green, CloseCard, 44); ((RectTransform)ok.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(100, 36), new Vector2(380, 110));
                    Sfx.Play("star", 0.8f);
                }
                GameRoot.Instance.Save();
            }
            if (on && g != null)
            {
                string line = S.GoalDone ? "Nicely done! <b>Tap CLAIM</b> on the goal card for your reward." : Coach[g.Id];
                string key = g.Id + (S.GoalDone ? "!" : "");
                if (_coachFor != key) { _coachFor = key; ShowCoach(line); }
            }
            if (_coach == null) return;
            if (_coach.gameObject.activeSelf != on) _coach.gameObject.SetActive(on);
            if (!on) { if (st.tutorialStep != 1) _coachFor = null; return; }
            _coach.localScale = Vector3.Lerp(_coach.localScale, Vector3.one, 1 - Mathf.Exp(-14 * Time.unscaledDeltaTime));
            // The upgrade sheet takes the bottom of the screen: step up out of its way.
            float y = Mathf.Lerp(196, 384, 1 - Mathf.Clamp01(_sheetY / SheetHide));
            _coach.anchoredPosition = new Vector2(-16, y);
        }

        // ---- lessons: the bigger parts of the game, each introduced once by Marta when it opens ----

        sealed class Lesson { public string Feature, Title, Short, Text, Go, Opens; public Action<Hud> Do; }

        static readonly Lesson[] Lessons =
        {
            new Lesson { Feature = "crew", Short = "Hands from chests speed up a stage and have a skill to set off.", Title = "THE CREW", Opens = "With your third dock", Go = "OPEN CREW", Do = h => h.Open(Page.Crew),
                Text = "Hands come out of <b>chests</b>, and there's a free one waiting.\n\nPut a hand <b>on duty</b> and their part of the harbour runs faster. Each has a <b>skill</b> too: the round buttons on the right set it off for a short burst." },
            new Lesson { Feature = "angling", Short = "Cast, strike and reel for bigger, rarer fish. As often as you like.", Title = "FISH BY HAND", Opens = "With your second dock, once Marta has shown you round", Go = "LET'S FISH", Do = h => h.OpenAngling(),
                Text = "Fancy a go yourself? <b>Cast</b> for the gold water, <b>strike</b> when the float dips, then <b>hold to reel</b> and ease off when the fish runs.\n\nFish caught by hand are bigger and rarer, and you can fish as often as you like." },
            new Lesson { Feature = "works", Short = "Buildings on the plots that help the whole harbour.", Title = "HARBOUR WORKS", Opens = "With your third dock, once Marta has shown you round", Go = "SHOW ME", Do = h => h.ShowWork(0),
                Text = "See the <b>hammer signs</b> on the empty plots? Each is a building that helps the whole harbour: a net loft for bigger catches, a boat shed for faster trips, an ice house for better prices.\n\nThree levels each. They last until you renovate." },
            new Lesson { Feature = "bait", Short = "A level 10 dock chooses what it fishes with.", Title = "BAIT", Opens = "When a dock reaches level 10", Go = "CHOOSE BAIT", Do = h => h.ShowBaits(h.BestBerth()),
                Text = "A dock at <b>level 10</b> can choose what it fishes with.\n\n<b>Groundbait</b> lands half as much again but nothing rare; later baits go after the rare fish and the giants. Change it any time from the dock's panel." },
            new Lesson { Feature = "orders", Short = "The board by the market: fill an order for cash, gems and a note.", Title = "TOWN ORDERS", Opens = "With your fourth dock", Go = "SEE THE BOARD", Do = h => h.Open(Page.Orders),
                Text = "The town wants fish. Look at the <b>board by the market</b>: fill an order and it pays cash and gems <b>on top of</b> what the fish sold for, and a <b>note</b> for the chart room.\n\nOrders never run out of time. Bait helps you land the right fish." },
            new Lesson { Feature = "research", Short = "Spend notes on improvements you keep for good.", Title = "THE CHART ROOM", Opens = "With your first note, from an order or a new species", Go = "OPEN IT", Do = h => h.Open(Page.Research),
                Text = "You've earned a <b>note</b>. Every filled order and every new species gives one.\n\nSpend them in the chart room on things you keep <b>for good</b>: in every harbour, and through every renovation." },
            new Lesson { Feature = "voyages", Short = "Send a spare boat away for cash and gems.", Title = "VOYAGES", Opens = "With your fourth dock", Go = "OPEN TODAY", Do = h => h.Open(Page.Today),
                Text = "You've a boat to spare. Send it away from <b>TODAY</b> for half an hour, two hours or eight.\n\nThe longer the voyage, the more cash and gems it brings home, and it sails whether the game is open or not." },
            new Lesson { Feature = "harbours", Short = "Start a harbour again to earn double for good, and open new waters.", Title = "RENOVATE, AND NEW WATERS", Opens = "With your seventh dock", Go = "SHOW ME", Do = h => h.Open(Page.Harbours),
                Text = "Tap the <b>harbour's name</b> at the top. <b>Renovating</b> starts this harbour again from the Old Jetty, but it earns <b>double for good</b> and gives renown that lifts every harbour.\n\nThere are new waters to open too, each with thirty fish of its own." },
        };

        static Lesson LessonFor(string feature) { foreach (var l in Lessons) if (l.Feature == feature) return l; return null; }
        int BestBerth() { var h = H; int best = 0; for (int i = 1; i < h.berths.Count; i++) if (h.berths[i].level > h.berths[best].level) best = i; return best; }

        /// <summary>Marta's card for a part of the game that has just opened (or is being read again from How it works).</summary>
        void ShowLesson(Lesson l, bool news)
        {
            var card = Card(1020, 620, l.Title);
            var p = Portrait(card, 210); p.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(46, 30), p.rectTransform.sizeDelta);
            if (news)
            {
                var tag = Ui.ArtImg("New", card, "tag", Ui.Gold, 30); tag.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(76, -112), new Vector2(150, 50));
                var tl = Ui.Label("T", tag.transform, "NEW!", 28, Ui.Ink); tl.rectTransform.Fill(); tl.rectTransform.offsetMin = new Vector2(0, 4);
            }
            var t = Ui.Label("T", card, l.Text, 31, Ui.Ink, TextAnchor.UpperLeft, false);
            t.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(290, -92), new Vector2(660, 340));
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 22; t.resizeTextMaxSize = 31;
            var go = Ui.Button("Go", card, l.Go, Ui.Green, () => { CloseCard(); l.Do(this); }, 42);
            ((RectTransform)go.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(150, 40), new Vector2(440, 112));
            var later = Ui.Button("Later", card, news ? "LATER" : "CLOSE", Ui.Off, CloseCard, 32);
            ((RectTransform)later.transform).Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(60, 52), new Vector2(220, 86));
        }

        /// <summary>How it works: every lesson again, and what opens the ones still to come.</summary>
        void ShowGuide()
        {
            var s = S;
            Frame("HOW IT WORKS");
            float y = 0;
            y = Heading(y, "THE HARBOUR");
            y = Entry(y, Ui.Ic("compass"), Ui.Ink, "Boats, porters, hall, market", "Boats land fish, porters carry it up, the hall cleans it, the trucks sell it. The slowest of the four sets what you earn: it shows red.", null, Ui.Off, null);
            y = Entry(y, Ui.Ic("target"), Ui.Ink, "Levels and milestones", "Every level catches more. At 10, 25, 50, 100 and each hundred after, a dock doubles or its boat speeds up.", null, Ui.Off, null);
            y = Entry(y, Ui.Ic("user"), Ui.Ink, "Skippers and managers", "Hire one for each stage and it runs without a tap, and goes on earning while the game is closed.", null, Ui.Off, null);
            y = Heading(y, "MORE TO DO");
            foreach (var l in Lessons)
            {
                var lesson = l; bool open = s.Unlocked(l.Feature);
                y = Entry(y, Ui.Ic(open ? "medal" : "lock"), Ui.Ink, Title(l.Title), open ? l.Short : "Opens: " + l.Opens.Substring(0, 1).ToLowerInvariant() + l.Opens.Substring(1) + ".", open ? "READ" : null, Ui.Blue, open ? (Action)(() => { CloseScreen(); ShowLesson(lesson, false); }) : null, open ? null : "LATER");
            }
            EndList(-y);
        }

        static string Title(string caps) { return caps.Substring(0, 1) + caps.Substring(1).ToLowerInvariant(); }
    }
}

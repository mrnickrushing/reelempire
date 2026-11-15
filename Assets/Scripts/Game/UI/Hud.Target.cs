using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The species card (from a BOOK tile: where it lives, when it bites, the bait, the next useful action, PIN) and
    /// the pinned target under the guide card, which says what to do next and leads to it.
    /// </summary>
    public sealed partial class Hud
    {
        RectTransform _targetCard; Text _targetName, _targetNext; Image _targetPic; Button _targetDo; string _targetShown;

        void BuildTarget()
        {
            var g = Ui.ArtImg("Target", _root, "plate", Color.white, 36);
            g.raycastTarget = true; g.gameObject.AddComponent<Button>().onClick.AddListener(() => { var sp = S.Pinned; if (sp != null) ShowSpecies(sp); });
            _targetCard = g.rectTransform; _targetCard.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -364), new Vector2(410, 124));
            var tab = Ui.ArtImg("Tab", _targetCard, "tag", Ui.Coral, 30); tab.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(20, -4), new Vector2(134, 46));
            var t = Ui.Label("H", tab.transform, "TARGET", 24, Color.white); t.rectTransform.Fill(); t.rectTransform.offsetMin = new Vector2(0, 4); t.Stroke(2f);
            _targetPic = Ui.Img("Pic", _targetCard, Color.white); _targetPic.preserveAspect = true; _targetPic.raycastTarget = false; _targetPic.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(14, -6), new Vector2(84, 84));
            _targetName = Ui.Label("N", _targetCard, "", 26, Ui.Ink, TextAnchor.UpperLeft); _targetName.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(104, -26), new Vector2(290, 32));
            _targetName.resizeTextForBestFit = true; _targetName.resizeTextMinSize = 16; _targetName.resizeTextMaxSize = 26;
            _targetNext = Ui.Label("X", _targetCard, "", 20, Ui.Dim, TextAnchor.UpperLeft, false); _targetNext.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(104, -58), new Vector2(290, 58));
            _targetNext.resizeTextForBestFit = true; _targetNext.resizeTextMinSize = 14; _targetNext.resizeTextMaxSize = 20; _targetNext.verticalOverflow = VerticalWrapMode.Truncate;
            _targetDo = Ui.Button("Do", _targetCard, "DO IT", Ui.Green, DoTarget, 24);
            ((RectTransform)_targetDo.transform).Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-10, 10), new Vector2(104, 48));
            _targetCard.gameObject.SetActive(false);
        }

        float _targetIn;

        void UpdateTarget()
        {
            var s = S; if (_targetCard == null) return;
            if (s.PinnedLanded)
            {
                var got = s.Pinned; s.Pin("");
                Telemetry.Log("Target.landed", ("Target.species", got.Id));
                Toast("Target landed: " + got.Name + "!"); Sfx.Play("star", 0.9f); Haptics.Success();
                GameRoot.Instance.Save();
            }
            var sp = s.Pinned;
            bool show = sp != null && s.State.tutorialStep != 1 && !ScreenOpen;
            if (_targetCard.gameObject.activeSelf != show) _targetCard.gameObject.SetActive(show);
            if (!show) return;
            // The guide card may be gone (every written step done): the target takes its place.
            _targetCard.anchoredPosition = new Vector2(18, _guide.gameObject.activeSelf ? -364 : -178);
            if ((_targetIn -= Time.unscaledDeltaTime) > 0 && _targetShown == sp.Id) return;
            _targetIn = 0.5f; _targetShown = sp.Id;
            var p = s.Plan(sp);
            _targetPic.sprite = SpeciesPicture(sp);
            _targetName.text = sp.Name + (p.BitingNow ? "  <color=#1E9E4A><size=18>BITING</size></color>" : "");
            _targetNext.text = p.Next;
            _targetDo.gameObject.SetActive(p.CanApply || p.Open && !p.Here);
            _targetNext.rectTransform.sizeDelta = new Vector2(_targetDo.gameObject.activeSelf ? 190 : 290, 58);
        }

        void DoTarget()
        {
            var s = S; var sp = s.Pinned; if (sp == null) return;
            var p = s.Plan(sp);
            if (p.CanApply && s.ApplyPlan(p)) { Sfx.Play("confirm", 0.7f); Toast(p.Bait + " is on at " + Content.Berths[p.Berth].Name); Telemetry.Log("Target.applied", ("Target.species", sp.Id)); GameRoot.Instance.Save(); _targetIn = 0; return; }
            if (p.Open && !p.Here) { int i = s.State.harbours.FindIndex(x => x.id == sp.Harbour); if (s.Travel(i)) { Toast("Welcome to " + p.Where); GameRoot.Instance.Save(); } _targetIn = 0; }
        }

        /// <summary>A species from the book: its picture, what is known about it, a plan to catch it and PIN.</summary>
        void ShowSpecies(SpeciesDef sp)
        {
            var s = S; var e = s.Entry(sp.Id); bool got = e != null; var p = s.Plan(sp);
            var card = Card(1100, 640, got ? sp.Name.ToUpperInvariant() : "NOT CAUGHT YET");
            var pic = Ui.Img("Pic", card, got ? Color.white : new Color(0.086f, 0.196f, 0.31f, 0.4f)); pic.sprite = SpeciesPicture(sp); pic.preserveAspect = true;
            pic.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(60, -110), new Vector2(330, 260));
            var band = Ui.ArtImg("Band", card, "well", RarityInk[(int)sp.Rarity], 20); band.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(90, -380), new Vector2(270, 40));
            var rl = Ui.Label("R", band.transform, RarityNames[(int)sp.Rarity], 22, Color.white); rl.rectTransform.Fill();
            string known = got ? "Best " + e.best.ToString(e.best < 10 ? "0.0" : "0") + " kg  ·  landed " + e.count + (e.count == 1 ? " time" : " times") : "Weighs " + sp.MinKg.ToString("0.#") + " to " + sp.MaxKg.ToString("0.#") + " kg";
            var kl = Ui.Label("K", card, known, 24, Ui.Dim, TextAnchor.MiddleCenter, false); kl.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -430), new Vector2(370, 40));
            // The plan, line by line.
            string Line(string k, string v) { return "<color=#5A6E82>" + k + "</color>   " + v + "\n"; }
            string body = Line("WHERE", p.Where + (p.Here ? " (here)" : !p.Open ? " (not open yet)" : ""))
                + Line("WHEN", p.When + (p.BitingNow ? "  <color=#1E9E4A>biting now</color>" : ""))
                + Line("BAIT", p.Bait)
                + "\n<b>Next:</b> " + p.Next;
            var bl = Ui.Label("Plan", card, body, 30, Ui.Ink, TextAnchor.UpperLeft, false); bl.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(440, -112), new Vector2(620, 330));
            bl.resizeTextForBestFit = true; bl.resizeTextMinSize = 20; bl.resizeTextMaxSize = 30; bl.lineSpacing = 1.1f;
            bool pinned = s.State.pinned == sp.Id;
            var pin = Ui.Button("Pin", card, pinned ? "UNPIN" : "PIN AS TARGET", pinned ? Ui.Off : Ui.Coral, () =>
            {
                S.Pin(pinned ? "" : sp.Id);
                if (!pinned) Telemetry.Log("Target.pinned", ("Target.species", sp.Id), ("Target.caught", got ? "true" : "false"));
                Sfx.Play(pinned ? "close" : "confirm", 0.6f); GameRoot.Instance.Save(); CloseCard();
                if (_screen == Page.Book) ShowBook();
                if (!pinned) Toast("Pinned: " + sp.Name + ". The card under your goal shows what to do next.");
            }, 32);
            ((RectTransform)pin.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(p.CanApply ? -260 : -150, 40), new Vector2(330, 100));
            if (p.CanApply)
            {
                var doit = Ui.Button("Do", card, "PUT ON " + p.Bait.ToUpperInvariant(), Ui.Green, () => { if (S.ApplyPlan(p)) { Sfx.Play("confirm", 0.7f); Toast(p.Bait + " is on at " + Content.Berths[p.Berth].Name); GameRoot.Instance.Save(); CloseCard(); } }, 26);
                ((RectTransform)doit.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(90, 40), new Vector2(330, 100));
            }
            var close = Ui.Button("Close", card, "CLOSE", Ui.Blue, CloseCard, 32);
            ((RectTransform)close.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(p.CanApply ? 400 : 200, 40), new Vector2(220, 100));
        }
    }
}

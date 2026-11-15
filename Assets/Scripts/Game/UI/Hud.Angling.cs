using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// Fishing by hand: cast (stop the float in the gold), strike when it dips, then hold to reel and ease off when
    /// the fish runs. One big button does all three; the rules are <see cref="Fight"/> and <c>Sim.Angling</c>.
    /// </summary>
    public sealed partial class Hud
    {
        enum Cast { None, Aim, Wait, Bite, Fight, Done }
        Cast _cast;
        RectTransform _angRoot, _aimMark, _aimBar, _bobber, _tensionMark, _reelFill, _ring;
        Text _angHead, _angHint, _angButtonText;
        Image _angButton, _tensionBack;
        PressArea _press;
        Fight _fight; SpeciesDef _hooked; bool _warned;
        float _aimT, _waitLeft, _biteLeft, _quality, _hapticAt;

        public bool AnglingOpen { get { return _cast != Cast.None && _angRoot != null; } }

        void OpenAngling()
        {
            if (ModalOpen || ScreenOpen) return;
            CloseSheet();
            var card = Card(1320, 740, "FISH BY HAND");
            _angRoot = card;
            var close = Ui.Button("X", card, "✕", Ui.Coral, () => { _cast = Cast.None; CloseCard(); }, 30); ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-20, -24), new Vector2(72, 72));
            _angHead = Ui.Label("Head", card, "", 38, Ui.Ink); _angHead.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -84), new Vector2(1200, 50));
            _angHint = Ui.Label("Hint", card, "", 26, Ui.Dim, TextAnchor.MiddleCenter, false); _angHint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -132), new Vector2(1200, 40));

            // The water: a pane of the harbour's own blue with the float on it.
            var th = Theme.For(H.id);
            var water = Ui.ArtImg("Water", card, "card", Color.Lerp(th.Shallow, th.Deep, 0.35f), 26); water.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(46, -180), new Vector2(980, 330));
            water.gameObject.AddComponent<RectMask2D>();
            for (int i = 0; i < 5; i++)
            {
                var w = Ui.Img("Wave", water.transform, new Color(1, 1, 1, 0.16f), true); w.rectTransform.Place(new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector2(120 + i * 190, -60 - (i * 97) % 220), new Vector2(150, 10));
            }
            // The cast: a bar across the water with the best water in gold; the mark runs along it.
            var bar = Ui.ArtImg("Aim", water.transform, "well", new Color(1, 1, 1, 0.55f), 20); bar.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(880, 34)); _aimBar = bar.rectTransform;
            var good = Ui.ArtImg("Good", bar.transform, "well", Ui.Green, 20); good.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 34));
            var best = Ui.ArtImg("Best", bar.transform, "well", Ui.Gold, 20); best.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90, 34));
            var mark = Ui.Img("Mark", bar.transform, Ui.Ink, true); _aimMark = mark.rectTransform; _aimMark.Place(new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(16, 58));
            var ring = Ui.ArtImg("Ring", water.transform, "badge", new Color(1, 1, 1, 0.35f), 0); _ring = ring.rectTransform; _ring.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(120, 60));
            var bob = Ui.ArtImg("Float", water.transform, "badge", Ui.Coral, 0); _bobber = bob.rectTransform; _bobber.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(54, 58));

            // The line's tension, with the band to keep it in.
            var tl = Ui.Label("TL", card, "LINE", 24, Ui.Dim); tl.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-60, -176), new Vector2(200, 30));
            _tensionBack = Ui.ArtImg("Tension", card, "well", Ui.Faint, 20); _tensionBack.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-126, -212), new Vector2(68, 298));
            var red = Ui.ArtImg("Red", _tensionBack.transform, "well", Ui.Coral, 20); red.rectTransform.anchorMin = new Vector2(0, 0.92f); red.rectTransform.anchorMax = Vector2.one; red.rectTransform.offsetMin = red.rectTransform.offsetMax = Vector2.zero;
            var band = Ui.ArtImg("Band", _tensionBack.transform, "well", Ui.Green, 20); band.rectTransform.anchorMin = new Vector2(0, Fight.BandLow); band.rectTransform.anchorMax = new Vector2(1, Fight.BandHigh); band.rectTransform.offsetMin = band.rectTransform.offsetMax = Vector2.zero;
            var tm = Ui.Img("TM", _tensionBack.transform, Ui.Ink, true); _tensionMark = tm.rectTransform; _tensionMark.anchorMin = new Vector2(0, 0); _tensionMark.anchorMax = new Vector2(1, 0); _tensionMark.pivot = new Vector2(0.5f, 0.5f); _tensionMark.sizeDelta = new Vector2(34, 16);
            // How near the fish is.
            var rb = Ui.ArtImg("Reel", card, "well", Ui.Faint, 20); rb.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(46, 178), new Vector2(980, 36));
            var rl = Ui.Label("RL", rb.transform, "REELED IN", 20, Ui.Dim); rl.rectTransform.Fill();
            var rf = Ui.ArtImg("Fill", rb.transform, "well", Ui.Gold, 20); _reelFill = rf.rectTransform; _reelFill.anchorMin = Vector2.zero; _reelFill.anchorMax = new Vector2(0, 1); _reelFill.offsetMin = _reelFill.offsetMax = Vector2.zero;

            // One button for everything: it is pressed, held and let go.
            _angButton = Ui.ArtImg("Do", card, "button", Ui.Green, 44); _angButton.raycastTarget = true; _angButton.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 38), new Vector2(760, 124));
            _angButtonText = Ui.Label("L", _angButton.transform, "", 48, Color.white); _angButtonText.rectTransform.Fill(); _angButtonText.rectTransform.offsetMin = new Vector2(0, 12); _angButtonText.Stroke(2.5f);
            _press = _angButton.gameObject.AddComponent<PressArea>(); _press.Down = AnglingPress;
            StartCast();
        }

        void StartCast()
        {
            _cast = Cast.Aim; _aimT = Random.value * 2; _fight = null; _hooked = null;
            _angHead.text = "Cast for the gold water"; _angHint.text = "Stop the mark as near the middle as you can: the best water holds the best fish.";
            _angButtonText.text = "CAST"; _angButton.color = Ui.Green;
            _bobber.gameObject.SetActive(false); _ring.gameObject.SetActive(false);
            _reelFill.anchorMax = new Vector2(0, 1); _aimBar.gameObject.SetActive(true);
            _tensionMark.anchoredPosition = new Vector2(0, 0.45f * 298);
            var old = _angRoot.Find("Result"); if (old != null) Destroy(old.gameObject);
        }

        void AnglingPress()
        {
            switch (_cast)
            {
                case Cast.Aim:
                {
                    float p = Mathf.PingPong(_aimT, 1); float off = Mathf.Abs(p - 0.5f) * 2;       // 0 in the middle
                    _quality = off < 0.1f ? 1 : off < 0.34f ? Mathf.Lerp(0.85f, 0.45f, (off - 0.1f) / 0.24f) : Mathf.Lerp(0.35f, 0, (off - 0.34f) / 0.66f);
                    _cast = Cast.Wait; _waitLeft = Random.Range(1.3f, 3.6f); _aimBar.gameObject.SetActive(false);
                    Sfx.Play("dispatch", 0.6f, 1.3f); Haptics.Light();
                    _bobber.gameObject.SetActive(true); _ring.gameObject.SetActive(true);
                    _bobber.anchoredPosition = new Vector2((p - 0.5f) * 880, 40); _ring.anchoredPosition = new Vector2((p - 0.5f) * 880, 28);
                    _angHead.text = off < 0.1f ? "A perfect cast!" : off < 0.34f ? "A good cast" : "Short of the best water";
                    _angHint.text = "Wait for the float to dip... then strike.";
                    _angButtonText.text = "WAIT..."; _angButton.color = Ui.Off;
                    break;
                }
                case Cast.Wait:
                    Sfx.Play("error", 0.4f);
                    Finish("Too soon!", "You struck before the bite and scared the fish off. Wait for the float to dip.");
                    break;
                case Cast.Bite:
                {
                    // A quick strike sets the hook better: a little more toward the good fish.
                    float quick = Mathf.Clamp01(_biteLeft / 0.9f);
                    _hooked = S.HandBite(Mathf.Clamp01(_quality + quick * 0.15f));
                    var style = Sim.StyleOf(_hooked);
                    _fight = new Fight(_hooked.Rarity, S.FightSeed(), style); _warned = false;
                    _cast = Cast.Fight;
                    Sfx.Play("confirm", 0.8f); Haptics.Success();
                    _angHead.text = "Fish on!"; _angHint.text = Sim.StyleHints[(int)style] + " Keep the line in the green.";
                    _angButtonText.text = "HOLD TO REEL"; _angButton.color = Ui.Blue;
                    break;
                }
                case Cast.Done: StartCast(); Sfx.Play("click", 0.5f); break;
            }
        }

        void UpdateAngling()
        {
            if (_cast == Cast.None) return;
            if (_angRoot == null) { _cast = Cast.None; return; }        // the card was closed from elsewhere
            float dt = Time.unscaledDeltaTime, t = Time.unscaledTime;
            switch (_cast)
            {
                case Cast.Aim:
                    _aimT += dt * 1.15f;
                    _aimMark.anchoredPosition = new Vector2(Mathf.PingPong(_aimT, 1) * 880, 0);
                    break;
                case Cast.Wait:
                    _bobber.anchoredPosition = new Vector2(_bobber.anchoredPosition.x, 40 + Mathf.Sin(t * 3) * 4);
                    _ring.localScale = Vector3.one * (1 + 0.06f * Mathf.Sin(t * 3));
                    if ((_waitLeft -= dt) <= 0)
                    {
                        _cast = Cast.Bite; _biteLeft = 0.9f;
                        Sfx.Play("chime", 0.9f); Haptics.Success();
                        _angHead.text = "A BITE!"; _angHint.text = "Strike now!";
                        _angButtonText.text = "STRIKE!"; _angButton.color = Ui.Gold;
                    }
                    break;
                case Cast.Bite:
                    _bobber.anchoredPosition = new Vector2(_bobber.anchoredPosition.x, 18 + Mathf.Sin(t * 30) * 5);
                    _ring.localScale = Vector3.one * (1.3f + 0.2f * Mathf.Sin(t * 20));
                    _angButton.rectTransform.localScale = Vector3.one * (1 + 0.05f * Mathf.Sin(t * 24));
                    if ((_biteLeft -= dt) <= 0) { _angButton.rectTransform.localScale = Vector3.one; Finish("It got away", "The float dipped and you missed it. Strike as soon as it goes under."); }
                    break;
                case Cast.Fight:
                {
                    _angButton.rectTransform.localScale = Vector3.one;
                    bool hold = _press != null && _press.Held;
                    // Whole steps of a sixtieth, so a slow frame plays the same fight.
                    for (float left = Mathf.Min(dt, 0.1f); left > 0; left -= 1f / 60) _fight.Step(Mathf.Min(left, 1f / 60), hold);
                    _tensionMark.anchoredPosition = new Vector2(0, Mathf.Clamp01(_fight.Tension) * 298);
                    _reelFill.anchorMax = new Vector2(_fight.Progress, 1);
                    _bobber.anchoredPosition = new Vector2(Mathf.Lerp(_bobber.anchoredPosition.x, (_fight.Running ? Mathf.Sin(t * 5) * 300 : Mathf.Sin(t * 1.5f) * 80), dt * 3), 30 + _fight.Progress * -10 + Mathf.Sin(t * 12) * (_fight.Running ? 8 : 3));
                    _ring.anchoredPosition = _bobber.anchoredPosition - new Vector2(0, 12);
                    _tensionBack.color = _fight.Tension > 0.92f ? Color.Lerp(Ui.Faint, Ui.Coral, 0.5f + 0.5f * Mathf.Sin(t * 30)) : Ui.Faint;
                    // A run is telegraphed: the fish tenses, the float shivers and a creak sounds a moment before it bolts.
                    if (_fight.Warning && !_warned) { _warned = true; Sfx.Play("click", 0.9f, 0.55f); Haptics.Light(); }
                    if (!_fight.Warning) _warned = false;
                    if (_fight.Warning) _bobber.anchoredPosition += new Vector2(Mathf.Sin(t * 60) * 6, 0);
                    _angHead.text = _fight.Running ? "It's running: EASE OFF!" : _fight.Warning ? "It's tensing up..." : _fight.Tension < Fight.BandLow ? "Too slack: reel!" : "Fish on!  <size=26><color=#5A6E82>" + Sim.StyleNames[(int)_fight.Style].ToUpperInvariant() + "</color></size>";
                    _angButton.color = _fight.Running ? Ui.Coral : _fight.Warning ? Ui.Gold : Ui.Blue;
                    if (hold && t > _hapticAt) { Haptics.Light(); _hapticAt = t + (_fight.Tension > 0.85f ? 0.08f : 0.22f); }
                    if (_fight.State == FightState.Landed) Landed();
                    else if (_fight.State == FightState.Snapped) { Sfx.Play("error", 0.7f); Finish("The line snapped!", "Too tight for too long. Let go the moment it runs, then reel again."); }
                    else if (_fight.State == FightState.Escaped) { Sfx.Play("close", 0.6f); Finish("It slipped the hook", "The line went slack. Keep it in the green: little holds, not one long one."); }
                    break;
                }
            }
        }

        void Landed()
        {
            var c = S.LandByHand(_hooked);
            Sfx.Play("star", 1f); Sfx.Play("coin", 0.8f); Haptics.Success(); CoinBurst(c.Species.Rarity >= Rarity.Rare ? 14 : 6);
            string tag = c.First ? "  <color=#D9631E>NEW SPECIES" + (c.Gems > 0 ? " +" + c.Gems + " gems" : "") + "</color>" : c.Record ? "  <color=#D9631E>YOUR BIGGEST YET</color>" : "";
            Finish(c.Species.Name + "  " + c.Kg.ToString(c.Kg < 10 ? "0.0" : "0") + " kg", RarityName(c.Species.Rarity) + "  ·  +" + Format.Money(c.Cash) + tag, c.Species);
            GameRoot.Instance.Save();
            if (c.Record) { _cast = Cast.None; ShowRecord(c); }       // a record is an event of its own
        }

        static string RarityName(Rarity r) { return r == Rarity.Common ? "Common" : r == Rarity.Uncommon ? "Uncommon" : r == Rarity.Rare ? "Rare" : r == Rarity.Epic ? "Epic" : "Legendary"; }

        /// <summary>The end of a cast, good or bad: say what happened, and the button casts again.</summary>
        void Finish(string head, string hint, SpeciesDef caught = null)
        {
            _cast = Cast.Done;
            _angHead.text = head; _angHint.text = hint;
            _angButtonText.text = "CAST AGAIN"; _angButton.color = Ui.Green; _angButton.rectTransform.localScale = Vector3.one;
            _bobber.gameObject.SetActive(false); _ring.gameObject.SetActive(false); _aimBar.gameObject.SetActive(false);
            if (caught == null) return;
            var res = Ui.Rect("Result", _angRoot); res.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(46, -180), new Vector2(980, 330));
            var rays = Ui.ArtImg("Rays", res, "rays", RarityInk[(int)caught.Rarity], 0); rays.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 420));
            var pic = Ui.Img("Pic", res, Color.white); pic.sprite = SpeciesPicture(caught); pic.preserveAspect = true; pic.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 290));
        }
    }

    /// <summary>A pressed-or-not area: fires when the finger comes down and knows while it stays down.</summary>
    public sealed class PressArea : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public System.Action Down;
        public bool Held { get; private set; }
        public void OnPointerDown(PointerEventData e) { Held = true; if (Down != null) Down(); }
        public void OnPointerUp(PointerEventData e) { Held = false; }
        void OnDisable() { Held = false; }
    }
}

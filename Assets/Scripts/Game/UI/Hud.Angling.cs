using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// Fishing by hand off the end of the quay: cast (stop the mark in the gold water), strike when the float goes
    /// under, then hold to reel and let go when the fish runs. A tap anywhere is the one button; the rules are
    /// <see cref="Fight"/> and <c>Sim.Angling</c>, the scene is <see cref="AnglerView"/>.
    /// </summary>
    public sealed partial class Hud
    {
        enum Cast { None, Aim, Wait, Bite, Fight, Done }
        Cast _cast;
        RectTransform _angRoot, _tensionMark, _reelFill, _reelBar;
        Text _angHead, _angHint, _angButtonText;
        Image _angButton, _tensionBack;
        PressArea _press;
        Fight _fight; SpeciesDef _hooked; bool _warned;
        float _aimT, _waitLeft, _biteLeft, _quality, _hapticAt;

        static readonly Color Gauge = new Color(0.72f, 0.78f, 0.84f);

        public bool AnglingOpen { get { return _cast != Cast.None && _angRoot != null; } }

        /// <summary>Goes down to the end of the quay to fish: the camera comes in over the water, the rest of the HUD
        /// steps aside, and a tap anywhere is the one button.</summary>
        void OpenAngling()
        {
            if (ModalOpen || ScreenOpen) return;
            if (_tryBar != null) EndTry(false);
            if (TrophyHall.Open) CloseHall();
            CloseSheet();
            AnglerView.Show();
            // The whole screen is the button: pressed, held and let go.
            var root = Ui.Img("Angling", _root, new Color(0, 0, 0, 0)); root.raycastTarget = true; _angRoot = root.rectTransform; _angRoot.Fill();
            _angRoot.SetSiblingIndex(_modal.GetSiblingIndex());       // under the cards (a record catch shows over it)
            _press = root.gameObject.AddComponent<PressArea>(); _press.Down = AnglingPress;
            var bar = Ui.ArtImg("Bar", _angRoot, "panel", Color.white, 40); bar.raycastTarget = false;
            bar.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(1560, 170));
            _angHead = Ui.Label("Head", bar.transform, "", 36, Ui.Ink, TextAnchor.MiddleLeft); _angHead.raycastTarget = false;
            _angHead.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -22), new Vector2(900, 54));
            _angHint = Ui.Label("Hint", bar.transform, "", 24, Ui.Dim, TextAnchor.UpperLeft, false); _angHint.raycastTarget = false;
            _angHint.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -80), new Vector2(900, 70));
            // How near the fish is: a strip along the top of the bar.
            var rb = Ui.ArtImg("Reel", bar.transform, "well", Gauge, 14); rb.raycastTarget = false; _reelBar = rb.rectTransform;
            _reelBar.Place(new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(900, 30));
            var rf = Ui.ArtImg("Fill", rb.transform, "well", Ui.Gold, 14); rf.raycastTarget = false; _reelFill = rf.rectTransform; _reelFill.anchorMin = Vector2.zero; _reelFill.anchorMax = new Vector2(0, 1); _reelFill.offsetMin = _reelFill.offsetMax = Vector2.zero;
            var rl = Ui.Label("RL", rb.transform, "REELED IN", 18, Ui.Ink); rl.raycastTarget = false; rl.rectTransform.Fill();
            // The button: it only shows what a tap does now (the tap itself lands on the whole screen).
            _angButton = Ui.ArtImg("Do", bar.transform, "button", Ui.Green, 44); _angButton.raycastTarget = false;
            _angButton.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-150, 0), new Vector2(420, 126));
            _angButtonText = Ui.Label("L", _angButton.transform, "", 44, Color.white); _angButtonText.raycastTarget = false; _angButtonText.rectTransform.Fill(); _angButtonText.rectTransform.offsetMin = new Vector2(0, 12); _angButtonText.Stroke(2.5f);
            // The line's tension, with the band to keep it in, beside the button.
            _tensionBack = Ui.ArtImg("Tension", bar.transform, "well", Gauge, 14); _tensionBack.raycastTarget = false;
            _tensionBack.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(56, 136));
            var red = Ui.ArtImg("Red", _tensionBack.transform, "well", Ui.Coral, 14); red.raycastTarget = false; red.rectTransform.anchorMin = new Vector2(0, 0.92f); red.rectTransform.anchorMax = Vector2.one; red.rectTransform.offsetMin = red.rectTransform.offsetMax = Vector2.zero;
            var band = Ui.ArtImg("Band", _tensionBack.transform, "well", Ui.Green, 14); band.raycastTarget = false; band.rectTransform.anchorMin = new Vector2(0, Fight.BandLow); band.rectTransform.anchorMax = new Vector2(1, Fight.BandHigh); band.rectTransform.offsetMin = band.rectTransform.offsetMax = Vector2.zero;
            var tm = Ui.Img("TM", _tensionBack.transform, Ui.Ink, true); tm.raycastTarget = false; _tensionMark = tm.rectTransform; _tensionMark.anchorMin = new Vector2(0, 0); _tensionMark.anchorMax = new Vector2(1, 0); _tensionMark.pivot = new Vector2(0.5f, 0.5f); _tensionMark.sizeDelta = new Vector2(20, 10);
            var tl = Ui.Label("TL", _tensionBack.transform, "LINE", 18, Ui.Dim); tl.raycastTarget = false; tl.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 0), new Vector2(0, 2), new Vector2(90, 24));
            var close = Ui.Button("X", _angRoot, "✕", Ui.Coral, CloseAngling, 34);
            ((RectTransform)close.transform).Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(96, 96));
            TryClear(true);
            Telemetry.Log("Angling.opened", ("Angling.handCaught", S.State.handCaught.ToString()));
            StartCast();
        }

        /// <summary>Back up the quay: the HUD and the camera as they were.</summary>
        void CloseAngling()
        {
            _cast = Cast.None;
            if (_angRoot != null) { Destroy(_angRoot.gameObject); _angRoot = null; }
            AnglerView.Hide();
            TryClear(false);
            Sfx.Play("close", 0.4f);
        }

        void StartCast()
        {
            _cast = Cast.Aim; _aimT = Random.value * 2; _fight = null; _hooked = null;
            _angHead.text = "Cast for the gold water"; _angHint.text = "Tap to stop the mark on the water: the big fish lie out in the gold.";
            _angButtonText.text = "CAST"; _angButton.color = Ui.Green;
            _reelFill.anchorMax = new Vector2(0, 1); _reelBar.gameObject.SetActive(false); _tensionBack.gameObject.SetActive(false);
            if (AnglerView.Instance != null) AnglerView.Instance.Reset();
        }

        void AnglingPress()
        {
            var view = AnglerView.Instance;
            switch (_cast)
            {
                case Cast.Aim:
                {
                    float p = Mathf.PingPong(_aimT, 1); float off = Mathf.Abs(p - 0.5f) * 2;       // 0 in the middle
                    _quality = off < 0.1f ? 1 : off < 0.34f ? Mathf.Lerp(0.85f, 0.45f, (off - 0.1f) / 0.24f) : Mathf.Lerp(0.35f, 0, (off - 0.34f) / 0.66f);
                    _cast = Cast.Wait; _waitLeft = Random.Range(1.6f, 3.9f);      // the float is in the air for the first moment
                    Sfx.Play("dispatch", 0.6f, 1.3f); Haptics.Light();
                    view?.Cast(p);
                    _angHead.text = off < 0.1f ? "A perfect cast!" : off < 0.34f ? "A good cast" : p < 0.5f ? "Short of the gold water" : "Past the gold water";
                    _angHint.text = "Watch the float... strike when it goes under.";
                    _angButtonText.text = "WAIT..."; _angButton.color = Ui.Off;
                    break;
                }
                case Cast.Wait:
                    Sfx.Play("error", 0.4f);
                    view?.Lost(false);
                    Finish("Too soon!", "You struck before the bite and scared the fish off. Wait for the float to go under.");
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
                    view?.Hooked(_hooked);
                    _reelBar.gameObject.SetActive(true); _tensionBack.gameObject.SetActive(true);
                    _angHead.text = "Fish on!"; _angHint.text = Sim.StyleHints[(int)style] + " Hold to reel, keep the line in the green.";
                    _angButtonText.text = "HOLD TO REEL"; _angButton.color = Ui.Blue;
                    break;
                }
                case Cast.Done: StartCast(); Sfx.Play("click", 0.5f); break;
            }
        }

        void UpdateAngling()
        {
            if (_cast == Cast.None) return;
            if (_angRoot == null || AnglerView.Instance == null) { CloseAngling(); return; }      // closed from elsewhere
            var view = AnglerView.Instance;
            float dt = Time.unscaledDeltaTime, t = Time.unscaledTime;
            switch (_cast)
            {
                case Cast.Aim:
                    _aimT += dt * 1.15f / 1.4f;           // a little slower than on the old panel: the strip is further away
                    view.Aim(Mathf.PingPong(_aimT, 1));
                    break;
                case Cast.Wait:
                    if ((_waitLeft -= dt) <= 0)
                    {
                        _cast = Cast.Bite; _biteLeft = 0.9f;
                        Sfx.Play("chime", 0.9f); Haptics.Success();
                        view.Bite();
                        _angHead.text = "A BITE!"; _angHint.text = "Strike now: tap!";
                        _angButtonText.text = "STRIKE!"; _angButton.color = Ui.Gold;
                    }
                    break;
                case Cast.Bite:
                    _angButton.rectTransform.localScale = Vector3.one * (1 + 0.05f * Mathf.Sin(t * 24));
                    if ((_biteLeft -= dt) <= 0) { _angButton.rectTransform.localScale = Vector3.one; view.Lost(false); Finish("It got away", "The float went under and you missed it. Strike as soon as it dips."); }
                    break;
                case Cast.Fight:
                {
                    _angButton.rectTransform.localScale = Vector3.one;
                    bool hold = _press != null && _press.Held;
                    // Whole steps of a sixtieth, so a slow frame plays the same fight.
                    for (float left = Mathf.Min(dt, 0.1f); left > 0; left -= 1f / 60) _fight.Step(Mathf.Min(left, 1f / 60), hold);
                    view.Fight(_fight.Tension, _fight.Progress, _fight.Running, _fight.Warning);
                    _tensionMark.anchoredPosition = new Vector2(0, Mathf.Clamp01(_fight.Tension) * 136);
                    _reelFill.anchorMax = new Vector2(_fight.Progress, 1);
                    _tensionBack.color = _fight.Tension > 0.92f ? Color.Lerp(Gauge, Ui.Coral, 0.5f + 0.5f * Mathf.Sin(t * 30)) : Gauge;
                    // A run is telegraphed: the fish tenses, the float shivers and a creak sounds a moment before it bolts.
                    if (_fight.Warning && !_warned) { _warned = true; Sfx.Play("click", 0.9f, 0.55f); Haptics.Light(); }
                    if (!_fight.Warning) _warned = false;
                    _angHead.text = _fight.Running ? "It's running: EASE OFF!" : _fight.Warning ? "It's tensing up..." : _fight.Tension < Fight.BandLow ? "Too slack: reel!" : "Fish on!  <size=24><color=#5A6E82>" + Sim.StyleNames[(int)_fight.Style].ToUpperInvariant() + "</color></size>";
                    _angButtonText.text = _fight.Running ? "LET GO!" : "HOLD TO REEL";
                    _angButton.color = _fight.Running ? Ui.Coral : _fight.Warning ? Ui.Gold : Ui.Blue;
                    if (hold && t > _hapticAt) { Haptics.Light(); _hapticAt = t + (_fight.Tension > 0.85f ? 0.08f : 0.22f); }
                    if (_fight.State == FightState.Landed) Landed();
                    else if (_fight.State == FightState.Snapped) { Sfx.Play("error", 0.7f); view.Lost(true); Finish("The line snapped!", "Too tight for too long. Let go the moment it runs, then reel again."); }
                    else if (_fight.State == FightState.Escaped) { Sfx.Play("close", 0.6f); view.Lost(false); Finish("It slipped the hook", "The line went slack. Keep it in the green: little holds, not one long one."); }
                    break;
                }
            }
        }

        void Landed()
        {
            var c = S.LandByHand(_hooked);
            AnglerView.Instance?.Land(c.Species, RarityInk[(int)c.Species.Rarity]);
            Sfx.Play("star", 1f); Sfx.Play("coin", 0.8f); Haptics.Success();
            string tag = c.First ? "  <color=#D9631E>NEW SPECIES" + (c.Gems > 0 ? " +" + c.Gems + " gems" : "") + "</color>" : c.Record ? "  <color=#D9631E>YOUR BIGGEST YET</color>" : "";
            Finish(c.Species.Name + "  " + c.Kg.ToString(c.Kg < 10 ? "0.0" : "0") + " kg", RarityName(c.Species.Rarity) + "  ·  +" + Format.Money(c.Cash) + tag);
            GameRoot.Instance.Save();
            if (c.Record) ShowRecord(c);       // a record is an event of its own, over the fish held up on the quay
        }

        static string RarityName(Rarity r) { return r == Rarity.Common ? "Common" : r == Rarity.Uncommon ? "Uncommon" : r == Rarity.Rare ? "Rare" : r == Rarity.Epic ? "Epic" : "Legendary"; }

        /// <summary>The end of a cast, good or bad: say what happened, and the button casts again.</summary>
        void Finish(string head, string hint)
        {
            _cast = Cast.Done;
            _angHead.text = head; _angHint.text = hint;
            _angButtonText.text = "CAST AGAIN"; _angButton.color = Ui.Green; _angButton.rectTransform.localScale = Vector3.one;
            _reelBar.gameObject.SetActive(false); _tensionBack.gameObject.SetActive(false);
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

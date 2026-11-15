using System;
using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>Going into the trophy hall and out again: the bar along the bottom (what the trophies are worth, WALLS,
    /// BACK), a tap on a fish to put it in the glass case.</summary>
    public sealed partial class Hud
    {
        RectTransform _hallBar; Action _hallBack;

        /// <summary>Opens the trophy hall (with <paramref name="highlight"/> lit, a fish just mounted); BACK runs <paramref name="back"/>.</summary>
        public void OpenHall(string highlight = null, Action back = null)
        {
            if (_tryBar != null) EndTry(false);
            if (ScreenOpen) CloseScreen();
            CloseCard(); CloseSheet();
            _hallBack = back;
            Telemetry.Log("Hall.visited", ("Hall.trophies", S.State.trophies.Count.ToString()), ("Hall.from", highlight != null ? "mount" : back != null ? "book" : "harbour"));
            TrophyHall.Show(highlight);
            HallBar();
            TryClear(true);
            Sfx.Play("open", 0.5f);
        }

        void HallBar()
        {
            if (_hallBar != null) Destroy(_hallBar.gameObject);
            var s = S;
            var bar = Ui.ArtImg("Hall", _root, "panel", Color.white, 40); bar.raycastTarget = true;
            _hallBar = bar.rectTransform; _hallBar.Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(1320, 150));
            int pct = Mathf.RoundToInt((float)((s.TrophyMult - 1) * 100)), n = s.State.trophies.Count;
            string text = n == 0 ? "<b>The trophy hall</b>\n<size=24><color=#5A6E82>Mount a rare fish or better and it hangs here.</color></size>"
                : "<b>The trophy hall</b>  ·  " + n + " mounted  ·  +" + pct + "% income everywhere\n<size=24><color=#5A6E82>Tap a fish to put it in the glass case.</color></size>";
            var l = Ui.Label("L", _hallBar, text, 30, Ui.Ink, TextAnchor.MiddleLeft, false);
            l.rectTransform.Fill(); l.rectTransform.offsetMin = new Vector2(40, 0); l.rectTransform.offsetMax = new Vector2(-560, 0);
            var walls = Ui.Button("Walls", _hallBar, "WALLS", Ui.Blue, () =>
            {
                S.NextHallWall(); Sfx.Play("click", 0.6f); GameRoot.Instance.Save();
                Telemetry.Log("Hall.walls", ("Hall.wall", Sim.HallWalls[S.State.hallWall]));
                TrophyHall.Instance?.Rebuild(); Toast(Sim.HallWalls[S.State.hallWall]);
            }, 34);
            ((RectTransform)walls.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-270, 0), new Vector2(240, 96));
            var back = Ui.Button("Back", _hallBar, "BACK", Ui.Coral, CloseHall, 34);
            ((RectTransform)back.transform).Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(220, 96));
        }

        void CloseHall()
        {
            TryClear(false);
            if (_hallBar != null) { Destroy(_hallBar.gameObject); _hallBar = null; }
            TrophyHall.Hide();
            Sfx.Play("close", 0.4f);
            var back = _hallBack; _hallBack = null; back?.Invoke();
        }

        /// <summary>A tap inside the hall: a fish on the wall goes into the case.</summary>
        void TapHall(Vector2 screen)
        {
            var id = TrophyHall.Instance.Pick(screen);
            if (id == null || id == S.HallStar?.Id) return;
            if (!S.SetHallStar(id)) return;
            Telemetry.Log("Hall.star", ("Hall.species", id));
            Sfx.Play("confirm", 0.7f); Haptics.Light();
            Toast(Book.Get(id).Name + " is in the glass case");
            GameRoot.Instance.Save();
            TrophyHall.Instance.Rebuild();
        }

        /// <summary>Mounts a fish and shows it hanging in the hall.</summary>
        void MountAndShow(SpeciesDef sp, Action back = null)
        {
            if (!S.Mount(sp.Id)) return;
            int pct = Mathf.RoundToInt((float)(Sim.TrophyBonus(sp.Rarity) * 100));
            Telemetry.Log("Record.mounted", ("Record.species", sp.Id));
            Sfx.Play("star", 0.8f); Haptics.Success();
            GameRoot.Instance.Save();
            OpenHall(sp.Id, back);
            Toast("Mounted: +" + pct + "% income everywhere");
        }
    }
}

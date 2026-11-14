using ReelEmpire.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReelEmpire.Game
{
    /// <summary>
    /// The crew's skills: a round button down the right edge for each hand on duty. Gold and breathing when ready,
    /// green with the seconds left while it runs, grey with the wait while they rest.
    /// </summary>
    public sealed partial class Hud
    {
        readonly RectTransform[] _skillRt = new RectTransform[4];
        readonly Image[] _skillBack = new Image[4], _skillFace = new Image[4];
        readonly Text[] _skillText = new Text[4];
        readonly string[] _skillWho = new string[4];
        static readonly string[] SkillStage = { "boats", "porters", "hall", "market" };

        void BuildSkills()
        {
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                var b = Ui.Button("Skill" + i, _root, null, new Color(0, 0, 0, 0), () => TapSkill((Stage)k), 30);
                var rt = (RectTransform)b.transform; _skillRt[i] = rt; rt.Place(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-26, 150 - i * 112), new Vector2(100, 106));
                var img = b.GetComponent<Image>(); img.sprite = Ui.Art("badge"); img.type = Image.Type.Simple; _skillBack[i] = img;
                var face = Ui.Img("Face", rt, Color.white); face.preserveAspect = true; face.raycastTarget = false; face.rectTransform.Fill(14); face.rectTransform.offsetMin = new Vector2(14, 20); _skillFace[i] = face;
                var tag = Ui.ArtImg("Tag", rt, "tag", Color.white, 30); tag.raycastTarget = false; tag.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(6, 2), new Vector2(128, 46));
                _skillText[i] = Ui.Label("T", tag.transform, "", 22, Ui.Ink); _skillText[i].rectTransform.Fill(); _skillText[i].rectTransform.offsetMin = new Vector2(0, 4);
                rt.gameObject.SetActive(false);
            }
        }

        void UpdateSkills()
        {
            var s = S; if (s == null || _skillRt[0] == null) return;
            bool on = s.Unlocked("crew") && s.State.tutorialStep != 1;
            for (int i = 0; i < 4; i++)
            {
                var d = on ? s.OnDuty((Stage)i) : null;
                if (_skillRt[i].gameObject.activeSelf != (d != null)) _skillRt[i].gameObject.SetActive(d != null);
                if (d == null) continue;
                if (_skillWho[i] != d.Id) { _skillWho[i] = d.Id; _skillFace[i].sprite = Resources.Load<Sprite>("UI/Faces/crew_" + d.Id); }
                double left = s.SkillLeft((Stage)i), rest = s.SkillRest((Stage)i);
                bool ready = left <= 0 && rest <= 0;
                _skillBack[i].color = left > 0 ? Ui.Green : ready ? Ui.Gold : Ui.Off;
                _skillFace[i].color = left > 0 || ready ? Color.white : new Color(1, 1, 1, 0.55f);
                _skillText[i].text = left > 0 ? Mathf.CeilToInt((float)left) + "s" : ready ? "SKILL" : Format.Time(rest);
                _skillRt[i].localScale = Vector3.one * (ready ? 1 + 0.05f * Mathf.Sin(Time.unscaledTime * 5 + i) : left > 0 ? 1.06f : 1);
            }
        }

        void TapSkill(Stage stage)
        {
            var s = S; var d = s.OnDuty(stage); if (d == null) return;
            var sk = Sim.SkillOf(d); var own = s.Owned(d.Id);
            if (s.UseSkill(stage))
            {
                Sfx.Play("star", 0.9f); Haptics.Success();
                Toast(d.Name.Split(' ')[0] + ": " + sk.Name + "!  For " + Format.Time(Sim.SkillSeconds(d, own != null ? own.level : 1)) + " " + sk.Blurb + ".");
                GameRoot.Instance.Save();
            }
            else if (s.SkillLeft(stage) > 0) Toast(sk.Name + " is running: " + sk.Blurb + ".");
            else { Sfx.Play("error", 0.4f); Toast(d.Name.Split(' ')[0] + " is resting. " + sk.Name + " is ready in " + Format.Time(s.SkillRest(stage)) + "."); }
        }
    }
}

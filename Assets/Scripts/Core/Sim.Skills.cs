using System;

namespace ReelEmpire.Core
{
    /// <summary>What a crew member can do when called on: a burst of work at their stage, and for the best of them something more.</summary>
    public sealed class SkillDef
    {
        public string Name, Blurb;
        public double Mult = 2;         // their stage works this many times as fast while it lasts
        public double Seconds = 60, Cooldown = 600;
        public string Special;          // "rare", "clear", "value", "price": see Sim.Skills
    }

    /// <summary>
    /// Crew skills. The hand on duty at a stage has a skill to call on: it runs for a minute or two and then rests.
    /// It only helps where the harbour is held up, so the skill to use is the one at the stage marked TOO SLOW; the
    /// Epic and Legendary hands add something that always pays. Each level of the card makes the skill last 10% longer.
    /// </summary>
    public sealed partial class Sim
    {
        public static SkillDef SkillOf(CrewDef d)
        {
            switch (d.Id)
            {
                case "finn": return new SkillDef { Name = "All Hands", Blurb = "boats land twice as much" };
                case "odile": return new SkillDef { Name = "Mended Nets", Blurb = "boats land three times as much", Mult = 3, Seconds = 90, Cooldown = 900 };
                case "brannock": return new SkillDef { Name = "The Old Grounds", Blurb = "boats land four times as much, and every fish is rare or better", Mult = 4, Seconds = 90, Cooldown = 1800, Special = "rare" };
                case "pip": return new SkillDef { Name = "Double Time", Blurb = "porters carry twice as much" };
                case "greta": return new SkillDef { Name = "Every Barrow Out", Blurb = "porters carry three times as much", Mult = 3, Seconds = 90, Cooldown = 900 };
                case "samson": return new SkillDef { Name = "The Crane", Blurb = "clears the quay at a stroke, then porters carry four times as much", Mult = 4, Seconds = 120, Cooldown = 1200, Special = "clear" };
                case "nell": return new SkillDef { Name = "Quick Knives", Blurb = "the hall works twice as fast" };
                case "ivo": return new SkillDef { Name = "Stoke the Fires", Blurb = "the hall works three times as fast", Mult = 3, Seconds = 90, Cooldown = 900 };
                case "marisol": return new SkillDef { Name = "Master's Cut", Blurb = "the hall works four times as fast and what it makes is worth half as much again", Mult = 4, Seconds = 120, Cooldown = 1200, Special = "value" };
                case "dot": return new SkillDef { Name = "Roll Up!", Blurb = "the market sells twice as fast" };
                case "rufus": return new SkillDef { Name = "Foot Down", Blurb = "the market sells three times as fast", Mult = 3, Seconds = 90, Cooldown = 900 };
                case "augusta": return new SkillDef { Name = "Going Twice", Blurb = "the market sells four times as fast and every sale pays double", Mult = 4, Seconds = 120, Cooldown = 1200, Special = "price" };
            }
            return new SkillDef { Name = "A Hand", Blurb = "works twice as fast" };
        }

        public static double SkillSeconds(CrewDef d, int level) { return SkillOf(d).Seconds * (1 + 0.1 * (level - 1)); }

        /// <summary>Seconds the skill of the hand on duty at a stage still has to run (0: not running).</summary>
        public double SkillLeft(Stage stage)
        {
            var d = OnDuty(stage); if (d == null) return 0;
            var o = Owned(d.Id); return o == null ? 0 : Math.Max(0, o.skillUntil - RealNow());
        }

        /// <summary>Seconds until it can be used again (0: ready, or nobody on duty).</summary>
        public double SkillRest(Stage stage)
        {
            var d = OnDuty(stage); if (d == null) return 0;
            var o = Owned(d.Id); return o == null ? 0 : Math.Max(0, o.skillReady - RealNow());
        }

        public bool SkillReady(Stage stage) { return OnDuty(stage) != null && SkillRest(stage) <= 0 && SkillLeft(stage) <= 0; }

        bool _steady;

        bool SkillRunning(string special)
        {
            if (_steady) return false;
            for (int st = 0; st < 4; st++)
            {
                var d = OnDuty((Stage)st);
                if (d != null && SkillOf(d).Special == special && SkillLeft((Stage)st) > 0) return true;
            }
            return false;
        }

        double SkillMult(Stage stage)
        {
            if (stage == _probe) return _probeMult;      // SkillGain asking "what if"
            if (_steady) return 1;
            var d = OnDuty(stage);
            return d != null && SkillLeft(stage) > 0 ? SkillOf(d).Mult : 1;
        }

        /// <summary>Calls on the skill of the hand on duty at a stage. False if nobody is there or they are resting.</summary>
        public bool UseSkill(Stage stage)
        {
            if (!SkillReady(stage)) return false;
            var d = OnDuty(stage); var o = Owned(d.Id); var sk = SkillOf(d);
            o.skillUntil = RealNow() + SkillSeconds(d, o.level);
            o.skillReady = o.skillUntil + sk.Cooldown * (Researched("rota") ? 0.75 : 1);
            if (sk.Special == "clear")
            {
                // The crane swings every crate on the quay up to the hall, as far as the floor has room.
                var h = H; double moved = TakeCrates(h, PileRoom(h));
                if (moved > 0) { h.raw += moved; Hauled?.Invoke(moved); }
            }
            State.skillsUsed++;
            Touch();
            return true;
        }
    }
}

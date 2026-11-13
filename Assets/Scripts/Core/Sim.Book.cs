using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    /// <summary>The species book: what bites, and what has been caught.</summary>
    public sealed partial class Sim
    {
        SpeciesDef PickSpecies(HarbourState h)
        {
            int roll = (int)(Roll() * 1000), r = 0, acc = 0;
            for (; r < Content.RarityOdds.Length - 1; r++) { acc += Content.RarityOdds[r]; if (roll < acc) break; }
            var all = Book.In(h.id);
            // The rolled rarity, or the nearest commoner one that is biting now.
            for (int want = r; want >= 0; want--)
            {
                var pool = all.FindAll(s => (int)s.Rarity == want && Biting(s));
                if (pool.Count > 0) return pool[(int)(Roll() * pool.Count)];
            }
            return all[0];
        }

        /// <summary>Whether a species bites in the present conditions.</summary>
        public bool Biting(SpeciesDef s) { return s.When == Bite.Any || ConditionNow(s.When); }

        /// <summary>Whether a condition holds now (the clock, the weather, the season).</summary>
        public Func<Bite, bool> ConditionNow = b => false;

        public SpeciesEntry Entry(string id) { return State.species.Find(e => e.id == id); }
        public int CaughtIn(string harbour) { int n = 0; foreach (var e in State.species) if (e.id.StartsWith(harbour + ".", StringComparison.Ordinal)) n++; return n; }
        public int SpeciesCaught { get { return State.species.Count; } }

        /// <summary>Notes a catch. True the first time the species is caught (which pays gems).</summary>
        bool Record(SpeciesDef sp, float kg)
        {
            var e = Entry(sp.Id);
            bool first = e == null;
            if (first) { e = new SpeciesEntry { id = sp.Id }; State.species.Add(e); State.gems += Content.RarityGems[(int)sp.Rarity]; }
            e.count++;
            if (kg > e.best) e.best = kg;
            return first;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ReelEmpire.Core
{
    /// <summary>A local notification to schedule: seconds from now, and what it says.</summary>
    public sealed class Reminder { public double In; public string Title, Body; }

    public sealed partial class Sim
    {
        /// <summary>
        /// What is worth a notification once the app is closed: only things that have happened in the harbour (never an
        /// offer). At most two, none at night.
        /// </summary>
        public List<Reminder> ReminderPlan()
        {
            var all = new List<Reminder>();
            if (IdleRate(H) > 0)
                all.Add(new Reminder { In = AwayCap, Title = "The docks are full at " + Def.Name, Body = "Your crews have landed all they can hold. Come and collect." });
            foreach (var v in State.voyages)
                if (VoyageLeft(v) > 60) all.Add(new Reminder { In = VoyageLeft(v), Title = "A boat is back", Body = Voyages[v.kind].Name + ": the hold is full. Come and unload." });
            if (DerbyIn > 600 && DerbyIn < 3 * 86400)
                all.Add(new Reminder { In = DerbyIn, Title = "The weekend derby is on", Body = "Every fish you land counts until Sunday night. Fifty skippers, one prize table." });
            if (DerbyOn && DerbyLeft > 4 * 3600 && State.derbyScore > 0)
                all.Add(new Reminder { In = DerbyLeft - 3 * 3600, Title = "The derby ends in 3 hours", Body = "There's still time to climb the table." });
            if (ChestIn > 600)
                all.Add(new Reminder { In = ChestIn, Title = "A crew chest is waiting", Body = "Your free chest is ready. Who's signing on?" });
            if (BoostFullIn > 600)
                all.Add(new Reminder { In = BoostFullIn, Title = "The tide has turned", Body = "All three Tide Boosts are ready: double income for an hour each." });
            all.Add(new Reminder { In = 24 * 3600, Title = "A new day at " + Def.Name, Body = "The market has a new favourite today, and the boats are waiting." });

            // Two at most, never two within ten minutes, and none in the night: one due between 22:00 and 08:00 waits
            // for the morning.
            var plan = new List<Reminder>();
            foreach (var r in all.OrderBy(r => r.In))
            {
                double local = ((RealNow() + UtcOffset + r.In) % 86400 + 86400) % 86400;
                if (local >= 22 * 3600) r.In += 86400 - local + 8 * 3600;
                else if (local < 8 * 3600) r.In += 8 * 3600 - local;
            }
            foreach (var r in all.OrderBy(r => r.In))
            {
                if (plan.Count >= 2) break;
                if (plan.Count > 0 && r.In - plan[plan.Count - 1].In < 600) continue;
                plan.Add(r);
            }
            return plan;
        }
    }
}

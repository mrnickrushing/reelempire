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
        /// What is worth a notification once the app is closed. At most four, never two within ten minutes.
        /// </summary>
        public List<Reminder> ReminderPlan()
        {
            var all = new List<Reminder>();
            if (IdleRate(H) > 0)
                all.Add(new Reminder { In = AwayCap, Title = "The docks are full at " + Def.Name, Body = "Your crews have landed all they can hold. Come and collect." });
            foreach (var v in State.voyages)
                if (VoyageLeft(v) > 60) all.Add(new Reminder { In = VoyageLeft(v), Title = "A boat is back", Body = Voyages[v.kind].Name + ": the hold is full. Come and unload." });
            if (BoostFullIn > 600)
                all.Add(new Reminder { In = BoostFullIn, Title = "The tide has turned", Body = "All three Tide Boosts are ready: double income for an hour each." });
            all.Add(new Reminder { In = 24 * 3600, Title = "A new day at " + Def.Name, Body = "The market has a new favourite today, and the boats are waiting." });

            var plan = new List<Reminder>();
            foreach (var r in all.OrderBy(r => r.In))
            {
                if (plan.Count >= 4) break;
                if (plan.Count > 0 && r.In - plan[plan.Count - 1].In < 600) continue;
                plan.Add(r);
            }
            return plan;
        }
    }
}

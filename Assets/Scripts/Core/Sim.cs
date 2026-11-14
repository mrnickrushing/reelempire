using System;
using System.Collections.Generic;

namespace ReelEmpire.Core
{
    public enum Stage { Berth, Dock, Hall, Market }

    /// <summary>
    /// The game's rules. The harbour on screen runs boat by boat and round by round (<see cref="Tick"/>); the others
    /// earn at their idle rate. Fish are counted in units worth one coin fresh; the hall multiplies what it works.
    /// </summary>
    public sealed partial class Sim
    {
        public readonly GameState State;
        public Sim(GameState state) { State = state; state.Repair(); foreach (var h in state.harbours) FitCrew(h); if (state.goalIndex >= Goals.Count && state.goalTarget <= 0) PrepareGenerated(); }

        /// <summary>Something the interface shows has changed (levels, unlocks, owned things).</summary>
        public event Action Changed;
        /// <summary>A boat is back: berth, species, kilos, units landed, first time this species was caught.</summary>
        public event Action<int, SpeciesDef, float, double, bool> Caught;
        /// <summary>A hauler tipped its load on the hall floor.</summary>
        public event Action<double> Hauled;
        /// <summary>A truck sold its load for this much cash.</summary>
        public event Action<double> Sold;

        public Func<double> RealNow = () => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        public HarbourState H { get { return State.harbours[State.at]; } }
        public HarbourDef Def { get { return Content.Harbour(H.id); } }
        public HarbourDef DefOf(HarbourState h) { return Content.Harbour(h.id); }

        uint Next()
        {
            uint x = State.seed;
            x ^= x << 13; x ^= x >> 17; x ^= x << 5;
            State.seed = x == 0 ? 2463534242 : x;
            return State.seed;
        }
        /// <summary>0 &lt;= value &lt; 1.</summary>
        public double Roll() { return (Next() >> 8) / 16777216.0; }

        void Touch() { Changed?.Invoke(); }

        /// <summary>Advances the game. Long steps are cut up so a fast-forward plays the same as real time.</summary>
        public void Tick(double dt)
        {
            if (dt <= 0) return;
            RollTasks();        // the day's counts start when the day does, not when the page is first opened
            TickOrders();
            TickBonus(dt);
            if (State.firstDay < 0) State.firstDay = Today;
            State.played += dt;
            while (dt > 0)
            {
                double step = Math.Min(dt, 0.25);
                Step(H, (float)step);
                dt -= step;
            }
        }

        /// <summary>The harbours that aren't on screen earn at their idle rate.</summary>
        public void TickOthers(double dt)
        {
            for (int i = 0; i < State.harbours.Count; i++)
                if (i != State.at) Pay(State.harbours[i], IdleRate(State.harbours[i]) * dt);
        }

        void Step(HarbourState h, float dt)
        {
            // Boats.
            for (int i = 0; i < h.berths.Count; i++)
            {
                var b = h.berths[i];
                if (!b.sailing)
                {
                    if (b.skipper && b.crate < DeckOf(h, i)) { b.sailing = true; b.trip = TripOf(h, i); }
                    continue;
                }
                b.trip -= dt * (float)SpeedMult(h);
                if (b.trip > 0) continue;
                b.sailing = false;
                Land(h, i);
            }

            // Haulers: out to the quay (first half), back with a load (second half).
            double cap = HaulLoad(h);
            double space = PileRoom(h);
            foreach (var c in h.haulers)
            {
                if (!c.busy) { if (h.dockSkipper && CrateTotal(h) > 0 && space > 0) { c.busy = true; c.t = 0; c.turned = false; } else continue; }
                c.t += dt * (float)SpeedMult(h);
                if (!c.turned && c.t >= Content.HaulRound / 2)
                {
                    c.turned = true;
                    c.load = TakeCrates(h, Math.Min(cap, space));
                    space -= c.load;
                }
                if (c.t < Content.HaulRound) continue;
                c.busy = false;
                if (c.load > 0) { h.raw += c.load; State.hauls++; Hauled?.Invoke(c.load); c.load = 0; }
            }

            // The hall works the floor into the chosen product.
            var g = Content.Goods[h.goods];
            if (h.goods > 0 && h.raw > 0)
            {
                double n = Math.Min(h.raw, HallRate(h) * dt * SpeedMult(h));
                h.raw -= n; h.done += n; h.doneValue += n * GoodsValue(h, h.goods);
            }

            // Trucks: load at the hall, sell at the market (halfway), come back.
            double tcap = TruckLoad(h);
            foreach (var c in h.trucks)
            {
                if (!c.busy)
                {
                    if (!h.marketSkipper || Sellable(h) <= 0) continue;
                    LoadTruck(h, c, tcap);
                }
                c.t += dt * (float)SpeedMult(h);
                if (!c.turned && c.t >= Content.TruckRound / 2)
                {
                    c.turned = true;
                    double cash = c.value * IncomeMult(h) * (SkillRunning("price") ? 2 : 1);
                    c.load = 0; c.value = 0;
                    Pay(h, cash);
                    if (cash > 0) { State.sales++; Sold?.Invoke(cash); }
                }
                if (c.t >= Content.TruckRound) c.busy = false;
            }
        }

        void LoadTruck(HarbourState h, Carrier c, double cap)
        {
            c.busy = true; c.t = 0; c.turned = false; c.load = 0; c.value = 0;
            // Worked fish first; fresh fish only beyond what the hall will get to before the truck is back.
            double take = Math.Min(cap, h.done);
            if (take > 0)
            {
                double v = h.doneValue * (take / h.done);
                h.done -= take; h.doneValue -= v; c.load += take; c.value += v;
                OrderSold(h, h.goods, take);
                if (h.done < 1e-9) { h.done = 0; h.doneValue = 0; }
            }
            double spare = h.goods > 0 ? h.raw - HallRate(h) * Content.TruckRound : h.raw;
            double fresh = Math.Min(cap - c.load, Math.Max(0, spare));
            if (fresh > 0) { h.raw -= fresh; c.load += fresh; c.value += fresh * GoodsValue(h, 0); OrderSold(h, 0, fresh); }
        }

        /// <summary>What a truck could take right now.</summary>
        double Sellable(HarbourState h)
        {
            double spare = h.goods > 0 ? h.raw - HallRate(h) * Content.TruckRound : h.raw;
            return h.done + Math.Max(0, spare);
        }

        double CrateTotal(HarbourState h) { double n = 0; foreach (var b in h.berths) n += b.crate; return n; }

        /// <summary>Takes up to <paramref name="want"/> units from the quay, the fullest crate first.</summary>
        double TakeCrates(HarbourState h, double want)
        {
            double got = 0;
            while (want - got > 1e-9)
            {
                BerthState best = null;
                foreach (var b in h.berths) if (b.crate > 0 && (best == null || b.crate > best.crate)) best = b;
                if (best == null) break;
                double n = Math.Min(best.crate, want - got);
                best.crate -= n; got += n;
            }
            return got;
        }

        void Land(HarbourState h, int berth)
        {
            var b = h.berths[berth];
            var bait = BaitOf(h, berth);
            var sp = PickSpecies(h, bait);
            float kg = sp.MinKg + (sp.MaxKg - sp.MinKg) * (float)Math.Pow(Roll(), 2.2);
            double units = CatchOf(h, berth) * bait.Catch * Content.RarityValue[(int)sp.Rarity];
            b.crate += units;
            State.trips++; State.fish++;
            bool first = Record(sp, kg);
            DerbyCatch(sp.Rarity);
            OrderCatch(h, sp);
            if (first) AddSeasonPoints(20);
            Caught?.Invoke(berth, sp, kg, units, first);
        }

        void Pay(HarbourState h, double cash)
        {
            if (cash <= 0) return;
            h.cash += cash; h.earned += cash;
            State.lifetime += cash / DefOf(h).Scale;
        }

        // ---- what the player does by hand ----

        /// <summary>Sends a berth's boat out (no skipper yet). False if it is out already or the crate is full.</summary>
        public bool TapBerth(int berth)
        {
            var h = H;
            if (berth < 0 || berth >= h.berths.Count) return false;
            var b = h.berths[berth];
            if (b.sailing || b.crate >= DeckOf(h, berth)) return false;
            b.sailing = true; b.trip = TripOf(h, berth); State.taps++;
            return true;
        }

        /// <summary>Sends the haulers that are standing about.</summary>
        public bool TapDock()
        {
            var h = H; bool any = false;
            if (CrateTotal(h) <= 0 || PileRoom(h) <= 0) return false;
            foreach (var c in h.haulers) if (!c.busy) { c.busy = true; c.t = 0; c.turned = false; any = true; }
            if (any) State.taps++;
            return any;
        }

        /// <summary>Sends the trucks that are standing about.</summary>
        public bool TapMarket()
        {
            var h = H; bool any = false;
            double cap = TruckLoad(h);
            foreach (var c in h.trucks) if (!c.busy && Sellable(h) > 0) { LoadTruck(h, c, cap); any = true; }
            if (any) State.taps++;
            return any;
        }
    }
}

using System;

namespace ReelEmpire.Core
{
    public enum FightState { Fighting, Landed, Snapped, Escaped }

    /// <summary>
    /// A fish on a hand line. Hold to reel: the line tightens while you hold and slackens when you let go. Keep the
    /// tension in the band and the fish comes in; too tight for too long and the line parts, too slack and it swims
    /// off. The fish runs now and then, and a run on a tight line is what breaks it.
    /// </summary>
    public sealed class Fight
    {
        public const float BandLow = 0.38f, BandHigh = 0.78f;
        public float Tension = 0.45f, Progress = 0.3f;
        public bool Running;            // the fish is making a run
        public bool Warning;            // a run is coming: the fish tenses (the HUD shows and sounds it)
        public FightState State = FightState.Fighting;
        public readonly FightStyle Style;
        readonly float _strength;       // 0 a minnow, 1 a legend
        float _runIn, _runLeft, _over;
        uint _seed;
        const float WarnFor = 0.45f;

        public Fight(Rarity rarity, uint seed, FightStyle style = FightStyle.Runner)
        {
            _strength = (int)rarity / 4f; _seed = seed == 0 ? 1u : seed; Style = style;
            _runIn = NextRunIn() + 0.6f;
        }

        float Rnd() { _seed ^= _seed << 13; _seed ^= _seed >> 17; _seed ^= _seed << 5; return (_seed >> 8) / 16777216f; }

        public bool InBand { get { return Tension >= BandLow && Tension <= BandHigh; } }

        /// <summary>Seconds until the next run: rare for a runner, constant for a darter.</summary>
        float NextRunIn()
        {
            switch (Style)
            {
                case FightStyle.Steady: return 3.2f + Rnd() * 2.2f;
                case FightStyle.Darter: return 0.7f + Rnd() * 0.7f;
                case FightStyle.Stayer: return Math.Max(1f, 2.4f - _strength * 0.8f) + Rnd() * 1.4f;
                default: return Math.Max(1.6f, 3.4f - _strength * 1.2f) + Rnd() * 1.8f;
            }
        }

        float RunLength()
        {
            switch (Style)
            {
                case FightStyle.Steady: return 0.5f + _strength * 0.3f + Rnd() * 0.2f;
                case FightStyle.Darter: return 0.25f + _strength * 0.15f + Rnd() * 0.15f;
                case FightStyle.Stayer: return 0.7f + _strength * 0.6f + Rnd() * 0.4f;
                default: return 1.0f + _strength * 0.9f + Rnd() * 0.5f;
            }
        }

        float RunPull()
        {
            switch (Style)
            {
                case FightStyle.Steady: return 0.3f + _strength * 0.35f;
                case FightStyle.Darter: return 0.55f + _strength * 0.4f;
                case FightStyle.Stayer: return 0.4f + _strength * 0.45f;
                default: return 0.6f + _strength * 0.55f;
            }
        }

        public void Step(float dt, bool holding)
        {
            if (State != FightState.Fighting) return;
            // The fish: runs of its own kind, telegraphed a moment before.
            if (_runLeft > 0) { _runLeft -= dt; Running = _runLeft > 0; if (!Running) _runIn = NextRunIn(); }
            else { _runIn -= dt; if (_runIn <= 0) { _runLeft = RunLength(); Running = true; } }
            Warning = !Running && _runIn > 0 && _runIn < WarnFor;
            float pull = Running ? RunPull() : 0;
            // A steady fish always leans on the line a little.
            float lean = Style == FightStyle.Steady ? 0.18f + _strength * 0.12f : 0;
            Tension += (holding ? 0.62f + pull + lean : -0.75f + pull * 0.6f + lean) * dt;
            Tension = Math.Max(0, Math.Min(1.05f, Tension));
            // In the band the fish comes in; out of it, it takes line back. A stayer comes in slowly.
            float gain = (0.23f - _strength * 0.08f) * (Style == FightStyle.Stayer ? 0.65f : 1);
            if (InBand) Progress += gain * (holding ? 1 : 0.35f) * dt;
            else if (Tension < BandLow) Progress -= (0.12f + _strength * 0.1f) * (Style == FightStyle.Stayer ? 0.7f : 1) * dt;
            if (Tension >= 0.98f) { _over += dt; if (_over > 0.45f) State = FightState.Snapped; } else _over = Math.Max(0, _over - dt * 2);
            if (Progress >= 1) { Progress = 1; State = FightState.Landed; }
            else if (Progress <= 0) { Progress = 0; State = FightState.Escaped; }
        }
    }

    /// <summary>What a hand-caught fish came to.</summary>
    public struct HandCatch { public SpeciesDef Species; public float Kg, Before; public bool First, Record; public double Cash; public int Gems; }

    /// <summary>
    /// Fishing by hand off the quay: cast, strike, play the fish. It never runs out (there is no energy here). A good
    /// cast and a quick strike bring better fish than the boats do; landing one writes it in the book (hand-caught
    /// fish run heavier, so records fall here), counts for orders and the derby, and pays a little.
    /// </summary>
    public sealed partial class Sim
    {
        /// <summary>Out of 1000 by rarity for a poor cast and for a perfect one; a cast in between is in between.</summary>
        static readonly int[] HandPoor = { 720, 210, 60, 9, 1 }, HandBest = { 430, 330, 180, 52, 8 };

        /// <summary>The fish that takes a hand line: quality 0-1 from the cast and the strike.</summary>
        public SpeciesDef HandBite(double quality)
        {
            if (Focused(H, "collect")) quality += 0.15;       // a collecting harbour knows where the good fish lie
            quality = Math.Max(0, Math.Min(1, quality));
            int roll = (int)(Roll() * 1000), r = 0, acc = 0;
            for (; r < HandPoor.Length - 1; r++) { acc += (int)Math.Round(HandPoor[r] + (HandBest[r] - HandPoor[r]) * quality); if (roll < acc) break; }
            var all = Book.In(H.id);
            for (int want = r; want >= 0; want--)
            {
                var pool = all.FindAll(s => (int)s.Rarity == want && Biting(s));
                if (pool.Count > 0) return pool[(int)(Roll() * pool.Count)];
            }
            return all[0];
        }

        /// <summary>A seed for the fight, so the fish behaves the same however fast the screen draws.</summary>
        public uint FightSeed() { return Next(); }

        /// <summary>Lands a hand-caught fish: the book, the orders, the derby, and its price.</summary>
        public HandCatch LandByHand(SpeciesDef sp)
        {
            var h = H; var e = Entry(sp.Id);
            float kg = sp.MinKg + (sp.MaxKg - sp.MinKg) * (float)Math.Pow(Roll(), 1.1);       // heavier than the boats bring
            float before = e != null ? e.best : 0; int gems = State.gems;
            bool first = Record(sp, kg);
            DerbyCatch(sp, true); OrderCatch(h, sp, true);
            if (first) AddSeasonPoints(20);
            double cash = Math.Max(4 * DefOf(h).Scale, 8 * RewardRate(h)) * Content.RarityValue[(int)sp.Rarity];
            Pay(h, cash);
            State.handCaught++; State.fish++;
            Touch();
            return new HandCatch { Species = sp, Kg = kg, Before = before, First = first, Record = !first && kg > before, Cash = cash, Gems = State.gems - gems };
        }
    }
}

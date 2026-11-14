using System;

namespace ReelEmpire.Core
{
    public enum Weather { Clear, Rain, Fog, Storm }

    /// <summary>
    /// The harbour's clock, weather and season, all read off the real time so they need no saving and are the same on
    /// every device: a 24-minute day (the last quarter is night), weather that changes every three hours, and
    /// four-week seasons. They only ever add fish that bite.
    /// </summary>
    public sealed partial class Sim
    {
        public const double DayLength = 24 * 60, WeatherEvery = 3 * 3600, SeasonLength = 28 * 86400;

        /// <summary>0 at dawn, 0.5 mid-afternoon, 0.75 dusk, then night.</summary>
        public float DayPhase { get { double t = ((RealNow() + State.dayShift) % DayLength + DayLength) % DayLength; return (float)(t / DayLength); } }

        /// <summary>A new game opens on a fresh morning, whatever the clock says.</summary>
        public void StartAtMorning() { State.dayShift = 0.08 * DayLength - RealNow() % DayLength; }

        /// <summary>0 by day, 1 in the dark, easing through dusk and dawn.</summary>
        public float Night
        {
            get
            {
                float p = DayPhase;
                if (p < 0.04f) return 1 - p / 0.04f;        // dawn
                if (p < 0.72f) return 0;
                if (p < 0.78f) return (p - 0.72f) / 0.06f;  // dusk
                return 1;
            }
        }

        public bool IsNight { get { return Night > 0.5f; } }

        static uint Hash(long n)
        {
            uint x = (uint)(n * 2654435761L) ^ (uint)(n >> 13);
            x ^= x >> 16; x *= 0x7feb352d; x ^= x >> 15; x *= 0x846ca68b; x ^= x >> 16;
            return x;
        }

        public Weather Weather
        {
            get
            {
                if (ForcedWeather >= 0) return (Weather)ForcedWeather;
                uint r = Hash((long)Math.Floor(RealNow() / WeatherEvery)) % 100;
                return r < 58 ? Weather.Clear : r < 80 ? Weather.Rain : r < 90 ? Weather.Fog : Weather.Storm;
            }
        }
        /// <summary>Tests and screenshots: a weather to hold (-1: none).</summary>
        public int ForcedWeather = -1;
        public double WeatherLeft { get { return WeatherEvery - RealNow() % WeatherEvery; } }

        /// <summary>0 spring, 1 summer, 2 autumn, 3 winter.</summary>
        public int Season { get { return (int)(Math.Floor(RealNow() / SeasonLength) % 4); } }
        public static readonly string[] SeasonNames = { "Spring", "Summer", "Autumn", "Winter" };
        public double SeasonLeft { get { return SeasonLength - RealNow() % SeasonLength; } }

        bool Holds(Bite b)
        {
            switch (b)
            {
                case Bite.Day: return !IsNight;
                case Bite.Night: return IsNight;
                case Bite.Rain: return Weather == Weather.Rain || Weather == Weather.Storm;
                case Bite.Storm: return Weather == Weather.Storm;
                case Bite.Spring: return Season == 0;
                case Bite.Summer: return Season == 1;
                case Bite.Autumn: return Season == 2;
                case Bite.Winter: return Season == 3;
            }
            return true;
        }

        public static string BiteText(Bite b)
        {
            switch (b)
            {
                case Bite.Day: return "By day"; case Bite.Night: return "At night"; case Bite.Rain: return "In the rain"; case Bite.Storm: return "In a storm";
                case Bite.Spring: return "In spring"; case Bite.Summer: return "In summer"; case Bite.Autumn: return "In autumn"; case Bite.Winter: return "In winter";
            }
            return "Any time";
        }
    }
}

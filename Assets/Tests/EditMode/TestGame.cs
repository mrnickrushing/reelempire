using ReelEmpire.Core;

namespace ReelEmpire.Tests
{
    /// <summary>A new game with a fixed clock, so no test depends on the machine's time.</summary>
    public static class TestGame
    {
        public static Sim New(double now = 1_700_000_000)
        {
            var s = new Sim(GameState.New());
            double t = now;
            s.RealNow = () => t;
            return s;
        }
    }
}
